using System;
using System.Threading.Tasks;
using Efflux.Data;

namespace Efflux.Samples.DataFrame
{
    public class DataFrameSample
    {
        // DataFrame / DataView access method (column-based)
        public static async Task Demo(ITopic topic2)
        {
            Console.WriteLine("Efflux Messaging Sample");

            var dataFrame = new TopicDataFrame(topic2, DataSchema.From<Person>());

            var newrow = new DataRow<Person>(new Person() { FullName = "Bob Smith" });
            await dataFrame.AppendAsync(newrow);
            var newrow2 = new DataRow<Person>(new Person() { FullName = "Sally Jones" });
            await dataFrame.AppendAsync(newrow2);

            var dataview = await dataFrame.GetDataViewAsync("consumer2");

            await foreach (var row in dataview)
            {
                Console.WriteLine(row["FullName"]);
            }
        }
    }
}
