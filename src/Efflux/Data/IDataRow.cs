using System.Collections.Generic;

namespace Efflux.Data
{
    public interface IDataRow
    {
        object this[string index]
        {
            get;
        }

        IEnumerable<string> Keys { get; }
    }
}
