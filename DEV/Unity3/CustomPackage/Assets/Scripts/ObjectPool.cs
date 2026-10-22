using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace TTest.UnityUtilities.Pooling
{
    /// <summary>A pool for one prefab. It never relies on prefab names or resource-loading conventions.</summary>
    public class ObjectPool : IDisposable
    {
        private readonly GameObject _prefab;
        private readonly Transform _inactiveRoot;
        private readonly ObjectPool<GameObject> _pool;
        private readonly HashSet<int> _instanceIds = new();
        private bool _disposed;

        public ObjectPool(GameObject prefab, string poolName = null, Transform poolParent = null,
            int defaultCapacity = 8, int maxSize = 128)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            if (defaultCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(defaultCapacity));
            if (maxSize < 1 || maxSize < defaultCapacity)
                throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be at least the default capacity.");

            _prefab = prefab;
            var root = new GameObject(poolName ?? $"{prefab.name} Pool");
            _inactiveRoot = root.transform;
            _inactiveRoot.SetParent(poolParent, false);
            _pool = new ObjectPool<GameObject>(Create, OnGet, OnRelease, OnDestroy, true, defaultCapacity, maxSize);
        }

        /// <summary>Number of instances currently available for reuse.</summary>
        public int CountInactive => _pool.CountInactive;
        /// <summary>Total number of live instances created by this pool.</summary>
        public int CountAll => _pool.CountAll;
        /// <summary>Number of instances currently checked out from this pool.</summary>
        public int CountActive => _pool.CountActive;

        public GameObject Spawn(Transform parent = null)
        {
            ThrowIfDisposed();
            var instance = _pool.Get();
            instance.transform.SetParent(parent, false);
            InvokeSpawned(instance);
            return instance;
        }

        /// <summary>Spawns an instance at a world-space pose.</summary>
        public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var instance = Spawn(parent);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        /// <summary>Spawns and returns a required component on the pooled instance.</summary>
        public T Spawn<T>(Transform parent = null) where T : Component
        {
            var instance = Spawn(parent);
            if (instance.TryGetComponent<T>(out var component))
                return component;

            Despawn(instance);
            throw new InvalidOperationException($"Pooled prefab '{_prefab.name}' does not contain {typeof(T).Name}.");
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null || _disposed)
                return;
            if (!_instanceIds.Contains(instance.GetInstanceID()))
                throw new ArgumentException("This instance was not created by this pool.", nameof(instance));

            _pool.Release(instance);
        }

        public void Prewarm(int count)
        {
            ThrowIfDisposed();
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var instances = new GameObject[count];
            for (var index = 0; index < count; index++)
                instances[index] = Spawn();
            for (var index = 0; index < count; index++)
                Despawn(instances[index]);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _pool.Clear();
            if (_inactiveRoot != null)
                UnityEngine.Object.Destroy(_inactiveRoot.gameObject);
        }

        private GameObject Create()
        {
            var instance = UnityEngine.Object.Instantiate(_prefab);
            instance.name = _prefab.name;
            _instanceIds.Add(instance.GetInstanceID());
            instance.SetActive(false);
            return instance;
        }

        private static void OnGet(GameObject instance)
        {
            instance.SetActive(true);
        }

        private void OnRelease(GameObject instance)
        {
            InvokeDespawned(instance);
            instance.SetActive(false);
            instance.transform.SetParent(_inactiveRoot, false);
        }

        private void OnDestroy(GameObject instance)
        {
            _instanceIds.Remove(instance.GetInstanceID());
            UnityEngine.Object.Destroy(instance);
        }

        private static void InvokeSpawned(GameObject instance)
        {
            foreach (var behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour is IPoolable poolable)
                    poolable.OnPoolSpawned();
        }

        private static void InvokeDespawned(GameObject instance)
        {
            foreach (var behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour is IPoolable poolable)
                    poolable.OnPoolDeSpawned();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ObjectPool));
        }
    }

    /// <summary>
    /// Compatibility name for versions of this package before ObjectPool was introduced.
    /// New code should use <see cref="ObjectPool"/>.
    /// </summary>
    public sealed class GameObjectPool : ObjectPool
    {
        public GameObjectPool(GameObject prefab, string poolName = null, Transform poolParent = null,
            int defaultCapacity = 8, int maxSize = 128)
            : base(prefab, poolName, poolParent, defaultCapacity, maxSize)
        {
        }
    }
}