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
    [Tooltip("Ban kinh random xung quanh diem spawn de tranh bi chong len nhau")]
    [SerializeField] private float _spawnRadius = 5f; 

    [Header("Ranged Enemy")]
    [Tooltip("Cu spawn 5 enemy can chien thi spawn 1 ban xa")]
    [SerializeField] private int _meleePerRanged = 5;
    [SerializeField] private string _rangedTag = "EnemyRanged";

    [Header("Difficulty Scale")]
    [SerializeField] private float _intervalDecreaseRate = 0.05f;  
    [SerializeField] private float _minSpawnInterval = 0.3f;    

    [Header("Stats Scaling")]
    [SerializeField] private float _statsMultiplier = 1f;
    [SerializeField] private float _statsIncreasePerMinute = 0.15f;
    [SerializeField] private float _statsIncreasePerMinuteLate = 1.5f;
    [SerializeField] private int _lateMinuteThreshold = 10;
    [SerializeField] private float _lateGameBurstMultiplier = 2f;

    private int _currentEnemyCount = 0;
    private int _meleeSpawnCount = 0;
    private bool _hasAppliedLateBurst = false;

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

        Vector3 spawnPos = spawnPoint.position;
        if (_spawnRadius > 0f)
        {
            Vector2 randomCircle = Random.insideUnitCircle * _spawnRadius;
            spawnPos += new Vector3(randomCircle.x, 0f, randomCircle.y);
        }

        bool spawnRanged = _meleeSpawnCount >= _meleePerRanged;
        string tag = spawnRanged ? _rangedTag : "Enemy";

        GameObject enemy = ObjectPool.Instance.SpawnFromPool(tag, spawnPos, spawnPoint.rotation);

        if (enemy != null)
        {
            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            enemyBase?.OnSpawn();
            enemyBase?.ApplyStatsMultiplier(_statsMultiplier);

            EnemyChase enemyChase = enemy.GetComponent<EnemyChase>();
            enemyChase?.ApplyAttackSpeedMultiplier(1f / _statsMultiplier);
            
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
        bool isLate = minute >= _lateMinuteThreshold;

        if (isLate && !_hasAppliedLateBurst)
        {
            _hasAppliedLateBurst = true;
            _statsMultiplier += _lateGameBurstMultiplier;
            _maxEnemyAlive = Mathf.Min(100, _maxEnemyAlive + 15);
            _minSpawnInterval = Mathf.Max(0.15f, _minSpawnInterval - 0.1f);
            _intervalDecreaseRate += 0.05f;
        }
        else
        {
            _maxEnemyAlive = Mathf.Min(100, _maxEnemyAlive + (isLate ? 10 : 5));
            _minSpawnInterval = Mathf.Max(0.15f, _minSpawnInterval - (isLate ? 0.05f : 0.02f));
            _intervalDecreaseRate += isLate ? 0.03f : 0.01f;
        }

        float increase = isLate ? _statsIncreasePerMinuteLate : _statsIncreasePerMinute;
        _statsMultiplier += increase;

        Bullet.EnemySpeedMultiplier = _statsMultiplier;
    }

    public static EnemySpawner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}