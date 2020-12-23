using System;
using System.Collections.Generic;
using System.Data;

namespace Efflux.Data
{
    public class DataSchema
    {
        public DataSchema()
        {
        }

        private readonly List<DataSchemaColumn> columns = new List<DataSchemaColumn>();

        public IList<DataSchemaColumn> Columns => columns;

        public static DataSchema From<T>() where T : class
        {
            var schema = new DataSchema();

            foreach (var p in typeof(T).GetProperties())
            {
                schema.Columns.Add(new DataSchemaColumn()
                { Name = p.Name,
                    DataType = DataTypeConverter.FromType(p.PropertyType) });
            }

            return schema;
        }
    }
}
