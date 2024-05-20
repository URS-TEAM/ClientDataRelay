using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryUtility.Models.Summaries
{
    public class ItemSalesSummary
    {
        public int ItemID { get; set; }
        public int DepartamentID { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public double Quantity { get; set;}
        public double Total {  get; set; }
        public double Cost { get; set; }
    }

}
