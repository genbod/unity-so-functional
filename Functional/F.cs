using System;
using Unit = System.ValueTuple;

public static partial class F
{
    public static Unit Unit() => default(Unit);

    // function manipulation 
    public static Func<T1, Func<T2, R>> Curry<T1, T2, R>(this Func<T1, T2, R> func)
        => t1 => t2 => func(t1, t2);

    public static Func<T1, Func<T2, Func<T3, R>>> Curry<T1, T2, T3, R>(this Func<T1, T2, T3, R> func)
        => t1 => t2 => t3 => func(t1, t2, t3);

    public static Func<T1, Func<T2, T3, R>> CurryFirst<T1, T2, T3, R>
       (this Func<T1, T2, T3, R> @this) => t1 => (t2, t3) => @this(t1, t2, t3);

    public static Func<T, T> Tap<T>(Action<T> act)
       => x => { act(x); return x; };

}
