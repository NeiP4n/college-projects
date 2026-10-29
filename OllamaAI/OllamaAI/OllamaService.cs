using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace OllamaAI
{
    public class OllamaModel
    {
        public string Name { get; set; }
        public string DisplayName => Name != null ? Name.Split(':')[0] : "";
        public long Size { get; set; }
        public string SizeDisplay => Size > 0 ? string.Format("{0:F1} GB", Size / 1073741824.0) : "";
    }

    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class OllamaStatus
    {
        public bool IsRunning { get; set; }
        public string Version { get; set; }
        public string Message { get; set; }
        public DateTime CheckedAtUtc { get; set; }
    }

    public class OllamaService
    {
        private static readonly HttpClient Http = new HttpClient();
        private readonly JavaScriptSerializer _json = new JavaScriptSerializer();
        private string _baseUrl;
        private int _timeoutSeconds;

        public OllamaService(string url = "http://localhost:11434", int timeoutSeconds = 120)
        {
            _baseUrl = NormalizeUrl(url);
            SetTimeoutSeconds(timeoutSeconds);
        }

        public void SetBaseUrl(string url)
        {
            _baseUrl = NormalizeUrl(url);
        }

        public void SetTimeoutSeconds(int timeoutSeconds)
        {
            if (timeoutSeconds < 10)
            {
                timeoutSeconds = 10;
            }

            _timeoutSeconds = timeoutSeconds;
        }

        public async Task<bool> IsRunningAsync()
        {
            try
            {
                var resp = await SendGetAsync(_baseUrl + "/api/version").ConfigureAwait(false);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<OllamaStatus> GetStatusAsync()
        {
            var status = new OllamaStatus
            {
                IsRunning = false,
                Version = "",
                Message = "OFF",
                CheckedAtUtc = DateTime.UtcNow
            };

            try
            {
                var resp = await SendGetAsync(_baseUrl + "/api/version").ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                {
                    status.Message = "OFF: HTTP " + (int)resp.StatusCode;
                    return status;
                }

                status.IsRunning = true;
                status.Message = "ON";

                var jsonText = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                var root = _json.DeserializeObject(jsonText) as Dictionary<string, object>;
                if (root != null && root.ContainsKey("version") && root["version"] != null)
                {
                    status.Version = root["version"].ToString();
                }
            }
            catch (TaskCanceledException)
            {
                status.Message = "OFF: timeout";
            }
            catch (HttpRequestException ex)
            {
                status.Message = "OFF: " + ex.Message;
            }
            catch (Exception ex)
            {
                status.Message = "OFF: " + ex.Message;
            }

            return status;
        }

        public async Task<string> DiscoverLanEndpointAsync(int port = 11434, int perProbeTimeoutMs = 700)
        {
            var candidates = BuildLanCandidates(port);
            if (candidates.Count == 0)
            {
                return "";
            }

            var pending = new List<Task<string>>();
            foreach (var endpoint in candidates)
            {
                pending.Add(ProbeEndpointAsync(endpoint, perProbeTimeoutMs));
            }

            while (pending.Count > 0)
            {
                var finished = await Task.WhenAny(pending).ConfigureAwait(false);
                pending.Remove(finished);

                var found = await finished.ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(found))
                {
                    return found;
                }
            }

            return "";
        }

        public async Task<List<OllamaModel>> GetModelsAsync()
        {
            var list = new List<OllamaModel>();

            try
            {
                var resp = await SendGetAsync(_baseUrl + "/api/tags").ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                {
                    return list;
                }

                var jsonText = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                var root = _json.DeserializeObject(jsonText) as Dictionary<string, object>;
                if (root == null || !root.ContainsKey("models"))
                {
                    return list;
                }

                var models = AsEnumerable(root["models"]);
                if (models == null)
                {
                    return list;
                }

                foreach (var entry in models)
                {
                    var modelData = entry as Dictionary<string, object>;
                    if (modelData == null)
                    {
                        continue;
                    }

                    var model = new OllamaModel
                    {
                        Name = GetString(modelData, "name"),
                        Size = ParseLong(modelData.ContainsKey("size") ? modelData["size"] : null)
                    };

                    if (string.IsNullOrWhiteSpace(model.Name))
                    {
                        model.Name = GetString(modelData, "model");
                    }

                    if (string.IsNullOrWhiteSpace(model.Name))
                    {
                        continue;
                    }

                    list.Add(model);
                }
            }
            catch
            {
            }

            list.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
            return list;
        }

        public async Task<string> GenerateAsync(string model, string prompt, string system = "")
        {
            var payload = new Dictionary<string, object>
            {
                { "model", model },
                { "prompt", prompt },
                { "system", system },
                { "stream", false }
            };

            var body = new StringContent(_json.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await SendPostAsync(_baseUrl + "/api/generate", body).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var jsonText = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var root = _json.DeserializeObject(jsonText) as Dictionary<string, object>;
            if (root != null && root.ContainsKey("response") && root["response"] != null)
            {
                return root["response"].ToString();
            }

            return "";
        }

        public async Task<string> GenerateStreamAsync(string model, string prompt, string system, Action<string> onChunk, CancellationToken cancellationToken = default(CancellationToken))
        {
            var payload = new Dictionary<string, object>
            {
                { "model", model },
                { "prompt", prompt },
                { "system", system },
                { "stream", true }
            };

            using (var timeoutCts = new CancellationTokenSource())
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken))
            {
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));
                using (var req = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "/api/generate"))
                {
                    req.Content = new StringContent(_json.Serialize(payload), Encoding.UTF8, "application/json");
                    using (var resp = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false))
                    {
                        resp.EnsureSuccessStatusCode();

                        var full = new StringBuilder();
                        using (var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            while (!reader.EndOfStream)
                            {
                                cts.Token.ThrowIfCancellationRequested();
                                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                                if (string.IsNullOrWhiteSpace(line))
                                {
                                    continue;
                                }

                                Dictionary<string, object> obj = null;
                                try
                                {
                                    obj = _json.DeserializeObject(line) as Dictionary<string, object>;
                                }
                                catch
                                {
                                    continue;
                                }

                                if (obj == null)
                                {
                                    continue;
                                }

                                if (obj.ContainsKey("response") && obj["response"] != null)
                                {
                                    var chunk = obj["response"].ToString();
                                    if (!string.IsNullOrEmpty(chunk))
                                    {
                                        full.Append(chunk);
                                        onChunk?.Invoke(chunk);
                                    }
                                }

                                if (obj.ContainsKey("done") && obj["done"] != null)
                                {
                                    var doneText = obj["done"].ToString();
                                    bool done;
                                    if (bool.TryParse(doneText, out done) && done)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        return full.ToString();
                    }
                }
            }
        }

        public async Task<string> ChatAsync(string model, List<ChatMessage> messages)
        {
            var payload = new Dictionary<string, object>
            {
                { "model", model },
                { "messages", messages },
                { "stream", false }
            };

            var body = new StringContent(_json.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await SendPostAsync(_baseUrl + "/api/chat", body).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var jsonText = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var root = _json.DeserializeObject(jsonText) as Dictionary<string, object>;
            if (root == null || !root.ContainsKey("message"))
            {
                return "";
            }

            var message = root["message"] as Dictionary<string, object>;
            if (message == null || !message.ContainsKey("content") || message["content"] == null)
            {
                return "";
            }

            return message["content"].ToString();
        }

        private static string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "http://localhost:11434";
            }

            return url.Trim().TrimEnd('/');
        }

        private static long ParseLong(object value)
        {
            if (value == null)
            {
                return 0;
            }

            try
            {
                return Convert.ToInt64(value);
            }
            catch
            {
                return 0;
            }
        }

        private static string GetString(Dictionary<string, object> data, string key)
        {
            if (data == null || !data.ContainsKey(key) || data[key] == null)
            {
                return "";
            }

            return data[key].ToString();
        }

        private static IEnumerable AsEnumerable(object value)
        {
            if (value == null)
            {
                return null;
            }

            var list = value as ArrayList;
            if (list != null)
            {
                return list;
            }

            var enumerable = value as IEnumerable;
            if (enumerable != null && !(value is string))
            {
                return enumerable;
            }

            return null;
        }

        private async Task<string> ProbeEndpointAsync(string endpoint, int timeoutMs)
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(Math.Max(200, timeoutMs));
                    var resp = await Http.GetAsync(endpoint + "/api/version", cts.Token).ConfigureAwait(false);
                    if (resp.IsSuccessStatusCode)
                    {
                        return endpoint;
                    }
                }
            }
            catch
            {
            }

            return "";
        }

        private async Task<HttpResponseMessage> SendGetAsync(string url)
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));
                return await Http.GetAsync(url, cts.Token).ConfigureAwait(false);
            }
        }

        private async Task<HttpResponseMessage> SendPostAsync(string url, HttpContent content)
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));
                return await Http.PostAsync(url, content, cts.Token).ConfigureAwait(false);
            }
        }

        private static List<string> BuildLanCandidates(int port)
        {
            var endpoints = new List<string>();
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var prefix in GetLocalSubnetPrefixes())
            {
                for (var i = 1; i <= 254; i++)
                {
                    var endpoint = "http://" + prefix + "." + i + ":" + port;
                    if (unique.Add(endpoint))
                    {
                        endpoints.Add(endpoint);
                    }
                }
            }

            return endpoints;
        }

        private static List<string> GetLocalSubnetPrefixes()
        {
            var prefixes = new List<string>();
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var ni in interfaces)
                {
                    if (ni.OperationalStatus != OperationalStatus.Up)
                    {
                        continue;
                    }

                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }

                    var props = ni.GetIPProperties();
                    foreach (var address in props.UnicastAddresses)
                    {
                        if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        {
                            continue;
                        }

                        var ip = address.Address.ToString();
                        if (!IsPrivateIpv4(ip))
                        {
                            continue;
                        }

                        var parts = ip.Split('.');
                        if (parts.Length != 4)
                        {
                            continue;
                        }

                        var prefix = parts[0] + "." + parts[1] + "." + parts[2];
                        if (unique.Add(prefix))
                        {
                            prefixes.Add(prefix);
                        }
                    }
                }
            }
            catch
            {
            }

            if (prefixes.Count == 0)
            {
                prefixes.Add("192.168.1");
                prefixes.Add("192.168.0");
            }

            return prefixes;
        }

        private static bool IsPrivateIpv4(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                return false;
            }

            var p = ip.Split('.');
            if (p.Length != 4)
            {
                return false;
            }

            int a;
            int b;
            if (!int.TryParse(p[0], out a) || !int.TryParse(p[1], out b))
            {
                return false;
            }

            if (a == 10)
            {
                return true;
            }

            if (a == 172 && b >= 16 && b <= 31)
            {
                return true;
            }

            if (a == 192 && b == 168)
            {
                return true;
            }

            return false;
        }
    }
}
