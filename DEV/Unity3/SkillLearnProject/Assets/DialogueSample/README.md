# Dialogue Sample

Unity 스크립트 컴파일 후 Editor API가 최초 1회 Scene, SO, 샘플 아이콘을 생성합니다.
자동 생성되지 않았다면 `GameLecture > Dialogue > Create Sample Scene`을 실행하세요.
기존 샘플을 덮어쓰지 않으며 열려 있는 Scene을 저장하지 않습니다. Scene YAML/meta를 직접 작성하지 않습니다.

1. `Assets/DialogueSample/Scenes/DialogueSampleScene.unity`를 엽니다. 기존 Scene은 아래 UGUI 이전 메뉴를 실행한 뒤 Play합니다.
2. JSON / ScriptableObject 버튼으로 소스 전환. Previous / Next / Restart로 대사 확인.
3. Play 종료 후 데이터를 수정하고 다시 실행.

## JSON 작성

`Resources/Data/JsonData/DialogueSampleData.json`의 lines에 speaker, spriteKey, text를 입력합니다.
spriteKey는 확장자/경로가 아닌 Sprite 이름입니다. 비우면 초상화가 생략됩니다.
새 이미지는 Resources/Config 아래에 넣고 Unity Inspector에서 Sprite로 임포트하세요.
기본 로더는 같은 이름/타입을 중복 등록하지 않으므로 이름은 고유해야 합니다.
잘못된 JSON, 빈 화자/대사, 존재하지 않는 Sprite 키는 화면과 Console에 오류를 표시합니다.

## SO 작성

`Resources/Config/DialogueSampleConfig.asset`의 Lines에서 speaker, sprite, text를 편집합니다.
Sprite는 직접 드래그해 연결하며 해당 Sprite는 Resources 밖에 있어도 됩니다.
Create > GameLecture > Dialogue Config로 새 설정을 만들고 Resources/Config에 저장할 수 있습니다.
Scene 컴포넌트의 configKey를 해당 파일명으로 변경하세요.

## 데이터 흐름

ResourceManager.LoadAll → 완료 콜백 → DataManager.LoadJson 또는 LoadConfig → 화면 표시.
JSON의 Sprite 키는 ResourceManager.Get<Sprite>로 해석하고 SO는 직렬화된 참조를 사용합니다.
기존 전역 로더를 교체하거나 리소스를 해제하지 않습니다. 커스텀 로더 사용 시 샘플 키들을 등록해야 합니다.

## 선택 기준과 한계

소량의 대사/이미지를 Inspector에서 편집하는 샘플에는 SO가 편리합니다.
대량 대사, 번역, 외부 작성 도구, 텍스트 변경 비교에는 JSON이 유리하지만 리소스 키 검증이 필요합니다.
확장 시 JSON 대사(speakerId/text) + SO 화자 카탈로그(이름/Sprite)의 혼합 구성을 고려하세요.
현재 JSON 로더는 Resources TextAsset만 읽습니다. 서버 갱신 기능은 별도 구현이 필요합니다.
SO에는 JSON 파싱 비용이 없지만 실제 로딩 성능은 이미지 크기/로딩 정책에 크게 좌우됩니다.

UGUI/TMP와 새 Input System을 지원합니다. 이전하지 않은 Scene은 기존 IMGUI 모드로 실행됩니다.
UGUI 초상화는 Image.sprite를 사용합니다. 기존 IMGUI 모드의 초상화는 SpriteAtlas 회전/타이트 패킹을 지원하지 않습니다.
프레임워크 Singleton의 _quitting 상태 때문에 Domain Reload를 끈 반복 Play에는 프레임워크 리셋 처리가 필요합니다.

## Dialogue Settings: 타이핑 + 자판 효과음

Unity 컴파일 후 기존 Scene을 변경하지 않고 `Resources/Config/DialogueSettings.asset`과
`Resources/Sounds/DialogueKeyboard.wav`를 생성합니다. 기존 설정/음원은 덮어쓰지 않습니다.
`GameLecture > Dialogue > Select Dialogue Settings` 메뉴에서 설정을 선택할 수 있습니다.
JSON/SO 양쪽에 같은 설정이 적용되며 DataManager.LoadConfig로 로드합니다.

- Typewriter Enabled: 한 글자씩 출력. 끄면 즉시 전체 표시.
- Characters Per Second: 초당 글자 수(기본 30).
- Punctuation Pause: 문장부호 다음 추가 대기(기본 0.12초).
- Use Unscaled Time: timeScale=0에서도 진행(기본 켜짐).
- Sound Enabled / Typing Clip: 자판 효과음 사용 여부와 교체할 AudioClip.
- Volume: 기본 0.35. Minimum Sound Interval: 과도한 중첩 제한(기본 0.035초).
- Pitch Min / Max: 글자마다 음높이 변화 범위(기본 0.95~1.05).

출력 중 Complete를 누르면 소리 없이 즉시 완성합니다. 이후 Next로 다음 대사를 출력합니다.
Previous, Restart, 소스 전환 시 이전 소리를 멈추고 처음부터 출력합니다.
공백/줄바꿈은 무음이며 긴 프레임에서 여러 글자를 따라잡아도 효과음은 프레임당 최대 한 번 재생합니다.
한글 결합 문자와 UTF-16 surrogate pair를 분리하지 않도록 StringInfo 텍스트 요소 단위를 사용합니다.
복합 이모지의 묶음 처리는 Unity 런타임의 Unicode 지원에 따릅니다.

효과음은 이 컴포넌트가 소유하는 2D AudioSource에서 재생하여 다른 게임 사운드 채널에 영향을 주지 않습니다.
샘플에 활성 AudioListener가 없으면 추가합니다. 설정의 Clip이 비어 있으면 타이핑은 계속하고 소리만 생략합니다.
설정 자산은 편집 모드에서 수정하는 것을 권장합니다. 별도 설정은 Create > GameLecture > Dialogue Settings로
Resources/Config에 만들고 플레이어의 settingsKey에 자산 이름을 입력하세요.

## 분기 및 저장/불러오기

기존 Scene에서 Play 후 `Branch JSON` 또는 `Branch SO`로 숲/마을 분기 예제를 엽니다.
SO 예제는 Unity 컴파일 후 Editor API로 최초 생성됩니다. 생성되지 않았다면 Create Sample Scene 메뉴를 실행하세요.
기존 대사 자산과 Scene은 덮어쓰지 않습니다.

JSON의 각 lines 항목, SO의 각 Lines 항목에 다음 필드를 입력합니다.

- id: 필수 정수 대사 ID. `10000000`~`10009999` 범위이며 같은 대화 데이터 안에서 중복될 수 없습니다.
- choices: 선택지 목록. text는 버튼 문구, targetId는 이동할 대사의 id입니다.
- nextId: 선택지 없이 Next로 이동할 대사의 정수 ID. 생략하거나 `0`이면 순차 진행합니다.
- end: 이 대사에서 Finish로 대화를 종료.

choices, nextId, end는 동시에 사용하지 않습니다. 모두 생략하면 배열의 다음 대사로 진행하고 마지막에서 종료합니다.
id 생략/0, 범위 밖 ID, 중복 ID, 없는 목적지, 잘못된 선택지는 로딩 오류로 표시합니다.
선택지는 타이핑 완료 후 표시됩니다. 선택이 필요한 대사는 Next로 건너뛸 수 없습니다.
Previous는 배열 순서가 아닌 실제 방문 이력을 되돌립니다. Finish 후 Previous는 종료 직전 대사로 돌아갑니다.

Save/Load는 프레임워크 JsonSaveStore를 사용합니다. 저장 경로는
`Application.persistentDataPath/dialogue-progress.json`이며 하나의 수동 저장 슬롯을 사용합니다.
Save를 다시 누르면 해당 슬롯을 덮어씁니다. 재실행 후에도 유지되며 자동 저장은 하지 않습니다.
소스(JSON/SO), 리소스 키, 대사/분기 지문, 현재 대사 ID, 방문 이력, 종료 상태를 저장합니다.
Load 전에는 저장할 때와 같은 소스 및 리소스를 선택해야 합니다. 불러오기는 데이터 소스를 자동 전환하지 않습니다.
타이핑 중간에 저장해도 불러오면 해당 대사의 첫 글자부터 다시 시작합니다. 종료 상태는 그대로 복원합니다.
저장 없음, 손상된 JSON, 버전/소스/내용 불일치, 불가능한 방문 경로는 안내하고 현재 진행을 유지합니다.
지문은 변경 감지용이며 위변조 방지/암호화 기능은 아닙니다. 기존 JsonSaveStore의 단일 파일 저장 방식을 사용하므로
파일 쓰기 중 강제 종료에 대한 원자적 교체/백업은 제공하지 않습니다.

분기 예제 JSON과 생성된 SO는 최초 생성 이후 독립적으로 편집합니다. 테스트용 순수 C# 검증 코드는 Tests~에 있습니다.

## 정수 ID 규칙 (저장 버전 2)

`1000,0000`~`1000,9999`는 표기상의 구분이며 실제 int/JSON에는 쉼표 없는
`10000000`~`10009999`를 저장합니다. 총 10,000개이며 ID의 유일성 범위는 하나의 대화 리소스입니다.
서로 다른 JSON/SO는 같은 ID를 사용할 수 있습니다. 저장은 소스/리소스 키도 함께 확인합니다.
choices[].targetId에도 같은 범위의 존재하는 ID를 입력해야 하며 0은 허용하지 않습니다.
0은 nextId의 미지정 값으로만 사용합니다. ID는 배열 순서가 바뀌어도 임의로 재번호하지 않는 것을 권장합니다.

```json
{
  "id": 10000000,
  "speaker": "안내자",
  "text": "어디로 갈까요?",
  "choices": [{ "text": "숲으로", "targetId": 10000001 }]
}
```

SO Inspector의 Id / Next Id / Target Id도 정수 입력입니다. 기존 SO의 문자열은 숨겨진 이전용 필드로
보존하고 Unity Editor에서 최초 1회 숫자로 이전합니다. 기존에 유효한 숫자 ID가 있으면 유지하며,
나머지는 10000000부터 빈 ID를 할당하고 이전 문자열의 분기 연결을 함께 변환합니다.
화자/Sprite/대사는 유지하고 전체 검증을 통과한 자산만 AssetDatabase로 저장합니다.
필요하면 `GameLecture > Dialogue > Migrate Legacy IDs`를 실행하세요.
이전 실패 시 Console에서 중복/누락된 이전 목적지를 확인해야 합니다. 완료된 자산은 다시 번호를 부여하지 않습니다.
두 샘플 JSON은 정수로 갱신했습니다. 사용자 작성 JSON은 같은 규칙으로 직접 이전해야 합니다.

저장 데이터 currentId와 history도 int/int[]이며 버전은 2입니다. 버전 1 문자열 기반 저장은 호환되지 않습니다.
기존 저장 파일을 자동 삭제/덮어쓰지 않으며 새 플레이에서 Save를 눌러 버전 2로 저장하세요.

## 대화 생성 에디터

Unity 메뉴 `GameLecture > Dialogue > Dialogue Creator`에서 엽니다.

1. **새 대화**를 만들거나 상단 Object 필드에 기존 JSON(TextAsset)/DialogueConfig SO를 넣고 **JSON / SO 불러오기**를 누릅니다.
2. 왼쪽 **대사 추가**로 항목을 만들고 오른쪽에서 화자, Sprite, 대사, 진행 방식을 입력합니다.
3. 진행 방식은 **다음 순서 / 특정 ID로 이동 / 선택지 분기 / 대화 종료** 중 하나입니다.
   분기 목적지는 현재 대사 목록의 ID 드롭다운에서 선택합니다.
4. **분기 검증** 후 출력 리소스 이름(확장자 없이)을 입력하고 **JSON 내보내기** 또는 **SO 내보내기**를 누릅니다.
5. 하단 **대상 Player**에 Scene의 DialogueSamplePlayer를 넣고 **대상 Player에 적용**을 누릅니다.
   Scene을 저장한 뒤 Play하면 내보낸 데이터를 사용합니다. 적용은 마지막으로 내보낸 자산을 대상으로 하며
   이후 초안 수정은 다시 내보내야 반영됩니다.

출력 경로:

- JSON: `Assets/DialogueSample/Resources/Data/JsonData/<리소스 이름>.json`
- SO: `Assets/DialogueSample/Resources/Config/<리소스 이름>.asset`

내보내기 전에 ID 범위/중복, 화자/대사, 목적지 존재 여부, 상충하는 진행 설정, 리소스 이름 중복을 검증합니다.
JSON은 UTF-8이며 ID는 숫자로 저장합니다. SO는 AssetDatabase API로 생성/갱신하며 기존 자산의 GUID를 유지합니다.
기존 파일을 내보내기 경로에 덮어쓸 때는 에디터에서 확인합니다. 불러오기만으로 원본을 수정하지 않습니다.
JSON→SO, SO→JSON 변환은 불러온 다음 반대 형식으로 내보내면 됩니다.

새 ID는 10000000부터 비어 있는 값을 자동 배정합니다. ID를 편집하면 문서 내 nextId/targetId도 함께 갱신합니다.
복제는 새 ID를 배정하고 선택지를 복사합니다. 다른 대사가 참조하는 항목은 삭제할 수 없으며 먼저 연결을 바꿔야 합니다.
위/아래 이동은 ID를 유지하지만 순차 진행 순서를 바꿉니다. 첫 항목이 시작 대사입니다.
마지막 선택지를 삭제하면 해당 대사는 다음 순서 진행으로 바뀝니다.

Sprite 규칙:

- SO에는 프로젝트 내 Sprite를 직접 지정할 수 있습니다.
- JSON으로 내보낼 Sprite는 기본 ResourceManager 로딩 루트인 `Resources/Config`, `Resources/Prefabs`,
  `Resources/Data/JsonData`, `Resources/Sounds` 중 하나에 있어야 하고 Sprite 이름이 유일해야 합니다.
- Sprite를 선택하면 JSON 키를 자동으로 채웁니다. 키만 입력하면 내보낼 때 해당 Sprite를 찾습니다.
- 찾을 수 없거나 중복된 JSON Sprite 키, 삭제된 Sprite 참조는 오류로 처리합니다. **초상화 없음**으로 의도적으로 비울 수 있습니다.
- 외부 위치의 Sprite를 참조하는 SO를 JSON으로 변환하려면 Sprite를 적절한 Resources 경로로 옮기고 다시 선택하세요.
  생성 에디터가 이미지 파일을 자동 복사하거나 임포트 설정을 변경하지는 않습니다.

편집 초안은 프로젝트 경로별 EditorPrefs에 자동 보관되어 창을 다시 열어 복원할 수 있습니다.
Sprite 참조는 GlobalObjectId로 보관합니다. 초안 편집은 Unity Undo/Redo를 지원합니다.
초안은 이 컴퓨터에만 있으므로 협업/버전 관리에는 내보낸 JSON/SO를 사용하세요.
Scene 적용은 Undo와 prefab override 기록을 지원하며 Scene을 변경된 상태로 표시합니다. 자동 저장하지 않습니다.
Scene YAML 및 meta를 직접 수정하지 않습니다.

## 현재 Scene을 UGUI로 이전

1. Play를 종료하고 대상 Scene을 활성화합니다.
2. `GameLecture > Dialogue > Migrate Current Scene to UGUI`를 엽니다.
3. **Scene Player**에 기존 `DialogueSamplePlayer`를 지정합니다. 활성 Scene에 하나만 있으면 자동 선택됩니다.
4. **TMP Font (한글 지원)**에 프로젝트의 TMP Font Asset을 연결합니다. 기본 LiberationSans만으로는 한글이 표시되지 않으므로 한글 폰트 또는 fallback을 준비하세요. 부족한 글리프는 이전 후 Console에도 경고합니다.
5. **현재 Scene의 Player를 UGUI로 이전**을 누르고 생성된 Canvas와 View를 확인한 뒤 Scene을 저장합니다.

메뉴는 Unity Editor API만 사용합니다. 기존 Player 컴포넌트와 `source`, `jsonKey`, `configKey`, `settingsKey`,
JSON/SO, 설정, 저장 파일을 보존하고 `Use UGUI`만 켭니다. Scene YAML/meta를 직접 편집하지 않으며 Scene은 자동 저장하지 않습니다.
생성된 UI는 독립 Canvas에 있으므로 기존 Canvas의 렌더 모드나 계층은 바꾸지 않습니다.
선택지는 별도 자산을 덮어쓰지 않는 **비활성 Scene 템플릿**으로 생성합니다. 필요하면 이를 Unity에서 Prefab으로 저장한 뒤
`Choice Button Prefab`에 연결해도 됩니다. 템플릿은 Choice Content 밖에 두세요.

전체 이전은 한 번의 Undo로 되돌릴 수 있습니다. 같은 Player로 재실행하면 연결된 View를 재사용하고 디자인/폰트는 덮어쓰지 않습니다.
Player에 View가 여러 개 연결되어 있거나 기존 View의 필수 참조가 빠져 있으면 이전을 중단합니다.
활성 EventSystem이 없으면 생성하고, 현재 Scene의 기존 EventSystem이 있으면 새 Input System 모듈을 연결합니다.
기존 다른 입력 모듈은 비활성화하며, 이미 설정된 InputSystem UI Actions는 유지합니다.
여러 활성 EventSystem, 비활성 EventSystem 또는 다른 Scene의 활성 EventSystem이 있으면 정리 방법을 안내하고 중단합니다.

수동으로 구성한 UI에는 `DialogueUGUIView`를 부착하고 Player, Speaker Text, Dialogue Text,
Advance Button / Label을 연결하세요. 초상화에는 Portrait Image, 분기에는 Choice Root와
Button/Label을 연결한 `DialogueChoiceButton` Prefab 또는 비활성 템플릿이 필요합니다.
Previous/Restart/Save/Load/Close, JSON/SO 소스 버튼과 Status Text는 선택 항목입니다.
필수 참조가 빠지면 입력을 막고 상태 문구와 Console로 알립니다. Play 중 참조를 고쳤다면 View를 껐다 켜세요.
Player의 **Use UGUI**를 켜면 기존 OnGUI는 실행하지 않습니다. 미이전 Scene 보호를 위해 IMGUI 코드는 전환 기간 동안 유지합니다.

UGUI는 전체 대사를 TMP에 고정하고 UTF-16 노출 경계를 TMP 문자 범위에 대응시켜 타이핑합니다.
Rich Text는 비활성입니다. 대사와 선택지에는 각각 ScrollRect가 있고 긴 선택지는 내용에 맞게 높이가 늘어납니다.
Panel을 숨기면 타이핑/효과음이 멈추며 다시 켜면 데이터 재로딩 없이 이어집니다. Player는 Panel 외부에 유지하세요.
**닫기**는 표시와 진행을 멈추며 **재시작/불러오기/소스 전환**으로 다시 열 수 있습니다.
**종료**는 기존 진행 그래프의 완료 상태이며 버전 2 저장/복원을 지원합니다.
같은 프레임의 이동 명령은 한 번만 처리하고 이전 대사의 선택지 콜백은 표시 세대 번호로 거부합니다.
View/선택지는 자신이 등록한 리스너만 해제하므로 Inspector의 다른 이벤트는 보존됩니다.

새 샘플 Scene 생성 시 TMP 기본 폰트가 있으면 UGUI도 생성합니다. 기존 Scene은 자동 이전하지 않습니다.
Dialogue Creator의 **대상 Player에 적용**은 동일한 Player와 필드를 계속 사용합니다.

검증 실행: PowerShell에서 `& './DialogueSample/Tests~/VerifyUGUI.ps1'`.
설치 위치가 다르면 `-UnityData '<Unity Editor>/Data'`를 지정하세요.
이 스크립트는 프로젝트의 기존 Library 어셈블리를 참조해 모든 Dialogue 런타임/에디터 소스를 컴파일하고,
순수 C# 분기/버전 2 저장 복원 및 한글/결합 문자/surrogate pair/문장부호 타이핑을 검증합니다.
산출물은 시스템 임시 폴더에만 생성합니다. Unity Play Mode 테스트를 대체하지 않습니다.

Scene 이전 후 확인할 항목:

- 같은 JSON/SO의 화자·초상화·본문·분기, 타이핑 효과음과 즉시 완성.
- 분기 대사 Next 차단, Previous 방문 이력, Finish 후 저장/복원, 없는/불일치 저장의 안내.
- 패널 재활성화, 연속 클릭과 키보드 Submit, 선택 직후 이전 버튼의 중복 입력 차단.
- 긴 대사/선택지 스크롤, 한글 폰트와 fallback, 작은 창/넓은 화면 배치.
- 이전 메뉴 재실행 시 중복 생성 없음, Undo/Redo 후 참조와 Use UGUI 복원.

검증 도구: `GameLecture > Dialogue > Run Creator Conversion Checks`는 자산을 저장하지 않고
JSON→SO→JSON 변환, 한글/줄바꿈/분기 유지 및 원본 분리를 검사합니다.
`Tests~/DialogueAuthoringChecks.cs`는 작성 로직의 독립 실행 테스트이며,
`Tests~/DialogueCreatorBatchChecks.cs`는 별도 임시 Unity 프로젝트에서만 실행하는 실제 자산 변환 테스트입니다.
