using TMPro;
using UnityEngine;

public class DamagePopUp : MonoBehaviour
{
    private TextMeshPro _textMesh;
    private ObjectPool _pool;

    [SerializeField] private float _moveSpeed = 1f;

    private void Awake()
    {
        _textMesh = GetComponent<TextMeshPro>();
        _pool = ObjectPool.Instance;
    }

    private void OnEnable()
    {
        if (_textMesh != null)
            _textMesh.alpha = 1f;
    }

    public void SetUp(int damage, Color color)
    {
        _textMesh.SetText(damage.ToString());
        _textMesh.color = color;
    }

    public void SetUp(int damage)
    {
        SetUp(damage, Color.white);
    }

    public static DamagePopUp Create(Vector3 position, int damage, Color color)
    {
        GameObject go = ObjectPool.Instance.SpawnFromPool("PopUpDamage", position, Quaternion.identity);
        if (go == null) return null;

        DamagePopUp popup = go.GetComponent<DamagePopUp>();
        popup.SetUp(damage, color);
        return popup;
    }

    public static DamagePopUp Create(Vector3 position, int damage)
    {
        return Create(position, damage, Color.white);
    }

    private void Update()
    {
        if (_textMesh == null) return;

        transform.position += new Vector3(0, 1, 0) * Time.deltaTime * _moveSpeed;
        _textMesh.alpha -= 2f * Time.deltaTime;
        if (_textMesh.alpha <= 0)
        {
            _pool.ReturnToPool("PopUpDamage", gameObject);
        }
    }
}
