using System;
using System.Collections.Generic;
using System.Linq;
using Unit = System.ValueTuple;

namespace DragonDogStudios.UnitySoFunctional.Functional
{
    public static class EnumerableExt
    {
        /// <summary>
        /// Like Map but operates on IEnumerable and returns IEnumerable of R results.
        /// </summary>
        /// <param name="ts">This IEnumerable of T</param>
        /// <param name="func">Function to perform on values of list</param>
        /// <typeparam name="T">Original type of list</typeparam>
        /// <typeparam name="R">Return type of IEnumerable</typeparam>
        /// <returns>IEnumerable of transformed values of list</returns>
        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> ts, Func<T, R> func)
            => ts.Select(func);

        /// <summary>
        /// Like ForEach but operates on IEnumerable of T.
        /// </summary>
        /// <param name="ts">This IEnumerable of T</param>
        /// <param name="action">Action to perform with each value of ts</param>
        /// <typeparam name="T">Original type of list</typeparam>
        /// <returns>(VOID) IEnumerable of Unit</returns>
        public static IEnumerable<Unit> ForEach<T>(this IEnumerable<T> ts, Action<T> action)
            => ts.Map(action.ToFunc()).ToList(); // TODO make this immutable

        /// <summary>
        /// Like Bind but operates on IEnumerable and func returns IEnumerable for each value.
        /// </summary>
        /// <param name="list">This IEnumerable of T</param>
        /// <param name="func">Function to perform on values of list</param>
        /// <typeparam name="T">Original type of list</typeparam>
        /// <typeparam name="R">Return type of IEnumerable</typeparam>
        /// <returns>IEnumerable of transformed values of list</returns>
        public static IEnumerable<R> Bind<T, R>(this IEnumerable<T> list, Func<T, IEnumerable<R>> func)
              => list.SelectMany(func);

        /// <summary>
        /// Like Bind but func returns Option of R.
        /// </summary>
        /// <param name="list">This IEnumerable of T</param>
        /// <param name="func">Function to perform on values of list.  Returns Option of R</param>
        /// <typeparam name="T">Original type of list</typeparam>
        /// <typeparam name="R">Return type of IEnumerable</typeparam>
        /// <returns>IEnumerable of transformed values of list</returns>
        public static IEnumerable<R> Bind<T, R>(this IEnumerable<T> list, Func<T, Option<R>> func)
            => list.Bind(t => func(t).AsEnumerable());
    }
}