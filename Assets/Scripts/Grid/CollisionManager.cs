using UnityEngine;
using System.Collections.Generic;
using System;

// This class manages grid-based collisions between objects implementing the IGridCollider interface.
public class CollisionManager : Singleton<CollisionManager>
{
    public List<Transform> _colliders = new();
    private Dictionary<Tuple<Transform, Transform>, bool> _collisions = new();

    private void Start()
    {
        FindAllColliders();
    }

    private void FindAllColliders()
    {
        foreach (GameObject gameObject in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (gameObject.GetComponent<IGridCollider>() == null) continue;
            _colliders.Add(gameObject.GetComponent<Transform>());
            if (gameObject.GetComponent<GridMovement>() == null) continue;
            gameObject.GetComponent<GridMovement>().OnNewTileReached.AddListener(()
                => UpdateColliderCollisions(gameObject.GetComponent<Transform>()));
            
        }
        InitializeCollisions();
    }

    private void InitializeCollisions()
    {
        foreach (Transform col1 in _colliders)
        {
            foreach (Transform col2 in _colliders)
            {
                bool areColliding = LevelGrid.Instance.WorldToGridPos(col1.position) == LevelGrid.Instance.WorldToGridPos(col2.position);
                _collisions[Tuple.Create(col1, col2)] = areColliding;
                
            }
        }
    }


    private void UpdateAllCollisions()
    {
        foreach (Transform col1 in _colliders)
        {
            UpdateColliderCollisions(col1);
        }
    }

    private void UpdateColliderCollisions(Transform col1)
    {
        foreach (Transform col2 in _colliders)
        {
            if (col1 == col2) continue;
            bool areColliding = LevelGrid.Instance.WorldToGridPos(col1.position) == LevelGrid.Instance.WorldToGridPos(col2.position);
            if (_collisions[Tuple.Create(col1, col2)] == areColliding) continue;
            _collisions[Tuple.Create(col1, col2)] = areColliding;
            if (areColliding)
            {
                col1.GetComponent<IGridCollider>().OnGridCollisionEnter(col2);
                col2.GetComponent<IGridCollider>().OnGridCollisionEnter(col1);
            }
            else
            {
                col1.GetComponent<IGridCollider>().OnGridCollisionExit(col2);
                col2.GetComponent<IGridCollider>().OnGridCollisionExit(col1);
            }
        }
    }
}
