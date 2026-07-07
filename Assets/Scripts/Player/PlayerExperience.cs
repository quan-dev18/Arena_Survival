using System.Collections.Generic;
using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [Header("Collection Zone")]
    [SerializeField] private float _collectionRadius = 5f;
    [SerializeField] private float _attractSpeed = 8f;
    [SerializeField] private float _collectDistance = 0.5f;

    [Header("Level Settings")]
    [SerializeField] private int _baseXPToLevel = 200;
    [SerializeField] private AnimationCurve _levelCurve = new AnimationCurve(
        new Keyframe(1, 1),
        new Keyframe(10, 2f),
        new Keyframe(25, 5f),
        new Keyframe(50, 15f),
        new Keyframe(75, 40f),
        new Keyframe(100, 80f)
    );

    private float _currentXP;
    private int _currentLevel = 1;
    private float _xpToNextLevel;

    private SphereCollider _collectionZone;
    private List<ExpOrb> _trackedOrbs = new List<ExpOrb>();

    public event System.Action<int> OnLevelUp;
    public event System.Action<float, float> OnXPChanged;

    public int CurrentLevel => _currentLevel;
    public float CurrentXP => _currentXP;
    public float XPToNextLevel => _xpToNextLevel;
    public float XPNormalized => _currentXP / _xpToNextLevel;
    public float CollectionRadius => _collectionRadius;

    public void SetCollectionRadius(float radius)
    {
        _collectionRadius = radius;
        _collectionZone.radius = _collectionRadius;
    }

    private void Awake()
    {
        _collectionZone = gameObject.AddComponent<SphereCollider>();
        _collectionZone.isTrigger = true;
        _collectionZone.radius = _collectionRadius;

        _xpToNextLevel = GetXPRequiredForLevel(_currentLevel);
    }

    private void Update()
    {
        for (int i = _trackedOrbs.Count - 1; i >= 0; i--)
        {
            ExpOrb orb = _trackedOrbs[i];
            if (orb == null || !orb.gameObject.activeInHierarchy)
            {
                _trackedOrbs.RemoveAt(i);
                continue;
            }

            Vector3 dir = transform.position - orb.transform.position;
            float dist = dir.magnitude;

            if (dist <= _collectDistance)
            {
                if (orb.TryMarkCollected()) 
                    CollectOrb(orb);
                else
                    _trackedOrbs.RemoveAt(i);
            }
            else
            {
                orb.Rb.linearVelocity = dir.normalized * _attractSpeed;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ExpOrb orb = other.GetComponent<ExpOrb>();
        if (orb != null && !_trackedOrbs.Contains(orb))
        {
            _trackedOrbs.Add(orb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ExpOrb orb = other.GetComponent<ExpOrb>();
        if (orb != null)
        {
            _trackedOrbs.Remove(orb);
        }
    }

    public void CollectOrb(ExpOrb orb)
    {
        _currentXP += orb.xpValue;
        CheckLevelUp();
        OnXPChanged?.Invoke(_currentXP, _xpToNextLevel);
        _trackedOrbs.Remove(orb);
        ObjectPool.Instance.ReturnToPool("ExpOrb", orb.gameObject);
    }

    private void CheckLevelUp()
    {
        int safetyGuard = 0;
        while (_currentXP >= _xpToNextLevel && safetyGuard < 1000)
        {
            _currentXP -= _xpToNextLevel;
            _currentLevel++;
            _xpToNextLevel = GetXPRequiredForLevel(_currentLevel);
            OnLevelUp?.Invoke(_currentLevel);
            safetyGuard++;
        }

        if (safetyGuard >= 1000)
            Debug.LogError("CheckLevelUp: Exceeded safety guard limit. Possible infinite loop in level up logic.");
    }

    private float GetXPRequiredForLevel(int level)
    {
        float xp = _baseXPToLevel * _levelCurve.Evaluate(level);
        return Mathf.Max(1f, xp);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _collectionRadius);
    }
}
