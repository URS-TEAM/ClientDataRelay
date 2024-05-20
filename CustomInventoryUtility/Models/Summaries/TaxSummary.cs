using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryUtility.Models.Summaries
{
    public  class TaxSummary
    {
       public int  TaxID {  get; set; }
       public string TaxDescription { get; set; }
       public decimal Tax { get; set; }
       public double TotalToTax {  get; set; }

    }
}
