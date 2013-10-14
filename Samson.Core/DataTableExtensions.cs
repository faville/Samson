using Samson.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Samson.Core
{
    public static class DataTableExtensions
    {
        public static IList<T> ToList<T>(this DataTable table) where T : BaseModel, new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            IList<T> result = new List<T>();

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties);
                result.Add(item);
            }

            return result;
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : BaseModel, new()
        {
            T item = new T();
            foreach (var property in properties)
            {              
                var battr = property.GetCustomAttribute<BrowsableAttribute>();
                if (battr != null)
                {
                    if (battr.Browsable)
                    {
                        if (row[((ColumnAttribute)property.GetCustomAttribute(typeof(ColumnAttribute))).Name] != System.DBNull.Value)
                            property.SetValue(item, row[((ColumnAttribute)property.GetCustomAttribute(typeof(ColumnAttribute))).Name], null);
                    }
                    continue;
                }
                   
                if (row[((ColumnAttribute)property.GetCustomAttribute(typeof(ColumnAttribute))).Name] != System.DBNull.Value)
                    property.SetValue(item, row[((ColumnAttribute)property.GetCustomAttribute(typeof(ColumnAttribute))).Name], null);
            }

            item.BeginTrackingChanges(); //start tracking changes after all inital properties have been set.

            return item;
        }
    }
}
