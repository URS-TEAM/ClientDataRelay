using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryUtility.Services.Sql;

public class SqlConnectionService : ISqlConnectionService
{
    public OperationResponse ConnectionStatus { get; private set; } = OperationResponse.Failed;

    public async Task<OperationResponse> CheckConnectionAsync(string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand())
            {
                await connection.OpenAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                command.Connection = connection;
                command.CommandText = "SELECT GETDATE()";

                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    var executeTask = command.ExecuteNonQueryAsync(cts.Token);
                    await Task.WhenAny(executeTask, Task.Delay(Timeout.Infinite));

                    cts.Cancel();

                    cancellationToken.ThrowIfCancellationRequested();
                }

                ConnectionStatus = OperationResponse.Success;
                return OperationResponse.Success;
            }
        }
        catch (OperationCanceledException e)
        {
            ConnectionStatus = OperationResponse.Failed;
            return OperationResponse.Canceled;
        }
        catch (Exception e)
        {
            ConnectionStatus = OperationResponse.Failed;
            return OperationResponse.Failed;
        }
    }

}

public interface ISqlConnectionService
{
    OperationResponse ConnectionStatus { get; }
    Task<OperationResponse> CheckConnectionAsync(string connectionString, CancellationToken cancellationToken);
}

