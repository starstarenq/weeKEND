using Potfolio.RetroLifeBattle;
using System;
using System.Threading.Tasks;

namespace RetroLifeBattle
{
    class Program
    {
        // 비동기(Task) 진입점을 설정하여 실시간 대기 기능 지원
        static async Task Main(string[] args)
        {
            // Fight 클래스 인스턴스 생성
            Fight gameFight = new Fight();

            Console.WriteLine("==================================================");
            Console.WriteLine("      회고인생 (Realtime Action RPG) 테스트      ");
            Console.WriteLine("==================================================");
            Console.WriteLine("기획 규칙: 인물을 공격하기 전까지는 전투를 벌이지 않습니다.");
            Console.WriteLine("몬스터가 앞에 대기하고 있습니다.");
            Console.WriteLine("==================================================\n");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("👉 엔터키(Enter)를 누르면 [선제공격]을 가하며 전투를 시작합니다.");
            Console.ResetColor();

            // 유저가 선제공격(엔터 입력)을 선언할 때까지 대기
            Console.ReadLine();

            // Fight 클래스의 전투 시뮬레이션 개시 및 종료 대기
            await gameFight.InitiatePreemptiveStrike();

            Console.WriteLine("\n전투 시뮬레이션이 종료되었습니다. 프로그램을 종료하려면 엔터를 누르세요.");
            Console.ReadLine();
        }
    }
}

