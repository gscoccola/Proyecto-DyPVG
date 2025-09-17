using UnityEngine;
using System.Collections.Generic;

// This class manages the creation, storage, and removal of visual markers on the game grid.
public class MarkerManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _markerPrefabs;
    private List<GameObject> _markers = new();
    private Transform _container;

    private void Start()
    {
        _container = GameObject.Find("Markers").transform;
        if (_container == null) _container = transform.parent;
    }

    public void AddMarker(int index, Vector3 worldPosition, Color color)
    {
        GameObject marker = Instantiate(_markerPrefabs[index], worldPosition, Quaternion.identity, _container);
        _markers.Add(marker);
        marker.GetComponent<SpriteRenderer>().color = color;
    }

    public void AddMarker(int index, Vector2Int gridPosition, Color color)
    {
        AddMarker(index, LevelGrid.Instance.GridToWorldPos(gridPosition), color);
    }

    public void DeleteLastMarker()
    {
        _markers.RemoveAt(_markers.Count - 1);
        Destroy(_markers[_markers.Count - 1]);
    }

    public void ClearAllMarkers()
    {
        foreach (GameObject marker in _markers)
        {
            Destroy(marker);
        }
        _markers.Clear();
    }
}
