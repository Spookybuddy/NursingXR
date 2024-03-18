using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// Utility methods that can help during the serialization process
    /// </summary>
    public class SerializationUtilities
    {
        public static byte[] ObjectToByteArray<TSerializable>(TSerializable obj)
        {
            try
            {
                var json = JsonConvert.SerializeObject(obj);
                return Encoding.UTF8.GetBytes(json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Could not serialize {obj}:{obj.GetType()} to a byte array. Returning null.");
                Debug.LogException(exception);
                return null;
            }
        }

        public static TSerializable ByteArrayToObject<TSerializable>(byte[] arrBytes)
        {
            try
            {
                var json = Encoding.UTF8.GetString(arrBytes);
                return JsonConvert.DeserializeObject<TSerializable>(json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Could not deserialize byte array to object. Returning default.");
                Debug.LogException(exception);
                return default;
            }
        }
    }
}