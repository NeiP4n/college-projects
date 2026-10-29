using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeLearn.Controllers
{
    // Скачивание текущего трека в .mp3 через yt-dlp (+ffmpeg).
    // Требует установленных в PATH: yt-dlp и ffmpeg.
    [Authorize]
    [Route("api/flowy")]
    public class FlowyController : Controller
    {
        private static readonly Regex _allowed =
            new(@"^https://(www\.)?(youtube\.com|youtu\.be|m\.youtube\.com|soundcloud\.com)/",
                RegexOptions.IgnoreCase);

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string? url)
        {
            if (string.IsNullOrWhiteSpace(url) || !_allowed.IsMatch(url))
                return BadRequest("Поддерживаются только ссылки YouTube / SoundCloud");

            var dir = Path.Combine(Path.GetTempPath(), "flowy_dl");
            Directory.CreateDirectory(dir);
            var baseName = Guid.NewGuid().ToString("N");
            var outTmpl  = Path.Combine(dir, baseName + ".%(ext)s");

            var psi = new ProcessStartInfo
            {
                FileName               = "yt-dlp",
                RedirectStandardError  = true,
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            };
            // Аргументы — списком (без shell), чтобы исключить инъекции
            psi.ArgumentList.Add("-x");
            psi.ArgumentList.Add("--audio-format"); psi.ArgumentList.Add("mp3");
            psi.ArgumentList.Add("--audio-quality"); psi.ArgumentList.Add("0");
            psi.ArgumentList.Add("--no-playlist");
            psi.ArgumentList.Add("-o"); psi.ArgumentList.Add(outTmpl);
            psi.ArgumentList.Add(url);

            try
            {
                using var proc = Process.Start(psi);
                if (proc == null) return StatusCode(500, "Не удалось запустить yt-dlp");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
                _ = proc.StandardOutput.ReadToEndAsync();
                var err = await proc.StandardError.ReadToEndAsync();
                try { await proc.WaitForExitAsync(cts.Token); }
                catch (OperationCanceledException) { try { proc.Kill(true); } catch { } return StatusCode(504, "Таймаут скачивания"); }

                var file = Directory.GetFiles(dir, baseName + ".*").FirstOrDefault();
                if (file == null)
                    return StatusCode(500, "Не удалось скачать. Установлены ли yt-dlp и ffmpeg? " +
                                           (string.IsNullOrWhiteSpace(err) ? "" : err[..Math.Min(err.Length, 300)]));

                var bytes = await System.IO.File.ReadAllBytesAsync(file);
                try { System.IO.File.Delete(file); } catch { }

                var name = "track" + Path.GetExtension(file);
                return File(bytes, "audio/mpeg", name);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return StatusCode(500, "yt-dlp не найден. Установи yt-dlp и ffmpeg (в PATH).");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка: " + ex.Message);
            }
        }
    }
}
