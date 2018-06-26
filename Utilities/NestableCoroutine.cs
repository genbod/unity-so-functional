using UnityEngine;
using System;
using System.Collections;
using static F;
using System.Linq;

public static class MonoBehaviorExt
{
    public static NestableCoroutine<T> StartCoroutine<T>(this MonoBehaviour obj, IEnumerator coroutine)
    {
        NestableCoroutine<T> coroutineObject = new NestableCoroutine<T>(coroutine);
        coroutineObject.coroutine = coroutineObject.StartCoroutine(obj);
        return coroutineObject;
    }

    public static IEnumerator OverTime(
        this MonoBehaviour obj,
        float time,
        Func<float, float> f,
        Action<float> action)
    {

        float startTime = Time.time;
        while (Time.time - startTime < time)
        {
            float u = f((Time.time - startTime) / time);
            action(u);
            yield return null;
        }
        action(f(1));
        yield break;
    }
}

public class NestableCoroutine<T>
{
    public NestableCoroutine(Option<IEnumerator> routine)
    {
        IEnumerator enumerator = routine.Match(
            () => Enumerable.Empty<System.Object>().GetEnumerator(),
            (e) => e);
        Routine = InternalRoutine(enumerator);
    }

    public NestableCoroutine(IEnumerator routine)
    {
        Routine = InternalRoutine(routine);
    }

    public IEnumerable Routine { get; private set; }

    public Exceptional<T> Value
    {
        get
        {
            return returnVal;
        }
    }

    public void Cancel()
    {
        isCancelled = true;
    }

    private bool isCancelled = false;
    private Exceptional<T> returnVal;
    public Exception e;
    public Coroutine coroutine;

    public Coroutine StartCoroutine(MonoBehaviour obj)
    {
        return obj.StartCoroutine(Routine.GetEnumerator());
    }

    private IEnumerable InternalRoutine(IEnumerator coroutine)
    {
        while (true)
        {
            if (isCancelled)
            {
                e = new CoroutineCancelledException();
                yield break;
            }
            try
            {
                if (!coroutine.MoveNext())
                {
                    yield break;
                }
            }
            catch (Exception e)
            {
                this.e = e;
                yield break;
            }
            object yielded = coroutine.Current;

            // Support nested Nestable Coroutines by returning the underlying
            // system coroutine so that Unity will recognise it and process it.
            // Otherwise we will continue executing on the next frame.
            if (yielded is NestableCoroutine<T>)
            {
                yield return (yielded as NestableCoroutine<T>).coroutine;
            }
            else
            {
                if (yielded != null && yielded is T)
                {
                    returnVal = Exceptional((T)yielded);
                    yield break;
                }
                else if (yielded != null && yielded is Option<T>)
                {
                    returnVal = ((Option<T>)yielded).Match(
                        () => Exceptional.Of<T>(new NullReferenceException()),
                        (f) => Exceptional(f));
                    yield break;
                }
                else if(yielded != null && yielded is Exceptional<T>)
                {
                    returnVal = (Exceptional<T>)yielded;
                    yield break;
                }
                else
                {
                    yield return coroutine.Current;
                }
            }
        }
    }
}

public class CoroutineCancelledException : System.Exception
{
    public CoroutineCancelledException() : base("Coroutine was cancelled")
    {

    }
}