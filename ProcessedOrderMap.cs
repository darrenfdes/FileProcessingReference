using System;
using CsvHelper.Configuration;

namespace sandbox
{
    public class ProcessedOrderMap : ClassMap<ProcessedOrder>
    {
        public ProcessedOrderMap()
        {
            AutoMap(System.Globalization.CultureInfo.InvariantCulture);
            //Map(po => po.OrderNumber).Name("OrderNumber");
            Map(po => po.Customer).Name("CustomerNumber");
            Map(po => po.Amount).Name("Quantity").TypeConverter<RomanTypeConverter>();
        }
    }
}