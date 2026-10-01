# Unity Skill System Architecture Manual

본 문서는 유니티(Unity) 프로젝트 내에서 확장 가능하고 유지보수가 용이한 **4단계 스킬 시스템**을 구축하기 위한 가이드라인 및 통합 설계 프롬프트입니다.
LLM(AI) 또는 개발자가 개별 스크립트(7개 핵심 기능)를 분할 구현할 때, **데이터와 로직이 분리되고, 이벤트 기반으로 디커플링(Decoupling)된 공통 설계 구조**를 강제하기 위해 작성되었습니다.

개별 코드를 작성하기 전에 반드시 이 문서의 **[설계 원칙]**과 **[폴더 및 파일 구조]**를 숙지하고 작업에 임해야 합니다.

---

## 📌 4단계 공정 프로세스

1. **1단계 (입력):** 사용자 입력(Input)을 감지하고, 입력된 스킬 슬롯/식별자를 매니저로 전달합니다. (입력 방식과 비즈니스 로직의 분리)
2. **2단계 (데이터 매니저):** 전체 스킬 데이터를 보유 및 캐싱하고, 현재 자원(MP) 및 쿨타임을 검증하여 시전 가능 여부를 판단합니다.
3. **3단계 (구조 설계):** 스킬의 데이터와 핵심 실행 로직을 규격화합니다. `ScriptableObject`, 인터페이스, 구조체, 열거형을 결합한 유연한 데이터 구조를 제공합니다.
4. **4단계 (Callback 및 연출):** 스킬 시전 성공 시 이벤트를 발행하여 사운드, 시각 효과(VFX), UI가 각각 독립적으로 반응하도록 처리합니다.

---

## 📂 유니티 폴더 및 스크립트 구조

```text
Assets/SkillSystem/
├── Prefabs/                        # 투사체, VFX, UI 프리팹
├── ScriptableObjects/
│   └── Skills/                     # 생성된 스킬 데이터 에셋 (.asset)
└── Scripts/
    ├── Input/
    │   └── SkillInputController.cs # [1] 입력 감지 및 매니저 호출
    ├── Managers/
    │   ├── SkillDataManager.cs     # [2] 전체 스킬 데이터베이스 관리
    │   └── SkillCastManager.cs     # [3] 시전 조건 검증 (쿨타임, 비용) 및 시전 실행
    ├── Data/
    │   └── SkillDataTypes.cs       # [4] 열거형(Enum) 및 구조체(Struct) 정의
    ├── Core/
    │   └── SkillBase.cs            # [5] 스킬 기본 추상 클래스 (ScriptableObject)
    ├── Interfaces/
    │   └── ISkillExecute.cs        # [6] 스킬 실행 인터페이스
    └── Callbacks/
        └── SkillCallbackBridge.cs  # [7] 시전 완료 이벤트 및 연출(사운드/UI/VFX) 라우팅
```

---

## 유니티 스크립트 루트 네임스페이스 

모든 SkillSystem 코드들의 namespace 이름은 namespace KDH_SkillSystem 을 사용한다.

## 🛠️ [중요] 개별 구현할 7개 핵심 스크립트 기능 정의

모든 파일은 상호 참조 시 디커플링 원칙을 지켜야 하며, 아래 명시된 역할과 책임을 정확히 준수해야 합니다.

### 1. `SkillInputController.cs` (Input)
* **역할:** 플레이어의 입력(유니티 Input System 또는 legacy Input)을 수신합니다.
* **핵심 기능:** 특정 키(예: Q, W, E, R) 입력을 감지하면, 해당 슬롯 인덱스 정보를 가지고 `SkillCastManager`에게 시전 요청(`TryCastSkill(int slotIndex)`)을 보냅니다.
* **제한 사항:** 입력부에서는 쿨타임이나 MP 자원을 절대 직접 계산하지 않습니다.

### 2. `SkillDataManager.cs` (Managers)
* **역할:** 게임 내 존재하는 모든 스킬 데이터(`SkillBase`)의 레파지토리(Repository) 역할을 수행합니다.
* **핵심 기능:** 리소스 폴더나 Addressable 등에서 스킬 에셋 리스트를 로드하여 ID 기반 Dictionary로 관리하고, 외부에서 요청 시 스킬 데이터를 제공합니다.

### 3. `SkillCastManager.cs` (Managers)
* **역할:** 스킬 시전의 중추적인 제어 흐름(Flow Control)을 담당합니다.
* **핵심 기능:** 
  * `SkillDataManager`로부터 데이터를 조회합니다.
  * 해당 스킬의 현재 쿨타임 유무 및 시전자(Owner)의 자원 상태를 검증합니다.
  * 검증 성공 시 쿨타임을 돌리고, 자원을 소모한 후 스킬의 실행 로직을 트리거하며, `SkillCallbackBridge`를 통해 시전 성공 이벤트를 전파합니다.

### 4. `SkillDataTypes.cs` (Data)
* **역할:** 스킬 시스템 전반에서 공통으로 사용할 원시 데이터 타입을 모아둡니다.
* **포함 요소:**
  * **열거형(Enum):** `SkillType` (액티브/패시브), `TargetType` (타겟팅/논타겟팅/범위), `Element` (화염/냉기/전기 등)
  * **구조체(Struct):** 스킬의 기본 스탯 정보를 묶은 `SkillCostInfo` (소모 자원량, 재사용 대기시간), `SkillImpactInfo` (데미지 계수, 사거리) 등

### 5. `SkillBase.cs` (Core)
* **역할:** 모든 스킬 에셋의 원형이 되는 추상 클래스(`abstract class`)입니다. 유니티 인스펙터 관리를 위해 `ScriptableObject`를 상속받습니다.
* **핵심 기능:** 스킬 고유 ID, 이름, 아이콘, `SkillDataTypes`에서 정의한 구조체들을 멤버 변수로 가집니다. 추상 메서드로 `public abstract void Initialize()` 및 기본 실행 형태를 정의할 수 있습니다.

### 6. `ISkillExecute.cs` (Interfaces)
* **역할:** 스킬의 "실행" 행동을 다형성 있게 처리하기 위한 인터페이스입니다.
* **핵심 기능:** `void Execute(GameObject caster, Vector3 targetPosition);` 메서드를 선언합니다. `SkillBase` 또는 이를 상속받은 세부 스킬 클래스가 이 인터페이스를 구현하여 각기 다른 스킬 로직(예: 투사체 발사, 즉시 범위 딜링)을 실행하도록 만듭니다.

### 7. `SkillCallbackBridge.cs` (Callbacks)
* **역할:** 스킬 시전 성공 이후 발생하는 시각/청각/UI적 연출 요소(Side-Effects)를 메인 로직과 완전히 분리하는 이벤트 버스(Event Bus)입니다.
* **핵심 기능:** 
  * 스킬 시전 성공 시 트리거되는 C# `Action<SkillBase, Vector3>` 또는 `UnityEvent` 기반의 이벤트를 보유합니다.
  * 사운드 매니저, VFX 매니저, UI 매니저는 이 브릿지의 이벤트를 구독(Subscribe)하여 스킬이 시전되었을 때 각자의 프리팹을 생성하거나 사운드를 출력하고 UI(쿨타임 슬롯 등)를 갱신합니다.

---

## 🤖 AI 프롬프트 명령어 (코드 생성 시 탑재용)

이후 단계에서 코드를 생성하거나 수정하도록 요청할 때는 다음 지시사항을 기본 컨텍스트로 적용하십시오.

```text
[명령어 및 제약 조건]
1. 당신은 위 'Unity Skill System Architecture Manual'을 완벽히 이해한 시니어 유니티 개발자입니다.
2. 앞으로 구현할 7개의 스크립트는 이 문서에 명시된 클래스 이름, 구조, 책임을 100% 준수해야 합니다.
3. 스킬 데이터(Data)와 시전 메커니즘(Manager), 그리고 연출(Callback)은 상호 의존성을 최소화하는 '디커플링' 구조를 유지하세요.
4. 주석은 가독성을 위해 한글로 작성하고, 객체 지향 원칙(SOLID)을 준수하여 작성하세요.
5. 단 하나의 스크립트를 구현하더라도, 다른 6개 스크립트와의 연결 인터페이스가 매끄럽게 호환되도록 설계 규격을 맞추십시오.
```