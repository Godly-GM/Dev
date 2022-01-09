#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.Shared
{
    public static class Constants
    {
        public const string AppServiceName = "DevToysOOPService";

        public const int AppServiceBufferSize = 2048;

        public static readonly TimeSpan AppServiceTimeout = TimeSpan.FromSeconds(10);

        public static readonly JsonSerializerSettings AppServiceJsonSerializerSettings 
            = new()
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
    }
}
