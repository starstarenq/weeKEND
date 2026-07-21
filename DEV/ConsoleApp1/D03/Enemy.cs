using System;
using System.Collections.Generic;
using System.Text;
using D03;

namespace D03
{

    public class Enemy
    {
        // --- 몬스터 변수 ---
        public string Name { get; private set; }
        public MonsterType Type { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public float BaseDamage { get; private set; }
        public bool IsDead { get; private set; }

        public (float x, float y) Direction { get; set; }
        public float MoveSpeed { get; set; }

        private bool isProvoked;      // 선제공격 유무 flag
        private Player targetPlayer;   // 반격 대상 플레이어 참조
        private const int AttackIntervalMs = 1500;

        // --- 몬스터 메서드 ---
        public void Move(float x, float y) { }

        public void TakeDamage(float damage, Player attacker) { }

        private void StartCounterAttackLoop() { }
    }

}
