using System;
using UnityEngine;

namespace TTest.UnityUtilities.Pooling
{
    /// <summary>
    /// Inspector-friendly owner for one <see cref="ObjectPool"/>. Place it in a scene,
    /// assign a prefab, then use Spawn and Despawn from game code.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PoolSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform inactivePoolParent;
        [SerializeField, Min(0)] private int prewarmCount = 8;
        [SerializeField, Min(1)] private int maxSize = 128;

        private ObjectPool _pool;

        public GameObject Prefab => prefab;
        public int CountInactive => _pool?.CountInactive ?? 0;
        public bool IsInitialized => _pool != null;

        private void Awake() => Initialize();

        /// <summary>Creates the backing pool. Safe to call more than once.</summary>
        public void Initialize()
        {
            if (_pool != null)
                return;
            if (prefab == null)
                throw new InvalidOperationException($"{nameof(PoolSpawner)} on '{name}' needs a prefab.");

            _pool = new ObjectPool(prefab, $"{prefab.name} Pool", inactivePoolParent != null ? inactivePoolParent : transform,
                prewarmCount, Mathf.Max(prewarmCount, maxSize));
            _pool.Prewarm(prewarmCount);
        }

        public GameObject Spawn(Transform parent = null)
        {
            Initialize();
            return _pool.Spawn(parent);
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            Initialize();
            return _pool.Spawn(position, rotation, parent);
        }

        public T Spawn<T>(Transform parent = null) where T : Component
        {
            Initialize();
            return _pool.Spawn<T>(parent);
        }

        public void Despawn(GameObject instance) => _pool?.Despawn(instance);

        private void OnDestroy() => _pool?.Dispose();
    }
}