# Procedural Maze Generator (Unity Editor Tool)

X·Z 규격의 Cell 격자 위에 미로 알고리즘으로 **입구·출구가 있는 미로**를 생성하고,
각 칸의 열린 방향을 **5종 방**으로 자동 분류·회전해 프리팹(또는 임시 큐브)으로 배치하는 에디터 툴이다.

> Unity 프로젝트의 `Assets/` 아래에 이 폴더를 통째로 복사해 넣으면 된다.
> `Editor/` 하위 스크립트만 에디터 전용 어셈블리로 컴파일되고, 나머지는 런타임에서도 쓸 수 있다.
> (Unity의 "Editor 폴더" 규칙을 사용하므로 별도 asmdef가 필요 없다.)

---

## 5가지 방 종류 (열린 방향 기준)

| RoomType | 뚫린 방향 수 | 설명 | 기준(회전 0) 열림 |
|---|---|---|---|
| `DeadEnd`   | 1 | 막다른 방 (한 방향만) | `N` |
| `Straight`  | 2 | 일자 방 (마주보는 2방향) | `N + S` |
| `Corner`    | 2 | ㄱ자 방 (인접한 2방향) | `N + E` |
| `TJunction` | 3 | T자 방 (한쪽만 막힘) | `N + E + S` (서쪽 막힘) |
| `Cross`     | 4 | 십자 방 (사방) | `N + E + S + W` |

완전 미로(스패닝 트리)의 **모든 칸은 이 5종 중 정확히 하나**에 대응된다.
`RoomClassifier.Classify(open, out type, out rotSteps)` 가 열린 방향 마스크를
`(방 종류, 90도 회전 스텝)` 으로 변환한다. 방향은 전적으로 **미로 알고리즘**이 정한다.

---

## 프리팹 제작 규칙 (모듈 3×3 규격)

각 프리팹은 **3×3 통로 모듈**이다. 한 변이 `Cell Size` 유닛(현재 프리팹 실측 = **3유닛**)이고,
**열린 변의 문은 항상 그 변의 가운데**에 온다. 그래서 이웃 모듈을 나란히 붙이면 가운데 문끼리 정확히 맞물린다.

프리팹은 **회전 0 기준**으로 위 표의 "기준 열림" 방향이 뚫려 있게 만든다.
- 좌표: `N = +Z`, `E = +X`, `S = -Z`, `W = -X`
- 예) `Corner` 프리팹은 **북(+Z)과 동(+X)** 이 열려 있어야 한다.
- 제작 방향이 기준과 다르면 `RoomModule.rotationOffsetSteps`(90도 단위)로 보정한다.

**피벗 위치는 자유롭다.** 툴이 프리팹의 Renderer 바운드로 기하 중심을 계산해,
모서리에 피벗이 있어도(현재 프리팹은 피벗이 모서리 큐브에 있다) 중심을 칸 좌표에 맞춰 배치한다.
회전은 `Quaternion.Euler(0, 90 * (rotSteps + offset), 0)` 로 **중심 기준**으로 돌린다.

> **칸 간격(Cell Size)은 모듈 한 변 크기와 반드시 같아야** 통로가 끊기지 않는다.
> `Cell Size 자동` 을 켜면 대표 프리팹의 실측 XZ 크기를 그대로 칸 간격으로 쓴다(권장).

---

## 사용법

1. 이 폴더를 Unity 프로젝트 `Assets/` 아래로 복사.
2. 기본 제공 `MazeRoomSet` 에셋에 5종 프리팹(DeadEnd·Straight·Corner·T·Cross)이 **이미 연결**되어 있다.
   - 직접 만들려면 **Create ▸ Procedural Maze ▸ Room Set**. 각 종류 슬롯에 프리팹을 연결.
   - 프리팹을 비워 두면 임시 큐브로 대체된다(구조만 먼저 확인 가능).
3. **Tools ▸ Procedural Maze ▸ Generator Window** 로 창을 연다.
4. `X (Cols)` · `Z (Rows)` 를 직접 입력하거나 `5x5 / 10x10 / 20x20` 프리셋 클릭.
   `Cell Size 자동` 을 켜면 프리팹 실측 크기(=모듈 3유닛)를 칸 간격으로 자동 사용한다.
5. `Room Set` 을 지정(없으면 임시 큐브). 창 안에서 배열을 바로 편집할 수도 있다.
6. **Generate** 클릭 → Scene에 `Maze_{X}x{Z}_{seed}` 루트 아래로 방들이 배치된다.
   - Console에 방 종류별 개수 / 입구 / 출구 좌표가 로그된다.
   - **Ctrl+Z** 한 번으로 전체 되돌리기.

같은 `Seed` 는 같은 미로를 재현한다. 알고리즘은 `RecursiveBacktracker`(기본, 구불구불)와
`BinaryTree`(단순) 중 선택.

---

## 입구 / 출구 로직

- **입구**: 격자 네 모서리(코너) 중 **랜덤** 하나.
- **출구**: 입구에서 **가장 먼 테두리 칸** (Dijkstra=BFS 거리 최대). 바깥으로 문을 낼 수 있도록 테두리로 제한.
- 두 칸은 격자 바깥 방향 벽을 뚫어(`OpenToOutside`) 실제 출입문을 만든다.
  → 그래서 입구·출구 칸의 방 종류는 바깥 문까지 반영해 분류된다.

---

## 파일 구성

| 파일 | 역할 | 어셈블리 |
|---|---|---|
| `Direction.cs` | 4방향 비트플래그 + 회전/개수 헬퍼 | 런타임 |
| `RoomType.cs` | 5종 enum + `RoomClassifier` (마스크→종류·회전) | 런타임 |
| `MazeCell.cs` | 칸: 이웃 참조 + links + OpenMask | 런타임 |
| `MazeGrid.cs` | Cols×Rows 격자, 이웃 연결 | 런타임 |
| `MazeAlgorithms.cs` | RecursiveBacktracker / BinaryTree / Distances / 입구·출구 | 런타임 |
| `MazeRoomSet.cs` | 5종 프리팹 직렬화 배열 (ScriptableObject) | 런타임 |
| `Editor/MazeGeneratorWindow.cs` | 창 UI + 분류·회전 배치 + Undo | **Editor** |

---

## 확장 (2층 / 지하 / 계단)

- 층은 `y = floor * floorHeight` 로 쌓는다. 배치 루프에 `floor` 파라미터를 추가하고
  같은 격자를 재사용하면 다층이 된다.
- 출구(가장 먼 칸)나 특정 칸을 **계단 Cell** 로 지정해 위층·아래층과 연결한다.
- `RoomType` 에 계단 종류를 추가하고 `MazeRoomSet` 배열에 슬롯을 늘리면 확장 가능.
