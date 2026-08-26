using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MazeEditorWindow : EditorWindow
{
    private int width = 10;
    private int height = 10;
    private float cellSize = 1f;

    // 단축키 Ctrl + Alt + O로 윈도우 열기
    [MenuItem("Tools/Maze Generator %&o")]
    public static void ShowWindow()
    {
        GetWindow<MazeEditorWindow>("Maze Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Kruskal Maze Generator", EditorStyles.boldLabel);

        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        cellSize = EditorGUILayout.FloatField("Cell Size", cellSize);

        if (GUILayout.Button("Generate Maze"))
        {
            GenerateMaze();
        }
    }

    // 미로 생성 핵심 로직
    private void GenerateMaze()
    {
        if (width <= 0 || height <= 0) return;

        // 1. 임시 큐브들을 담을 부모 오브젝트 생성
        GameObject mazeRoot = new GameObject("Generated_Maze");
        Undo.RegisterCreatedObjectUndo(mazeRoot, "Generate Maze");

        // 2. 셀(방) 데이터 배열 초기화 및 시작 입구 설정
        Cell[,] grid = new Cell[width, height];
        List<Edge> edges = new List<Edge>();
        DisjointSet ds = new DisjointSet(width * height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int id = y * width + x;
                grid[x, y] = new Cell(x, y, id);

                // 간선(벽) 리스트 추가 (오른쪽, 위쪽 연결성 검사용)
                if (x < width - 1) edges.Add(new Edge(id, id + 1, Direction.East));
                if (y < height - 1) edges.Add(new Edge(id, id + width, Direction.North));
            }
        }

        // 입구 방 설정 (0,0) - 시작점 표시
        grid[0, 0].isEntrance = true;

        // 3. 크루스칼 알고리즘 (간선 무작위 셔플 후 연결)
        Shuffle(edges);

        foreach (Edge edge in edges)
        {
            if (ds.Find(edge.cellA) != ds.Find(edge.cellB))
            {
                ds.Union(edge.cellA, edge.cellB);

                // 두 셀 사이의 벽을 허묾 (길을 연결)
                int ax = edge.cellA % width;
                int ay = edge.cellA / width;
                int bx = edge.cellB % width;
                int by = edge.cellB / width;

                if (edge.dir == Direction.East)
                {
                    grid[ax, ay].east = true;
                    grid[bx, by].west = true;
                }
                else if (edge.dir == Direction.North)
                {
                    grid[ax, ay].north = true;
                    grid[bx, by].south = true;
                }
            }
        }

        // 4. 셀의 연결 상태(벽의 유무)에 따라 5가지 방 종류 결정 및 임시 큐브 배치
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateCellRoom(grid[x, y], mazeRoot.transform);
            }
        }

        // 계층 구조 갱신
        Selection.activeObject = mazeRoot;
    }

    // 방 종류 분석 및 임시 큐브 생성
    private void CreateCellRoom(Cell cell, Transform parent)
    {
        Vector3 position = new Vector3(cell.x * cellSize, 0, cell.y * cellSize);

        // 유니티 내장 기본 큐브(프리펩 아님)로 1x1 한 칸 생성
        GameObject roomObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roomObj.transform.position = position;
        roomObj.transform.localScale = new Vector3(cellSize * 0.9f, 0.2f, cellSize * 0.9f); // 구분을 위해 약간 여백을 둠
        roomObj.transform.parent = parent;

        // 연결된 통로 개수 계산
        int connectionCount = 0;
        if (cell.north) connectionCount++;
        if (cell.south) connectionCount++;
        if (cell.east) connectionCount++;
        if (cell.west) connectionCount++;

        string roomType = "Unknown";
        Color roomColor = Color.white;

        // 5가지 방 종류 판별 알고리즘
        if (cell.isEntrance)
        {
            roomType = "Entrance (입구)";
            roomColor = Color.green; // 입구는 초록색
        }
        else if (connectionCount == 1)
        {
            roomType = "Dead End (막다른 길)";
            roomColor = Color.red;
        }
        else if (connectionCount == 2)
        {
            // 마주보는 방향이 모두 뚫려있으면 직선 통로, 아니면 꺾임 방
            if ((cell.north && cell.south) || (cell.east && cell.west))
            {
                roomType = "Straight (직선 통로)";
                roomColor = Color.gray;
            }
            else
            {
                roomType = "Corner (꺾임 방)";
                roomColor = Color.cyan;
            }
        }
        else if (connectionCount == 3)
        {
            roomType = "T-Shape (T자형 방)";
            roomColor = Color.blue;
        }
        else if (connectionCount == 4)
        {
            roomType = "Crossroad (십자가 방)";
            roomColor = Color.magenta;
        }

        // 오브젝트 이름 변경 및 에디터 뷰 구분을 위한 임시 컬러 적용
        roomObj.name = $"Room_{cell.x}_{cell.y} [{roomType}]";

        Renderer renderer = roomObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = new Material(Shader.Find("Standard"));
            renderer.sharedMaterial.color = roomColor;
        }
    }

    // 리스트 무작위 셔플
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    // --- 내부 데이터 구조체 정의 ---
    private enum Direction { North, East }

    private class Cell
    {
        public int x, y, id;
        public bool north = false, south = false, east = false, west = false;
        public bool isEntrance = false;

        public Cell(int x, int y, int id)
        {
            this.x = x;
            this.y = y;
            this.id = id;
        }
    }

    private class Edge
    {
        public int cellA, cellB;
        public Direction dir;

        public Edge(int a, int b, Direction d)
        {
            cellA = a;
            cellB = b;
            dir = d;
        }
    }

    // 크루스칼 알고리즘용 DisjointSet (서로소 집합)
    private class DisjointSet
    {
        private int[] parent;

        public DisjointSet(int size)
        {
            parent = new int[size];
            for (int i = 0; i < size; i++) parent[i] = i;
        }

        public int Find(int i)
        {
            if (parent[i] == i) return i;
            return parent[i] = Find(parent[i]);
        }

        public void Union(int i, int j)
        {
            int rootI = Find(i);
            int rootJ = Find(j);
            if (rootI != rootJ) parent[rootI] = rootJ;
        }
    }
}
