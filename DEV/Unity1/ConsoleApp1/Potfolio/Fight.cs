using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Potfolio
{


namespace RetroLifeBattle
    {
        public class Fight
        {
            // 플레이어 데이터 변수 (체력과 공격력만 가짐)
            private int playerHp = 100;
            private const int PlayerMaxHp = 100;
            private readonly int playerAtk = 20;

            // 몬스터 데이터 변수 (체력과 공격력만 가짐)
            private int monsterHp = 150;
            private const int MonsterMaxHp = 150;
            private readonly int monsterAtk = 10;

            // 상태 제어 플래그
            private bool isCombatStarted = false;
            private bool isBattleOver = false;

            // 선제공격을 실행하는 메서드
            public async Task InitiatePreemptiveStrike()
            {
                if (isCombatStarted) return;

                isCombatStarted = true;
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚔️ [선제공격] 플레이어가 몬스터를 먼저 공격하며 실시간 전투가 시작됩니다!\n");
                Console.ResetColor();

                // 실시간 교전 루프 시작 (둘 중 하나가 죽을 때까지 0.5초 간격 반복)
                while (!isBattleOver)
                {
                    ExecuteCombatTurn();
                    PrintStatus();

                    if (isBattleOver) break;

                    // 0.5초(500ms) 대기 후 다음 턴 진행 (실시간 액션 연출)
                    await Task.Delay(500);
                }
            }

            // 1회 교전 처리 메서드 (체력과 공격력 연산)
            private void ExecuteCombatTurn()
            {
                Console.WriteLine("--------------------------------------------------");

                // 1. 플레이어가 몬스터를 선제공격
                monsterHp -= playerAtk;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"▶ 주인공이 몬스터에게 {playerAtk}의 피해를 입혔습니다.");

                // 몬스터 사망 판정
                if (monsterHp <= 0)
                {
                    monsterHp = 0;
                    isBattleOver = true;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n💀 몬스터를 처치했습니다! 전투에서 승리했습니다.");
                    Console.ResetColor();
                    return;
                }

                // 2. 몬스터의 실시간 반격
                playerHp -= monsterAtk;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"◀ 몬스터가 반격하여 주인공에게 {monsterAtk}의 피해를 입혔습니다.");

                // 플레이어 사망 판정
                if (playerHp <= 0)
                {
                    playerHp = 0;
                    isBattleOver = true;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n❌ 주인공이 쓰러졌습니다... 게임 오버.");
                    Console.ResetColor();
                }

                Console.ResetColor();
            }

            // 실시간으로 갱신되는 체력 바 화면 출력
            private void PrintStatus()
            {
                Console.WriteLine();
                Console.WriteLine($"[주인공] HP: {playerHp}/{PlayerMaxHp} {GenerateHpBar(playerHp, PlayerMaxHp)} (ATK: {playerAtk})");
                Console.WriteLine($"[몬스터] HP: {monsterHp}/{MonsterMaxHp} {GenerateHpBar(monsterHp, MonsterMaxHp)} (ATK: {monsterAtk})");
                Console.WriteLine("--------------------------------------------------\n");
            }

            // 시각적인 체력 게이지 바 문자열 생성기
            private string GenerateHpBar(int current, int max)
            {
                int barLength = 10;
                int filledLength = (int)Math.Round((double)current / max * barLength);

                // 게이지가 남았으나 반올림으로 0이 되는 것 방지
                if (current > 0 && filledLength == 0) filledLength = 1;

                string filled = new string('■', filledLength);
                string empty = new string('□', barLength - filledLength);
                return $"[{filled}{empty}]";
            }
        }
    }

}

