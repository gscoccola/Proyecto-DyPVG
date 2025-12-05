using UnityEngine;
using System.Collections.Generic;

public class WaterStream : MonoBehaviour, IRevertable
{
    [SerializeField] private UnitTrigger[] _waterTiles;

    [SerializeField] private RuntimeAnimatorController _middleSprite;
    [SerializeField] private RuntimeAnimatorController _endSprite;

    [SerializeField] private Vector2Int _direction;

    private float SoundCooldown = 0.4f;

    private float _soundTimer; 

    private bool _isBlocked;
    //private float _blockedTimer;
    private AudioSource _loopingSource;

    public List<StreamInformation> _history = new();

    public int _activeLength;
    public Dog InterruptingDog;

    private void Awake()
    {
        _loopingSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        SFXPlayer.Instance.StartWater();
        _activeLength = _waterTiles.Length;
        foreach (var waterTile in _waterTiles)
        {
            waterTile.Triggered.AddListener(value => UpdateStream(value));
            waterTile.Untriggered.AddListener(value => UpdateStream(value));
        }
        _history.Add(new StreamInformation(_activeLength, null));
        UpdateStream(false);
    }

    private void Update()
    {
        //UpdateStream(true);
        _soundTimer -= Time.deltaTime;
        if (_isBlocked)
        {
            _loopingSource.volume += Time.deltaTime * 0.8f;
        }
    }

    private void UpdateStream(bool playSFX)
    {
        for (int i = 0; i < _waterTiles.Length; i++)
        {
            if (_waterTiles[i].IsTriggered)
            {
                if (playSFX && i < _activeLength) TryPlaySound();
                _activeLength = i;
                CutStreamAt(_activeLength);
                if (_waterTiles[i].OccupyingDogs.Count > 0)
                {
                    Dog newInterruptingDog = _waterTiles[i].OccupyingDogs[0];
                    if (InterruptingDog != newInterruptingDog)
                    {
                        if (InterruptingDog != null) InterruptingDog.GetComponent<SplashVFXManager>().ToggleVFX(false, _direction);
                        newInterruptingDog.GetComponent<SplashVFXManager>().ToggleVFX(true, _direction);
                        InterruptingDog = newInterruptingDog;
                    }
                    
                }
                SetBlocked(true);
                return;
            }
            _activeLength = _waterTiles.Length;
            CutStreamAt(_waterTiles.Length);
            if (InterruptingDog != null) InterruptingDog.GetComponent<SplashVFXManager>().ToggleVFX(false, _direction);
            InterruptingDog = null;
            SetBlocked(false);
        }
    }

    private void TryPlaySound()
    {
        if (_soundTimer <= 0f)
        {
            SFXPlayer.Instance.PlayClip(WorldSounds.Instance.WaterCross);
            _soundTimer = SoundCooldown;
        }
    }

    private void SetBlocked(bool blocked)
    {
        _isBlocked = blocked;
        if (!blocked)
        {
            //_blockedTimer = 1f;
            _loopingSource.volume = 0.0f;
        }
    }

    public void CutStreamAt(int index)
    {
        for (int i = 0; i < _waterTiles.Length; i++)
        {
            _waterTiles[i].GetComponent<Blocking>().IsEnabled = i < index;
            _waterTiles[i].GetComponent<SpriteRenderer>().enabled = i < index;
            if (i < index)
            {
                if (i == index - 1)
                {
                    _waterTiles[i].GetComponent<Animator>().runtimeAnimatorController = 
                        i == _waterTiles.Length - 1 ? _endSprite: _middleSprite;
                }
                else
                {
                    _waterTiles[i].GetComponent<Animator>().runtimeAnimatorController = _middleSprite;
                }
                    _waterTiles[i].GetComponent<Animator>().speed = Random.Range(0.9f, 1.1f);
            }
        }
    }

    public void RevertToHistoryPoint(int turnIndex)
    {
        if (InterruptingDog != null)
            InterruptingDog.GetComponent<SplashVFXManager>().ToggleVFX(false, _direction);
        InterruptingDog = _history[turnIndex].InterruptingDog;
        if (InterruptingDog != null)
            InterruptingDog.GetComponent<SplashVFXManager>().ToggleVFX(true, _direction);
        _activeLength = _history[turnIndex].ActiveLength;
        CutStreamAt(_activeLength);
    }

    public void SaveHistoryPoint(int turnIndex, bool deleteFuturePoints = true)
    {
        if (deleteFuturePoints)
        {
            while (_history.Count > turnIndex)
                _history.RemoveAt(_history.Count - 1);
        }
        if (_history.Count == turnIndex)
            _history.Add(new StreamInformation(_activeLength, InterruptingDog));
        else
        {
            _history[turnIndex].ActiveLength = _activeLength;
            _history[turnIndex].InterruptingDog = InterruptingDog;
        }
    }
}


public class StreamInformation
{
    public int ActiveLength;
    public Dog InterruptingDog;

    public StreamInformation(int activeLength, Dog interruptingDog)
    {
        ActiveLength = activeLength;
        InterruptingDog = interruptingDog;
    }
}