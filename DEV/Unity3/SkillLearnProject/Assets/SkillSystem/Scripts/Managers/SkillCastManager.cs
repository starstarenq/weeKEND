using UnityEngine;
using UnityEngine.InputSystem;

namespace KDH_SkillSystem
{
    public class SkillCastManager : Singleton<SkillCastManager>
    {
        [Header("연결할 컴포넌트")]
        [SerializeField] private SimpleController_UsingPlayerInput playerInput;

        [Header("테스트용 시전 설정")]
        [SerializeField] private int fireballSkillId = 101; // DataManager에 등록한 파이어볼 ID

        protected override void Awake()
        {
            // 기존 싱글톤 초기화 로직 가정
        }

        private void OnEnable()
        {
            // 플레이어 제어기의 AltFire 입력 이벤트를 구독하여 매니저와 연결
            if (playerInput != null)
            {
                playerInput.OnAltFireInputTriggered += HandleAltFireInput;
            }
        }

        private void OnDisable()
        {
            // 메모리 누수 방지를 위한 이벤트 해제
            if (playerInput != null)
            {
                playerInput.OnAltFireInputTriggered -= HandleAltFireInput;
            }
        }

        /// <summary>
        /// 입력 이벤트를 수신했을 때 실행되는 핸들러
        /// </summary>
        private void HandleAltFireInput()
        {
            Debug.Log("[SkillCastManager] 플레이어의 AltFire 입력을 감지했습니다. 시전을 시도합니다.");
            TryCastSkill(fireballSkillId);
        }

        /// <summary>
        /// 스킬 ID를 받아 자원/쿨타임을 검증하고 최종 시전하는 핵심 메서드
        /// </summary>
        public void TryCastSkill(int skillId)
        {
            // 1. 데이터 매니저로부터 순수 데이터 조회 (이전 단계 기능 활용)
            SkillBase skillData = SkillManager.Instance.GetSkill(skillId);
            if (skillData == null) return;

            // [추후 확장] 2. 시전 가능 여부 검증 구조 배치 가능 공간 (현재는 쿨타임/MP 패스)
            // if (playerMp < skillData.CostInfo.mpCost) return;

            // 3. 다형성(Interface)을 활용하여 실제 스킬 로직 트리거
            if (skillData is ISkillExecute executableSkill)
            {
                // 2D 환경에서의 타겟팅 타겟 포지션 구하기 (예시: 마우스의 월드 좌표)
                Vector3 targetWorldPosition = GetMouseWorldPosition2D();

                // 시전자(플레이어 자신)와 목표 좌표를 넘겨 실행
                executableSkill.Execute(playerInput.gameObject, targetWorldPosition);

                // [추후 확장] 4. 시전 성공 Callback 브릿지 이벤트 전파 처리 공간
                // SkillCallbackBridge.Instance.OnSkillCastSuccess(skillData, targetWorldPosition);
            }
            else
            {
                Debug.LogError($"[SkillCastManager] ID {skillId} 스킬이 ISkillExecute 인터페이스를 구현하지 않았습니다.");
            }
        }

        /// <summary>
        /// 2D 마우스 월드 좌표를 계산하는 헬퍼 메서드
        /// </summary>
        private Vector3 GetMouseWorldPosition2D()
        {
            if (Camera.main == null) return Vector3.zero;
            Vector3 mousePos = Mouse.current.position.ReadValue();         //  Input 옛날 버전 new inputsystem 버전으로 바꿔줘
            mousePos.z = -Camera.main.transform.position.z; // 2D 평면 보정
            return Camera.main.ScreenToWorldPoint(mousePos);
        }
    }
}
