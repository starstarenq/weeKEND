# Volume / 그래픽 품질 세팅

현재 씬에 **Global Volume** 을 추가하고 포스트 프로세싱 + **SSAO** 로 전체 그래픽 품질을 올리는 에디터입니다.
Unity 6 (URP). YAML/GUID 를 직접 편집·참조하지 않고 Unity 에디터 API 로만 구성합니다.

## 실행
Unity 메뉴 **Tools ▸ Volume Setup ▸ Global Volume & Quality** → 옵션 설정 → **▶ Apply Volume & Quality**

## 하는 일
1. `Volume/Profiles/GlobalVolumeProfile.asset` 생성(또는 재사용).
2. 프로파일에 포스트 프로세싱 오버라이드 구성:
   - **Tonemapping** (기본 ACES)
   - **Bloom**
   - **Color Adjustments** (Contrast / Saturation / Exposure)
   - **Vignette**
3. 현재 씬에 **Global Volume** 오브젝트 추가 (`Volume`, isGlobal=true, 프로파일 연결).
4. **SSAO** — 활성 3D **Universal Renderer** 에 `ScreenSpaceAmbientOcclusion` Renderer Feature 추가/설정.

## SSAO 관련 주의
- URP 에서 SSAO 는 Volume 오버라이드가 **아니라 Renderer Feature** 입니다. 따라서 3D Universal Renderer 에셋에 적용됩니다.
- 현재 활성 렌더러가 **2D Renderer** 이면 SSAO 는 적용되지 않으며, Console 에 안내가 출력됩니다.
- 자동 추가가 실패할 경우: 렌더러 에셋 선택 → **Add Renderer Feature ▸ Screen Space Ambient Occlusion** 으로 수동 추가 후, 이 창에서 다시 Apply 하면 값이 설정됩니다.

## 폴더 구조 (모두 Volume 하위)
| 폴더 | 용도 |
|------|------|
| `Editor` | 생성기 윈도우 & 빌더 |
| `Profiles` | 생성된 `VolumeProfile` 에셋 |

## 참고
- Bloom/Tonemapping 등이 화면에 보이려면 카메라의 **Post Processing** 이 켜져 있어야 하며, URP 에셋의 Post-processing 이 활성화되어 있어야 합니다.
- 여러 Global Volume 이 있을 경우 `Priority` 가 높은 것이 우선합니다.
