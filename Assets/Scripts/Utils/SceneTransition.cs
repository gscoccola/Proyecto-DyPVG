using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : Singleton<SceneTransition>
{
    [SerializeField] float transitionDuration = 0.4f;
    private Animator _animator;

    private new void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }

    public void LoadScene(int index)
    {
        StartCoroutine(ILoadScene(index));
    }

    public IEnumerator ILoadScene(int index)
    {
        PersistentInfo.Instance.Save();
        _animator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(transitionDuration);
        if (index == 0 && SceneManager.GetActiveScene().buildIndex != 0 || index !=0 && SceneManager.GetActiveScene().buildIndex == 0) MusicPlayer.Instance.DestroyPlayer(); 
        SceneManager.LoadScene(index);
    }
}
