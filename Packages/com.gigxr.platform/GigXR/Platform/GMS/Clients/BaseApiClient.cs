namespace GIGXR.GMS.Clients
{
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using Platform.GMS.Exceptions;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using UnityEngine.Networking;

    public abstract class BaseApiClient
    {
        protected async UniTask ProcessErrorsAsync(HttpResponseMessage response)
        {
            if ((int)response.StatusCode >= 200 &&
                (int)response.StatusCode < 400)
            {
                return;
            }

            // Some API endpoints return a ProblemDetails object with useful information for debugging.
            var problemDetails = await response.Content.ReadAsStringAsync();

            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new GmsApiUnauthorizedException(problemDetails),
                _ => new GmsApiException($"{response.StatusCode}: {problemDetails}"),
            };
        }

        protected void ProcessErrorsAsync(UnityWebRequest response)
        {
            var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(response.downloadHandler?.text);
            string detailMessage;

            if (problemDetails != null)
            {
                detailMessage = problemDetails.ToString();
            }
            else
            {
                detailMessage = response.error;
            }

            if (response.responseCode >= 400 && response.responseCode < 600)
            {
                if(response.responseCode == (long)HttpStatusCode.Unauthorized ||
                   response.responseCode == (long)HttpStatusCode.Forbidden)
                {
                    UnityEngine.Debug.LogError(new GmsApiUnauthorizedException(detailMessage));
                }
                else if(response.responseCode >= 500 && response.responseCode < 600)
                {
                    UnityEngine.Debug.LogError(new GmsServerException(detailMessage));
                }
                else
                {
                    UnityEngine.Debug.LogError($"{response.method}:{response.url}\n{detailMessage}"); 
                }
            }
        }
    }

    public class ProblemDetails
    {
        public string? type;
        public string? title;
        public string? traceId;
        public int? status;
        public string? detail;
        public string? instance;
        public Dictionary<string, string[]>? errors;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var property in GetType().GetFields())
            {
                sb.Append(property.Name);
                sb.Append(" : ");
                if (property.Name == nameof(errors) && errors != null)
                {
                    sb.AppendLine();

                    // Iterate the dictionary
                    foreach (var currentError in errors)
                    {
                        sb.Append(currentError.Key);
                        sb.Append(" ");

                        foreach(var errorString in currentError.Value)
                        {
                            sb.Append(errorString);
                            sb.AppendLine();
                        }
                    }
                }
                else
                {
                    sb.Append(property.GetValue(this));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
