namespace InventoryUtility
{
    public enum ConnectionStatus
    {
        None = 0,
        Waiting = 1,
        Success = 2,
        Failed = 3
    }

    public enum OperationResponse
    {
        Success = 0,
        Failed = 1,
        Canceled = 2
    }
}
