#!/bin/sh
# OpenWrt Security Monitor + Telegram Bot v4
# Fixes: child processes killed on exit, crisis healer via log file, load threshold, WG names

CONFIG="/etc/telegram-bot.conf"
LOG="/tmp/bot-monitor.log"
[ -f "$CONFIG" ] && . "$CONFIG"

TG_API="https://api.telegram.org/bot${BOT_TOKEN}/sendMessage"
MONITOR_IP="192.168.1.142"
WG_IFACE="Zapret"
WG_SUBNET="10.10.0."

COOLDOWN_DIR="/tmp/bot-cooldown"
COOLDOWN_SSH=10
COOLDOWN_DHCP=300
COOLDOWN_WARN=60
COOLDOWN_CRIT=300
LOAD_THRESHOLD="2.0"
TEMP_THRESHOLD=80

mkdir -p "$COOLDOWN_DIR"

# Kill all children on exit so procd restart doesn't leave orphans
cleanup() {
    kill 0 2>/dev/null
    exit 0
}
trap cleanup TERM INT

notify() {
    local msg="$1"
    local priority="${2:-INFO}"
    local ts="$(date '+%Y-%m-%d %H:%M:%S')"
    echo "$ts [$priority] $msg" >> "$LOG"
    curl -s --max-time 5 "${TG_API}" \
        -d "chat_id=${CHAT_ID}&parse_mode=HTML&text=<b>${priority}</b>: ${msg}" \
        >/dev/null 2>&1 &
}

cooldown() {
    local key="$1"
    local secs="${2:-60}"
    local file="${COOLDOWN_DIR}/${key}"
    local now="$(date +%s)"
    if [ -f "$file" ]; then
        local last=$(cat "$file")
        [ $((now - last)) -lt $secs ] && return 1
    fi
    echo "$now" > "$file"
    return 0
}

clean_cooldowns() {
    local now=$(date +%s)
    for f in "$COOLDOWN_DIR"/*; do
        [ -f "$f" ] || continue
        local last=$(cat "$f")
        local key=$(basename "$f")
        local ttl=3600
        case "$key" in
            dhcp*) ttl=7200 ;;
            wg-*)  ttl=3600 ;;
            auth*) ttl=1800 ;;
        esac
        [ $((now - last)) -gt $ttl ] && rm -f "$f"
    done
}

# Build IP->Name map from UCI config
build_wg_names() {
    local WG_NAMES_FILE="/tmp/bot-wg-names"
    uci show network 2>/dev/null | awk '
    /wireguard_Zapret.*\.description=/ {
        gsub(/.*='"'"'|'"'"'$/, "")
        desc = $0
    }
    /wireguard_Zapret.*\.allowed_ips=/ {
        gsub(/.*='"'"'|\/.*'"'"'$/, "")
        ip = $0
        if (ip && desc) print ip " " desc
        desc = ""
    }
    ' > "$WG_NAMES_FILE"
}

wg_get_name() {
    local ip="$1"
    local name
    name=$(grep "^${ip} " /tmp/bot-wg-names 2>/dev/null | cut -d' ' -f2-)
    echo "${name:-$ip}"
}

notify "Монитор запущен" "START"
build_wg_names

WG_KNOWN="/tmp/bot-wg-known"
: > "$WG_KNOWN"

wg_monitor_cycle() {
    local now=$(date +%s)
    wg show "$WG_IFACE" dump 2>/dev/null | awk -v subnet="$WG_SUBNET" -v now="$now" '
    NR == 1 { next }
    {
        peer_key = $1
        ips = $4
        handshake_ts = $5 + 0
        if (handshake_ts == 0) next
        split(ips, a, ",")
        for (i in a) {
            ip = a[i]; sub(/\/.*/, "", ip)
            if (index(ip, subnet) == 1) {
                online = ((now - handshake_ts) < 120) ? "yes" : "no"
                print ip, online
            }
        }
    }' | while read ip online; do
        local key="wg-${ip}"
        local known=""
        [ -f "$WG_KNOWN" ] && known=$(grep "^${ip} " "$WG_KNOWN" 2>/dev/null | cut -d' ' -f2)
        local name
        name=$(wg_get_name "$ip")
        if [ "$online" = "yes" ] && [ "$known" != "online" ]; then
            cooldown "$key" 300 && notify "🟢 VPN подключился: <b>${name}</b> (${ip})" "INFO"
        elif [ "$online" = "no" ] && [ "$known" = "online" ]; then
            cooldown "$key" 300 && notify "🔴 VPN отключился: <b>${name}</b> (${ip})" "WARN"
        fi
        grep -v "^${ip} " "$WG_KNOWN" 2>/dev/null > "${WG_KNOWN}.tmp"
        echo "$ip $online" >> "${WG_KNOWN}.tmp"
        mv "${WG_KNOWN}.tmp" "$WG_KNOWN"
    done
}

check_load() {
    local load1 load5 load15
    read load1 load5 load15 _ < /proc/loadavg
    local high
    high=$(awk -v l="$load1" -v t="$LOAD_THRESHOLD" 'BEGIN{print (l+0 >= t+0) ? "yes" : "no"}')
    if [ "$high" = "yes" ]; then
        cooldown "load" 300 && notify "⚡ Высокая нагрузка: ${load1} ${load5} ${load15}" "WARN"
    fi
}

check_temp() {
    local temp_file temp_val temp_c
    for temp_file in /sys/class/thermal/thermal_zone*/temp; do
        [ -f "$temp_file" ] || continue
        temp_val=$(cat "$temp_file" 2>/dev/null)
        temp_c=$((temp_val / 1000))
        if [ "$temp_c" -ge "$TEMP_THRESHOLD" ]; then
            cooldown "temp-overheat" 600 && notify "🌡️ Перегрев роутера: ${temp_c}°C" "WARN"
            break
        fi
    done
}

check_wan() {
    local ip
    ip=$(ip route get 8.8.8.8 2>/dev/null | awk '/src/{for(i=1;i<NF;i++) if($i=="src") print $(i+1)}')
    [ -z "$ip" ] && return
    local prev_file="/tmp/bot-wan-ip"
    if [ -f "$prev_file" ]; then
        local prev
        prev=$(cat "$prev_file")
        if [ "$prev" != "$ip" ]; then
            notify "🌐 WAN IP изменился: ${prev} → ${ip}" "INFO"
        fi
    fi
    echo "$ip" > "$prev_file"
}

send_daily_summary() {
    local hour min
    hour=$(date +%H)
    min=$(date +%M)
    [ "$hour" = "08" ] && [ "$min" -lt 2 ] || return
    cooldown "daily-summary" 3600 || return

    local wg_online=0
    local wg_list=""
    local ip state name
    while read ip state; do
        if [ "$state" = "online" ]; then
            name=$(wg_get_name "$ip")
            wg_online=$((wg_online + 1))
            wg_list="${wg_list}  • ${name} (${ip})
"
        fi
    done < "$WG_KNOWN"

    local load1 load5 load15
    read load1 load5 load15 _ < /proc/loadavg
    local uptime_s
    uptime_s=$(cut -d. -f1 /proc/uptime)
    local uptime_h=$((uptime_s / 3600))

    local msg
    msg="📊 <b>Ежедневная сводка</b>
🕐 Аптайм: ${uptime_h}ч
⚡ Нагрузка: ${load1} ${load5} ${load15}
🔐 VPN онлайн: ${wg_online}"
    [ -n "$wg_list" ] && msg="${msg}
${wg_list}"
    notify "$msg" "INFO"
}

check_crisis_healer() {
    local crisis_log="/tmp/crisis-healer.log"
    [ -f "$crisis_log" ] || return
    # Get timestamp of last entry
    local last_line
    last_line=$(tail -1 "$crisis_log" 2>/dev/null)
    [ -z "$last_line" ] && return
    # Only alert on WATCHDOG/ROTATE entries (not recovered ones)
    echo "$last_line" | grep -qE "WATCHDOG|ROTATE|FAILED" || return
    # Check if this is a new entry (not seen before)
    local last_seen_file="/tmp/bot-crisis-last"
    local last_seen=""
    [ -f "$last_seen_file" ] && last_seen=$(cat "$last_seen_file")
    [ "$last_line" = "$last_seen" ] && return
    echo "$last_line" > "$last_seen_file"
    local detail
    detail=$(echo "$last_line" | sed 's/^[0-9-]* [0-9:]* //')
    cooldown "crisis" 240 && notify "🔧 Crisis Healer: ${detail}" "WARN"
}

nft_check_cycle() {
    local drops
    drops=$(nft list chain inet fw4 forward 2>/dev/null | grep -c "drop" 2>/dev/null)
    [ "$drops" -gt 100 ] && cooldown "nft-drops" 600 && notify "Много DROP-правил в fw4: ${drops}" "WARN"
}

# Periodic checks in background
periodic_checker() {
    while :; do
        wg_monitor_cycle
        check_load
        check_temp
        check_wan
        check_crisis_healer
        send_daily_summary
        nft_check_cycle
        clean_cooldowns
        sleep 30
    done
}
periodic_checker &

logread -f 2>/dev/null | while IFS= read -r line; do
    msg="$line"

    case "$msg" in
        *"authentication failed"*)
            mac=$(echo "$msg" | grep -oE '([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}')
            [ -n "$mac" ] && cooldown "wifi-${mac}" 3600 && notify "📶 WiFi неверный пароль: ${mac}" "ALERT"
            ;;

        *"dropbear"*"auth"*"failed"*|*"Failed password"*)
            ip=$(echo "$msg" | grep -oE '([0-9]{1,3}\.){3}[0-9]{1,3}')
            [ "$ip" = "$MONITOR_IP" ] && continue
            [ -n "$ip" ] && cooldown "ssh-fail-${ip}" $COOLDOWN_SSH && notify "🚫 SSH неудачная попытка: ${ip}" "ALERT"
            ;;

        *"dropbear"*"auth"*"succeeded"*)
            ip=$(echo "$msg" | grep -oE '([0-9]{1,3}\.){3}[0-9]{1,3}')
            [ "$ip" = "$MONITOR_IP" ] && continue
            [ -n "$ip" ] && cooldown "ssh-ok-${ip}" $COOLDOWN_SSH && notify "🔑 SSH успешный вход: ${ip}" "ALERT"
            ;;

        *"DHCPACK"*)
            mac=$(echo "$msg" | grep -oE '([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}')
            ip=$(echo "$msg" | grep -oE '([0-9]{1,3}\.){3}[0-9]{1,3}')
            host=$(echo "$msg" | awk '{for(i=1;i<NF;i++) if($i=="name") print $(i+1)}')
            [ -n "$ip" ] && cooldown "dhcp-${ip}" $COOLDOWN_DHCP && notify "📱 Новое устройство: ${host:-?} (${ip}, ${mac:-?})" "INFO"
            ;;

        *"DPT="*"DROP"*|*"DPT="*"REJECT"*)
            ip=$(echo "$msg" | grep -oE 'SRC=[0-9.]{7,15}' | cut -d= -f2)
            port=$(echo "$msg" | grep -oE 'DPT=[0-9]+' | cut -d= -f2)
            [ -n "$ip" ] && cooldown "drop-${ip}" $COOLDOWN_WARN && notify "🛡️ Пакет отклонён: ${ip}:${port}" "WARN"
            ;;

        # crisis-healer events handled in periodic_checker via log file — skip cron spam here
        *"crisis-healer watchdog"*|*"crisis-healer rotate"*)
            ;;


        *"conntrack"*"full"*|*"nf_conntrack"*"table full"*)
            cooldown "conntrack-full" 600 && notify "💥 Connection table full — возможна атака" "CRITICAL"
            ;;

        *"luci"*"auth"*|*"rpc"*"session"*)
            ip=$(echo "$msg" | grep -oE '([0-9]{1,3}\.){3}[0-9]{1,3}')
            [ "$ip" = "$MONITOR_IP" ] && continue
            [ -n "$ip" ] && cooldown "luci-${ip}" 300 && notify "🖥️ LUCI: попытка входа с ${ip}" "ALERT"
            ;;

        *"dns"*"amplification"*|*"dns"*"attack"*|*"query"*"denied"*)
            ip=$(echo "$msg" | grep -oE '([0-9]{1,3}\.){3}[0-9]{1,3}')
            [ -n "$ip" ] && cooldown "dns-attack-${ip}" 600 && notify "🎯 Подозрительный DNS: ${ip}" "CRITICAL"
            ;;

        *"firewall"*"reload"*|*"nftables"*)
            cooldown "fw-reload" 600 && notify "🔥 Межсетевой экран перезагружен" "WARN"
            ;;

        *"pppoe"*"disconnected"*|*"modem"*"hangup"*)
            cooldown "wan-down" 600 && notify "❌ WAN отключился!" "CRITICAL"
            ;;

        *"pppoe"*"connected"*)
            cooldown "wan-up" 600 && notify "✅ WAN подключился" "INFO"
            ;;
    esac

    clean_cooldowns
done
