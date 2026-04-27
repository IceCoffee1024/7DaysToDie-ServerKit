using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// AutoBackup
    /// </summary>
    public class AutoBackup : FunctionBase<AutoBackupSettings>
    {
        private readonly SubTimer _timer;
        private DateTime _lastServerStateChange = DateTime.Now;
        private volatile bool _isBackupRunning = false;

        /// <inheritdoc/>
        public AutoBackup()
        {
            _timer = new SubTimer(TryBackup);
        }

        /// <inheritdoc/>
        protected override void OnDisableFunction()
        {
            GlobalTimer.UnregisterSubTimer(_timer);
            ModEventHub.PlayerSpawnedInWorld -= OnPlayerSpawnedInWorld;
            ModEventHub.PlayerDisconnected -= OnPlayerDisconnected;
            ModEventHub.GameStartDone -= OnGameStartDone;
        }

        private void OnGameStartDone()
        {
            if (Settings.IsEnabled && Settings.AutoBackupOnServerStartup)
            {
                ExecuteBackupAsync();
            }
        }

        /// <inheritdoc/>
        protected override void OnEnableFunction()
        {
            GlobalTimer.RegisterSubTimer(_timer);
            ModEventHub.PlayerSpawnedInWorld += OnPlayerSpawnedInWorld;
            ModEventHub.PlayerDisconnected += OnPlayerDisconnected;
            ModEventHub.GameStartDone += OnGameStartDone;
        }

        /// <inheritdoc/>
        protected override void OnSettingsChanged()
        {
            _timer.Interval = Settings.Interval;
            _timer.IsEnabled = Settings.IsEnabled;
        }

        private void OnPlayerDisconnected(ManagedPlayer player)
        {
            _lastServerStateChange = DateTime.Now;
        }

        private void OnPlayerSpawnedInWorld(SpawnedPlayer spawnedPlayer)
        {
            try
            {
                switch (spawnedPlayer.RespawnType)
                {
                    case Models.RespawnType.EnterMultiplayer:
                    case Models.RespawnType.JoinMultiplayer:
                        _lastServerStateChange = DateTime.Now;
                        break;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Error in AutoBackup.PlayerSpawnedInWorld");
            }
        }

        private void TryBackup()
        {
            try
            {
                DateTime now = DateTime.Now;
                if (
                    Settings.SkipIfThereAreNoPlayers
                    && LivePlayerManager.Count == 0
                    && (now - _lastServerStateChange).TotalSeconds > Settings.Interval
                )
                {
                    CustomLogger.Info("AutoBackup: Skipped because there are no players.");
                    return;
                }

                ExecuteBackupAsync();
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Error in AutoBackup.TryBackup");
            }
        }

        private void ExecuteBackupAsync()
        {
            if (_isBackupRunning)
            {
                CustomLogger.Info("AutoBackup: The previous backup has not been completed, skip this backup.");
                return;
            }

            _isBackupRunning = true;
            Task.Run(() =>
            {
                try
                {
                    ExecuteInternal();
                }
                catch (Exception ex)
                {
                    CustomLogger.Warn(ex, "AutoBackup: Exception in background backup task");
                }
                finally
                {
                    _isBackupRunning = false;
                }
            });
        }

        private void ExecuteInternal()
        {
            string tempDir = null;
            try
            {
                // 等待游戏存档线程完成当前队列，确保文件处于一致状态
                WaitForSaveDone();

                string backupSrcPath = GameIO.GetSaveGameDir();
                string backupDestPath = Settings.ArchiveFolder;

                if (!Path.IsPathRooted(backupDestPath))
                {
                    backupDestPath = Path.Combine(AppContext.BaseDirectory, backupDestPath);
                }

                Directory.CreateDirectory(backupDestPath);

                string sourceFolderName = Path.GetFileName(backupSrcPath);
                tempDir = Path.Combine(Path.GetTempPath(), $"AutoBackup_{sourceFolderName}_{Guid.NewGuid():N}");
                Directory.CreateDirectory(tempDir);

                CopyDirectoryWithRetry(backupSrcPath, tempDir, maxRetries: 3, retryDelayMs: 500);

                string archiveFileName = GenerateArchiveFileName(backupDestPath);
                if (File.Exists(archiveFileName))
                {
                    //如果文件已存在，追加GUID生成唯一文件名
                    archiveFileName = Path.Combine(
                        backupDestPath, 
                        $"{Path.GetFileNameWithoutExtension(archiveFileName)}_{Guid.NewGuid():N}.zip"
                    );
                    CustomLogger.Info("AutoBackup: File exists, using new name: {0}", archiveFileName);
                    //如果文件已存在，直接跳过此次备份
                    //CustomLogger.Info("AutoBackup: Backup file already exists, skipping this backup: {0}", archiveFileName);
                    //return; 
                }

                ZipFile.CreateFromDirectory(
                    sourceDirectoryName: tempDir,
                    destinationArchiveFileName: archiveFileName,
                    compressionLevel: CompressionLevel.Optimal,
                    includeBaseDirectory: true
                );

                CustomLogger.Info("AutoBackup: Backup created: {0}", archiveFileName);

                ApplyRetentionPolicy(backupDestPath);
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Error in AutoBackup.ExecuteInternal");
            }
            finally
            {
                try
                {
                    if (tempDir != null && Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, recursive: true);
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.Warn(ex, "Error cleaning temp directory");
                }
            }
        }

        // 服务端版本、游戏世界、游戏名称、游戏时间
        private string GenerateArchiveFileName(string backupDestPath)
        {
            string serverVersion = global::Constants.cVersionInformation.LongString.Replace('_', ' ');
            string gameWorld = GamePrefs.GetString(EnumGamePrefs.GameWorld).Replace('_', ' ');
            string gameName = GamePrefs.GetString(EnumGamePrefs.GameName).Replace('_', ' ');

            var worldTime = GameManager.Instance.World.GetWorldTime();
            int days = GameUtils.WorldTimeToDays(worldTime);
            int hours = GameUtils.WorldTimeToHours(worldTime);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string guid = Guid.NewGuid().ToString("N").Substring(0, 4);
            string title = $"{serverVersion}_{gameWorld}_{gameName}_Day{days}_Hour{hours}_{timestamp}_{guid}";
            return Path.Combine(backupDestPath, $"{title}.zip");
        }

        private void CopyDirectoryWithRetry(
            string sourceDir,
            string targetDir,
            int maxRetries,
            int retryDelayMs
        )
        {
            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = GetRelativePath(dirPath, sourceDir);
                string newDir = Path.Combine(targetDir, relativePath);
                Directory.CreateDirectory(newDir);
            }

            foreach (string filePath in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = GetRelativePath(filePath, sourceDir);
                string destPath = Path.Combine(targetDir, relativePath);

                int retryCount = 0;
                while (retryCount < maxRetries)
                {
                    try
                    {
                        using (FileStream sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (FileStream destStream = File.Create(destPath))
                            {
                                sourceStream.CopyTo(destStream);
                            }
                        }
                        break;
                    }
                    catch (IOException ex) when (retryCount < maxRetries - 1)
                    {
                        CustomLogger.Warn($"Failed to copy {filePath}, retry {retryCount + 1}/{maxRetries}: {ex.Message}");
                        Thread.Sleep(retryDelayMs);
                        retryCount++;
                    }
                }

                if (retryCount == maxRetries)
                {
                    throw new IOException($"Failed to copy {filePath} after {maxRetries} attempts");
                }
            }
        }

        private string GetRelativePath(string fullPath, string basePath)
        {
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                basePath += Path.DirectorySeparatorChar;
            }
            return fullPath.Replace(basePath, "");
        }

        private void ApplyRetentionPolicy(string backupDestPath)
        {
            if (Settings.RetainedFileCountLimit <= 0)
                return;
            // 根据文件的创建日期对文件进行排序
            var files = Directory
                .GetFiles(backupDestPath, "*.zip")
                .Select(f => new FileInfo(f))
                .OrderBy(f => f.CreationTime)
                .ToList();

            int removeCount = files.Count - Settings.RetainedFileCountLimit;
            if (removeCount > 0)
            {
                foreach (var file in files.Take(removeCount))
                {
                    try
                    {
                        // 删除最旧的文件
                        file.Delete();
                        CustomLogger.Info("AutoBackup: Deleted old backup: {0}", file.Name);
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Warn(ex, $"Failed to delete old backup: {file.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// 等待游戏存档线程完成当前队列，确保 .7rg 文件处于一致状态后再复制
        /// </summary>
        private void WaitForSaveDone()
        {
            try
            {
                var chunkProvider = GameManager.Instance?.World?.ChunkClusters?[0]?.ChunkProvider
                    as ChunkProviderGenerateWorld;

                if (chunkProvider?.m_RegionFileManager == null)
                    return;

                CustomLogger.Info("AutoBackup: 等待存档线程完成写入...");
                chunkProvider.m_RegionFileManager.WaitSaveDone();
                CustomLogger.Info("AutoBackup: 存档线程已完成，开始复制文件。");
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "AutoBackup: 等待存档完成时出错，将直接继续备份");
            }
        }

        /// <summary>
        /// 手动备份
        /// </summary>
        public void ManualBackup()
        {
            if (_isBackupRunning)
            {
                CustomLogger.Info("AutoBackup: There is already a backup task being executed. Please try again later.");
                return;
            }

            if (Settings.ResetIntervalAfterManualBackup)
            {
                _timer.IsEnabled = false;
                _timer.IsEnabled = Settings.IsEnabled;
            }

            ExecuteBackupAsync();
        }
    }
}
