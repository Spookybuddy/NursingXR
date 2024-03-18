namespace GIGXR.Platform.Networking
{
    using System;

    public class GuidSerializer
    {
        public static object Deserialize(byte[] data)
        {
            return new Guid(data);
        }

        public static byte[] Serialize(object customType)
        {
            return ((Guid)customType).ToByteArray();
        }
    }
}