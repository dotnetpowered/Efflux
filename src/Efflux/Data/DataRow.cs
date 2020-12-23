using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Efflux.Data
{
    public class DataRow<T> : IDataRow where T : class
    {
        readonly Type t;

        public DataRow()
        {
            Value = default;
            t = typeof(T);
        }

        public DataRow(T value)
        {
            Value = value;
            t = typeof(T);
        }

        public T Value { get; internal set; }

        public virtual IEnumerable<string> Keys => from p in t.GetProperties() select p.Name;

        public virtual object this[string s] => t.GetProperty(s).GetValue(Value);


    }

    public class DataRow : DataRow<Dictionary<string, object>>, IDataRow
    {
        public DataRow(EffluxMessage message)
        {
            Console.WriteLine("Message=" + message.PayloadAsString());
            if (message.MetaData.ContentType == "application/json")
                this.Value = JsonSerializer.Deserialize<Dictionary<string, object>>(message.PayloadAsBytes());
            else
                throw new InvalidDataException();
            
        }

        public DataRow() : base(new Dictionary<string, object>())
        {
        }

        public DataRow(Dictionary<string, object> values) : base(values)
        {
        }

        public override object this[string s] => Value[s];

        public override IEnumerable<string> Keys => Value.Keys;
    }

    public static class DataRowExtension
    {
        public static EffluxMessage ToMessage(this IDataRow dataRow)
        {
            var builder = new List<string>();

            foreach (var key in dataRow.Keys)
            {
                // TODO: data types
                // TODO: escape strings
                builder.Add($"\"{key}\":\"{dataRow[key]}\"");
            }

            var data = "{" + string.Join(',', builder) + "}";
            var bytes = Encoding.UTF8.GetBytes(data);
            var message = new EffluxMessage(bytes, "JSON");
            return message;
        }

        public static async Task<DataRowResult> ReadDataRowAsync(this ITopicConsumer consumer, bool autoCommit = false)
        {
            var readResult = await consumer.ReadMessageAsync();
            var result = new DataRowResult();
            if (!readResult.EndOfStream)
            {
                result.Row = new DataRow(readResult.Message);
            }
            result.NextOffset = readResult.NextMessageOffset;
            result.EndOfStream = readResult.EndOfStream;
            return result;
        }
    }
}
