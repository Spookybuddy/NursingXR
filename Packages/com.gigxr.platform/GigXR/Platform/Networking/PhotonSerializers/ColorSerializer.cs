namespace GIGXR.Platform.Networking
{
    using System;
    using System.IO;
    using UnityEngine;

    public class ColorSerializer
    {
        public static object Deserialize(byte[] data)
        {
            Color color = new Color();

            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    color.r = reader.ReadSingle();
                    color.g = reader.ReadSingle();
                    color.b = reader.ReadSingle();
                    color.a = reader.ReadSingle();
                }
            }

            return color;
        }

        public static byte[] Serialize(object customType)
        {
            var color = (Color)customType;

            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(color.r);
                    writer.Write(color.g);
                    writer.Write(color.b);
                    writer.Write(color.a);
                }

                return m.ToArray();
            }
        }
    }
}