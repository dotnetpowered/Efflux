using System;

namespace Efflux.Data
{
    // TODO: implement other data types
    public enum DataType
    {
        Int,
        Int64,
        String
    }

    public static class DataTypeConverter
    {
        public static Type ToType(DataType dt)
        {
            switch (dt)
            {
                case DataType.Int:
                    return typeof(int);
                case DataType.Int64:
                    return typeof(long);
                case DataType.String:
                    return typeof(string);
            }
            throw new InvalidOperationException();
        }

        public static DataType FromType(Type t)
        {
            if (t == typeof(int))
               return DataType.Int;
            if (t == typeof(long))
                return DataType.Int64;
            if (t == typeof(string))
                return DataType.String;
            throw new InvalidOperationException();
        }
    }

    public class DataSchemaColumn
    {
        public DataSchemaColumn()
        {
        }

        public string Name { get; set; }
        public DataType DataType { get; set; }
    }
}
