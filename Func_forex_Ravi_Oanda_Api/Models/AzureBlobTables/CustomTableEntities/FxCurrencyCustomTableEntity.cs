//using Microsoft.WindowsAzure.Storage.Table;
//using Microsoft.WindowsAzure.Storage;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables.CustomTableEntities
//{
//    public class FxCurrencyCustomTableEntity : TableEntity
//    {
//        private const string DecimalPrefix = "D_";

//        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
//        {
//            var entityProperties = base.WriteEntity(operationContext);
//            var objectProperties = GetType().GetProperties();

//            foreach (var item in objectProperties.Where(f => f.PropertyType == typeof(decimal)))
//            {
//                entityProperties.Add(DecimalPrefix + item.Name, new EntityProperty(item.GetValue(this, null).ToString()));
//            }

//            return entityProperties;
//        }
//        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
//        {
//            base.ReadEntity(properties, operationContext);
//            var objectProperties = GetType().GetProperties();
//            foreach (var item in objectProperties.Where(f => f.Name.StartsWith(DecimalPrefix)))
//            {
//                var value = decimal.Parse(item.GetValue(this, null).ToString());
//                item.SetValue(this, value);
//            }
//        }
//    }
//}
