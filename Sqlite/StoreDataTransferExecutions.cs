using Microsoft.Data.Sqlite;
using Sqlite.Models;
using System;

namespace Sqlite
{
    public class StoreDataTransferExecutions : Connection
    {
        private string connString = "Filename=data.db";
        public async Task AddExecution(StoreDataTransferExecution execution)
        {
            using (var db = new SqliteConnection(connString))
            {
                await db.OpenAsync();

                using (var command = new SqliteCommand())
                {
                    command.Connection = db;
                    command.CommandText =
                        "INSERT INTO StoreDataTransferExecutions " +
                        "(ExecutionTime, TransferType, Status, ErrorMessage) " +
                        "VALUES (@ExecutionTime, @TransferType, @Status, @ErrorMessage);";

                    command.Parameters.AddWithValue("@ExecutionTime", execution.ExecutionTime);
                    command.Parameters.AddWithValue("@TransferType", execution.TransferType);
                    command.Parameters.AddWithValue("@Status", execution.Status);
                    command.Parameters.AddWithValue("@ErrorMessage",
                        string.IsNullOrEmpty(execution.ErrorMessage) ? DBNull.Value : execution.ErrorMessage);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<StoreDataTransferExecution>> GetAllExecutions()
        {
            List<StoreDataTransferExecution> executions = new List<StoreDataTransferExecution>();

            using (var db = new SqliteConnection(connString))
            {
                await db.OpenAsync();

                using (var command = new SqliteCommand())
                {
                    command.Connection = db;
                    command.CommandText = "SELECT * FROM StoreDataTransferExecutions;";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            executions.Add(new StoreDataTransferExecution
                            {
                                Id = reader.GetInt32(0),
                                ExecutionTime = reader.GetInt32(1),
                                TransferType = reader.GetInt32(2),
                                Status = reader.GetInt32(3),
                                ErrorMessage = reader.IsDBNull(4) ? null : reader.GetString(3)
                            });
                        }
                    }
                }
            }

            return executions;
        }

        public async Task<StoreDataTransferExecution?> GetLastSuccessfulExecution()
        {
            using (var db = new SqliteConnection(connString))
            {
                await db.OpenAsync();

                using (var command = new SqliteCommand())
                {
                    command.Connection = db;
                    command.CommandText = "SELECT * FROM StoreDataTransferExecutions WHERE Status = @Status " +
                        "AND TransferType = @TransferType";
                    command.Parameters.AddWithValue("@Status", ExecutionStatus.Completed);
                    command.Parameters.AddWithValue("@TransferType", TransferType.Scheduled);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new StoreDataTransferExecution
                            {
                                Id = reader.GetInt32(0),
                                ExecutionTime = reader.GetInt32(1),
                                TransferType = reader.GetInt32(2),
                                Status = reader.GetInt32(3),
                                ErrorMessage = reader.IsDBNull(4) ? null : reader.GetString(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task DeleteExecution(int executionId)
        {
            using (var db = new SqliteConnection(connString))
            {
                await db.OpenAsync();

                using (var command = new SqliteCommand())
                {
                    command.Connection = db;
                    command.CommandText = "DELETE FROM StoreDataTransferExecutions WHERE Id = @Id;";
                    command.Parameters.AddWithValue("@Id", executionId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }


}

