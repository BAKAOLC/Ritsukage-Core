using Newtonsoft.Json;
using Ritsukage.Library.OCRSpace.Attribute;
using Ritsukage.Library.OCRSpace.Enum;
using Ritsukage.Library.OCRSpace.Struct;
using Ritsukage.Tools.Console;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ritsukage.Library.OCRSpace
{
    public class OCRSpaceApi
    {
        const string ApiPath = "/parse/image";

        public ApiHost ApiHost;

        public OCREngine OCREngine;

        string ApiUrl => ApiHost.GetDescription() + ApiPath;

        int OCREngineType => (int)OCREngine;

        readonly string ApiKey;

        public OCRSpaceApi(string key, ApiHost apiHost = ApiHost.Free, OCREngine engine = OCREngine.Default)
        {
            ApiKey = key;
            ApiHost = apiHost;
            OCREngine = engine;
        }

        async Task<Response> InnerDoOCR(MultipartFormDataContent form, int timeOut = 60000)
        {
            HttpClient httpClient = new()
            {
                Timeout = TimeSpan.FromMilliseconds(timeOut)
            };
            HttpResponseMessage response = await httpClient.PostAsync(ApiUrl, form);
            string strContent = await response.Content.ReadAsStringAsync();
            ConsoleLog.Debug(nameof(OCRSpaceApi), strContent);
            return JsonConvert.DeserializeObject<Response>(strContent);
        }

        public async Task<Response> DoOCR(Stream stream,
            Language language = Language.Default,
            FileType fileType = FileType.PNG,
            int timeOut = 60000)
        {
            MultipartFormDataContent form = new()
            {
                { new StringContent(ApiKey), "apikey" },
                { new StringContent(OCREngineType.ToString()), "ocrengine" },
                { new StringContent("true"), "scale" },
                { new StringContent("true"), "istable" },
                { new StreamContent(stream), fileType.ToString(), "file" + fileType.GetDescription() }
            };
            if (language != Language.Default)
                form.Add(new StringContent(language.ToString()), "language");
            return await InnerDoOCR(form, timeOut);
        }

        public async Task<Response> DoOCR(string url,
            Language language = Language.Default,
            FileType fileType = FileType.Auto,
            int timeOut = 60000)
        {
            MultipartFormDataContent form = new()
            {
                { new StringContent(ApiKey), "apikey" },
                { new StringContent(OCREngineType.ToString()), "ocrengine" },
                { new StringContent("true"), "scale" },
                { new StringContent("true"), "istable" },
                { new StringContent(url), "url" },
            };
            if (language != Language.Default)
                form.Add(new StringContent(language.ToString()), "language");
            if (fileType != FileType.Auto)
                form.Add(new StringContent(fileType.ToString()), "filetype");
            return await InnerDoOCR(form, timeOut);
        }
    }
}