using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryUtility.Models.Summaries
{
    public class Sale
    {
        public int TransactionNumber { get; set; }
        public int CashierID { get; set; }
        public decimal Total { get; set; }
        public decimal SalesTax { get; set; }
        public string Time { get; set; }
        public string CashierName { get; set; }
        public int RegisterID { get; set; }
    }
}
