using System;
using System.Collections.Generic;

namespace ProceduralMaze
{
    /// <summary>
    /// 4방향 비트플래그. 방 하나의 "열린 방향"을 하나의 마스크로 표현한다.
    /// N = +Z, E = +X, S = -Z, W = -X (Unity 좌표 기준).
    /// </summary>
    [Flags]
    public enum Dir
    {
        None = 0,
        N = 1,
        E = 2,
        S = 4,
        W = 8,
    }

    public static class DirExt
    {
        public static readonly Dir[] All = { Dir.N, Dir.E, Dir.S, Dir.W };

        /// <summary>
        /// 90도 시계방향(위에서 봤을 때) 회전. N→E→S→W→N.
        /// Unity의 +90도 Y 회전과 일치한다(+Z가 +X로 간다).
        /// </summary>
        public static Dir Rotate(this Dir d, int steps)
        {
            steps = ((steps % 4) + 4) % 4;
            for (int i = 0; i < steps; i++)
            {
                Dir r = Dir.None;
                if ((d & Dir.N) != 0) r |= Dir.E;
                if ((d & Dir.E) != 0) r |= Dir.S;
                if ((d & Dir.S) != 0) r |= Dir.W;
                if ((d & Dir.W) != 0) r |= Dir.N;
                d = r;
            }
            return d;
        }

        /// <summary> 열린 방향 개수. </summary>
        public static int Count(this Dir d)
        {
            int c = 0;
            foreach (var one in All)
                if ((d & one) != 0) c++;
            return c;
        }

        public static Dir Opposite(this Dir d)
        {
            switch (d)
            {
                case Dir.N: return Dir.S;
                case Dir.E: return Dir.W;
                case Dir.S: return Dir.N;
                case Dir.W: return Dir.E;
                default: return Dir.None;
            }
        }

        // 방향 → 격자 오프셋
        public static int Dx(this Dir d) => d == Dir.E ? 1 : (d == Dir.W ? -1 : 0);
        public static int Dz(this Dir d) => d == Dir.N ? 1 : (d == Dir.S ? -1 : 0);

        public static IEnumerable<Dir> Each(this Dir mask)
        {
            foreach (var one in All)
                if ((mask & one) != 0) yield return one;
        }
    }
}
