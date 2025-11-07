using UnityEngine;
using System.Collections.Generic;

public class WaterStream : MonoBehaviour, IRevertable
{
    [SerializeField] private UnitTrigger[] _waterTiles;

    [SerializeField] private RuntimeAnimatorController _middleSprite;
    [SerializeField] private RuntimeAnimatorController _endSprite;

    private float SoundCooldown = 0.4f;

    private float _soundTimer; 

    private bool _isBlocked;
    //private float _blockedTimer;
    private AudioSource _loopingSource;

    public List<int> _lengthHistory = new();

    public int _activeLength;

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
        _lengthHistory.Add(_activeLength);
        UpdateStream(false);
    }

    private void Update()
    {
        _soundTimer -= Time.deltaTime;
        //_blockedTimer -= Time.deltaTime;
        if (_isBlocked /*&& _blockedTimer <= 0f*/)
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
                SetBlocked(true);
                return;
            }
            _activeLength = _waterTiles.Length;
            CutStreamAt(_waterTiles.Length);
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
                    _waterTiles[i].GetComponent<Animator>().runtimeAnimatorController = _endSprite;
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
        CutStreamAt(_lengthHistory[turnIndex]);
    }

    public void SaveHistoryPoint(int turnIndex, bool deleteFuturePoints = true)
    {
        if (deleteFuturePoints)
        {
            while (_lengthHistory.Count > turnIndex)
                _lengthHistory.RemoveAt(_lengthHistory.Count - 1);
        }
        if (_lengthHistory.Count == turnIndex)
            _lengthHistory.Add(_activeLength);
        else
            _lengthHistory[turnIndex] = _activeLength;
    }
}
