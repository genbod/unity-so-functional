using System;
using Unit = System.ValueTuple;

using static F;

public static class FuncExt
{
    public static Func<T> ToNullary<T>(this Func<Unit, T> f)
        => () => f(Unit());

    public static Func<T1, R> Compose<T1, T2, R>(this Func<T2, R> g, Func<T1, T2> f)
       => x => g(f(x));

    public static Func<T, bool> Negate<T>(this Func<T, bool> pred) => t => !pred(t);

    public static Func<T2, R> Apply<T1, T2, R>(this Func<T1, T2, R> func, T1 t1)
        => t2 => func(t1, t2);

    public static Func<T2, T3, R> Apply<T1, T2, T3, R>(this Func<T1, T2, T3, R> func, T1 t1)
        => (t2, t3) => func(t1, t2, t3);

    public static Func<I1, I2, R> Map<I1, I2, T, R>(this Func<I1, I2, T> @this, Func<T, R> func)
       => (i1, i2) => func(@this(i1, i2));
}

