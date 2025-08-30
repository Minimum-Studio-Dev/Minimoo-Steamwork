using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Steamworks;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Minimoo.SteamWork
{
    public static class SteamCloudSave
    {
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (!SteamManager.Instance.IsSteamInitialized)
            {
                Debug.LogError("Steam is not initialized. Cannot initialize SteamCloudSave.");
                return;
            }

            isInitialized = true;
            Debug.Log("SteamCloudSave initialized successfully.");
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
                Debug.LogError($"Failed to check cloud availability: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 파일을 Steam Cloud에 저장합니다.
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 제외)</param>
        /// <param name="data">저장할 데이터</param>
        /// <returns>저장 성공 여부</returns>
        public static bool SaveFile(string fileName, string data)
        {
            if (!isInitialized || !IsCloudAvailable()) return false;

            try
            {
                string fullFileName = fileName;
                byte[] bytes = Encoding.UTF8.GetBytes(data);

                bool result = SteamRemoteStorage.FileWrite(fullFileName, bytes, bytes.Length);
                if (result)
                {
                    Debug.Log($"Successfully saved file to Steam Cloud: {fullFileName}");
                }
                else
                {
                    Debug.LogError($"Failed to save file to Steam Cloud: {fullFileName}");
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while saving file: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 파일을 Steam Cloud에 저장합니다 (바이트 배열).
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 제외)</param>
        /// <param name="data">저장할 바이트 데이터</param>
        /// <returns>저장 성공 여부</returns>
        public static bool SaveFile(string fileName, byte[] data)
        {
            if (!isInitialized || !IsCloudAvailable()) return false;

            try
            {
                string fullFileName = fileName;
                bool result = SteamRemoteStorage.FileWrite(fullFileName, data, data.Length);

                if (result)
                {
                    Debug.Log($"Successfully saved file to Steam Cloud: {fullFileName}");
                }
                else
                {
                    Debug.LogError($"Failed to save file to Steam Cloud: {fullFileName}");
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while saving file: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Steam Cloud에서 파일을 로드합니다.
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 제외)</param>
        /// <returns>로드된 데이터 (실패 시 null)</returns>
        public static string LoadFile(string fileName)
        {
            if (!isInitialized || !IsCloudAvailable()) return null;

            try
            {
                string fullFileName = fileName;

                if (!SteamRemoteStorage.FileExists(fullFileName))
                {
                    Debug.Log($"File does not exist in Steam Cloud: {fullFileName}");
                    return null;
                }

                var fileSize = SteamRemoteStorage.GetFileSize(fullFileName);
                var data = new byte[fileSize];
                var bytesRead = SteamRemoteStorage.FileRead(fullFileName, data, fileSize);

                if (bytesRead == fileSize)
                {
                    string result = Encoding.UTF8.GetString(data);
                    Debug.Log($"Successfully loaded file from Steam Cloud: {fullFileName}");
                    return result;
                }
                else
                {
                    Debug.LogError($"Failed to read complete file from Steam Cloud: {fullFileName}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while loading file: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Steam Cloud에서 파일을 로드합니다 (바이트 배열).
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 제외)</param>
        /// <returns>로드된 바이트 데이터 (실패 시 null)</returns>
        public static byte[] LoadFileBytes(string fileName)
        {
            if (!isInitialized || !IsCloudAvailable()) return null;

            try
            {
                string fullFileName = fileName;

                if (!SteamRemoteStorage.FileExists(fullFileName))
                {
                    Debug.Log($"File does not exist in Steam Cloud: {fullFileName}");
                    return null;
                }

                int fileSize = SteamRemoteStorage.GetFileSize(fullFileName);
                byte[] data = new byte[fileSize];
                int bytesRead = SteamRemoteStorage.FileRead(fullFileName, data, fileSize);

                if (bytesRead == fileSize)
                {
                    Debug.Log($"Successfully loaded file from Steam Cloud: {fullFileName}");
                    return data;
                }
                else
                {
                    Debug.LogError($"Failed to read complete file from Steam Cloud: {fullFileName}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while loading file: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 파일이 Steam Cloud에 존재하는지 확인합니다.
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 제외)</param>
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
                Debug.LogError($"Exception while checking file existence: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Steam Cloud에서 파일을 삭제합니다.
        /// </summary>
        /// <param name="fileName">파일 이름 (확장자 제외)</param>
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
                    Debug.Log($"Successfully deleted file from Steam Cloud: {fullFileName}");
                }
                else
                {
                    Debug.LogError($"Failed to delete file from Steam Cloud: {fullFileName}");
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while deleting file: {e.Message}");
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
                Debug.LogError($"Exception while getting file list: {e.Message}");
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
                Debug.LogError($"Exception while getting cloud storage info: {e.Message}");
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
