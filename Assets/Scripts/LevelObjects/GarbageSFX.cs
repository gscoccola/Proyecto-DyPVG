using UnityEngine;

public class GarbageSFX : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 1f);
    }


}
