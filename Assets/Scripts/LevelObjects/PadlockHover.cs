using UnityEngine;

public class PadlockHover : MonoBehaviour
{
    [SerializeField] private float _frequency;
    [SerializeField] private float _amplitude;
    private Vector3 _initialPos;

    private void Start()
    {
        _initialPos = transform.position;
    }

    private void Update()
    {
        transform.position = _initialPos + new Vector3(Mathf.PingPong(Time.time * _frequency, _amplitude),
            Mathf.PingPong(Time.time * _frequency, _amplitude), transform.position.z);
    }
}
