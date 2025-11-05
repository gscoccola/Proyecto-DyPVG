using UnityEngine;
using System.Collections.Generic;

public class WaterStream : MonoBehaviour, IRevertable
{
    [SerializeField] private UnitTrigger[] _waterTiles;

    [SerializeField] private RuntimeAnimatorController _middleSprite;
    [SerializeField] private RuntimeAnimatorController _endSprite;

    public List<int> _lengthHistory = new();

    public int _activeLength;

    private void Start()
    {
        _activeLength = _waterTiles.Length;
        foreach (var waterTile in _waterTiles)
        {
            waterTile.Triggered.AddListener(value => UpdateStream());
            waterTile.Untriggered.AddListener(value => UpdateStream());
        }
        _lengthHistory.Add(_activeLength);
        UpdateStream();
    }

    private void UpdateStream()
    {
        for (int i = 0; i < _waterTiles.Length; i++)
        {
            if (_waterTiles[i].IsTriggered)
            {
                _activeLength = i;
                CutStreamAt(_activeLength);
                return;
            }
            _activeLength = _waterTiles.Length;
            CutStreamAt(_waterTiles.Length);
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
