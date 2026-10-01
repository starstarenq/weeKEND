using UnityEngine;

namespace KDH_SkillSystem
{
    public interface ISkillExecute
    {
        // 2D 환경이므로 Vector3 targetPosition은 마우스 world 좌표나 타겟의 위치가 됩니다.
        void Execute(GameObject caster, Vector3 targetPosition);
    }
}
