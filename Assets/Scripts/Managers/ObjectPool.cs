using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> _poolDict;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _poolDict = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            var queue = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                var obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            _poolDict[pool.tag] = queue;
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!_poolDict.TryGetValue(tag, out var queue))
        {
            Debug.LogWarning($"Pool '{tag}' không tồn tại!");
            return null;
        }

        // Tìm object đang không active
        int count = queue.Count;
        for (int i = 0; i < count; i++)
        {
            var obj = queue.Dequeue();
            queue.Enqueue(obj);
            if (!obj.activeInHierarchy)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
                return obj;
            }
        }

        // Pool hết → tạo thêm
        var poolData = pools.Find(p => p.tag == tag);
        var newObj = Instantiate(poolData.prefab, position, rotation, transform);
        queue.Enqueue(newObj);
        return newObj;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        obj.SetActive(false);
    }
}