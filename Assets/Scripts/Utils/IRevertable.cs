using System.Collections.Generic;

public interface IRevertable
{
    //public List<T> StatusHistory { get; set; }

    public void SaveHistoryPoint(int turnIndex);

    public void RevertToHistoryPoint(int turnIndex);

}