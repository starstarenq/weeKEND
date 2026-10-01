using UnityEngine;

namespace KDH_SkillSystem
{
    public abstract class SkillBase : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private int skillId;
        [SerializeField] private string skillName;
        [TextArea(2, 5)][SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("스킬 속성 및 스탯")]
        [SerializeField] private SkillType skillType;
        [SerializeField] private TargetType targetType;
        [SerializeField] private Element element;
        [SerializeField] private SkillCostInfo costInfo;
        [SerializeField] private SkillImpactInfo impactInfo;

        // 외부(DataManager 등)에서 데이터 접근을 위한 프로퍼티 (Getter)
        public int SkillId => skillId;
        public string SkillName => skillName;
        public string Description => description;
        public Sprite Icon => icon;
        public SkillType SkillType => skillType;
        public TargetType TargetType => targetType;
        public Element Element => element;
        public SkillCostInfo CostInfo => costInfo;
        public SkillImpactInfo ImpactInfo => impactInfo;

        /// <summary>
        /// 스킬 데이터 초기화가 필요할 경우 사용할 추상 메서드
        /// </summary>
        public abstract void Initialize();
    }
}
