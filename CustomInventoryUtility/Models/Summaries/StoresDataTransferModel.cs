using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryUtility.Models.Summaries;
public class StoresDataTransferModel
{
    //DATA FROM SPECIFIC DAY TO SEND TO API
    //public string StoreName { get; set; }
    public List<Store> Stores { get; set; }
    public List<Sale> Sales { get; set; }
    public List<Void> Voids { get; set; }
    public List<Department> Departaments { get; set; }

    public List<DepartmentSale> DepartmentSales { get; set; }
    public List<HourSales> HourSalesSummaries { get; set; }
    public List<TransactionTenderSummary> TenderTotalSummaries { get; set; }
    public List<ItemSalesSummary> ItemSalesSummaries { get; set; }
    public List<TransactionEntrySummary> TransactionEntriesSummaries { get; set; }
    public List<TaxSummary> TaxDetailsSummaries { get; set; }

}