using CommunityToolkit.Mvvm.Input;
using InventoryUtility.Models.Summaries;
using System;
using System.Threading.Tasks;
using Wpf.Ui;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;
using InventoryUtility.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Diagnostics;

namespace InventoryUtility.Models.Functionalities
{

    public partial class StoreDataTransferFunctionality : BaseFunctionality, IFunctionality
    {

        private readonly IContentDialogService ContentDialogService;
        private readonly StoreDataFetchService StoreDataFetchService;
        public DateTime SelectedDate { get; set; } = DateTime.Now;

        public StoreDataTransferFunctionality(
            string description,
            string buttonContent,
            IContentDialogService contentDialogService,
            StoreDataFetchService storeDataFetchService) :
            base(description, buttonContent)
        {
            this.ContentDialogService = contentDialogService;
            this.StoreDataFetchService = storeDataFetchService;

            this.ActivateFuncCommand = new RelayCommand(InitiateStoreDataTransfer);
        }

        public async Task TransferStoreData(DateTime selectedDate)
        {
            StoresDataTransferModel data = await FetchStoreDataFromDB(selectedDate);
            string resultingJson = JsonConvert.SerializeObject(data);
            Debug.WriteLine(resultingJson);
            using (var wb = new WebClient())
            {
                wb.Headers[HttpRequestHeader.ContentType] = "application/json";
                string uploadResponse = await wb.UploadStringTaskAsync("https://localhost:7066/api/Stores", resultingJson);
            }
        }

        private async void InitiateStoreDataTransfer()
        {
            try
            {
                BtnState = ButtonState.Waiting;
                await TransferStoreData(SelectedDate);

                BtnState = ButtonState.Done;
                await ContentDialogService.ShowAlertAsync("Notification", "Data extraction done successfully!", "Close");
            }
            catch (Exception ex)
            {
                BtnState = ButtonState.Default;
                Console.WriteLine($"An error occurred: {ex.Message}");
                await ContentDialogService.ShowAlertAsync("Notification", "Something went wrong, please try again later...", "Close");
            }
            return;
        }

        private async Task<StoresDataTransferModel> FetchStoreDataFromDB(DateTime queryTime)
        {

            Task<List<Store>> storeTask = StoreDataFetchService.FetchStores();
            Task<List<Sale>> salesTask = StoreDataFetchService.FetchSalesMadeIn(queryTime);
            Task<List<Summaries.Void>> voidsTask = StoreDataFetchService.FetchVoidsMadeIn(queryTime);

            List<Department> departmentList = await StoreDataFetchService.FetchDepartamentList();
            Task<List<DepartmentSale>> departmentSalesTask = StoreDataFetchService.FetchDepartmentSalesMadeIn(departmentList, queryTime);
            Task<List<HourSales>> hoursSalesTask = StoreDataFetchService.FetchHourSalesMadeInDay(queryTime);
            Task<List<TransactionTenderSummary>> tendersTask = StoreDataFetchService.FetchDayTransactionsTenderSummary(queryTime);
            Task<List<ItemSalesSummary>> itemsSalesTask = StoreDataFetchService.FetchDaySalesGroupedByItem(queryTime);
            Task<List<TransactionEntrySummary>> transactionEntriesTask = StoreDataFetchService.FetchTransactionEntriesMadeIn(queryTime);

            Task<List<TaxSummary>> taxesTask = StoreDataFetchService.FetchTaxesSummaries(queryTime);

            await Task.WhenAll(
                salesTask,
                voidsTask,
                storeTask,
                departmentSalesTask,
                hoursSalesTask,
                tendersTask,
                itemsSalesTask,
                transactionEntriesTask,
                taxesTask,
                Task.Delay(200));

            List<Sale> salesResult = salesTask.Result;
            List<Summaries.Void> voidsResult = voidsTask.Result;
            List<Store> storesResult = storeTask.Result;
            List<DepartmentSale> departamentSalesResult = departmentSalesTask.Result;
            List<HourSales> hourSalesResult = hoursSalesTask.Result;
            List<TransactionTenderSummary> tenderTotalResult = tendersTask.Result;
            List<ItemSalesSummary> itemsSalesResult = itemsSalesTask.Result;
            List<TransactionEntrySummary> transactionEntriesResult = transactionEntriesTask.Result;

            List<TaxSummary> taxesResult = taxesTask.Result;

            StoresDataTransferModel storeDataTransferModel = new()
            {
                Stores = storesResult,
                Sales = salesResult,
                Voids = voidsResult,
                Departaments = departmentList,
                DepartmentSales = departamentSalesResult,
                HourSalesSummaries = hourSalesResult,
                TenderTotalSummaries = tenderTotalResult,
                ItemSalesSummaries = itemsSalesResult,
                TransactionEntriesSummaries = transactionEntriesResult,

                TaxDetailsSummaries = taxesResult,
            };
            return storeDataTransferModel;
        }


    }
}
