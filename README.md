# Minimoo SteamWork

Unity Package Manager용 Steamworks.NET wrapper 패키지입니다. Steam 로그인, Cloud Save, Achievement, Activity, App Entitlements 기능을 쉽고 간편하게 사용할 수 있습니다.

## 기능

- ✅ **Steam Login**: 사용자 인증 및 프로필 정보 관리
- ✅ **Steam Cloud Save**: 게임 데이터 클라우드 저장
- ✅ **Steam Achievement**: 업적 시스템 관리
- ✅ **Steam Activity**: Rich Presence 및 친구 상태 표시
- ✅ **Steam App Entitlements**: 앱 구매 권한 및 DLC 관리

## 설치 방법

### 1. Unity Package Manager를 통한 설치

1. Unity 프로젝트에서 **Window > Package Manager**를 엽니다.
2. **+** 버튼을 클릭하고 **Add package from git URL**을 선택합니다.
3. 다음 URL을 입력합니다:
   ```
   https://github.com/Minimum-Studio-Dev/Minimoo-SteamWork.git
   ```
4. **Add** 버튼을 클릭하여 패키지를 설치합니다.

### 2. 수동 설치

1. 이 리포지토리를 다운로드하거나 클론합니다.
2. `Packages/manifest.json` 파일에 다음을 추가합니다:
   ```json
   {
     "dependencies": {
       "com.minimumstudio.minimoo-steamwork": "file:../Minimoo-SteamWork"
     }
   }
   ```

## 설정

### 1. Steam App ID 설정

게임을 Steam에 출시하기 전에 다음 단계를 따라야 합니다:

1. [Steamworks 웹사이트](https://partner.steamgames.com/)에서 게임을 등록합니다.
2. 발급받은 App ID를 프로젝트 루트에 `steam_appid.txt` 파일로 저장합니다.
   ```
   1234567890
   ```

### 2. Steamworks.NET 라이브러리 설정

패키지가 자동으로 Steamworks.NET 라이브러리를 포함하므로 별도의 설정이 필요하지 않습니다.

## 기본 사용법

### 초기화

SteamManager는 씬에 자동으로 추가되며, Awake()에서 Steam을 초기화합니다:

```csharp
using Minimoo.SteamWork;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        if (SteamManager.Instance.IsSteamInitialized)
        {
            Debug.Log("Steam 초기화 성공!");
        }
    }
}
```

### Steam 로그인

```csharp
using Minimoo.SteamWork;

public class LoginExample : MonoBehaviour
{
    private void Start()
    {
        if (SteamLogin.IsLoggedIn())
        {
            string userName = SteamLogin.GetUserName();
            Debug.Log($"환영합니다, {userName}!");

            // 오버레이 열기
            SteamLogin.OpenSteamOverlay();
        }
    }
}
```

### Cloud Save

```csharp
using Minimoo.SteamWork;

public class SaveExample : MonoBehaviour
{
    private void SaveGame()
    {
        string gameData = JsonUtility.ToJson(gameState);
        SteamCloudSave.SaveFile("save_game", gameData);
    }

    private void LoadGame()
    {
        string gameData = SteamCloudSave.LoadFile("save_game");
        if (!string.IsNullOrEmpty(gameData))
        {
            gameState = JsonUtility.FromJson<GameState>(gameData);
        }
    }
}
```

### Achievement

```csharp
using Minimoo.SteamWork;

public class AchievementExample : MonoBehaviour
{
    private void UnlockFirstWin()
    {
        SteamAchievements.UnlockAchievement("ACHIEVEMENT_FIRST_WIN");
    }

    private void CheckAchievement()
    {
        bool isUnlocked = SteamAchievements.IsAchievementUnlocked("ACHIEVEMENT_FIRST_WIN");
        Debug.Log($"업적 잠금 해제됨: {isUnlocked}");
    }
}
```

### Activity (Rich Presence)

```csharp
using Minimoo.SteamWork;

public class ActivityExample : MonoBehaviour
{
    private void Start()
    {
        SteamActivity.SetMenuActivity();
    }

    private void OnLevelStart(int level)
    {
        SteamActivity.SetPlayingActivity(level);
    }

    private void OnEnterLobby(int current, int max)
    {
        SteamActivity.SetLobbyActivity(current, max);
    }
}
```

### App Entitlements (구매 권한 및 DLC)

```csharp
using Minimoo.SteamWork;

public class EntitlementsExample : MonoBehaviour
{
    private void CheckAppOwnership()
    {
        // 현재 앱의 구독 상태 확인
        bool isSubscribed = SteamAppEntitlements.IsCurrentAppSubscribed();
        Debug.Log($"앱 구독 상태: {isSubscribed}");

        // 특정 DLC 소유권 확인
        uint dlcAppId = 1234567890; // DLC App ID
        bool ownsDlc = SteamAppEntitlements.IsAppOwned(dlcAppId);
        Debug.Log($"DLC 소유: {ownsDlc}");
    }

    private void CheckDlcAndInstall()
    {
        uint dlcAppId = 1234567890;

        // DLC 설치 상태 확인
        if (SteamAppEntitlements.IsDlcInstalled(dlcAppId))
        {
            Debug.Log("DLC가 이미 설치되어 있습니다.");
        }
        else
        {
            // DLC 설치 요청
            SteamAppEntitlements.InstallDlc(dlcAppId);
            Debug.Log("DLC 설치 요청을 보냈습니다.");
        }
    }

    private void CheckAppSubscription()
    {
        uint appId = 1234567890; // 다른 앱 ID

        // 특정 앱 구독 상태 확인
        bool isSubscribed = SteamAppEntitlements.IsSubscribedApp(appId);
        Debug.Log($"앱 {appId} 구독 상태: {isSubscribed}");
    }
}
```

## API 레퍼런스

### SteamManager
- `IsSteamInitialized`: Steam 초기화 상태
- `UserSteamId`: 현재 사용자 Steam ID
- `UserPersonaName`: 현재 사용자 이름

### SteamLogin
- `IsLoggedIn()`: 로그인 상태 확인
- `GetUserName()`: 사용자 이름 가져오기
- `GetUserSteamId()`: 사용자 Steam ID 가져오기
- `OpenSteamOverlay()`: Steam 오버레이 열기
- `OpenFriendsOverlay()`: 친구 목록 오버레이 열기

### SteamCloudSave
- `IsCloudAvailable()`: Cloud 사용 가능 여부
- `SaveFile(fileName, data)`: 파일 저장
- `LoadFile(fileName)`: 파일 로드
- `FileExists(fileName)`: 파일 존재 확인
- `GetFileList()`: 저장된 파일 목록

### SteamAchievements
- `UnlockAchievement(id)`: 업적 잠금 해제
- `IsAchievementUnlocked(id)`: 업적 잠금 상태 확인
- `GetAchievementDisplayName(id)`: 업적 표시 이름
- `GetAllAchievements()`: 모든 업적 목록

### SteamActivity
- `SetStatus(status)`: 상태 설정
- `SetMenuActivity()`: 메뉴 상태
- `SetPlayingActivity(level)`: 플레이 상태
- `SetLobbyActivity(current, max)`: 로비 상태
- `GetFriendList()`: 친구 목록

### SteamAppEntitlements
- `IsSubscribedApp(appId)`: 특정 앱 구독 상태 확인
- `IsDlcInstalled(appId)`: DLC 설치 상태 확인
- `InstallDlc(appId)`: DLC 설치 요청
- `IsCurrentAppSubscribed()`: 현재 앱 구독 상태 확인
- `IsAppOwned(appId)`: 앱 소유권 확인
- `IsAppFree()`: 앱 무료 여부 확인
- `ClearEntitlementCache()`: 캐시 클리어

## 샘플 코드

`Samples~/SteamWork Examples/` 폴더에 각 기능별 완전한 예제 코드가 포함되어 있습니다:

- `SteamManagerExample.cs`: 기본 초기화 및 오버레이 제어
- `CloudSaveExample.cs`: 클라우드 저장/로딩
- `AchievementExample.cs`: 업적 관리
- `ActivityExample.cs`: Rich Presence 설정
- `SteamAppEntitlementsExample.cs`: 앱 구매 권한 및 DLC 관리

## 요구사항

- Unity 2019.4 이상
- Steamworks SDK
- .NET 4.x

## 라이선스

이 패키지는 MIT 라이선스를 따릅니다.

## 지원

문의사항이나 버그 리포트는 [GitHub Issues](https://github.com/Minimum-Studio-Dev/Minimoo-SteamWork/issues)를 이용해주세요.
