using UnityEngine;
using UnityEngine.AI;

public class Dog : MonoBehaviour, IRevertable
{

    [Header("Parameters")]
    [SerializeField] private DogType Type;

    [Header("References")]
    [SerializeField] private NavMeshAgent Agent;
    private Transform _agentTransform;
    [SerializeField] private Transform Target;
    [SerializeField] private Transform Distraction;
    private Vector3 _startPos;
    public bool IsMoving;

    private void OnEnable()
    {
        _startPos = Agent.transform.position;
        _agentTransform = Agent.transform;
    }

    private void Update()
    {
        if (!IsMoving) return;

        if (Distraction == null || Distraction.GetComponent<Distraction>().IsOccupied
            || Type == DogType.Bully)
        {
            Agent.SetDestination(Target.position);
            return;
        }


        float distanceToDistraction = NavMeshDistance.CalculateDistance(Agent, _agentTransform.position, Distraction.position);
        
        if (distanceToDistraction < 5f) Agent.SetDestination(Distraction.position);
        else Agent.SetDestination(Target.position);

    }

    public void StartMovement()
    {
        //Agent.SetDestination(Distraction.position);
        Agent.SetDestination(Target.position);
        IsMoving = true;
    }

    public void Revert()
    {
        Agent.Warp(_startPos);
        Agent.ResetPath();
        IsMoving = false;
    }
}

public enum DogType
{
    Bully,
    Agile,
}