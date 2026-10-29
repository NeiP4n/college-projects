using System.Collections.Generic;
using Newtonsoft.Json;

namespace WordAIAgent.Models
{
    public class OllamaRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("messages")]
        public List<OllamaMessage> Messages { get; set; } = new();

        [JsonProperty("stream")]
        public bool Stream { get; set; } = false;

        [JsonProperty("options")]
        public OllamaOptions Options { get; set; } = new();
    }

    public class OllamaMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class OllamaOptions
    {
        [JsonProperty("temperature")]
        public double Temperature { get; set; } = 0.7;

        [JsonProperty("num_predict")]
        public int NumPredict { get; set; } = 2048;
    }

    public class OllamaResponse
    {
        [JsonProperty("message")]
        public OllamaMessage Message { get; set; }

        [JsonProperty("done")]
        public bool Done { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public class OllamaTagsResponse
    {
        [JsonProperty("models")]
        public List<OllamaModel> Models { get; set; } = new();
    }

    public class OllamaModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }
}
