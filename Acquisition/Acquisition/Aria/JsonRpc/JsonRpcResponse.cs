﻿using Newtonsoft.Json;

namespace Acquisition.Aria.JsonRpc
{
    public class JsonRpcResponse<T>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("jsonrpc")]
        public string Version { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }
    }
}
