using System;
using System.Collections.Generic;
using Unit = System.ValueTuple;
using static F;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[System.Serializable]
public struct Option<T> : IEquatable<Option.None>, IEquatable<Option<T>>
{
    [OdinSerialize]
    private T value;
    [OdinSerialize]
    readonly bool isSome;
    bool isNone => !isSome;

    private Option(T value)
    {
        if (value == null)
            throw new ArgumentNullException();
        this.isSome = true;
        this.value = value;
    }

    public static implicit operator Option<T>(Option.None _) => new Option<T>();
    public static implicit operator Option<T>(Option.Some<T> some) => new Option<T>(some.Value);

    // public static implicit operator Option<T>(T value)
    //    => value == null ? None : Some(value);

    public R Match<R>(Func<R> None, Func<T, R> Some)
        => isSome ? Some(value) : None();

    public IEnumerable<T> AsEnumerable()
    {
        if (isSome) yield return value;
    }

    public bool Equals(Option<T> other)
       => this.isSome == other.isSome
       && (this.isNone || this.value.Equals(other.value));

    public bool Equals(Option.None _) => isNone;

    public static bool operator ==(Option<T> @this, Option<T> other) => @this.Equals(other);
    public static bool operator !=(Option<T> @this, Option<T> other) => !(@this == other);

    public override string ToString() => isSome ? $"Some({value})" : "None";
}

