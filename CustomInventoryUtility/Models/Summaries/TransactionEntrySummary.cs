using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryUtility.Models.Summaries
{
    public class TransactionEntrySummary
    {
        public int ItemID { get; set; }
        public string Description { get; set; }
        public int TransactionId { get; set; }
        public decimal Price { get; set; }
        public double Quantity { get; set;}
        public decimal Salestax { get; set; }
        public int TransactionEntryID { get; set; }
    }
}
