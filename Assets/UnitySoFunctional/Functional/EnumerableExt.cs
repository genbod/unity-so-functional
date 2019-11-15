using System;
using System.Collections.Generic;
using System.Linq;
using Unit = System.ValueTuple;

namespace DragonDogStudios.UnitySoFunctional.Functional
{
    public static class EnumerableExt
    {
        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> list, Func<T, R> func)
            => list.Select(func);

        public static IEnumerable<Unit> ForEach<T>(this IEnumerable<T> ts, Action<T> action)
            => ts.Map(action.ToFunc()).ToList(); // TODO make this immutable

        public static IEnumerable<R> Bind<T, R>(this IEnumerable<T> list, Func<T, IEnumerable<R>> func)
              => list.SelectMany(func);

        public static IEnumerable<R> Bind<T, R>(this IEnumerable<T> list, Func<T, Option<R>> func)
            => list.Bind(t => func(t).AsEnumerable());
    }
}