# GameLecture Unity Framework

Unity 프로젝트에서 반복적으로 사용하는 리소스 로딩, 오브젝트 풀링, UI, 씬, 이벤트, 데이터 및 저장 기능을 제공하는 UPM 패키지입니다. 게임마다 달라지는 구현은 `Framework Starter` Sample로 분리되어 있으므로 Core를 수정하지 않고 프로젝트에 맞게 확장할 수 있습니다.

## 요구 사항

- Unity 6000.0 이상
- Newtonsoft Json `3.2.2`
- Unity LevelPlay Mediation `9.5.0`
- Unity UI `2.0.0`

위 패키지는 `package.json`의 종속성으로 선언되어 이 패키지를 설치할 때 함께 해석됩니다.

## 설치

이 저장소는 private 저장소입니다. GitHub 인증이 가능한 환경에서 Unity Package Manager의 **Install package from git URL**을 선택하고 다음 URL을 입력합니다.

```text
https://github.com/GameLecture2024/UnityFramework.git
```

또는 프로젝트의 `Packages/manifest.json`에 추가합니다.

```json
{
  "dependencies": {
    "com.gamelecture.framework": "https://github.com/GameLecture2024/UnityFramework.git"
  }
}
```

개발 중인 로컬 복사본을 사용하려면 Package Manager에서 **Install package from disk**를 선택하고 이 저장소의 `package.json`을 지정합니다.

## Sample 가져오기

1. `Window > Package Manager`를 엽니다.
2. `GameLecture Unity Framework`를 선택합니다.
3. Samples에서 `Framework Starter`의 **Import**를 누릅니다.
4. 코드는 `Assets/Samples/GameLecture Unity Framework/1.0.0/Framework Starter`에 복사됩니다.

가져온 `Player`, `Monster`, `Npc`, 데이터 모델, 설정, 현지화 및 광고 코드는 프로젝트 요구 사항에 맞게 직접 수정할 수 있습니다. Core는 Sample을 참조하지 않으며 Sample의 asmdef만 Core를 참조합니다.

## 빠른 시작

빈 GameObject에 Sample의 `FrameworkBootstrap`을 추가합니다. 기본 `ResourcesLoader`는 아래 경로의 에셋을 인덱싱합니다.

```text
Resources/Prefabs
Resources/Config
Resources/Data/JsonData
Resources/Sounds
```

다른 로딩 시스템을 사용하려면 `IResourceLoader`를 구현하여 교체합니다.

```csharp
ResourceManager.Instance.SetLoader(new MyAddressablesLoader());
ResourceManager.Instance.LoadAll(onComplete: StartGame);
```

## 주요 API

### 리소스

```csharp
var prefab = ResourceManager.Instance.Get<GameObject>("Player");
var instance = ResourceManager.Instance.Instantiate("Player");
```

리소스 키는 기본적으로 `에셋이름_타입이름`으로 내부 저장됩니다. 호출할 때는 에셋 이름만 전달합니다.

### 오브젝트 풀

```csharp
PoolManager.Instance.Reserve("Monster", 20);
GameObject monster = PoolManager.Instance.Pop("Monster");
PoolManager.Instance.Push(monster);
```

### 타입 안전 오브젝트 생성

```csharp
Player player = ObjectManager.Instance.Spawn<Player>(
    "Player",
    playerRoot,
    pooling: true);

ObjectManager.Instance.Despawn(player);
```

`ObjectManager`는 구체적인 게임 타입을 알지 않습니다. Sample의 `GameObjectManager`처럼 프로젝트별 생성 메서드를 추가할 수 있습니다.

### 이벤트

어떤 enum도 이벤트 키로 사용할 수 있습니다.

```csharp
EventManager.Instance.Add(GameEvent.GoldChanged, RefreshGold);
EventManager.Instance.Trigger(GameEvent.GoldChanged);
EventManager.Instance.Remove(GameEvent.GoldChanged, RefreshGold);
```

### 데이터와 설정

```csharp
GameConfig config = DataManager.Instance.LoadConfig<GameConfig>("GameConfig");
ItemDataLoader loader = DataManager.Instance.LoadJson<ItemDataLoader>("ItemData");
Dictionary<int, ItemData> items = loader.MakeDictionary();
```

JSON 로더는 `IValidate` 또는 `IDataLoader<TKey, TValue>`를 구현해야 합니다.

### 저장

```csharp
var store = new JsonSaveStore<MySaveData>("save.json");
store.Save(data);
MySaveData loaded = store.Load();
store.Delete();
```

저장 파일은 `Application.persistentDataPath`에 생성됩니다.

### UI

```csharp
UIManager.Instance.ShowScene<MySceneUI>();
UIManager.Instance.ShowPopup<MyPopupUI>();
UIManager.Instance.ClosePopup();
```

UI 프리팹 이름과 클래스 이름이 다르면 리소스 키를 인자로 전달합니다.

### 씬과 사운드

```csharp
SceneManager.Instance.LoadScene("GameScene");
SoundManager.Instance.Play2D("BGM", "MainTheme", loop: true);
SoundManager.Instance.Play2D("Effect", "ButtonClick");
```

### Unity LevelPlay 광고

1. Sample의 `AdsConfig` 에셋을 만듭니다.
2. Android/iOS App Key와 Interstitial/Rewarded Ad Unit ID를 입력합니다.
3. `FrameworkBootstrap`에 해당 설정을 연결하거나 직접 초기화합니다.

```csharp
LevelPlayAdsManager.Instance.Initialize(adsConfig);
LevelPlayAdsManager.Instance.ShowInterstitial();
LevelPlayAdsManager.Instance.ShowRewarded(() => GiveReward());
```

광고 보상은 LevelPlay의 rewarded 이벤트가 수신된 경우에만 실행됩니다. 실제 출시 전에는 반드시 LevelPlay 대시보드 설정, 테스트 모드, 플랫폼별 개인정보 동의 흐름을 별도로 검증하십시오.

## 확장 원칙

- 재사용 가능한 알고리즘과 인터페이스는 `Runtime`에 둡니다.
- 게임별 enum, 데이터 모델, 설정 및 구체 클래스는 Sample을 가져온 뒤 `Assets`에서 관리합니다.
- Core에서 Sample 타입을 참조하지 않습니다.
- 공통 기능을 수정해야 한다면 Registry/Git 설치본을 프로젝트의 `Packages/com.gamelecture.framework`로 복사해 Embedded 패키지로 전환합니다.

## 패키지 구성

```text
Runtime/       수정하지 않고 재사용하는 Core
Samples~/      프로젝트로 복사하여 수정하는 구현 예제
package.json   버전, Sample, Unity Registry 종속성
```
