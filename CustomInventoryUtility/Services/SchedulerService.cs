using InventoryUtility.Models;
using InventoryUtility.Models.Functionalities;
using InventoryUtility.Services.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sqlite;
using Sqlite.Models;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace InventoryUtility.Services;

public partial class SchedulerService : IHostedService
{

    private readonly DBData DBData;
    private readonly SqlConnectionService SqlConnectionService;
    private readonly StoreDataTransferExecutions StoreDataTransferExecutions;
    private readonly StoreDataTransferFunctionality StoreDataTransferFunctionality;

    private TimeSpan HOUR_TO_SCHEDULE = new(1, 0, 0);
    private Timer Timer = new Timer();


    public SchedulerService(DBData dbData, 
                            SqlConnectionService sqlConnService,
                            StoreDataTransferExecutions storeDataTransferExecutions,
                            StoreDataTransferFunctionality storeDataTransferFunctionality)
    {
        DBData = dbData;
        SqlConnectionService = sqlConnService;
        StoreDataTransferExecutions = storeDataTransferExecutions;
        StoreDataTransferFunctionality = storeDataTransferFunctionality;
    }

    public async Task StartAsync(System.Threading.CancellationToken cancellationToken)
    {
        if (StoreDataTransferExecutions != null)
        {
            Start();
        }

        DBData.DatabaseChanged += async (object sender, DatabaseChangedEventArgs e) =>
        {
            try
            {
                if (e.NewStatus && !Timer.Enabled)
                {
                    ResetTimer();
                }
                else if (!e.NewStatus && Timer.Enabled)
                {
                    Timer.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while resetting the timer: {ex.Message}");
            }
        };

        await Task.CompletedTask;
    }

    public Task StopAsync(System.Threading.CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private void Start()
    {
        Timer.Elapsed += Timer_Tick;
        Timer.AutoReset = false;
        Timer.Start();
    }

    private async void Timer_Tick(object sender, ElapsedEventArgs e)
    {
        if (SqlConnectionService.ConnectionStatus != OperationResponse.Success)
        {
            Timer.Stop();
            return;
        }

        DateTime currentTime = DateTime.Now;

        var lastExecuted = await StoreDataTransferExecutions.GetLastSuccessfulExecution();

        if (lastExecuted != null)
        {
            DateTime lastExecutedDateTime = new DateTime().AddTicks(lastExecuted.ExecutionTime);
            if (lastExecutedDateTime.Date == currentTime.Date)
            {
                ResetTimer();
                return;
            }
        }

        if (currentTime.TimeOfDay < HOUR_TO_SCHEDULE) {
            ResetTimer();
            return;
        };

        try {
            await StoreDataTransferFunctionality.TransferStoreData(currentTime);
        } catch (Exception ex) {
            Console.WriteLine("");
        }
        ResetTimer();
    }

    private void ResetTimer()
    {
        Timer.Stop();
        Timer.AutoReset = true;
        Timer.Interval = 60000;
        Timer.Start();
    }

    //public bool IsCashierLinkedToAJob(int cashierID)
    //{
    //    var jobsSqliteService = new SqliteServer.Services.ScheduledJobsService();
    //    var sqliteJobRecords = jobsSqliteService.GetAll();

    //    foreach (var sqliteJobType in sqliteJobRecords)
    //    {
    //        if (sqliteJobType.SelectedCashierID == cashierID)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}
}