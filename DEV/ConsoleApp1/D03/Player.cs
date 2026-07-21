using System;
using System.Collections.Generic;
using System.Text;

namespace D03
{
    
    public class Player
    {
        /* 변수 + 함수 기능을 주석으로 표현

        방향, 속도,

        어떻게? 총을 쏘고싶다


         플레이어 클래스를 만들 것인데, 플레이어가 총을 쏘고 방향과 속도가 있어야하고 타입은 관통형이다.
       
         
         */

        // 플레이어 클래스를 만들것인데, 플레이어가 3연타 콤보 공격을 지니고 방향과 이동속도가 있어야하며 데미지를 입어야하고 체력이 0일시 게임오버상태가 되어야하고, 에스페란사를 획득 시 체력이 모두 회복되어야한다.

        // --- 변수 (Fields & Properties) ---
        public string Name { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public bool GameOver { get; private set; }

        public (float x, float y) Direction { get; set; }
        public float MoveSpeed { get; set; }

        private int currentCombo;
        private const int MaxCombo = 3;
        private System.DateTime lastAttackTime;
        private const double ComboWindow = 1.5;

        // 열거형(Enum) 정의: 몬스터 유형 및 공격 종류
        public enum MonsterType { Normal, Medium, MidBoss, FinalBoss }
        public enum AttackType { Normal, Skill }


        // --- 메서드 (Methods) ---
        public void Move(float x, float y) { }

        public void AttackTarget(Enemy target) { }

        public void TakeDamage(float baseDamage, MonsterType monsterType, AttackType attackType)
        {
            if (GameOver) return;

            // 1. 몬스터 유형별 데미지 배율 적용 (보스일수록 커짐)
            float monsterMultiplier = monsterType switch
            {
                MonsterType.Normal => 1.0f,
                MonsterType.Medium => 1.3f,
                MonsterType.MidBoss => 1.7f,
                MonsterType.FinalBoss => 2.5f,
                _ => 1.0f
            };

            // 2. 공격 종류별 데미지 배율 적용
            float attackMultiplier = attackType switch
            {
                AttackType.Normal => 1.0f,
                AttackType.Skill => 1.5f,
                _ => 1.0f
            };

            // 3. 최종 데미지 계산 및 체력 차감
            float finalDamage = baseDamage * monsterMultiplier * attackMultiplier;
            Health -= finalDamage;

            // 4. 게임오버 검사
            if (Health <= 0)
            {
                Health = 0;
                TriggerGameOver();
            }
        }

        public void ObtainEsperanza() { }

        private void TriggerGameOver() { }

    }
}

