using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqlite.Models;

public class StoreDataTransferExecution
{
    public int Id { get; set; }
    public long ExecutionTime { get; set; }
    public int TransferType { get; set; }
    public int Status { get; set; }
    public string ErrorMessage { get; set; }
}

public enum ExecutionStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2
}
public enum TransferType
{
    Manual = 0,
    Scheduled = 1,
}
