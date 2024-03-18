namespace GIGXR.Platform.GMS
{
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public static class RestApiExtensions
    {
        public static HttpContent ToJsonHttpContent(this object obj)
        {
            var content = new StringContent(JsonConvert.SerializeObject(obj));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }

        public static string ToJsonString(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None);
        }

        public static async UniTask<T> ToObjectAsync<T>(this HttpResponseMessage response)
        {
            var res = await response.Content.ReadAsStringAsync().AsUniTask();
            return JsonConvert.DeserializeObject<T>(res);
        }
    }
}