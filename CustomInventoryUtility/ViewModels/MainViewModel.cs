using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryUtility.Models;
using Sqlite;
using InventoryUtility.Windows;
using Wpf.Ui;
using System.Data.SqlClient;
using System.Threading.Tasks;
using InventoryUtility.Utils;
using System.Threading;
using System;
using System.Diagnostics.Metrics;
using System.Windows.Documents;
using System.Windows.Input;
using System.Data.Common;
using System.Collections.Generic;
using InventoryUtility.Models.Functionalities;
using System.Linq;
using InventoryUtility.Services;
using InventoryUtility.Services.Sql;

namespace InventoryUtility.ViewModels
{

    public partial class MainViewModel : ObservableObject
    {

        private readonly SqlConnectionService _sqlConnService;
        private readonly DBData _dbData;
        private readonly IPageService _pageService;
        private readonly IContentDialogService _contentDialogService;

        [ObservableProperty]
        private string _applicationTitle = "Inventory Utility";

        [ObservableProperty]
        private DatabaseAccess databaseDetails;

        [ObservableProperty]
        private ConnectionStatus connectionStatus;

        [ObservableProperty]
        private bool expDateIsEnabled;

        [ObservableProperty]
        private List<IFunctionality> functionalityList;

        private string connectionString;

        public MainViewModel(IPageService pageService,
                             IContentDialogService contentDialogService,
                             IEnumerable<IFunctionality> functionalities,
                             SqlConnectionService connService,
                             DBData dbData)
        {
            _sqlConnService = connService;
            _dbData = dbData;
            _pageService = pageService;
            _contentDialogService = contentDialogService;
            FunctionalityList = functionalities.ToList();

            UpdateDBDetails();
            CheckDBConnection();
            _dbData.DatabaseChanged += _dbData_DatabaseChanged;
        }

        private async Task CheckDBConnection()
        {
            if (!DatabaseDetails.IsFormCompleted) return;

            ConnectionStatus = ConnectionStatus.Waiting;
            if (await _sqlConnService.CheckConnectionAsync(connectionString, cancellationTokenSource.Token) == OperationResponse.Success)
            {
                ConnectionStatus = ConnectionStatus.Success;
                return;
            }
            ConnectionStatus = ConnectionStatus.Failed;
        }

        private void _dbData_DatabaseChanged(object? sender, DatabaseChangedEventArgs e)
        {
            UpdateDBDetails();
        }
        private void UpdateDBDetails()
        {
            DatabaseDetails = new()
            {
                DatabaseName = _dbData.GetDb(),
                ServerName = _dbData.GetServer(),
                Username = _dbData.GetUser(),
                Password = _dbData.GetPass()
            };
            connectionString = @"Data Source=" + DatabaseDetails.ServerName + ";Initial Catalog=" + DatabaseDetails.DatabaseName + ";User ID=" +
                    DatabaseDetails.Username + ";Password=" + DatabaseDetails.Password;
        }

        [RelayCommand]
        private void EditDB()
        {
            ConnectionStatus = ConnectionStatus.None;
        }

        [RelayCommand]
        private void ClearDB()
        {
            ConnectionStatus = ConnectionStatus.None;
            DatabaseDetails = new DatabaseAccess();
            _dbData.Drop_Server();
        }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private void ResetCancellationToken()
        {
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
        }

        [RelayCommand]
        private void CancelDBConnection()
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
                ResetCancellationToken();
                ConnectionStatus = ConnectionStatus.None;
            }
        }

        [RelayCommand]
        private async void SaveDBAccess()
        {
            string[] requiredFields = { "ServerName", "DatabaseName", "Username", "Password" };
            foreach (string field in requiredFields)
            {
                if (DatabaseDetails[field] != null)
                {
                    Functions.BringControlWithErrorIntoView(_pageService.GetPage(typeof(MainWindow)));
                    return;
                }
            }

            ConnectionStatus = ConnectionStatus.Waiting;
            await Task.Delay(200);

            string connString = connectionString = @"Data Source=" + DatabaseDetails.ServerName + ";Initial Catalog=" + DatabaseDetails.DatabaseName + ";User ID=" + DatabaseDetails.Username + ";Password=" + DatabaseDetails.Password;

            ConnectionStatus = await _sqlConnService.CheckConnectionAsync(connString, cancellationTokenSource.Token) switch
            {
                OperationResponse.Success => ConnectionStatus.Success,
                OperationResponse.Failed => ConnectionStatus.Failed,
                OperationResponse.Canceled => ConnectionStatus.None,
                _ => ConnectionStatus.None
            };
            ResetCancellationToken();

            if (ConnectionStatus == ConnectionStatus.Success)
                _dbData.AddData(DatabaseDetails.ServerName, DatabaseDetails.Username, DatabaseDetails.Password, DatabaseDetails.DatabaseName);
        }

    }

}
