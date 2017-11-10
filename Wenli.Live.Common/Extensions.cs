using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.Common
{
    public static class Extensions
    {
        public static IList ToList(this IEnumerable enumerable)
        {
            var list = new List<object>();
            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
                list.Add(enumerator.Current);
            return list;
        }
    }
}
