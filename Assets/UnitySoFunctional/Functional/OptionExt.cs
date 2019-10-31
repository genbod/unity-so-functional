using System;
using System.Collections.Generic;
using Unit = System.ValueTuple;
using static F;
using Sirenix.OdinInspector;

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
    public static Option<R> Map<T, R>(this Option<T> optT, Func<T, R> f)
        => optT.Match(
            () => None,
            (t) => Some(f(t)));

    public static Option<Unit> ForEach<T>(this Option<T> @this, Action<T> action)
        => Map(@this, action.ToFunc());

    public static Option<R> Bind<T, R>(this Option<T> optT, Func<T, Option<R>> f)
        => optT.Match(
            () => None,
            (t) => f(t));

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

