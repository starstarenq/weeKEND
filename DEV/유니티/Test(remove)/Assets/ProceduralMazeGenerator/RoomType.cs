using System;

namespace ProceduralMaze
{
    /// <summary>
    /// 열린 방향의 개수와 배치로 결정되는 5가지 방 종류.
    /// 완전 미로(스패닝 트리)의 모든 칸은 이 5종 중 하나에 정확히 대응된다.
    /// </summary>
    public enum RoomType
    {
        /// <summary> 막다른 방 — 한 방향만 뚫림 (1방향). </summary>
        DeadEnd = 0,

        /// <summary> 일자 방 — 마주보는 2방향 뚫림 (2방향, 직선). </summary>
        Straight = 1,

        /// <summary> ㄱ자 방 — 인접한 2방향 뚫림 (2방향, 코너). </summary>
        Corner = 2,

        /// <summary> T자 방 — 3방향 뚫림, 한쪽만 막힘 (3방향). </summary>
        TJunction = 3,

        /// <summary> 십자 방 — 4방향 모두 뚫림 (4방향, 교차로). </summary>
        Cross = 4,
    }

    /// <summary>
    /// 열린 방향 마스크(Dir) → (방 종류, 90도 회전 스텝)으로 분류한다.
    /// 각 종류의 "기준 프리팹"은 회전 0에서 Canonical() 방향이 뚫려 있다고 본다.
    /// </summary>
    public static class RoomClassifier
    {
        /// <summary> 회전 0에서의 기준 열림 방향. </summary>
        public static Dir Canonical(RoomType t)
        {
            switch (t)
            {
                case RoomType.DeadEnd:   return Dir.N;                          // 북쪽만
                case RoomType.Straight:  return Dir.N | Dir.S;                  // 남-북 일자
                case RoomType.Corner:    return Dir.N | Dir.E;                  // 북+동 ㄱ자
                case RoomType.TJunction: return Dir.N | Dir.E | Dir.S;         // 서쪽만 막힘
                case RoomType.Cross:     return Dir.N | Dir.E | Dir.S | Dir.W; // 사방
                default:                 return Dir.None;
            }
        }

        /// <summary>
        /// 열린 방향 마스크를 5종 방 + 회전으로 분류한다.
        /// 성공 시 true, type/rotSteps(0..3)을 채운다.
        /// rotSteps는 "기준 프리팹을 Y축으로 +90*rotSteps도 회전하면 open이 된다"는 뜻.
        /// </summary>
        public static bool Classify(Dir open, out RoomType type, out int rotSteps)
        {
            foreach (RoomType t in Enum.GetValues(typeof(RoomType)))
            {
                Dir canon = Canonical(t);
                if (canon.Count() != open.Count()) continue; // 방향 개수부터 걸러낸다
                for (int r = 0; r < 4; r++)
                {
                    if (canon.Rotate(r) == open)
                    {
                        type = t;
                        rotSteps = r;
                        return true;
                    }
                }
            }
            type = RoomType.Cross;
            rotSteps = 0;
            return false; // 열린 방향이 0개(고립 칸)인 경우 등
        }
    }
}
