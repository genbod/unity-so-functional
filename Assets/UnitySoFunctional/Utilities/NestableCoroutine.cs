using DragonDogStudios.UnitySoFunctional.Functional;
using F = DragonDogStudios.UnitySoFunctional.Functional.F;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Utilities
{
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
                if (yielded is NestableCoroutine<T> nestableCoroutine)
                {
                    yield return nestableCoroutine.coroutine;
                }
                else
                {
                    if (yielded != null && yielded is T yieldedT)
                    {
                        returnVal = F.Exceptional(yieldedT);
                        yield break;
                    }
                    else if (yielded != null && yielded is Option<T> option)
                    {
                        returnVal = option.Match(
                            () => Exceptional.Of<T>(new NoneException()),
                            (f) => F.Exceptional(f));
                        yield break;
                    }
                    else if (yielded != null && yielded is Exceptional<T> exceptional)
                    {
                        returnVal = exceptional;
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

    public class NoneException : Exception
    {
        public NoneException() : base("Option was None") { }
    }
}