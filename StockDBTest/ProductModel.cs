using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace StockDBTest
{
    [DynamoDBTable ("ProductCatalog")] 
    internal class ProductModel
    {
        
        public string type { get; set; }
        
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string DepartmentName { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
}
