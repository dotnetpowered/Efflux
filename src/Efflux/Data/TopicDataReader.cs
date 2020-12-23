using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace Efflux.Data
{
    public class TopicDataReader : IDataReader
    {
        private bool disposedValue;
        DataSchemaColumn[] Columns;
        DataSchema schema;
        ITopicConsumer consumer;
        DataTable schemaTable;
        IDictionary<int, (DataSchemaColumn column, TypeConverter converter, Type type)> indexColumnMap =
            new Dictionary<int, (DataSchemaColumn, TypeConverter, Type)>();
        IDictionary<string, (int index, DataSchemaColumn column, TypeConverter converter, Type type)> nameColumnMap =
            new Dictionary<string, (int, DataSchemaColumn, TypeConverter, Type)>();

        internal TopicDataReader(ITopicConsumer consumer, DataSchema schema)
        {
            Columns = schema.Columns.ToArray();
            this.schema = schema;
            this.consumer = consumer;
            this.schemaTable = new DataTable();
            var i = 0;
            foreach (var col in schema.Columns)
            {
                var type = DataTypeConverter.ToType(col.DataType);
                var converter = TypeDescriptor.GetConverter(type);
                indexColumnMap.Add(i, (col, converter, type));
                nameColumnMap.Add(col.Name, (i, col, converter, type));
                schemaTable.Columns.Add(col.Name, DataTypeConverter.ToType(col.DataType));
                i++;
            }
        }

        public string GetName(int i) => Columns[i].Name;
        public DataSchema GetSchema() => schema;
        public IDataRow Current { get; private set; }
        public long Position { get; set; }
        public int FieldCount => Columns.Length;
        public DataTable GetSchemaTable() => schemaTable;
        public int Depth => 1; // TODO: support nested data?
        public bool NextResult() => false; // TODO: support multiple result sets?
        public int RecordsAffected => throw new NotImplementedException();

        public bool Read()
        {
            var readResult = consumer.ReadMessageAsync();
            readResult.Wait();
            Current = new DataRow(readResult.Result.Message);
            Position = readResult.Result.NextMessageOffset;
            return !readResult.Result.EndOfStream;
        }

        public string GetDataTypeName(int i) => indexColumnMap[i].type.FullName;
        public int GetOrdinal(string name) => nameColumnMap[name].index;
        public object this[string name] => Current[name];
        public object this[int i] => this[indexColumnMap[i].column.Name];

        public bool IsDBNull(int i) => string.IsNullOrEmpty(GetString(i));

        public bool GetBoolean(int i)
        {
            var col = indexColumnMap[i];
            return (bool) col.converter.ConvertTo(this[col.column.Name], col.type);
        }

        public byte GetByte(int i)
        {
            var col = indexColumnMap[i];
            return (byte)col.converter.ConvertTo(this[col.column.Name], col.type);
        }

        public char GetChar(int i)
        {
            var (column, converter, type) = indexColumnMap[i];
            return (char)converter.ConvertTo(this[column.Name], type);
        }

        public Type GetFieldType(int i)
        {
            var (_, _, type) = indexColumnMap[i];
            return type;
        }

        public DateTime GetDateTime(int i)
        {
            var (column, converter, type) = indexColumnMap[i];
            return (DateTime)converter.ConvertTo(this[column.Name], type);
        }

        public decimal GetDecimal(int i)
        {
            var (column, converter, type) = indexColumnMap[i];
            return (decimal)converter.ConvertTo(this[column.Name], type);
        }

        public double GetDouble(int i)
        {
            var (column, converter, type) = indexColumnMap[i];
            return (double)converter.ConvertTo(this[column.Name], type);
        }

        public float GetFloat(int i)
        {
            var (column, converter, type) = indexColumnMap[i];
            return (float)converter.ConvertTo(this[column.Name], type);
        }

        public Guid GetGuid(int i)
        {
            var (column, converter, type) = indexColumnMap[i];
            return (Guid)converter.ConvertTo(this[column.Name], type);
        }

        public short GetInt16(int i)
        {
            var (column, converter, type) = indexColumnMap[i];
            return (Int16)converter.ConvertTo(this[column.Name], type);
        }

        public int GetInt32(int i)
        {
            var (column, converter, type) = indexColumnMap[i];
            return (int)converter.ConvertTo(this[column.Name], type);
        }

        public long GetInt64(int i)
        {
            var (column, converter, type) = indexColumnMap[i];
            return (long)converter.ConvertTo(this[column.Name], type);
        }

        public string GetString(int i)
        {
            var (column, converter, type) = indexColumnMap[i];
            return (string)converter.ConvertTo(this[column.Name], type);
        }

        public object GetValue(int i)
        {
            var (column, converter, type) = indexColumnMap[i];
            return converter.ConvertTo(this[column.Name], type);
        }

        public int GetValues(object[] values)
        {
            for (int i = 0; i < Columns.Length; i++)
                values[i] = GetValue(i);
            return Columns.Length;
        }

        ///

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public bool IsClosed => false;

        public void Close()
        {
            throw new NotImplementedException();
        }
        public IDataReader GetData(int i) => throw new NotImplementedException();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TopicDataReader()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
