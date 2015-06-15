using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Cygnus.Common
{
    public static class Extensions
    {
        /// <summary>
        /// Wraps this object instance into an IEnumerable consisting of a single item.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="item">An IEnumerable consisting of a single item.</param>
        /// <returns></returns>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}