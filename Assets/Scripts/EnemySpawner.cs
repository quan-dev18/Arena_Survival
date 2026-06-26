using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Khoang thoi gian moi lan spawn")]
    [SerializeField] private float _spawnInterval = 2f;     
    [Tooltip("Toi da bao nhieu Enemy cung luc")]
    [SerializeField] private int _maxEnemyAlive = 20; 
    [Tooltip("Cac diem spawn")]       
    [SerializeField] private Transform[] _spawnPoints;  

    [Header("Difficulty Scale")]
    [SerializeField] private float _intervalDecreaseRate = 0.05f;  // Mỗi lần spawn giảm interval
    [SerializeField] private float _minSpawnInterval = 0.3f;       // Giới hạn tối thiểu

    private int _currentEnemyCount = 0;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);

            if (_currentEnemyCount < _maxEnemyAlive)
                SpawnEnemy();

            // Tăng dần độ khó
            _spawnInterval = Mathf.Max(_minSpawnInterval, _spawnInterval - _intervalDecreaseRate);
        }
    }

    private void SpawnEnemy()
    {
        // Chọn random spawn point
        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

        GameObject enemy = ObjectPool.Instance.SpawnFromPool("Enemy", spawnPoint.position, spawnPoint.rotation);

        if (enemy != null)
        {
            enemy.GetComponent<EnemyChase>()?.OnSpawn();
            _currentEnemyCount++;
        }
    }

    // Gọi từ EnemyChase khi Die()
    public void OnEnemyDied()
    {
        _currentEnemyCount--;
    }

    public static EnemySpawner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}