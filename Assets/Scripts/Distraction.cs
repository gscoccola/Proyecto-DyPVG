using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Distraction : MonoBehaviour
{
    public bool IsOccupied;
    public Dog OccupyingDog;


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.parent.GetComponent<Dog>() != null && !IsOccupied)
        {
            IsOccupied = true;
            OccupyingDog = col.transform.parent.GetComponent<Dog>();
             col.transform.parent.GetComponent<Dog>().IsMoving = false;
            //OccupyingDog.SetDistraction(this);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.transform.parent.GetComponent<Dog>() != null && IsOccupied)
        {
            IsOccupied = false;
            OccupyingDog = null;
            //col.transform.parent.GetComponent<Dog>().IsMoving = true;
            //OccupyingDog.SetDistraction(this);
        }
    }
}
