using System;
using System.Collections.Generic;
using System.Text;

namespace D03
{
    internal class Type
    {
        // 파일 맨 위에 enum을 배치합니다.
        public enum MonsterType { Normal, Medium, MidBoss, FinalBoss }
        public enum AttackType { Normal, Skill }

        // 그 아래에 원래 플레이어 클래스를 둡니다.
        public class Player
        {
            // ... 플레이어 변수와 메서드들
        }

    }
}
