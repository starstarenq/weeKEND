using UnityEditor;
using UnityEngine;

namespace ProceduralMaze.EditorTools
{
    /// <summary>
    /// 절차적 미로 건물 생성기.
    /// X·Z 칸 수를 입력받아 미로를 생성하고, 각 칸의 열린 방향을 5종 방(막다른/일자/ㄱ자/T자/십자)으로
    /// 분류해 프리팹(또는 임시 큐브)을 회전·배치한다. 입구·출구는 바깥으로 문이 뚫린다.
    ///
    /// 메뉴: Tools ▸ Procedural Maze ▸ Generator Window
    /// </summary>
    public class MazeGeneratorWindow : EditorWindow
    {
        // ── 규격 ──
        int cols = 10;
        int rows = 10;
        // 모듈 프리팹은 3×3 통로(가운데 입구) 규격이라 한 칸 = 3유닛이 기본값이다.
        // autoCellSize가 켜져 있으면 실제 프리팹의 XZ 크기를 재서 이 값을 덮어쓴다.
        float cellSize = 3f;
        bool autoCellSize = true;   // 프리팹 실측 크기를 Cell Size로 사용
        float wallHeight = 3f;
        int seed = 12345;

        float step; // 이번 Generate에서 확정된 한 칸(모듈) 간격. 임시 큐브 크기에도 쓴다.

        enum Algorithm { RecursiveBacktracker, BinaryTree }
        Algorithm algorithm = Algorithm.RecursiveBacktracker;

        // ── 방 프리팹 세트 (직렬화 배열) ──
        MazeRoomSet roomSet;
        bool usePlaceholderWhenMissing = true;
        SerializedObject roomSetSO;

        [MenuItem("Tools/Procedural Maze/Generator Window")]
        static void Open()
        {
            var w = GetWindow<MazeGeneratorWindow>("Maze Generator");
            w.minSize = new Vector2(340, 480);
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("규격 (Cell 단위)", EditorStyles.boldLabel);
            cols = Mathf.Max(2, EditorGUILayout.IntField("X (Cols)", cols));
            rows = Mathf.Max(2, EditorGUILayout.IntField("Z (Rows)", rows));

            autoCellSize = EditorGUILayout.Toggle(
                new GUIContent("Cell Size 자동", "프리팹의 실제 XZ 크기(모듈 한 칸)를 재서 Cell Size로 사용"),
                autoCellSize);

            // 잴 수 있는 프리팹이 있을 때만 자동값을 보여주고 필드를 잠근다.
            bool measuring = autoCellSize && MeasuredCellSize() > 0f;
            using (new EditorGUI.DisabledScope(measuring))
            {
                float shown = measuring ? MeasuredCellSize() : cellSize;
                float edited = Mathf.Max(0.1f, EditorGUILayout.FloatField("Cell Size", shown));
                if (!measuring) cellSize = edited;   // 자동 측정 중이 아닐 때만 반영
            }
            wallHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Wall Height (임시 큐브)", wallHeight));
            seed = EditorGUILayout.IntField("Seed", seed);
            algorithm = (Algorithm)EditorGUILayout.EnumPopup("Algorithm", algorithm);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("5 x 5"))   { cols = rows = 5; }
            if (GUILayout.Button("10 x 10")) { cols = rows = 10; }
            if (GUILayout.Button("20 x 20")) { cols = rows = 20; }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("방 프리팹 세트 (5종)", EditorStyles.boldLabel);
            roomSet = (MazeRoomSet)EditorGUILayout.ObjectField(
                "Room Set", roomSet, typeof(MazeRoomSet), false);
            usePlaceholderWhenMissing =
                EditorGUILayout.Toggle("프리팹 없으면 임시 큐브", usePlaceholderWhenMissing);

            if (roomSet != null)
            {
                // 직렬화 배열을 창 안에서 그대로 편집 (Editor 배열 기능)
                if (roomSetSO == null || roomSetSO.targetObject != roomSet)
                    roomSetSO = new SerializedObject(roomSet);

                roomSetSO.Update();
                EditorGUILayout.PropertyField(
                    roomSetSO.FindProperty("modules"), new GUIContent("Rooms"), true);
                roomSetSO.ApplyModifiedProperties();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Create ▸ Procedural Maze ▸ Room Set 으로 에셋을 만들어 지정하세요.\n" +
                    "지정하지 않으면 임시 큐브로 미로를 생성합니다.", MessageType.Info);
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(false))
            {
                if (GUILayout.Button("Generate", GUILayout.Height(34)))
                    Generate();
            }

            EditorGUILayout.HelpBox(
                "생성 후 Ctrl+Z 한 번으로 전체 되돌리기 가능. " +
                "각 칸은 열린 방향 개수/배치에 따라 5종 방으로 자동 분류·회전됩니다.",
                MessageType.None);
        }

        void Generate()
        {
            // 0) 한 칸(모듈) 크기 확정. 프리팹이 있으면 실측 크기를 써서 칸 간격을 정확히 맞춘다.
            //    (모듈 3×3 프리팹의 XZ 크기 = 칸 간격이어야 이웃 통로가 어긋나지 않는다)
            step = autoCellSize ? MeasuredCellSize() : 0f;
            if (step <= 0f) step = cellSize;

            // 1) 미로 데이터 생성 — 방향은 알고리즘이 결정한다
            var rng = new System.Random(seed);
            var grid = new MazeGrid(cols, rows);

            switch (algorithm)
            {
                case Algorithm.BinaryTree:
                    MazeAlgorithms.BinaryTree(grid, rng);
                    break;
                default:
                    MazeAlgorithms.RecursiveBacktracker(grid, rng);
                    break;
            }

            var (entrance, exit) = MazeAlgorithms.PlaceEntranceAndExit(grid, rng);

            // 2) 루트 오브젝트 + Undo 그룹
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();

            var root = new GameObject($"Maze_{cols}x{rows}_{seed}");
            Undo.RegisterCreatedObjectUndo(root, "Generate Maze");

            // 3) 모든 칸을 5종 방으로 분류해 배치
            int placed = 0, unclassified = 0;
            var counts = new int[5];

            foreach (var cell in grid.EachCell())
            {
                Dir open = cell.OpenMask();
                if (!RoomClassifier.Classify(open, out RoomType type, out int rotSteps))
                {
                    unclassified++;
                    continue;
                }
                counts[(int)type]++;

                // 모듈을 원점·무회전으로 만든 뒤, 기하 중심 기준으로 회전·정렬한다.
                GameObject go = BuildRoom(type, out int offsetSteps, out bool rotatable, open);
                if (go == null) continue;

                float yaw = rotatable ? 90f * (rotSteps + offsetSteps) : 0f;
                Quaternion rot = Quaternion.Euler(0f, yaw, 0f);

                // 프리팹 피벗이 모서리에 있어도 중심을 칸 좌표에 맞춘다:
                // world_center = cellCenter 가 되도록 position 을 역산.
                Vector3 localCenter = LocalCenterXZ(go);        // 원점·무회전 상태에서의 XZ 중심
                Vector3 cellCenter = new Vector3(cell.X * step, 0f, cell.Z * step);

                go.transform.rotation = rot;
                go.transform.position = cellCenter - rot * new Vector3(localCenter.x, 0f, localCenter.z);

                bool isEntrance = cell == entrance;
                bool isExit = cell == exit;
                string tag = isEntrance ? "_ENTRANCE" : (isExit ? "_EXIT" : "");
                go.name = $"Cell_{cell.X}_{cell.Z}_{type}{tag}";

                Undo.RegisterCreatedObjectUndo(go, "Place Room");
                go.transform.SetParent(root.transform, true);

                placed++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = root;

            Debug.Log(
                $"[Maze] {cols}x{rows} 생성 완료 · 방 {placed}개 " +
                $"(막다른 {counts[0]}, 일자 {counts[1]}, ㄱ자 {counts[2]}, T자 {counts[3]}, 십자 {counts[4]}) · " +
                $"입구 ({entrance.X},{entrance.Z}) · 출구 ({exit.X},{exit.Z}) · " +
                (unclassified > 0 ? $"미분류 {unclassified}" : "미분류 0"),
                root);
        }

        /// <summary>
        /// 방 종류로 오브젝트를 만든다(원점·무회전 상태로 반환). 회전·정렬은 호출부에서 한다.
        /// - 프리팹이 있으면 인스턴스화하고 rotatable=true, offsetSteps=모듈 보정값.
        /// - 없고 임시허용이면 open 마스크로 큐브 조립(이미 방향 반영 → rotatable=false).
        /// </summary>
        GameObject BuildRoom(RoomType type, out int offsetSteps, out bool rotatable, Dir open)
        {
            offsetSteps = 0;
            rotatable = false;

            RoomModule module = roomSet != null ? roomSet.Find(type) : null;

            if (module != null && module.prefab != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(module.prefab);
                go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                offsetSteps = module.rotationOffsetSteps;
                rotatable = true;
                return go;
            }

            if (usePlaceholderWhenMissing)
                return BuildPlaceholder(open, type); // 이미 open 방향대로 조립됨 → 추가 회전 없음

            return null;
        }

        /// <summary>
        /// Cell Size 자동 측정용 대표 프리팹(첫 번째로 지정된 것). 없으면 null.
        /// </summary>
        GameObject ModulePrefabForMeasure()
        {
            if (roomSet == null || roomSet.modules == null) return null;
            foreach (var m in roomSet.modules)
                if (m != null && m.prefab != null) return m.prefab;
            return null;
        }

        // 대표 프리팹의 실측 크기 캐시(같은 프리팹이면 재계산하지 않는다).
        GameObject measuredPrefab;
        float measuredSize;

        /// <summary>
        /// 대표 프리팹의 XZ 실측 크기(= 모듈 한 칸 간격). 프리팹이 없으면 0.
        /// 씬에 인스턴스화하지 않고 프리팹 에셋의 Mesh 바운드로 잰다(OnGUI에서 매 프레임 호출해도 안전).
        /// </summary>
        float MeasuredCellSize()
        {
            var prefab = ModulePrefabForMeasure();
            if (prefab == null) return 0f;
            if (prefab != measuredPrefab)
            {
                measuredPrefab = prefab;
                measuredSize = PrefabMeshBounds(prefab, out Bounds b) ? Mathf.Max(b.size.x, b.size.z) : 0f;
            }
            return measuredSize;
        }

        /// <summary>원점·무회전 상태의 인스턴스에서 XZ 기하 중심(모서리 피벗 보정용).</summary>
        static Vector3 LocalCenterXZ(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return Vector3.zero;
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds.center;
        }

        /// <summary>
        /// 프리팹 에셋(미인스턴스화)의 자식 Mesh 를 루트 로컬 공간에서 합친 바운드.
        /// 각 MeshFilter.sharedMesh.bounds 를 루트 기준 행렬로 변환해 8꼭짓점을 감싼다.
        /// </summary>
        static bool PrefabMeshBounds(GameObject prefab, out Bounds bounds)
        {
            bounds = default;
            bool has = false;
            Matrix4x4 rootInv = prefab.transform.worldToLocalMatrix;

            foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = mf.sharedMesh;
                if (mesh == null) continue;

                Matrix4x4 toRoot = rootInv * mf.transform.localToWorldMatrix;
                Vector3 c = mesh.bounds.center, e = mesh.bounds.extents;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = c + new Vector3(
                        (i & 1) == 0 ? -e.x : e.x,
                        (i & 2) == 0 ? -e.y : e.y,
                        (i & 4) == 0 ? -e.z : e.z);
                    Vector3 p = toRoot.MultiplyPoint3x4(corner);
                    if (!has) { bounds = new Bounds(p, Vector3.zero); has = true; }
                    else bounds.Encapsulate(p);
                }
            }
            return has;
        }

        /// <summary>
        /// 프리팹이 없을 때: 열린 방향 마스크를 그대로 반영한 임시 방.
        /// 바닥 + 닫힌 방향에만 벽 큐브(열린 방향은 통로). 회전 불필요.
        /// </summary>
        GameObject BuildPlaceholder(Dir open, RoomType type)
        {
            var room = new GameObject("Room");

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(room.transform, false);
            floor.transform.localScale = new Vector3(step, 0.1f, step);
            floor.transform.localPosition = new Vector3(0f, -0.05f, 0f);

            float t = Mathf.Max(0.1f, step * 0.08f); // 벽 두께
            foreach (var d in DirExt.All)
            {
                if ((open & d) != 0) continue; // 열린 방향 → 통로(벽 없음)

                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Wall_{d}";
                wall.transform.SetParent(room.transform, false);

                // 기본은 X를 따라 뻗는 얇은 벽(=N/S). E/W는 90도 돌려 Z를 따라 뻗게.
                bool alongZ = (d == Dir.E || d == Dir.W);
                wall.transform.localScale = alongZ
                    ? new Vector3(t, wallHeight, step)
                    : new Vector3(step, wallHeight, t);

                Vector3 edge = new Vector3(d.Dx(), 0f, d.Dz()) * (step * 0.5f);
                edge.y = wallHeight * 0.5f;
                wall.transform.localPosition = edge;
            }

            return room;
        }
    }
}
