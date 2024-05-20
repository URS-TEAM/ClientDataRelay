using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryUtility.Models.Summaries
{
    public class TransactionTenderSummary
    {
        public int TenderID { get; set; }
        public string Description { get; set; }
        public decimal TotalAmount { get; set; }
        public int TransationNumber { get; set; }
        public string Time { get; set; }

    }
}
