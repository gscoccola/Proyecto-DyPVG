// This interface is implemented by objects that can save their state each turn,
// and then revert back to it.

public interface IRevertable
{
    public void SaveHistoryPoint(int turnIndex, bool deleteFuturePoints = true);

    public void RevertToHistoryPoint(int turnIndex);

}