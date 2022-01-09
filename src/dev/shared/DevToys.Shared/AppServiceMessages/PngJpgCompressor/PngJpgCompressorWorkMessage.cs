using DevToys.Shared.Core.OOP;
using Newtonsoft.Json;

namespace DevToys.Shared.AppServiceMessages.PngJpgCompressor
{
    public sealed class PngJpgCompressorWorkMessage : AppServiceMessageBase
    {
        [JsonProperty]
        public string FilePath { get; set; } = null!;
    }
}
