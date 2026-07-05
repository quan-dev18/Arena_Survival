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

    [Header("Ranged Enemy")]
    [Tooltip("Cu spawn 5 enemy can chien thi spawn 1 ban xa")]
    [SerializeField] private int _meleePerRanged = 5;
    [SerializeField] private string _rangedTag = "EnemyRanged";

    [Header("Difficulty Scale")]
    [SerializeField] private float _intervalDecreaseRate = 0.05f;  
    [SerializeField] private float _minSpawnInterval = 0.3f;    

    private int _currentEnemyCount = 0;
    private int _meleeSpawnCount = 0;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);

            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                continue;

            if (_currentEnemyCount < _maxEnemyAlive)
                SpawnEnemy();

            float difficulty = GameManager.Instance.DifficultyMultiplier;

            float scaledInterval = _spawnInterval - _intervalDecreaseRate * difficulty;
            _spawnInterval = Mathf.Max(_minSpawnInterval, scaledInterval);
        }
    }

    private void SpawnEnemy()
    {
        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

        bool spawnRanged = _meleeSpawnCount >= _meleePerRanged;
        string tag = spawnRanged ? _rangedTag : "Enemy";

        GameObject enemy = ObjectPool.Instance.SpawnFromPool(tag, spawnPoint.position, spawnPoint.rotation);

        if (enemy != null)
        {
            enemy.GetComponent<EnemyBase>()?.OnSpawn();
            _currentEnemyCount++;

            if (spawnRanged)
                _meleeSpawnCount = 0;
            else
                _meleeSpawnCount++;
        }
    }

    // Gọi từ EnemyChase khi Die()
    public void OnEnemyDied()
    {
        _currentEnemyCount--;
    }

    public void OnTimeEvent(int minute)
    {
        _maxEnemyAlive = Mathf.Min(100, _maxEnemyAlive + 5);
        _minSpawnInterval = Mathf.Max(0.15f, _minSpawnInterval - 0.02f);
        _intervalDecreaseRate += 0.01f;
    }

    public static EnemySpawner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}