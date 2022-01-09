#nullable enable

using Newtonsoft.Json;

namespace DevToys.Shared.Core.OOP
{
    public sealed class AppServiceProgressMessage : AppServiceMessageBase
    {
        [JsonProperty]
        public int ProgressPercentage { get; set; }

        [JsonProperty]
        public string? Message { get; set; }
    }
}
