using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryUtility.Models.Summaries
{
    public class HourSales
    {
        public decimal total { get; set; }
        public decimal SalesTax { get; set; }
        public string InitialHour { get; set; }
        public string FinalHour { get; set; }
        public int TransQuantity { get; set; }
        public string Time { get; set; }  
    }
    
}
