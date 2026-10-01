# 2D Fireball 입력 중계 샘플

설계 기준: `Assets/unity-input-skillmanager-mediator.html`의 입력 → 중계자 → 스킬 데이터 → 이벤트 구독자 구조.

## 실행

1. Unity의 스크립트 컴파일이 끝나면 **Tools > Skill Manager > Fireball Sample 2D**를 연다.
2. EditorGUILayout 창에서 쿨타임, 마나 비용/회복량, 피해량, 속도를 정하고 **Create 2D Fireball Sample Scene**을 누른다.
3. 열려 있는 수정된 씬에 대한 Unity 저장 안내를 처리하면 새 샘플 씬이 생성되어 열린다.
4. Play 후 Game 뷰를 클릭하고 **Space** 또는 **게임패드 South 버튼**을 누른다. XY 평면에서 오른쪽(+X) 고정 발사이며 이동/조준은 이 샘플의 범위에 포함하지 않는다.

`Generated2D`, `Generated2D 1` 등 고유 폴더에 입력 액션, Fireball ScriptableObject, 스프라이트 PNG, 프리팹, 머티리얼, 발동음, `FireballSample2D.unity`를 생성한다. 씬과 meta를 직접 쓰는 코드는 없으며 Scene/Prefab/AssetDatabase/TextureImporter API를 사용한다.

이전 3D 생성 씬은 자동 변환하지 않는다. 투사체 런타임 코드가 2D로 변경되었으므로 반드시 새 2D 씬을 생성해서 사용한다. 카메라는 Z=-10에서 XY 평면을 바라보는 직교 카메라다. SpriteRenderer와 조명이 필요 없는 스프라이트 셰이더를 사용한다. 표적은 BoxCollider2D이며, 투사체는 이동 구간을 CircleCast로 검사하므로 Rigidbody2D나 자체 Collider2D가 필요 없다.

## 책임 분리

- `PlayerSkillInput`: 입력 자산을 런타임 복제하고 performed를 구독/해지한다. 콜백은 `manager.UseSkill(0)`만 호출한다.
- `SkillManager`: 싱글톤 수명, 해금/쿨타임/마나/설정 검증, 런타임 상태, Cast 호출, 공용 발동음, 성공/거부 이벤트를 담당한다. ScriptableObject 원본에 플레이 상태를 기록하지 않는다.
- `SkillData` / `FireballSkillData`: 공통 수치와 다형적 Cast. 매니저에 스킬 종류 switch가 없다.
- `FireballProjectile`: 로컬 +X 이동 구간의 Physics2D.CircleCast 충돌 판정, 피해 전달, 수명 종료. 트리거는 무시한다.
- `FireballSampleHUD`: 결과 이벤트 구독/해지, CanUse 및 쿨타임 조회로 상태를 표시한다.

구매·강화는 이번 Fireball 사용 예제에 포함하지 않는다. 기본 마나는 100이며 연속 시연용 자동 회복은 매니저가 처리한다. 표적은 체력이 0이 되면 2초 뒤 회복한다.

## 플레이 확인

### UGUI 핫바 / CoolTimeUI

스크립트 리로드 후 현재 활성 씬에 SkillManager와 PlayerSkillInput이 있으면 핫바를 한 번 자동 추가한다. 씬은 수정 상태로 남기므로 저장해야 유지된다. 수동으로는 **Tools > Skill Manager > Add Hotbar To Current Scene** 또는 생성 창의 같은 버튼을 사용한다. 기존 SkillHotbarUI가 있으면 중복 생성하지 않는다. 새 2D 씬에는 처음부터 포함된다.

- Screen Space Overlay Canvas 하단 중앙에 Fireball 버튼, 원형 쿨타임 오버레이, 남은 초, 사용 가능 상태를 표시한다.
- `CoolTimeUI.OnSkillUsed(int)`가 매니저의 `OnSkillUsed` C# 이벤트를 구독한다. 성공한 스킬의 콜백만 카운트다운 표시를 시작한다. 거부된 입력은 쿨타임을 재시작하지 않는다.
- 남은 시간은 별도 타이머 대신 매니저의 `RemainCooldown`을 조회한다. 일시정지와 UI 비활성화 후 재활성화에서도 실제 게임 쿨타임과 일치한다. 활성/비활성에 맞춰 구독과 해제를 처리한다.
- 슬롯 위 `TextMeshProUGUI`는 `PlayerSkillInput.FireballAction.GetBindingDisplayString`으로 실제 바인딩을 표시한다. 키보드/게임패드 바인딩을 함께 표시하고 런타임 `ApplyBindingOverride` 변경은 `BoundControlsChanged` 콜백으로 반영한다. 바인딩이 없으면 Unbound로 표시한다.
- 버튼 클릭도 `SkillManager.UseSkill(0)`을 호출한다. 버튼의 Submit 내비게이션을 끄므로 Space 입력과 Submit이 중복 발사하지 않는다.
- TMP Essential Resources의 기본 폰트를 사용한다. InputSystemUIInputModule이 있는 EventSystem이 없는 샘플 씬에는 함께 생성한다.

확인 항목: 스킬 사용 후 남은 초와 원형 표시 감소, 쿨타임 입력 거부 시 표시 유지, 종료 시 표시 해제, UI 재활성화 시 남은 시간 복원, 런타임 키 바인딩 변경 시 상단 텍스트 갱신, 슬롯 클릭 발사, 설치 재실행 시 중복 없음.

### 기본 스킬 흐름

- 첫 입력에 바로 발사되고 마나가 비용만큼 줄며 성공 횟수가 1 증가한다.
- 쿨타임 중 재입력하면 Cooldown으로 거부되며 추가 발사/마나 차감이 없다.
- 마나 회복을 0으로 생성하고 마나를 소진하면 Not enough mana로 거부된다.
- 투사체 명중 시 표적 HP와 Hits가 갱신된다.
- PlayerSkillInput을 비활성화하면 입력이 멈추고 다시 활성화해도 중복 발동하지 않는다.
- Play 종료 후 재진입하면 마나/쿨타임/표적이 초기화되고 FireballSkill 원본 수치는 유지된다.
- 생성 버튼을 다시 누르면 기존 생성물을 덮어쓰지 않는다.
