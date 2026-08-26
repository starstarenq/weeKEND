using System.Collections.Generic;

namespace ProceduralMaze
{
    /// <summary> Cols(X) × Rows(Z) 크기의 Cell 격자. 이웃을 서로 물려 둔다. </summary>
    public class MazeGrid
    {
        public readonly int Cols, Rows;
        readonly MazeCell[,] cells;

        public MazeGrid(int cols, int rows)
        {
            Cols = cols;
            Rows = rows;
            cells = new MazeCell[cols, rows];

            for (int z = 0; z < rows; z++)
                for (int x = 0; x < cols; x++)
                    cells[x, z] = new MazeCell(x, z);

            // 이웃 연결 (격자 밖은 null로 남는다). N은 z+1 방향.
            for (int z = 0; z < rows; z++)
                for (int x = 0; x < cols; x++)
                {
                    var c = cells[x, z];
                    if (z < rows - 1) c.North = cells[x, z + 1];
                    if (x < cols - 1) c.East  = cells[x + 1, z];
                    if (z > 0)        c.South = cells[x, z - 1];
                    if (x > 0)        c.West  = cells[x - 1, z];
                }
        }

        public MazeCell this[int x, int z] => cells[x, z];

        public bool IsBorder(MazeCell c) =>
            c.X == 0 || c.X == Cols - 1 || c.Z == 0 || c.Z == Rows - 1;

        public IEnumerable<MazeCell> EachCell()
        {
            for (int z = 0; z < Rows; z++)
                for (int x = 0; x < Cols; x++)
                    yield return cells[x, z];
        }

        public IEnumerable<MazeCell> Corners()
        {
            yield return cells[0, 0];
            yield return cells[Cols - 1, 0];
            yield return cells[0, Rows - 1];
            yield return cells[Cols - 1, Rows - 1];
        }
    }
}
