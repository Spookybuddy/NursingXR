/*
 * THIS CLASS IS NOT IN USE.
 * 
 * It is a part of old download management utilities, and has been left
 * as a reference for upcoming Content Management efforts - CU-1x0q7ce
 */

/*
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using GIGXR.GMS.Models;
using GIGXR.Platform.Utilities;

namespace GIGXR.Platform.Downloads
{
    /// <summary>
    /// A coroutine to get the content length of the provided <c>resource</c>.
    ///
    /// Will asynchronously get the content length from the server on the first call and then cache for future access.
    /// </summary>
    public class WaitForCachedResourceContentLength : CustomYieldInstruction
    {
        private readonly Resource resource;
        private readonly ResourceContentLengthCache cache;

        public WaitForCachedResourceContentLength(
            Resource resource,
            ResourceContentLengthCache cache,
            HttpClientFactory factory)
        {
            this.resource = resource ?? throw new ArgumentNullException(nameof(resource));
            this.cache = cache ?? throw new ArgumentNullException(nameof(resource));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            if (cache.TryGetValue(resource, out var contentLength))
            {
                ContentLength = contentLength;
                IsSuccess = true;
                IsCompleted = true;
            }
            else
            {
                var client = factory.CreateClient();
                client.SendAsync(new HttpRequestMessage(HttpMethod.Head, resource.GetUriForPlatform()))
                    .ContinueWithOnUnityThread(HttpResponseCallback);
            }
        }

        public WaitForCachedResourceContentLength(Resource resource) : this(
            resource,
            ResourceContentLengthCache.Instance,
            HttpClientFactory.Instance)
        {
        }

        public override bool keepWaiting => !IsCompleted;
        public long ContentLength { get; private set; } = -1;
        public bool IsSuccess { get; private set; }
        public bool IsCompleted { get; private set; }

        private void HttpResponseCallback(Task<HttpResponseMessage> response)
        {
            if (response.IsFaulted)
            {
                // TODO Add back in
                //CloudLogger.LogError(response.Exception);
                return;
            }

            if (response.Result == null)
            {
                // Network error
                // TODO Add back in
                //CloudLogger.LogDebug("dlm - result null");
                IsSuccess = false;
                IsCompleted = true;
                return;
            }

            if (!response.Result.IsSuccessStatusCode)
            {
                // TODO Add back in
                //CloudLogger.LogDebug($"dlm - bad status code: {response.Result.StatusCode}");
                IsSuccess = false;
                IsCompleted = true;
                return;
            }

            var contentLength = response.Result.Content.Headers.ContentLength;
            if (contentLength == null)
            {
                // TODO Add back in
                //CloudLogger.LogDebug("dlm - content length null");
                IsSuccess = false;
                IsCompleted = true;
                return;
            }

            // TODO Add back in
            //CloudLogger.LogDebug($"dlm - content length success: {contentLength}");
            ContentLength = contentLength.Value;
            cache.TryAdd(resource.ResourceId, ContentLength);

            IsSuccess = true;
            IsCompleted = true;
        }
    }
}
*/