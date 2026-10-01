using System.Collections.Generic;
using UnityEngine;

namespace KDH_SkillSystem
{
    public class SkillManager : Singleton<SkillManager>
    {
        // int ID를 키값으로 받는 스킬 데이터 저장소
        private readonly Dictionary<int, SkillBase> _skillDatabase = new Dictionary<int, SkillBase>();

        // Resources 내 스킬 에셋들이 위치할 하위 경로
        private const string SkillAssetsPath = "Skills";

        protected override void Awake()
        {
            base.Awake();
            // 싱글톤 중복 체크 로직이 기존 Singleton<T>에 있다면 생략 가능합니다.
            InitializeManager();
        }

        /// <summary>
        /// 매니저 초기화 및 스킬 에셋 자동 로드/세팅
        /// </summary>
        private void InitializeManager()
        {
            _skillDatabase.Clear();

            // 1. Resources/Skills/ 경로에 있는 모든 SkillBase 타입의 에셋을 로드합니다.
            SkillBase[] loadedSkills = Resources.LoadAll<SkillBase>(SkillAssetsPath);

            if (loadedSkills == null || loadedSkills.Length == 0)
            {
                Debug.LogWarning($"[SkillDataManager] '{SkillAssetsPath}' 경로에서 로드된 스킬 에셋이 없습니다. 에셋 위치를 확인하세요.");
                return;
            }

            // 2. 로드된 에셋들을 ID 기반으로 Dictionary에 등록합니다.
            foreach (SkillBase skill in loadedSkills)
            {
                if (skill == null) continue;

                if (_skillDatabase.ContainsKey(skill.SkillId))
                {
                    Debug.LogError($"[SkillDataManager] 중복된 스킬 ID 발견! ID: {skill.SkillId}, 스킬명: {skill.SkillName}");
                    continue;
                }

                // 스킬 데이터 초기화 수행 (필요 시)
                skill.Initialize();

                _skillDatabase.Add(skill.SkillId, skill);
                Debug.Log($"[SkillDataManager] 스킬 등록 완료 - ID: {skill.SkillId}, 이름: {skill.SkillName}");
            }

            Debug.Log($"[SkillDataManager] 총 {_skillDatabase.Count}개의 스킬 데이터가 로드되었습니다.");
        }

        /// <summary>
        /// ID를 통해 순수 스킬 데이터를 조회 및 반환합니다.
        /// </summary>
        /// <param name="skillId">조회할 스킬 고유 ID</param>
        /// <returns>존재하는 스킬 데이터 (없으면 null)</returns>
        public SkillBase GetSkill(int skillId)
        {
            if (_skillDatabase.TryGetValue(skillId, out SkillBase skill))
            {
                return skill;
            }

            Debug.LogWarning($"[SkillDataManager] ID '{skillId}'에 해당하는 스킬 데이터를 찾을 수 없습니다.");
            return null;
        }
    }
}

