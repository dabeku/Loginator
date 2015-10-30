using System;
using System.Collections.Generic;
using System.Linq;

namespace Common {
    public static class LinqExtensions {

        public static IEnumerable<T> Flatten<T>(
            this IEnumerable<T> e,
            Func<T, IEnumerable<T>> f) {
            IEnumerable<T> enumerable = e as T[] ?? e.ToArray();
            return enumerable.SelectMany(c => f(c).Flatten(f)).Concat(enumerable);
        }
    }
}