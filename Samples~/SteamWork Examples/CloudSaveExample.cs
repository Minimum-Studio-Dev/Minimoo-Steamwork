using UnityEngine;
using Minimoo.SteamWork;

public class CloudSaveExample : MonoBehaviour
{
    private string saveData = "Hello Steam Cloud!";
    private string loadedData = "";

    private void Start()
    {
        // Cloud 저장소가 사용 가능한지 확인
        if (SteamCloudSave.IsCloudAvailable())
        {
            Debug.Log("Steam Cloud를 사용할 수 있습니다!");

            // 저장 공간 정보 확인
            var (used, total) = SteamCloudSave.GetCloudStorageInfo();
            Debug.Log($"Cloud 저장소: {used}/{total} bytes 사용중");
        }
        else
        {
            Debug.LogError("Steam Cloud를 사용할 수 없습니다.");
        }
    }

    private void Update()
    {
        // S 키로 데이터 저장
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveGameData();
        }

        // L 키로 데이터 로드
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadGameData();
        }

        // F 키로 파일 목록 보기
        if (Input.GetKeyDown(KeyCode.F))
        {
            ShowFileList();
        }
    }

    private void SaveGameData()
    {
        // 게임 데이터를 Steam Cloud에 저장
        saveData = $"Game Data - Score: {Random.Range(0, 1000)} - Time: {Time.time}";

        if (SteamCloudSave.SaveFile("game_save", saveData))
        {
            Debug.Log($"게임 데이터 저장 성공: {saveData}");
        }
        else
        {
            Debug.LogError("게임 데이터 저장 실패!");
        }
    }

    private void LoadGameData()
    {
        // Steam Cloud에서 게임 데이터 로드
        loadedData = SteamCloudSave.LoadFile("game_save");

        if (!string.IsNullOrEmpty(loadedData))
        {
            Debug.Log($"게임 데이터 로드 성공: {loadedData}");
        }
        else
        {
            Debug.LogError("게임 데이터 로드 실패 또는 파일이 존재하지 않습니다.");
        }
    }

    private void ShowFileList()
    {
        // Cloud에 저장된 파일 목록 표시
        var files = SteamCloudSave.GetFileList();
        Debug.Log($"Cloud 파일 목록 ({files.Count}개):");

        foreach (var file in files)
        {
            Debug.Log($"- {file}");
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 20), $"저장할 데이터: {saveData}");
        GUI.Label(new Rect(10, 30, 400, 20), $"로드된 데이터: {loadedData}");

        GUI.Label(new Rect(10, 60, 400, 20), "키 설명:");
        GUI.Label(new Rect(10, 80, 400, 20), "S: 데이터 저장");
        GUI.Label(new Rect(10, 100, 400, 20), "L: 데이터 로드");
        GUI.Label(new Rect(10, 120, 400, 20), "F: 파일 목록 보기");
    }
}
