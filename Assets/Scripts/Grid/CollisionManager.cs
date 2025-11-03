using UnityEngine;
using System.Collections.Generic;
using System;

// This class manages grid-based collisions between objects implementing the IGridCollider interface.

public class CollisionManager : Singleton<CollisionManager>
{
    [Header("Debug")]
    public List<Transform> Colliders = new();
    public List<Blocking> Blockings = new();

    private Dictionary<Tuple<Transform, Transform>, bool> _collisions = new();

    #region SETUP

    private new void Awake()
    {
        base.Awake();
        SetupListAndEvents();
    }

    private void Start()
    {
        UpdateAllCollisions();
    }

    // Populate list with all colliders in the scene
    // Set up event listeners for moving objects
    private void SetupListAndEvents()
    {
        foreach (GameObject gameObject in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (gameObject.GetComponent<Blocking>() != null)
                Blockings.Add(gameObject.GetComponent<Blocking>());

            if (gameObject.GetComponent<IGridCollider>() == null) continue;
            Colliders.Add(gameObject.GetComponent<Transform>());

            if (gameObject.GetComponent<GridMovement>() == null) continue;
            gameObject.GetComponent<GridMovement>().NewTileReached.AddListener(()
                => UpdateColliderCollisions(gameObject.GetComponent<Transform>(), true, true));
            gameObject.GetComponent<GridMovement>().TargetAcquired.AddListener(()
                => CheckPreemptiveCollisions(gameObject.GetComponent<GridMovement>()));
            
        }
    }
    #endregion

    #region COLLISION UPDATE

    public void UpdateAllCollisions(bool invokeEvents = false)
    {
        foreach (Transform col1 in Colliders)
        {
            UpdateColliderCollisions(col1, invokeEvents);
        }
    }

    public void CheckPreemptiveCollisions(GridMovement gridMover)
    {
        
        foreach (Blocking col2 in Blockings)
        {
            if (col2.IsWater && gridMover.GetComponent<Dog>() != null &&
                gridMover.GetComponent<Dog>().DogParameters.Type == DogType.Water)
                continue;
            if (col2.IsEnabled && 
                LevelGrid.Instance.WorldToGridPos(gridMover.CurrentTarget)
                == LevelGrid.Instance.WorldToGridPos(col2.transform.position)
                )
            {
                if (col2.GetComponent<Door>() != null) SFXPlayer.Instance.PlayClip(WorldSounds.Instance.DoorInterrupt);
                gridMover.EndPath(true);
            }
        }
    }

    public void UpdateColliderCollisions(Transform col1, bool invokeSelfEvent = false, bool invokeOtherEvent = false)
    {
        foreach (Transform col2 in Colliders)
        {
            if (col1 == col2) continue;
            bool areColliding = LevelGrid.Instance.WorldToGridPos(col1.position) == LevelGrid.Instance.WorldToGridPos(col2.position);

            if (_collisions.ContainsKey(Tuple.Create(col1, col2)) &&
                _collisions[Tuple.Create(col1, col2)] == areColliding) 
                continue;

            _collisions[Tuple.Create(col1, col2)] = areColliding;
            //if (areColliding) Debug.Log(col1.gameObject.name + " " + col2.gameObject.name);

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
