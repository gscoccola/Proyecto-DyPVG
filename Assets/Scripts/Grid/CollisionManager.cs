using UnityEngine;
using System.Collections.Generic;
using System;

public class CollisionManager : Singleton<CollisionManager>
{
    public List<Transform> _colliders = new();
    //public List<Transform> _collidersTransforms = new();
    private Dictionary<Tuple<Transform, Transform>, bool> _collisions = new();

    private new void Awake()
    {
        base.Awake();

        foreach (GameObject gameObject in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (gameObject.GetComponent<IGridCollider>() != null) _colliders.Add(gameObject.GetComponent<Transform>());
        }

        /*foreach (GameObject gameObject in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Debug.Log($"Found GameObject: {gameObject.name}");
            if (gameObject.GetComponent<IGridCollider>() == null) return;
            _colliders.Add(gameObject.GetComponent<IGridCollider>());
            _collidersTransforms.Add(gameObject.GetComponent<Transform>());
        }*/
    }

    private void Start()
    {
        InitializeCollisions();
    }

    private void Update()
    {
        UpdateCollisions();
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


    private void UpdateCollisions()
    {
        foreach (Transform col1 in _colliders)
        {
            foreach (Transform col2 in _colliders)
            {
                if (col1 == col2) continue; // Skip self-collision
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
}
