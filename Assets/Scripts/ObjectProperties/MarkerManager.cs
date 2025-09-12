using UnityEngine;
using System.Collections.Generic;

public class MarkerManager : MonoBehaviour
{
    [SerializeField] private GameObject _markerPrefab;
    private List<GameObject> _markers = new();
    private Transform _container;

    private void Start()
    {
        _container = GameObject.Find("Markers").transform;
        if (_container == null) _container = transform.parent;
    }

    public void AddMarker(Vector3 worldPosition, Color color)
    {
        GameObject marker = Instantiate(_markerPrefab, worldPosition, Quaternion.identity, transform);
        _markers.Add(marker);
        marker.GetComponent<SpriteRenderer>().color = color;
    }

    public void AddMarker(Vector2Int gridPosition, Color color)
    {
        GameObject marker = Instantiate(_markerPrefab, LevelGrid.Instance.GridToWorldPos(gridPosition), Quaternion.identity, _container);
        _markers.Add(marker);
        marker.GetComponent<SpriteRenderer>().color = color;
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
