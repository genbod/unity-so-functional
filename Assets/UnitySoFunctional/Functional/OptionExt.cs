using System;
using System.Collections.Generic;
using Unit = System.ValueTuple;

namespace DragonDogStudios.UnitySoFunctional.Functional
{
    using static F;

    public static partial class F
    {
        public static Option<T> Some<T>(T value) => new Option.Some<T>(value); // wrap the given value into a Some
        public static Option.None None => Option.None.Default;  // the None value
    }

    namespace Option
    {
        public struct None
        {
            internal static readonly None Default = new None();
        }

        public struct Some<T>
        {
            internal T Value { get; }

            internal Some(T value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value)
                       , "Cannot wrap a null value in a 'Some'; use 'None' instead");
                Value = value;
            }
        }
    }

    public static class OptionExt
    {
        /// <summary>
        /// Takes in Function that returns R.
        /// If Option is None then return None.
        /// If Option is Some then execute func with value of Option and Return R wrapped in Some.
        /// </summary>
        /// <param name="this">This Option</param>
        /// <param name="func">Function to perform if Some</param>
        /// <typeparam name="T">Original Option Value Type</typeparam>
        /// <typeparam name="R">Returned Type</typeparam>
        /// <returns>None or Some/<R/></returns>
        public static Option<R> Map<T, R>(this Option<T> @this, Func<T, R> func)
            => @this.Match(
                () => None,
                (t) => Some(func(t)));

        /// <summary>
        /// Takes in Action.
        /// If Option is None then do nothing.
        /// If Option is Some then execute Action with value of Option.
        /// </summary>
        /// <param name="this">This Option</param>
        /// <param name="action">Action to perform if Some</param>
        /// <typeparam name="T">Option Value Type</typeparam>
        /// <returns>(VOID) Option/<Unit/></returns>
        public static Option<Unit> ForEach<T>(this Option<T> @this, Action<T> action)
            => Map(@this, action.ToFunc());

        /// <summary>
        /// Like Map but Takes in Function the returns Option<R>.
        /// If Option is None then return None.
        /// If Option is Some then execute func with value of Option and Return Option/<R/>.
        /// </summary>
        /// <param name="this">This Option</param>
        /// <param name="func">Function to perform if Some</param>
        /// <typeparam name="T">Original Option Value Type</typeparam>
        /// <typeparam name="R">Returned Type</typeparam>
        /// <returns></returns>
        public static Option<R> Bind<T, R>(this Option<T> @this, Func<T, Option<R>> func)
            => @this.Match(
                () => None,
                (t) => func(t));

        /// <summary>
        /// Same as Bind but creates IEnumerable from this Option and func returns IEnumerable of Rs.
        /// </summary>
        /// <param name="this">This Option</param>
        /// <param name="func">Function to perform if Some</param>
        /// <typeparam name="T">Oritinal Option Value Type</typeparam>
        /// <typeparam name="R">Returned Type</typeparam>
        /// <returns>Returns IEnumerable of Rs</returns>
        public static IEnumerable<R> Bind<T, R>(this Option<T> @this, Func<T, IEnumerable<R>> func)
            => @this.AsEnumerable().Bind(func);

        internal static bool IsSome<T>(this Option<T> @this)
             => @this.Match(
                () => false,
                (_) => true);

        // LINQ

        public static Option<R> Select<T, R>(this Option<T> @this, Func<T, R> func)
           => @this.Map(func);

        public static Option<T> Where<T>
           (this Option<T> optT, Func<T, bool> predicate)
           => optT.Match(
              () => None,
              (t) => predicate(t) ? optT : None);

        public static Option<RR> SelectMany<T, R, RR>
           (this Option<T> opt, Func<T, Option<R>> bind, Func<T, R, RR> project)
           => opt.Match(
              () => None,
              (t) => bind(t).Match(
                 () => None,
                 (r) => Some(project(t, r))));

    }
}