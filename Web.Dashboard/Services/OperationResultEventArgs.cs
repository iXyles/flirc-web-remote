using Web.Dashboard.Models;

namespace Web.Dashboard.Services;

public class OperationResultEventArgs : EventArgs
{
    public string Key { get; }
    public OperationResult OperationResult { get; }

    public OperationResultEventArgs(OperationResult operationResult, string key)
    {
        OperationResult = operationResult;
        Key = key;
    }
}
