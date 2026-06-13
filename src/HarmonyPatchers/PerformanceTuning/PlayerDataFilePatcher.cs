//using HarmonyLib;

//namespace SdtdServerKit.HarmonyPatchers.PerformanceTuning
//{
//    [HarmonyPatch(typeof(PlayerDataFile))]
//    internal class PlayerDataFilePatcher
//    {
//        // private static readonly object _syncLock = new object();

//        [HarmonyPrefix]
//        [HarmonyPatch(nameof(PlayerDataFile.Save))]
//        public static bool SavePlayerData(PlayerDataFile __instance, string _dir, string _playerName)
//        {
//            Task.Run(() =>
//            {
//                try
//                {
//                    if (Directory.Exists(_dir) == false)
//                    {
//                        Directory.CreateDirectory(_dir);
//                    }

//                    lock (_playerName)
//                    {
//                        string path = Path.Combine(_dir, _playerName + "." + PlayerDataFile.EXT);

//                        if (File.Exists(path))
//                        {
//                            File.Copy(path, path + ".bak", overwrite: true);
//                        }

//                        string tempPath = path + ".tmp";
//                        if (File.Exists(tempPath))
//                        {
//                            File.Delete(tempPath);
//                        }

//                        using (Stream baseStream = File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read))
//                        {
//                            using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
//                            pooledBinaryWriter.SetBaseStream(baseStream);
//                            pooledBinaryWriter.Write('t');
//                            pooledBinaryWriter.Write('t');
//                            pooledBinaryWriter.Write('p');
//                            pooledBinaryWriter.Write((byte)0);
//                            pooledBinaryWriter.Write((byte)53);
//                            __instance.Write(pooledBinaryWriter);
//                            __instance.bModifiedSinceLastSave = false;
//                        }

//                        if (File.Exists(tempPath))
//                        {
//                            File.Copy(tempPath, path, overwrite: true);
//                            File.Delete(tempPath);
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    CustomLogger.Error("Save PlayerData file: " + ex.Message + "\n" + ex.StackTrace);
//                }
//            });

//            return false; // skips the original function
//        }
//    }
//}
