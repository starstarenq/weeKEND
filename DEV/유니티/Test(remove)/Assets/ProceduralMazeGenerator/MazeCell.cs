using System.Collections.Generic;

namespace ProceduralMaze
{
    /// <summary>
    /// 미로의 한 칸(=방). 이웃 넷을 참조로 알고, 뚫린 이웃만 links에 담는다.
    /// (Mazes for Programmers의 Cell 구조를 4방향 격자에 맞춘 것)
    /// </summary>
    public class MazeCell
    {
        public readonly int X, Z;

        public MazeCell North, East, South, West;

        readonly HashSet<MazeCell> links = new HashSet<MazeCell>();

        /// <summary>
        /// 이웃이 아닌(격자 바깥으로 통하는) 추가 열림. 입구·출구의 바깥 문에 사용.
        /// </summary>
        public Dir OpenToOutside = Dir.None;

        public MazeCell(int x, int z)
        {
            X = x;
            Z = z;
        }

        /// <summary> 두 칸을 서로의 links에 넣어 양방향 문을 만든다. </summary>
        public void Link(MazeCell other, bool bidirectional = true)
        {
            if (other == null) return;
            links.Add(other);
            if (bidirectional) other.Link(this, false);
        }

        public bool Linked(MazeCell other) => other != null && links.Contains(other);

        public IEnumerable<MazeCell> Links() => links;

        public MazeCell Neighbor(Dir d)
        {
            switch (d)
            {
                case Dir.N: return North;
                case Dir.E: return East;
                case Dir.S: return South;
                case Dir.W: return West;
                default:    return null;
            }
        }

        public IEnumerable<MazeCell> Neighbors()
        {
            if (North != null) yield return North;
            if (East  != null) yield return East;
            if (South != null) yield return South;
            if (West  != null) yield return West;
        }

        /// <summary>
        /// 이 칸이 실제로 열린 방향 마스크.
        /// (링크된 이웃 방향) + (바깥으로 뚫은 방향).
        /// 이 마스크가 곧 RoomClassifier로 방 종류를 결정한다.
        /// </summary>
        public Dir OpenMask()
        {
            Dir open = OpenToOutside;
            foreach (var d in DirExt.All)
                if (Linked(Neighbor(d)))
                    open |= d;
            return open;
        }
    }
}
