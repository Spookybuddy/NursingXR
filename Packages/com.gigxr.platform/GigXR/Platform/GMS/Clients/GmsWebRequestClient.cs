namespace GIGXR.GMS.Clients
{
    using Cysharp.Threading.Tasks;
    using Models;
    using Newtonsoft.Json;
    using System.Net.Http.Headers;
    using System.Text;
    using UnityEngine.Networking;
    using System;
    using GIGXR.Platform.GMS.Exceptions;

    public class GmsWebRequestClient : BaseApiClient
    {
        public AuthenticationHeaderValue Authorization { get; set; }

        private readonly GmsApiClientConfiguration configuration;

        public GmsWebRequestClient(GmsApiClientConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async UniTask<T> Get<T>(string path, bool useVersioning = false, bool supressError = true) where T : class
        {
            var uri = new Uri(configuration.BaseAddress, path);

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                GIGXR.Platform.Utilities.Logger.Info($"GET {uri}", "WebRequest");

                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("User-Agent", configuration.UserAgent.ToString());
                webRequest.SetRequestHeader("AUTHORIZATION", Authorization.ToString());

                // If a version is not provided, GMS will default to v1.0
                if(useVersioning && configuration.GmsVersion != null)
                {
                    webRequest.SetRequestHeader("api-version", configuration.GmsVersion);
                }

                if(supressError)
                {
                    try
                    {
                        _ = webRequest.SendWebRequest();

                        await UniTask.WaitUntil(() => webRequest.isDone);

                        if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                            throw new Exception(webRequest.error);
                        else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                            throw new GmsApiException(webRequest.error);
                    }
                    catch
                    {
                        ProcessErrorsAsync(webRequest);

                        return null;
                    }
                }
                else
                {
                    _ = webRequest.SendWebRequest();

                    await UniTask.WaitUntil(() => webRequest.isDone);

                    if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                        throw new GmsApiException(webRequest.error);
                    else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                        throw new GmsServerException(webRequest.error);
                }
                
                var successResponse = JsonConvert.DeserializeObject<SuccessResponse<T>>(webRequest.downloadHandler.text);
                return successResponse.data;
            }
        }

        /// <summary>
        /// Sends content as a JSON string to a GMS Endpoint using the POST command.
        /// </summary>
        /// <typeparam name="T">The data type that is returned from the POST endpoint.</typeparam>
        /// <param name="path">The relative path of the GMS API end point</param>
        /// <param name="content">The content to post as a JSON string.</param>
        /// <returns></returns>
        public async UniTask<T> Post<T>(string path, string content, bool useVersioning = false, bool supressError = true) where T : class
        {
            if (string.IsNullOrEmpty(content))
                content = "{}";

            byte[] jsonBytes = Encoding.UTF8.GetBytes(content);

            var uri = new Uri(configuration.BaseAddress, path);

            using (UnityWebRequest webRequest = new UnityWebRequest(uri, "POST"))
            {
                GIGXR.Platform.Utilities.Logger.Info($"POST {uri}:{content}", "WebRequest");

                webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("User-Agent", configuration.UserAgent.ToString());

                // If a version is not provided, GMS will default to v1.0
                if (useVersioning && configuration.GmsVersion != null)
                {
                    webRequest.SetRequestHeader("api-version", configuration.GmsVersion);
                }

                // This occurs with the first POST request to log in, otherwise the Authorization header will be needed
                if (Authorization != null)
                    webRequest.SetRequestHeader("AUTHORIZATION", Authorization.ToString());

                if(supressError)
                {
                    try
                    {
                        _ = webRequest.SendWebRequest();

                        await UniTask.WaitUntil(() => webRequest.isDone);

                        if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                            throw new Exception(webRequest.error);
                        else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                            throw new GmsApiException(webRequest.error);
                    }
                    catch
                    {
                        ProcessErrorsAsync(webRequest);

                        return null;
                    }
                }
                else
                {
                    _ = webRequest.SendWebRequest();

                    await UniTask.WaitUntil(() => webRequest.isDone);

                    if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                        throw new Exception(webRequest.error);
                    else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                        throw new GmsApiException(webRequest.error);
                }
                
                var dataResponse = await UniTask.Create<T>(() =>
                {
                    var successResponse = JsonConvert.DeserializeObject<SuccessResponse<T>>(webRequest.downloadHandler.text);

                    return UniTask.FromResult(successResponse?.data);
                });

                return dataResponse;
            }
        }

        /// <summary>
        /// Posts content to an endpoint without a return value.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async UniTask Post(string path, string content, bool useVersioning = false, bool supressError = true)
        {
            if (string.IsNullOrEmpty(content))
                content = "{}";

            byte[] jsonBytes = Encoding.UTF8.GetBytes(content);

            var uri = new Uri(configuration.BaseAddress, path);

            using (UnityWebRequest webRequest = new UnityWebRequest(uri, "POST"))
            {
                GIGXR.Platform.Utilities.Logger.Info($"POST {uri}:{content}", "WebRequest");

                webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("User-Agent", configuration.UserAgent.ToString());
                webRequest.SetRequestHeader("AUTHORIZATION", Authorization.ToString());

                // If a version is not provided, GMS will default to v1.0
                if (useVersioning && configuration.GmsVersion != null)
                {
                    webRequest.SetRequestHeader("api-version", configuration.GmsVersion);
                }

                if (supressError)
                {
                    try
                    {
                        _ = webRequest.SendWebRequest();

                        await UniTask.WaitUntil(() => webRequest.isDone);

                        if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                            throw new Exception(webRequest.error);
                        else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                            throw new GmsApiException(webRequest.error);
                    }
                    catch
                    {
                        ProcessErrorsAsync(webRequest);
                    }
                }
                else
                {
                    _ = webRequest.SendWebRequest();

                    await UniTask.WaitUntil(() => webRequest.isDone);

                    if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                        throw new Exception(webRequest.error);
                    else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                        throw new GmsApiException(webRequest.error);
                }
            }
        }

        public async UniTask<T> Put<T>(string path, string content, bool useVersioning = false, bool supressError = true) where T : class
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(content);

            var uri = new Uri(configuration.BaseAddress, path);

            using (UnityWebRequest webRequest = new UnityWebRequest(uri, "PUT"))
            {
                GIGXR.Platform.Utilities.Logger.Info($"PUT {uri}:{content}", "WebRequest");

                webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("User-Agent", configuration.UserAgent.ToString());
                webRequest.SetRequestHeader("AUTHORIZATION", Authorization.ToString());

                // If a version is not provided, GMS will default to v1.0
                if (useVersioning && configuration.GmsVersion != null)
                {
                    webRequest.SetRequestHeader("api-version", configuration.GmsVersion);
                }

                if (supressError)
                {
                    try
                    {
                        _ = webRequest.SendWebRequest();

                        await UniTask.WaitUntil(() => webRequest.isDone);

                        if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                            throw new Exception(webRequest.error);
                        else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                            throw new GmsApiException(webRequest.error);
                    }
                    catch
                    {
                        ProcessErrorsAsync(webRequest);

                        return null;
                    }
                }
                else
                {
                    _ = webRequest.SendWebRequest();

                    await UniTask.WaitUntil(() => webRequest.isDone);

                    if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                        throw new Exception(webRequest.error);
                    else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                        throw new GmsApiException(webRequest.error);
                }

                var successResponse = JsonConvert.DeserializeObject<SuccessResponse<T>>(webRequest.downloadHandler.text);
                return successResponse.data;
            }
        }

        public async UniTask<bool> Put(string path, string content, bool useVersioning = false, bool supressError = true)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(content);

            var uri = new Uri(configuration.BaseAddress, path);

            using (UnityWebRequest webRequest = new UnityWebRequest(uri, "PUT"))
            {
                GIGXR.Platform.Utilities.Logger.Info($"PUT {uri}:{content}", "WebRequest");

                webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("User-Agent", configuration.UserAgent.ToString());
                webRequest.SetRequestHeader("AUTHORIZATION", Authorization.ToString());

                // If a version is not provided, GMS will default to v1.0
                if (useVersioning && configuration.GmsVersion != null)
                {
                    webRequest.SetRequestHeader("api-version", configuration.GmsVersion);
                }

                if (supressError)
                {
                    try
                    {
                        _ = webRequest.SendWebRequest();

                        await UniTask.WaitUntil(() => webRequest.isDone);

                        if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                            throw new Exception(webRequest.error);
                        else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                            throw new GmsApiException(webRequest.error);
                    }
                    catch
                    {
                        ProcessErrorsAsync(webRequest);

                        return false;
                    }
                }
                else
                {
                    _ = webRequest.SendWebRequest();

                    await UniTask.WaitUntil(() => webRequest.isDone);

                    if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                        throw new Exception(webRequest.error);
                    else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                        throw new GmsApiException(webRequest.error);
                }

                return webRequest.result == UnityWebRequest.Result.Success;
            }
        }

        public async UniTask Delete(string path, bool useVersioning = false, bool supressError = true)
        {
            var uri = new Uri(configuration.BaseAddress, path);

            using (UnityWebRequest webRequest = UnityWebRequest.Delete(uri))
            {
                GIGXR.Platform.Utilities.Logger.Info($"DELETE {uri}", "WebRequest");

                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("User-Agent", configuration.UserAgent.ToString());
                webRequest.SetRequestHeader("AUTHORIZATION", Authorization.ToString());

                // If a version is not provided, GMS will default to v1.0
                if (useVersioning && configuration.GmsVersion != null)
                {
                    webRequest.SetRequestHeader("api-version", configuration.GmsVersion);
                }

                if (supressError)
                {
                    try
                    {
                        _ = webRequest.SendWebRequest();

                        await UniTask.WaitUntil(() => webRequest.isDone);

                        if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                            throw new Exception(webRequest.error);
                        else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                            throw new GmsApiException(webRequest.error);
                    }
                    catch
                    {
                        ProcessErrorsAsync(webRequest);
                    }
                }
                else
                {
                    _ = webRequest.SendWebRequest();

                    await UniTask.WaitUntil(() => webRequest.isDone);

                    if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                        throw new Exception(webRequest.error);
                    else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                        throw new GmsApiException(webRequest.error);
                }
            }
        }

        public async UniTask<bool> Patch(string path, string content, bool useVersioning = false, bool supressError = true)
        {
            // Can't patch anything if there is empty data
            if (string.IsNullOrEmpty(content))
                return false;
            
            byte[] jsonBytes = Encoding.UTF8.GetBytes(content);

            var uri = new Uri(configuration.BaseAddress, path);

            using (UnityWebRequest webRequest = new UnityWebRequest(uri, "PATCH"))
            {
                GIGXR.Platform.Utilities.Logger.Info($"PATCH {uri}", "WebRequest");

                webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("User-Agent", configuration.UserAgent.ToString());
                webRequest.SetRequestHeader("AUTHORIZATION", Authorization.ToString());

                // If a version is not provided, GMS will default to v1.0
                if (useVersioning && configuration.GmsVersion != null)
                {
                    webRequest.SetRequestHeader("api-version", configuration.GmsVersion);
                }

                if (supressError)
                {
                    try
                    {
                        _ = webRequest.SendWebRequest();

                        await UniTask.WaitUntil(() => webRequest.isDone);

                        if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                            throw new Exception(webRequest.error);
                        else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                            throw new GmsApiException(webRequest.error);
                    }
                    catch
                    {
                        ProcessErrorsAsync(webRequest);
                    }
                }
                else
                {
                    _ = webRequest.SendWebRequest();

                    await UniTask.WaitUntil(() => webRequest.isDone);

                    if (webRequest.responseCode >= 400 && webRequest.responseCode < 500)
                        throw new Exception(webRequest.error);
                    else if (webRequest.responseCode >= 500 && webRequest.responseCode < 600)
                        throw new GmsApiException(webRequest.error);
                }

                return webRequest.result == UnityWebRequest.Result.Success;
            }
        }
    }
}