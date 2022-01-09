#nullable enable

using System.Text;

namespace DevToys.Shared.Core.OOP
{
    public class LowLevelAppServiceMessage
    {
        public byte[] Buffer { get; }

        public StringBuilder Message { get; }

        public LowLevelAppServiceMessage(int bufferSize, StringBuilder? stringBuilder)
        {
            Buffer = new byte[bufferSize];
            Message = stringBuilder ?? new StringBuilder();
        }
    }
}
