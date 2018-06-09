
using System;
using System.Collections.Generic;
using System.Linq;
using Unit = System.ValueTuple;
using static F;
using System.Collections;

public static class EnumerableExt
{
    public static IEnumerable<R> Map<T, R>(this IEnumerable<T> list, Func<T, R> func)
        => list.Select(func);

    public static IEnumerable<Unit> ForEach<T>(this IEnumerable<T> ts, Action<T> action)
        => ts.Map(action.ToFunc()).ToList();
}
