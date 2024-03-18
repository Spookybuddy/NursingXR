using System;
using System.Text;
using UnityEngine;

namespace GIGXR.Platform.Mobile.GMS
{
    public static class JsonWebTokenUtility
    {
        /// <summary>
        /// Get the JSON payload from a JSON Web Token (JWT).
        /// </summary>
        /// <param name="jsonWebToken">The JWT to parse.</param>
        /// <returns>The JSON payload.</returns>
        /// <exception cref="InvalidOperationException">If the JWT is invalid.</exception>
        public static string GetJsonWebTokenPayloadJson(string jsonWebToken)
        {
            var parts = jsonWebToken.Split('.');
            if (parts.Length != 3)
            {
                throw new InvalidOperationException("Invalid json web token!");
            }

            var jwtPayload = parts[1];
            var padLength = 4 - jwtPayload.Length % 4;
            if (padLength < 4)
            {
                jwtPayload += new string('=', padLength);
            }

            var bytes = Convert.FromBase64String(jwtPayload);
            var payloadJson = Encoding.UTF8.GetString(bytes);

            return payloadJson;
        }

        /// <summary>
        /// Deserializes the JSON payload from a JSON Web Token (JWT) into the provided type.
        /// </summary>
        /// <param name="jsonWebToken">The JWT to parse.</param>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <returns>The requested type with values deserialized from the JWT payload.</returns>
        public static T FromJsonWebToken<T>(string jsonWebToken)
        {
            var json = GetJsonWebTokenPayloadJson(jsonWebToken);

            return JsonUtility.FromJson<T>(json);
        }
    }
}