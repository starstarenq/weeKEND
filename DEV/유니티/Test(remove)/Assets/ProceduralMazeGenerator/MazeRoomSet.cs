using System;
using UnityEngine;

namespace ProceduralMaze
{
    /// <summary>
    /// 방 한 종류에 대한 직렬화 항목. Inspector/EditorWindow에서 프리팹을 연결한다.
    /// </summary>
    [Serializable]
    public class RoomModule
    {
        [Tooltip("이 항목이 담당하는 방 종류")]
        public RoomType type;

        [Tooltip("배치할 프리팹. 회전 0 기준으로 RoomClassifier.Canonical(type) 방향이 뚫려 있어야 한다.\n" +
                 "DeadEnd=북, Straight=남북, Corner=북동, TJunction=서쪽막힘, Cross=사방")]
        public GameObject prefab;

        [Tooltip("프리팹 제작 방향이 기준과 다를 때 90도 단위로 보정")]
        [Range(0, 3)]
        public int rotationOffsetSteps = 0;
    }

    /// <summary>
    /// 5가지 방 프리팹을 담는 에셋. "Editor의 배열 기능"으로 직렬화되어
    /// Inspector에서 배열 형태로 프리팹을 지정할 수 있다.
    /// Create ▸ Procedural Maze ▸ Room Set 으로 생성.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Maze/Room Set", fileName = "MazeRoomSet")]
    public class MazeRoomSet : ScriptableObject
    {
        [Tooltip("5가지 방 타입별 프리팹 (직렬화 배열)")]
        [SerializeField]
        public RoomModule[] modules = CreateDefault();

        /// <summary> 5종 슬롯을 기본값으로 채운 배열. </summary>
        public static RoomModule[] CreateDefault()
        {
            var values = (RoomType[])Enum.GetValues(typeof(RoomType));
            var arr = new RoomModule[values.Length];
            for (int i = 0; i < values.Length; i++)
                arr[i] = new RoomModule { type = values[i] };
            return arr;
        }

        /// <summary> 해당 방 종류의 모듈을 찾는다(없으면 null). </summary>
        public RoomModule Find(RoomType t)
        {
            if (modules == null) return null;
            foreach (var m in modules)
                if (m != null && m.type == t)
                    return m;
            return null;
        }

        /// <summary> 5종이 모두 프리팹까지 채워져 있는지. </summary>
        public bool IsComplete()
        {
            foreach (RoomType t in Enum.GetValues(typeof(RoomType)))
            {
                var m = Find(t);
                if (m == null || m.prefab == null) return false;
            }
            return true;
        }

        void Reset()
        {
            modules = CreateDefault();
        }
    }
}
