using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryUtility.Models.Summaries
{
    public class Void
    {
        public int Quantity { get; set; }
        public int CashierID { get; set; }
        public string CashierName { get; set; }
        public int TransactionType { get; set; }
        public string Time { get; set; }
    }
}
