using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryUtility.Models.Summaries
{
    public class DepartmentSale
    {
        public int DepartamentId { get; set; }
        public string DepartamentName { get; set; }
        public double QuantityDepartament { get; set; }
        public double DepartmentSalesNow { get; set; }
        public string Time { get; set; }
    }
}
