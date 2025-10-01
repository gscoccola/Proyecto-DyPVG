using UnityEngine;
using System.Collections.Generic;
using System;

// This class manages grid-based collisions between objects implementing the IGridCollider interface.

public class CollisionManager : Singleton<CollisionManager>
{
    [Header("Debug")]
    public List<Transform> _colliders = new();

    private Dictionary<Tuple<Transform, Transform>, bool> _collisions = new();

    #region SETUP

    private new void Awake()
    {
        base.Awake();
        SetupListAndEvents();
    }

    private void Start()
    {
        UpdateAllCollisions(false);
    }

    // Populate list with all colliders in the scene
    // Set up event listeners for moving objects
    private void SetupListAndEvents()
    {
        foreach (GameObject gameObject in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (gameObject.GetComponent<IGridCollider>() == null) continue;
            _colliders.Add(gameObject.GetComponent<Transform>());

            if (gameObject.GetComponent<GridMovement>() == null) continue;
            gameObject.GetComponent<GridMovement>().OnNewTileReached.AddListener(()
                => UpdateColliderCollisions(gameObject.GetComponent<Transform>(), true, true));
            
        }
    }
    #endregion

    #region COLLISION UPDATE

    public void UpdateAllCollisions(bool invokeEvents = false)
    {
        foreach (Transform col1 in _colliders)
        {
            UpdateColliderCollisions(col1, invokeEvents);
        }
    }

    private void UpdateColliderCollisions(Transform col1, bool invokeSelfEvent = false, bool invokeOtherEvent = false)
    {
        //if (invokeSelfEvent && invokeOtherEvent) Debug.Log("Updated cols for " + col1);
        foreach (Transform col2 in _colliders)
        {
            if (col1 == col2) continue;
            bool areColliding = LevelGrid.Instance.WorldToGridPos(col1.position) == LevelGrid.Instance.WorldToGridPos(col2.position);

            if (_collisions.ContainsKey(Tuple.Create(col1, col2)) &&
                _collisions[Tuple.Create(col1, col2)] == areColliding) 
                continue;

            _collisions[Tuple.Create(col1, col2)] = areColliding;

            if (invokeSelfEvent) 
            { 
                if (areColliding)
                    col1.GetComponent<IGridCollider>().OnGridCollisionEnter(col2);
                else
                    col1.GetComponent<IGridCollider>().OnGridCollisionExit(col2);
            }

            if (invokeOtherEvent) 
            { 
                if (areColliding)
                    col2.GetComponent<IGridCollider>().OnGridCollisionEnter(col1);
                else
                    col2.GetComponent<IGridCollider>().OnGridCollisionExit(col1);
            }
        }
    }
    #endregion
}
