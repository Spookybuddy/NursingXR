namespace GIGXR.Platform.Networking
{
    using GIGXR.Platform.Scenarios;
    using GIGXR.Platform.Scenarios.Data;
    using System.IO;

    public class ScenarioSyncDataSerializer
    {
        public static object Deserialize(byte[] data)
        {
            ScenarioSyncData syncData = new ScenarioSyncData();

            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    syncData.TimeStamp = reader.ReadInt32();
                    syncData.ScenarioStatus = (ScenarioStatus)reader.ReadInt32();
                    syncData.TotalMillisecondsInSimulation = reader.ReadInt32();
                    syncData.TotalMillisecondsInScenario = reader.ReadInt32();
                    syncData.TotalMillisecondsInCurrentStage = reader.ReadInt32();
                }
            }

            return syncData;
        }

        public static byte[] Serialize(object customType)
        {
            var syncData = (ScenarioSyncData)customType;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memoryStream))
                {
                    writer.Write(syncData.TimeStamp);
                    writer.Write((int)syncData.ScenarioStatus);
                    writer.Write(syncData.TotalMillisecondsInSimulation);
                    writer.Write(syncData.TotalMillisecondsInScenario);
                    writer.Write(syncData.TotalMillisecondsInCurrentStage);
                }

                return memoryStream.ToArray();
            }
        }
    }
}