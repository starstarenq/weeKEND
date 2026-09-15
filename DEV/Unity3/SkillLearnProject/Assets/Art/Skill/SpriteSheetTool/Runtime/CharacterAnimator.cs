using UnityEngine;

namespace SysKill.Characters
{
    /// <summary>
    /// 생성된 Animator(4방향 BlendTree + 파라미터)를 구동하기 위한 샘플 컨트롤러.
    /// SpriteSheetTool 이 생성한 Prefab 의 "컨트롤러 스크립트 슬롯"에 부착된다.
    ///
    /// Animator 파라미터 규약:
    ///   MoveX (Float), MoveY (Float) : 바라보는 방향(카디널 단위 벡터)
    ///   Speed (Float)                : 0 이면 idle, >0 이면 walk
    ///   Attack / Hit / Death (Trigger)
    /// 실제 이동 로직은 프로젝트에 맞게 교체/확장해서 사용한다.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimator : MonoBehaviour
    {
        static readonly int MoveXHash = Animator.StringToHash("MoveX");
        static readonly int MoveYHash = Animator.StringToHash("MoveY");
        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int AttackHash = Animator.StringToHash("Attack");
        static readonly int HitHash = Animator.StringToHash("Hit");
        static readonly int DeathHash = Animator.StringToHash("Death");

        Animator _animator;
        Vector2 _facing = Vector2.down;

        /// <summary>현재 바라보는 카디널 방향.</summary>
        public Vector2 Facing => _facing;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            SetFacing(_facing);
        }

        /// <summary>
        /// 이동 입력을 반영한다. 크기가 0 보다 크면 walk 로 전환되고 방향도 갱신된다.
        /// </summary>
        public void SetMovement(Vector2 direction)
        {
            float speed = direction.magnitude;
            _animator.SetFloat(SpeedHash, speed);
            if (speed > 0.01f)
                SetFacing(direction);
        }

        /// <summary>이동 없이 바라보는 방향만 갱신한다(가장 가까운 카디널로 스냅).</summary>
        public void SetFacing(Vector2 direction)
        {
            _facing = ToCardinal(direction);
            _animator.SetFloat(MoveXHash, _facing.x);
            _animator.SetFloat(MoveYHash, _facing.y);
        }

        public void PlayAttack() => _animator.SetTrigger(AttackHash);
        public void PlayHit() => _animator.SetTrigger(HitHash);
        public void PlayDeath() => _animator.SetTrigger(DeathHash);

        static Vector2 ToCardinal(Vector2 dir)
        {
            if (dir.sqrMagnitude < 1e-6f)
                return Vector2.down;
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                return dir.x < 0f ? Vector2.left : Vector2.right;
            return dir.y < 0f ? Vector2.down : Vector2.up;
        }
    }
}
