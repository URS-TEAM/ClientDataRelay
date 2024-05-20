using CommunityToolkit.Mvvm.Input;
using InventoryUtility.ViewModels;
using Sqlite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui;

namespace InventoryUtility.Models.Functionalities
{

    public partial class ExpDateFunctionality : BaseFunctionality, IFunctionality
    {

        private DBData DBData;
        private string connectionString;
        IContentDialogService ContentDialogService;

        private async Task<TableCheckResult> GetExpDateTableState()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    // Query the information schema for the table's columns
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT COLUMN_NAME
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_NAME = @TableName";
                        command.Parameters.AddWithValue("@TableName", "MP_ExpDate");

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                            {
                                return TableCheckResult.TableDoesNotExist;
                            }

                            string[] expectedColumns = { "ID", "ItemID", "ExpDate" };

                            int matchingColumnCount = 0;

                            while (await reader.ReadAsync())
                            {
                                string columnName = reader["COLUMN_NAME"].ToString();

                                if (Array.Exists(expectedColumns, col => col.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    matchingColumnCount++;
                                }
                            }

                            //Table exists but columns are wrong
                            return (matchingColumnCount == 3 ? TableCheckResult.TableExists : TableCheckResult.ColumnsMismatch);
                        }
                    }
                }
            }
            catch
            {

            }
            return TableCheckResult.Error;
        }


        public ExpDateFunctionality(string description, string buttonContent, IContentDialogService dialogService, DBData dBData) :
            base(description, buttonContent)
        {
            this.ActivateFuncCommand = new RelayCommand(async () => await ExecuteTaskAsync(CreateExpDateTable));
            this.DBData = dBData;
            this.ContentDialogService = dialogService;

            _ = UpdateStateButton();
            DBData.DatabaseChanged += DBData_DatabaseChanged;
        }

        private void DBData_DatabaseChanged(object? sender, DatabaseChangedEventArgs e)
        {
            _ = UpdateStateButton();
        }


        private async Task CreateExpDateTable()
        {
            TableCheckResult tableCheckResult = await GetExpDateTableState();

            string tableExistsMessage = "The Exp. Date table already exists, please check your DB.";
            string columnsMismatchMessage = "Exp. Date table already exists, but there is column mismatch(es).";
            string successMessage = $"Exp. Date functionality added to the DB.";
            string errorMessage = "There was an error while trying to add this functionality.";

            if (tableCheckResult == TableCheckResult.TableExists)
            {
                _ = await ContentDialogService.ShowAlertAsync("Notification", tableExistsMessage, "Close");
                return;
            }
            else if (tableCheckResult == TableCheckResult.ColumnsMismatch)
            {
                _ = await ContentDialogService.ShowAlertAsync("Notification", columnsMismatchMessage, "Close");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    BtnState = ButtonState.Waiting;
                    await Task.Delay(200);
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MP_ExpDate')
                    BEGIN
                        CREATE TABLE MP_ExpDate (
                            ID INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
                            ItemID INT NOT NULL UNIQUE,
                            ExpDate [datetime] NOT NULL
                        );
                        SELECT 1;
                    END
                    ELSE
                    BEGIN
                        SELECT 0;
                    END";

                        object resultObject = await command.ExecuteScalarAsync();
                        int? result = resultObject as int?;

                        BtnState = (result == 1 ? ButtonState.Done : ButtonState.Default);

                        _ = await ContentDialogService.ShowAlertAsync("Notification",
                            result == 1 ? successMessage : errorMessage, "Close");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately, log or show an error message
                Console.WriteLine($"An error occurred: {ex.Message}");
                _ = await ContentDialogService.ShowAlertAsync("Notification", errorMessage, "Close");
            }
        }

        private async Task UpdateStateButton()
        {
            connectionString = @"Data Source=" + DBData.GetServer() + ";Initial Catalog=" + DBData.GetDb() + ";User ID=" +
                DBData.GetUser() + ";Password=" + DBData.GetPass();
            BtnState = ButtonState.Waiting;

            var getTableStateTask = GetExpDateTableState();
            var delayTask = Task.Delay(200);

            await Task.WhenAll(getTableStateTask, delayTask);

            var tableState = await getTableStateTask;

            if (tableState == TableCheckResult.TableExists || tableState == TableCheckResult.ColumnsMismatch)
            {
                BtnState = ButtonState.Done;
                return;
            }
            BtnState = ButtonState.Default;
        }
    }
}
