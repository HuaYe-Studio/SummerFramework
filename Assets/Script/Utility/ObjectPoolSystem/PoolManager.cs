using System.Collections.Generic;
using Sirenix.OdinInspector;
using Utility.SingletonPatternSystem;
using UnityEngine;
using Utility.LogSystem;


namespace Utility.ObjectPoolSystem
{
    public class PoolManager : MonoSingleton<PoolManager>
    {
        public bool logStatus;
        [LabelText("池对象的父GameObject")] public Transform root;

        private Dictionary<GameObject, ObjectPool<GameObject>> _prefabLookup;
        private Dictionary<GameObject, ObjectPool<GameObject>> _instanceLookup;

        private bool _dirty = false;

        #region 生命周期

        protected override void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            base.Awake();
            _prefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
            _instanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
        }

        private void Update()
        {
            if (!logStatus || !_dirty) return;
            PrintStatus();
            _dirty = false;
        }

        #endregion

        #region Function

        #region 内部方法

        private void WarmPool_(GameObject prefab, int size, Transform parent = null)
        {
            if (_prefabLookup.ContainsKey(prefab))
            {
                LogSystem.LogSystem.Instance.Log($"Pool for prefab {prefab.name} has already been created",
                    LogLevelEnum.Debug);
                // Debug.Log("Pool for prefab " + prefab.name + " has already been created");
            }

            var pool = new ObjectPool<GameObject>(() => InstantiatePrefab(prefab, parent), size);
            _prefabLookup[prefab] = pool;

            _dirty = true;
        }


        private GameObject SpawnObject_(GameObject prefab, Vector3 position = new Vector3(),
            Quaternion rotation = new Quaternion())
        {
            if (!prefab)
                return null;

            if (!_prefabLookup.ContainsKey(prefab))
            {
                WarmPool(prefab, 1);
            }

            var pool = _prefabLookup[prefab];

            var clone = pool.GetItem();
            if (clone == null)
                return null;

            clone.transform.position = position;
            clone.transform.rotation = rotation;
            clone.SetActive(true);

            _instanceLookup.Add(clone, pool);
            _dirty = true;
            return clone;
        }

        private void ReleaseObject_(GameObject clone)
        {
            if (!clone)
                return;

            clone.SetActive(false);

            if (_instanceLookup.ContainsKey(clone))
            {
                _instanceLookup[clone].ReleaseItem(clone);
                _instanceLookup.Remove(clone);
                _dirty = true;
            }
            else
            {
                LogSystem.LogSystem.Instance.Log($"No pool contains the object: {clone.name}", LogLevelEnum.Debug);
            }
        }


        private GameObject InstantiatePrefab(GameObject prefab, Transform parent = null)
        {
            var go = Instantiate(prefab, parent);
            if (root != null) go.transform.SetParent(root, true);
            return go;
        }

        private void PrintStatus()
        {
            foreach (KeyValuePair<GameObject, ObjectPool<GameObject>> keyVal in _prefabLookup)
            {
                LogSystem.LogSystem.Instance.Log(
                    $"Object Pool for Prefab: {keyVal.Key.name} In Use: {keyVal.Value.CountUsedItems} Total {keyVal.Key.name}",
                    LogLevelEnum.Debug);
                // Debug.Log(string.Format("Object Pool for Prefab: {0} In Use: {1} Total {2}", keyVal.Key.name,
                //     keyVal.Value.CountUsedItems, keyVal.Value.Count));
            }
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 预热池
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="size"></param>
        /// <param name="parent"></param>
        private static void WarmPool(GameObject prefab, int size, Transform parent = null)
        {
            Instance.WarmPool_(prefab, size, parent);
        }

        /// <summary>
        /// 从池中实例化一个对象
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static GameObject SpawnObject(GameObject prefab)
        {
            return Instance.SpawnObject_(prefab);
        }

        public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Instance.SpawnObject_(prefab, position, rotation);
        }

        /// <summary>
        /// 释放一个对象
        /// </summary>
        /// <param name="clone"></param>
        public static void ReleaseObject(GameObject clone)
        {
            Instance.ReleaseObject_(clone);
        }

        #endregion

        #endregion
    }
}