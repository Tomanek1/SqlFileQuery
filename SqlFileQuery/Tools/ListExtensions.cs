using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlFileQuery.Tools
{
    public static class ListExtensions
    {
        /// <summary>
        /// Extension metoda pro <see cref="List{T}"/> která ho umožní procházet po částech
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="itemsPerSet"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this ICollection<T> source, int itemsPerSet)
        {
            //var sourceList = source as List<T> ?? source.ToList();
            for (var index = 0; index < source.Count; index += itemsPerSet)
            {
                yield return source.Skip(index).Take(itemsPerSet);
            }
        }
    }
}
