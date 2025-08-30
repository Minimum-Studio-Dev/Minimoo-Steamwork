using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Steamworks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Minimoo;

namespace Minimoo.SteamWork
{
    public static class SteamCloudSave
    {
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (!SteamManager.Instance.IsSteamInitialized)
            {
                D.Error("Steam is not initialized. Cannot initialize SteamCloudSave.");
                return;
            }

            isInitialized = true;
            D.Log("SteamCloudSave initialized successfully.");
        }

        /// <summary>
        /// Steam Cloud가 사용 가능한지 확인합니다.
        /// </summary>
        public static bool IsCloudAvailable()
        {
            if (!isInitialized) return false;

            try
            {
                return SteamRemoteStorage.IsCloudEnabledForAccount() && SteamRemoteStorage.IsCloudEnabledForApp();
            }
            catch (Exception e)
            {
                D.Error($"Failed to check cloud availability: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 파일을 Steam Cloud에 저장합니다.
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 포함)</param>
        /// <param name="data">저장할 데이터</param>
        /// <returns>저장 성공 여부</returns>
        public static bool SaveFile(string fileName, string data)
        {
            if (!isInitialized)
            {
                D.Error("SteamCloudSave is not initialized!");
                return false;
            }

            if (!IsCloudAvailable())
            {
                D.Error("Steam Cloud is not available!");
                DebugCloudStatus();
                return false;
            }

            if (string.IsNullOrEmpty(data))
            {
                D.Error("Data is null or empty!");
                return false;
            }

            try
            {
                string fullFileName = fileName;
                byte[] bytes = Encoding.UTF8.GetBytes(data);

                // 파일 크기 제한 확인 (Steam Cloud는 파일당 100MB 제한)
                if (bytes.Length > 100 * 1024 * 1024)
                {
                    D.Error($"File size too large: {bytes.Length} bytes (max: 100MB)");
                    return false;
                }

                // 저장소 용량 확인
                var (used, total) = GetCloudStorageInfo();
                if (used + bytes.Length > total)
                {
                    D.Error($"Not enough cloud storage space. Used: {used}, Total: {total}, Required: {bytes.Length}");
                    return false;
                }

                D.Log($"Attempting to save file: {fullFileName}, Size: {bytes.Length} bytes");

                bool result = SteamRemoteStorage.FileWrite(fullFileName, bytes);
                if (result)
                {
                    D.Log($"Successfully saved file to Steam Cloud: {fullFileName} ({bytes.Length} bytes)");
                }
                else
                {
                    D.Error($"Failed to save file to Steam Cloud: {fullFileName}");

                    // 추가 디버깅 정보
                    D.Error($"Cloud enabled for account: {SteamRemoteStorage.IsCloudEnabledForAccount}");
                    D.Error($"Cloud enabled for app: {SteamRemoteStorage.IsCloudEnabledForApp}");
                    D.Error($"Steam initialized: {SteamManager.Instance.IsSteamInitialized}");

                    // 마지막 오류 확인
                    uint lastError = SteamRemoteStorage.GetLastError();
                    D.Error($"Last Steam error code: {lastError}");
                }

                return result;
            }
            catch (Exception e)
            {
                D.Error($"Exception while saving file: {e.Message}");
                D.Error($"Stack trace: {e.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// 파일을 Steam Cloud에 저장합니다 (바이트 배열).
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 포함)</param>
        /// <param name="data">저장할 바이트 데이터</param>
        /// <returns>저장 성공 여부</returns>
        public static bool SaveFile(string fileName, byte[] data)
        {
            if (!isInitialized)
            {
                D.Error("SteamCloudSave is not initialized!");
                return false;
            }

            if (!IsCloudAvailable())
            {
                D.Error("Steam Cloud is not available!");
                DebugCloudStatus();
                return false;
            }

            if (data == null || data.Length == 0)
            {
                D.Error("Data is null or empty!");
                return false;
            }

            try
            {
                string fullFileName = fileName;

                // 파일 크기 제한 확인 (Steam Cloud는 파일당 100MB 제한)
                if (data.Length > 100 * 1024 * 1024)
                {
                    D.Error($"File size too large: {data.Length} bytes (max: 100MB)");
                    return false;
                }

                // 저장소 용량 확인
                var (used, total) = GetCloudStorageInfo();
                if (used + data.Length > total)
                {
                    D.Error($"Not enough cloud storage space. Used: {used}, Total: {total}, Required: {data.Length}");
                    return false;
                }

                D.Log($"Attempting to save file: {fullFileName}, Size: {data.Length} bytes");

                bool result = SteamRemoteStorage.FileWrite(fullFileName, data);
                if (result)
                {
                    D.Log($"Successfully saved file to Steam Cloud: {fullFileName} ({data.Length} bytes)");
                }
                else
                {
                    D.Error($"Failed to save file to Steam Cloud: {fullFileName}");

                    // 추가 디버깅 정보
                    DebugCloudStatus();
                }

                return result;
            }
            catch (Exception e)
            {
                D.Error($"Exception while saving file: {e.Message}");
                D.Error($"Stack trace: {e.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Steam Cloud에서 파일을 로드합니다.
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 포함)</param>
        /// <returns>로드된 데이터 (실패 시 null)</returns>
        public static string LoadFile(string fileName)
        {
            if (!isInitialized)
            {
                D.Error("SteamCloudSave is not initialized!");
                return null;
            }

            if (!IsCloudAvailable())
            {
                D.Error("Steam Cloud is not available!");
                DebugCloudStatus();
                return null;
            }

            try
            {
                string fullFileName = fileName;

                if (!SteamRemoteStorage.FileExists(fullFileName))
                {
                    D.Log($"File does not exist in Steam Cloud: {fullFileName}");
                    return null;
                }

                var fileSize = SteamRemoteStorage.GetFileSize(fullFileName);
                if (fileSize <= 0)
                {
                    D.Error($"Invalid file size: {fileSize} for file: {fullFileName}");
                    return null;
                }

                var data = new byte[fileSize];
                var bytesRead = SteamRemoteStorage.FileRead(fullFileName, data, fileSize);

                if (bytesRead == fileSize)
                {
                    string result = Encoding.UTF8.GetString(data);
                    D.Log($"Successfully loaded file from Steam Cloud: {fullFileName} ({bytesRead} bytes)");
                    return result;
                }
                else
                {
                    D.Error($"Failed to read complete file from Steam Cloud: {fullFileName} (read: {bytesRead}, expected: {fileSize})");
                    return null;
                }
            }
            catch (Exception e)
            {
                D.Error($"Exception while loading file: {e.Message}");
                D.Error($"Stack trace: {e.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Steam Cloud에서 파일을 로드합니다 (바이트 배열).
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 포함)</param>
        /// <returns>로드된 바이트 데이터 (실패 시 null)</returns>
        public static byte[] LoadFileBytes(string fileName)
        {
            if (!isInitialized || !IsCloudAvailable()) return null;

            try
            {
                string fullFileName = fileName;

                if (!SteamRemoteStorage.FileExists(fullFileName))
                {
                    D.Log($"File does not exist in Steam Cloud: {fullFileName}");
                    return null;
                }

                int fileSize = SteamRemoteStorage.GetFileSize(fullFileName);
                byte[] data = new byte[fileSize];
                int bytesRead = SteamRemoteStorage.FileRead(fullFileName, data, fileSize);

                if (bytesRead == fileSize)
                {
                    D.Log($"Successfully loaded file from Steam Cloud: {fullFileName}");
                    return data;
                }
                else
                {
                    D.Error($"Failed to read complete file from Steam Cloud: {fullFileName}");
                    return null;
                }
            }
            catch (Exception e)
            {
                D.Error($"Exception while loading file: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 파일이 Steam Cloud에 존재하는지 확인합니다.
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 포함)</param>
        /// <returns>파일 존재 여부</returns>
        public static bool FileExists(string fileName)
        {
            if (!isInitialized || !IsCloudAvailable()) return false;

            try
            {
                string fullFileName = fileName;
                return SteamRemoteStorage.FileExists(fullFileName);
            }
            catch (Exception e)
            {
                D.Error($"Exception while checking file existence: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Steam Cloud에서 파일을 삭제합니다.
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 포함)</param>
        /// <returns>삭제 성공 여부</returns>
        public static bool DeleteFile(string fileName)
        {
            if (!isInitialized || !IsCloudAvailable()) return false;

            try
            {
                string fullFileName = fileName;
                bool result = SteamRemoteStorage.FileDelete(fullFileName);

                if (result)
                {
                    D.Log($"Successfully deleted file from Steam Cloud: {fullFileName}");
                }
                else
                {
                    D.Error($"Failed to delete file from Steam Cloud: {fullFileName}");
                }

                return result;
            }
            catch (Exception e)
            {
                D.Error($"Exception while deleting file: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Steam Cloud에 저장된 모든 파일 목록을 가져옵니다.
        /// </summary>
        /// <returns>파일 이름 목록</returns>
        public static List<string> GetFileList()
        {
            List<string> files = new List<string>();

            if (!isInitialized || !IsCloudAvailable()) return files;

            try
            {
                var fileCount = SteamRemoteStorage.GetFileCount();

                for (var i = 0; i < fileCount; i++)
                {
                    var fileName = SteamRemoteStorage.GetFileNameAndSize(i, out var fileSize);
                    files.Add(fileName);
                }
            }
            catch (Exception e)
            {
                D.Error($"Exception while getting file list: {e.Message}");
            }

            return files;
        }

        /// <summary>
        /// 사용된/총 할당된 Steam Cloud 저장 공간을 가져옵니다.
        /// </summary>
        /// <returns>(사용된 바이트, 총 바이트) 튜플</returns>
        public static (ulong used, ulong total) GetCloudStorageInfo()
        {
            if (!isInitialized || !IsCloudAvailable()) return (0, 0);

            try
            {
                ulong totalBytes, availableBytes;
                SteamRemoteStorage.GetQuota(out totalBytes, out availableBytes);
                ulong usedBytes = totalBytes - availableBytes;

                return (usedBytes, totalBytes);
            }
            catch (Exception e)
            {
                D.Error($"Exception while getting cloud storage info: {e.Message}");
                return (0, 0);
            }
        }

        /// <summary>
        /// Unity의 PlayerPrefs를 Steam Cloud에 동기화합니다.
        /// </summary>
        /// <param name="key">PlayerPrefs 키</param>
        /// <returns>동기화 성공 여부</returns>
        public static bool SyncPlayerPrefsToCloud(string key)
        {
            if (!PlayerPrefs.HasKey(key)) return false;

            string value = PlayerPrefs.GetString(key);
            return SaveFile($"PlayerPrefs_{key}", value);
        }

        /// <summary>
        /// Steam Cloud에서 PlayerPrefs를 동기화합니다.
        /// </summary>
        /// <param name="key">PlayerPrefs 키</param>
        /// <returns>동기화 성공 여부</returns>
        /// <summary>
        /// Steam Cloud 상태를 디버깅합니다.
        /// </summary>
        private static void DebugCloudStatus()
        {
            try
            {
                D.Error($"=== Steam Cloud Debug Info ===");
                D.Error($"Cloud enabled for account: {SteamRemoteStorage.IsCloudEnabledForAccount}");
                D.Error($"Cloud enabled for app: {SteamRemoteStorage.IsCloudEnabledForApp}");
                D.Error($"Steam initialized: {SteamManager.Instance?.IsSteamInitialized ?? false}");

                var (used, total) = GetCloudStorageInfo();
                D.Error($"Cloud storage: {used}/{total} bytes");

                uint lastError = SteamRemoteStorage.GetLastError();
                D.Error($"Last Steam error code: {lastError}");

                D.Error($"Steam client running: {SteamClient.IsLoggedOn}");
                D.Error($"App ID: {SteamApps.AppId}");

                if (SteamManager.Instance?.IsSteamInitialized == true)
                {
                    D.Error($"User Steam ID: {SteamManager.Instance.UserSteamId}");
                    D.Error($"User logged on: {SteamClient.IsLoggedOn}");
                }

                D.Error($"================================");
            }
            catch (Exception e)
            {
                D.Error($"Exception in DebugCloudStatus: {e.Message}");
            }
        }

        public static bool SyncPlayerPrefsFromCloud(string key)
        {
            string value = LoadFile($"PlayerPrefs_{key}");
            if (value != null)
            {
                PlayerPrefs.SetString(key, value);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }
    }
}
