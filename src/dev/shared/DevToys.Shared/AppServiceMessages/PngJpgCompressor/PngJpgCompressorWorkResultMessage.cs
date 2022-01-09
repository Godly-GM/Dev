using DevToys.Shared.Core.OOP;
using Newtonsoft.Json;

namespace DevToys.Shared.AppServiceMessages.PngJpgCompressor
{
    public sealed class PngJpgCompressorWorkResultMessage : AppServiceMessageBase
    {
        [JsonProperty]
        public string? ErrorMessage { get; set; }

        [JsonProperty]
        public long NewFileSize { get; set; }

        [JsonProperty]
        public double PercentageSaved { get; set; }

        [JsonProperty]
        public string TempCompressedFilePath { get; set; } = string.Empty;
    }
}
