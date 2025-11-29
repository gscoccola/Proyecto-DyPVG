using UnityEngine;

public class Blocking : MonoBehaviour
{
    [ReadOnly] public bool IsBlocking { get; } = true;
    [ReadOnly] public bool IsEnabled { get; set; } = true;
    public bool IsWater = false;
    [ReadOnly] public SpriteRenderer Renderer;

    private void Awake()
    {
        Renderer = GetComponent<SpriteRenderer>();
    }

}
