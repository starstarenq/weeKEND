using System.Collections.Generic;
using System.Linq;

namespace ProceduralMaze
{
    /// <summary>
    /// 미로 생성 알고리즘 + Dijkstra 거리 기반 입구/출구 배치.
    /// 방향(어느 벽을 문으로 뚫을지)은 전적으로 이 알고리즘이 정한다.
    /// </summary>
    public static class MazeAlgorithms
    {
        /// <summary>
        /// Recursive Backtracker (랜덤 깊이우선). 막다른 길이 많은 미로.
        /// 스택으로 전진하며 Link()로 벽을 뚫고, 막히면 backtrack.
        /// </summary>
        public static void RecursiveBacktracker(MazeGrid grid, System.Random rng)
        {
            var start = grid[rng.Next(grid.Cols), rng.Next(grid.Rows)];
            var stack = new Stack<MazeCell>();
            var visited = new HashSet<MazeCell> { start };
            stack.Push(start);

            while (stack.Count > 0)
            {
                var cur = stack.Peek();
                var candidates = cur.Neighbors().Where(n => !visited.Contains(n)).ToList();
                if (candidates.Count == 0)
                {
                    stack.Pop(); // backtrack
                    continue;
                }

                var next = candidates[rng.Next(candidates.Count)];
                cur.Link(next);      // 벽을 뚫어 문을 만든다
                visited.Add(next);
                stack.Push(next);
            }
        }

        /// <summary>
        /// Binary Tree — 각 칸에서 북/동 중 하나를 랜덤으로 뚫는다. 단순·대각 편향.
        /// (RoomClassifier와 함께 알고리즘 교체 예시로 제공)
        /// </summary>
        public static void BinaryTree(MazeGrid grid, System.Random rng)
        {
            foreach (var c in grid.EachCell())
            {
                var opts = new List<MazeCell>();
                if (c.North != null) opts.Add(c.North);
                if (c.East  != null) opts.Add(c.East);
                if (opts.Count == 0) continue;
                c.Link(opts[rng.Next(opts.Count)]);
            }
        }

        /// <summary> 시작 칸에서 문(links)을 따라 BFS. 미로는 간선비용 1이라 Dijkstra=BFS. </summary>
        public static Dictionary<MazeCell, int> Distances(MazeCell root)
        {
            var dist = new Dictionary<MazeCell, int> { { root, 0 } };
            var frontier = new Queue<MazeCell>();
            frontier.Enqueue(root);

            while (frontier.Count > 0)
            {
                var cell = frontier.Dequeue();
                foreach (var next in cell.Links())
                {
                    if (dist.ContainsKey(next)) continue;
                    dist[next] = dist[cell] + 1;
                    frontier.Enqueue(next);
                }
            }
            return dist;
        }

        /// <summary>
        /// 입구 = 네 모서리(코너) 중 랜덤 하나.
        /// 출구 = 입구에서 가장 먼 "테두리" 칸(바깥으로 문을 낼 수 있도록 테두리로 제한).
        /// 두 칸의 바깥 방향 벽을 뚫어(OpenToOutside) 실제 출입문을 만든다.
        /// </summary>
        public static (MazeCell entrance, MazeCell exit) PlaceEntranceAndExit(
            MazeGrid grid, System.Random rng)
        {
            var corners = grid.Corners().ToList();
            var entrance = corners[rng.Next(corners.Count)];

            var dist = Distances(entrance);
            var exit = grid.EachCell()
                           .Where(c => grid.IsBorder(c) && c != entrance && dist.ContainsKey(c))
                           .OrderByDescending(c => dist[c])
                           .First();

            entrance.OpenToOutside |= OutwardDir(grid, entrance);
            exit.OpenToOutside     |= OutwardDir(grid, exit);
            return (entrance, exit);
        }

        /// <summary> 테두리 칸에서 격자 바깥을 향하는 방향 하나. </summary>
        static Dir OutwardDir(MazeGrid grid, MazeCell c)
        {
            if (c.Z == 0)             return Dir.S;
            if (c.Z == grid.Rows - 1) return Dir.N;
            if (c.X == 0)             return Dir.W;
            if (c.X == grid.Cols - 1) return Dir.E;
            return Dir.None;
        }
    }
}
