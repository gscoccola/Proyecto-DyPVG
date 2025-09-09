using UnityEngine;
using UnityEngine.AI;

public static class NavMeshDistance
{

    public static float CalculateDistance(NavMeshAgent Agent, Vector3 pointA, Vector3 pointB)
    {
        NavMeshPath Path = new NavMeshPath();

        if ( !Agent.CalculatePath(pointB, Path))
        {
            Debug.LogWarning("Failed to calculate path.");
            return float.MaxValue;
        }

        if (Path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning("No valid path found between the points.");
            return float.MaxValue;
        }

        float distance = Vector3.Distance(pointA, Path.corners[0]);

        for (int j = 1; j < Path.corners.Length; j++)
        {
            distance += Vector3.Distance(Path.corners[j - 1], Path.corners[j]);
        }

        return distance;
    }
}
