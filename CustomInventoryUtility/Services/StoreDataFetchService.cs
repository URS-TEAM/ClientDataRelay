using InventoryUtility.Models.Summaries;
using Sqlite;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using System;
using Void = InventoryUtility.Models.Summaries.Void;

namespace InventoryUtility.Services;
public class StoreDataFetchService
{

    private DBData DBData;
    private string connectionString;

    public StoreDataFetchService(DBData dBData)
    {
        this.DBData = dBData;
        UpdateConnectionString();
        DBData.DatabaseChanged += DBData_DatabaseChanged;
    }

    private void DBData_DatabaseChanged(object? sender, DatabaseChangedEventArgs e)
    {
        UpdateConnectionString();
    }

    private void UpdateConnectionString()
    {
        connectionString = @"Data Source=" + DBData.GetServer() + ";Initial Catalog=" + DBData.GetDb() + ";User ID=" +
        DBData.GetUser() + ";Password=" + DBData.GetPass();
    }

    public async Task<DateTime?> GetDateTimeAsync()
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            SqlCommand command = new SqlCommand("SELECT GETDATE();", connection);
            SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                DateTime dateTime = reader.GetDateTime(0);
                return dateTime;
            }
        }
        return null;
    }


    // Stores
    public async Task<List<Store>> FetchStores()
    {
        List<Store> stores = new List<Store>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT StoreID, StoreName, StoreAddress1, StoreCity FROM Configuration";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {

                    Store store = new Store
                    {
                        StoreName = reader.GetString(1),
                        StoreAddress = reader.GetString(2),
                        StoreCity = reader.GetString(3),
                        Time = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    };

                    stores.Add(store);
                }
            }
        }
        return stores;
    }


    //TOTAL SALES NOW
    public async Task<List<Sale>> FetchSalesMadeIn(DateTime queryDate)
    {
        List<Sale> sales = new List<Sale>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            string date = queryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var query = "SELECT TransactionNumber, ISNULL(Cashier.ID,0) AS CashierID, CAST(total AS decimal(18, 2)), CAST(salestax AS decimal(18, 2)), time, Cashier.Name, RegisterID, RegisterID FROM [Transaction] left join Batch on [Transaction].BatchNumber = Batch.BatchNumber " +
            "left join Cashier on CashierID = Cashier.ID where time between CONVERT(datetime,'" + date + " 00:00:01.000',101) and  CONVERT(datetime,'" + date + " 23:59:01.000',101)";
            using (SqlCommand command = new SqlCommand(query, connection))
            {

                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Sale sale = new Sale
                    {
                        TransactionNumber = reader.GetInt32(0),
                        CashierID = reader.GetInt32(1),
                        Total = reader.GetDecimal(2),
                        SalesTax = reader.GetDecimal(3),
                        Time = reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        CashierName = reader.GetString(5),
                        RegisterID = reader.GetInt32(6)
                    };
                    sales.Add(sale);


                }
            }
        }
        return sales;
    }


    //Employee Voids
    public async Task<List<Void>> FetchVoidsMadeIn(DateTime queryDate)
    {
        List<Void> voids = new List<Void>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            string date = queryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);


            var query = "SELECT ISNULL(COUNT(journal.Id),0) as Quantity, CashierID, Cashier.Name, " +
                            "transactiontype, MAX(time) as Time FROM [Journal] inner join Cashier on cashier.ID = journal.CashierID " +
                            "where Time BETWEEN CONVERT(datetime,'" + date + " 00:00:00.000',101) AND CONVERT(datetime,'" + date + " 23:59:00.000',101) " +
                            "group by cashierid, name, transactiontype";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Void @void = new Void
                    {
                        Quantity = reader.GetInt32(0),
                        CashierID = reader.GetInt32(1),
                        CashierName = reader.GetString(2),
                        TransactionType = reader.GetInt32(3),
                        Time = reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    };
                    voids.Add(@void);
                }

            }
        }
        return voids;
    }

    //DEPARTMENT LIST
    public async Task<List<Department>> FetchDepartamentList()
    {
        List<Department> transactions = new List<Department>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            var query = "SELECT ID, Name, code FROM Department";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Department transaction = new Department
                    {
                        id = reader.GetInt32(0),
                        Name = (string.IsNullOrEmpty(reader.GetString(1)) ? "NO DEPARTMENT" : reader.GetString(1)),
                        code = reader.GetString(2),
                    };

                    transactions.Add(transaction);
                }
                return transactions;
            }
        }
        return transactions;
    }

    //DEPARTMENT SALES
    public async Task<List<DepartmentSale>> FetchDepartmentSaleMadeIn(int deptId, string dateStr)
    {

        List<DepartmentSale> deptSales = new List<DepartmentSale>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {

            await connection.OpenAsync();


            var query = "SELECT Item.DepartmentID, department.name,sum([TransactionEntry].[Quantity]) as Quantity, ISNULL(SUM([TransactionEntry].[Price]*             [TransactionEntry].[Quantity]),0) as Sales" +
                        ",ISNULL(sum([TransactionEntry].[SalesTax]),0) as SalesTax, MAX([Transaction].Time) AS Time FROM [TransactionEntry]" +
                        " left join [Transaction] on [TransactionEntry].TransactionNumber = [Transaction].TransactionNumber" +
                        " left join item on item.ID = [TransactionEntry].ItemID left join Department on item.DepartmentID = Department.id" +
                        "   where [Transaction].time between  CONVERT(datetime,'" + dateStr + " 00:00:00.000',101) and  CONVERT(datetime,'" + dateStr + " 23:59:01.000',101) and  departmentId = '" + deptId + "' group by Item.DepartmentID, department.name";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    DepartmentSale deptSale = new DepartmentSale
                    {
                        DepartamentId = reader.GetInt32(0),
                        DepartamentName = (string.IsNullOrEmpty(reader.GetString(1)) ? "NO DEPARTMENT" : reader.GetString(1)),
                        QuantityDepartament = reader.GetDouble(2),
                        DepartmentSalesNow = reader.GetDouble(3),
                        Time = dateStr,
                    };
                    deptSales.Add(deptSale);
                }
                return deptSales;
            }
        }
    }

    public async Task<List<DepartmentSale>> FetchDepartmentSalesMadeIn(List<Department> departments, DateTime queryDate)
    {
        List<DepartmentSale> resultingList = new();
        string date = queryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        foreach (Department dept in departments)
        {
            resultingList.AddRange(await FetchDepartmentSaleMadeIn(dept.id, date));
        }

        return resultingList;
    }

    //HOURS SALE
    public async Task<HourSales> FetchSalesMadeInByHour(string date, string hour)
    {
        HourSales summary = null;

        string query = "SELECT ISNULL(CAST(SUM([Total]) AS decimal(18, 2)), 0 ) [Total], " +
                       "'" + hour + ":00:00' [InitialHour] ,'" + hour + ":59:59' [FinalHour], COUNT(TransactionNumber) As tranQuantity FROM [dbo].[Transaction] WHERE " +
                       " [Time] BETWEEN CONVERT(datetime,'" + date + " " + hour + ":00:00',101) AND  CONVERT(datetime,'" + date + " " + hour + ":59:59',101)";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        summary = new HourSales
                        {
                            total = reader.GetDecimal(0),
                            InitialHour = reader.GetString(1),
                            FinalHour = reader.GetString(2),
                            TransQuantity = reader.GetInt32(3),
                        };
                    }

                }
            }
        }
        return summary;
    }

    public async Task<List<HourSales>> FetchHourSalesMadeInDay(DateTime queryDate)
    {
        List<HourSales> hoursSales = new List<HourSales>();

        string date = queryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string[] arr = { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23" };

        foreach (string hour in arr)
        {
            HourSales summary = await FetchSalesMadeInByHour(date, hour);
            if (summary != null && summary.total > 0)
            {
                hoursSales.Add(summary);
            }
        }

        return hoursSales;
    }

    // TENDER TOTAL

    public async Task<List<int>> FetchTendersId()
    {
        List<int> tendersIDList = new List<int>();
        var query1 = "SELECT ISNULL(ID,0) as TenderID FROM [Tender] Order By ID";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand(query1, connection))
            {
                SqlDataReader read = await command.ExecuteReaderAsync();
                while (read.Read())
                {
                    tendersIDList.Add(read.GetInt32(0));
                }
            }
        }
        return tendersIDList;
    }

    //TODAS LAS TRANSACCIONES DE UN DIA, SUMMARY DE TOTAL PAGADO POR CADA TENDER
    public async Task<List<TransactionTenderSummary>> FetchDayTransactionsTenderSummary(DateTime queryDate)
    {
        string date = queryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        List<TransactionTenderSummary> tenders = new List<TransactionTenderSummary>();
        List<int> tendersIdList = await FetchTendersId();

        if (tendersIdList.Count <= 0) return tenders;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            string resultTender = string.Join(", ", tendersIdList);
            var query2 = "SELECT TenderEntry.TenderID, Tender.Description, SUM(TenderEntry.Amount), [Transaction].TransactionNumber," +
                            " MAX([Transaction].Time) FROM [Transaction]" +
                            " left join TenderEntry on TenderEntry.TransactionNumber = [Transaction].TransactionNumber" +
                            " left join Tender on Tender.ID = tenderentry.TenderID where [Transaction].time between  CONVERT(datetime,'" + date + " 00:00:01.000',101) and  " +
                            " CONVERT(datetime,'" + date + " 23:59:59.000',101) and  tenderentry.TenderId in (" + resultTender + ") GROUP BY [Transaction].TransactionNumber, tenderentry.TenderID, tender.Description " +
                            " Order By tenderentry.TenderID";

            await connection.CloseAsync();
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand(query2, connection))
            {
                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    TransactionTenderSummary transctTenderSummary = new TransactionTenderSummary
                    {
                        TenderID = reader.GetInt32(0),
                        Description = reader.GetString(1),
                        TotalAmount = reader.GetDecimal(2),
                        TransationNumber = reader.GetInt32(3),
                        Time = reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),

                    };
                    tenders.Add(transctTenderSummary);
                }
            }
        }
        return tenders;
    }

    //ITEM SALE

    public async Task<List<ItemSalesSummary>> FetchDaySalesGroupedByItem(DateTime queryDate)
    {

        List<ItemSalesSummary> allItemsSales = new List<ItemSalesSummary>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string date = queryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            await connection.OpenAsync();

            var query = "SELECT TransactionEntry.ItemID as itemId, Item.Description as description," +
                            " departmentid, item.price, ISNULL(SUM([TransactionEntry].[Quantity]),0) as quantity, " +
                            " ISNULL(SUM([TransactionEntry].[Price] *[TransactionEntry].[Quantity]),0) as sales, " +
                            " ISNULL(SUM([TransactionEntry].[Cost] *[TransactionEntry].[Quantity]),0) as cost " +
                            " FROM TransactionEntry INNER JOIN [Transaction] ON TransactionEntry.TransactionNumber = [Transaction].TransactionNumber " +
                            " INNER JOIN Item ON TransactionEntry.ItemID = Item.ID LEFT JOIN Department ON Item.DepartmentID = Department.id   " +
                            " WHERE [Transaction].Time BETWEEN CONVERT(datetime, '" + date + " 00:00:00.000', 101) AND CONVERT(datetime,'" + date + " 23:59:59.000',101)" +
                            " GROUP BY TransactionEntry.ItemID, Item.Description, item.DepartmentID, item.price";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ItemSalesSummary itemSales = new ItemSalesSummary
                    {
                        ItemID = reader.GetInt32(0),
                        Description = reader.GetString(1),
                        DepartamentID = reader.GetInt32(2),
                        Price = reader.GetDecimal(3),
                        Quantity = reader.GetDouble(4),
                        Total = reader.GetDouble(5),
                        Cost = reader.GetDouble(6),
                    };

                    allItemsSales.Add(itemSales);
                }
            }
        }
        return allItemsSales;
    }

    // Transaction Entry
    public async Task<List<TransactionEntrySummary>> FetchTransactionEntriesMadeIn(DateTime queryDate)
    {

        List<TransactionEntrySummary> transctEntries = new List<TransactionEntrySummary>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {

            await connection.OpenAsync();
            string date = queryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);


            var query = "SELECT [Transaction].TransactionNumber, TransactionEntry.ItemID as itemId, Item.Description as description," +
                            " [TransactionEntry].[Quantity], transactionentry.Price, transactionentry.Cost, [TransactionEntry].salestax, TransactionEntry.ID as transactionentryId FROM TransactionEntry LEFT JOIN [Transaction]" +
                            " ON TransactionEntry.TransactionNumber = [Transaction].TransactionNumber INNER JOIN Item ON TransactionEntry.ItemID = Item.ID LEFT JOIN" +
                            " Department ON Item.DepartmentID = Department.id WHERE[Transaction].Time BETWEEN CONVERT(datetime, '" + date + " 00:00:00.000', 101) AND " +
                            " CONVERT(datetime,'" + date + " 23:59:59.000',101)";
            using (SqlCommand command = new SqlCommand(query, connection))
            {

                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    TransactionEntrySummary trasnctEntry = new TransactionEntrySummary
                    {
                        TransactionId = reader.GetInt32(0),
                        ItemID = reader.GetInt32(1),
                        Description = reader.GetString(2),
                        Quantity = reader.GetDouble(3),
                        Price = reader.GetDecimal(4),
                        Salestax = reader.GetDecimal(6),
                        TransactionEntryID = reader.GetInt32(7),

                    };
                    if (trasnctEntry.Quantity != 0)
                        transctEntries.Add(trasnctEntry);
                }
            }
        }
        return transctEntries;
    }

    // TAX DETAIL
    public async Task<List<TaxSummary>> FetchTaxesSummaries(DateTime queryDate)
    {
        List<TaxSummary> taxes = new List<TaxSummary>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {

            await connection.OpenAsync();
            string date = queryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);


            var query = "SELECT ISNULL(NULLIF(taxid,''),0) as taxid, ISNULL(itemtax.CODE, 'UNDEFINED') as Description, SUM([TransactionEntry].SalesTax) AS tax," +
                            " SUM(transactionentry.Price * transactionentry.Quantity) as taxamount FROM [Transaction] " +
                            " left join[TransactionEntry] on [Transaction].TransactionNumber = [TransactionEntry].TransactionNumber " +
                            " left join Item on transactionentry.ItemID = item.ID left join ItemTax on item.TaxID = itemtax.ID " +
                            " where[Transaction].Time BETWEEN CONVERT(datetime, '" + date + " 00:00:00.000', 101) " +
                            " AND CONVERT(datetime, '" + date + " 23:59:59.000', 101) group by TaxID, itemtax.CODE order by TaxID";
            using (SqlCommand command = new SqlCommand(query, connection))
            {

                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    TaxSummary tax = new TaxSummary
                    {
                        TaxID = reader.GetInt32(0),
                        TaxDescription = reader.GetString(1),
                        Tax = reader.GetDecimal(2),
                        TotalToTax = reader.GetDouble(3),
                    };
                    taxes.Add(tax);
                }
            }
        }
        return taxes;
    }
}