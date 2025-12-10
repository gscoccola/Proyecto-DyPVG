using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;
    private AudioSource _audioSource;

    private void Awake()
    { 
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DestroyPlayer()
    {
        if (gameObject !=null) Destroy(gameObject);
    }
}
