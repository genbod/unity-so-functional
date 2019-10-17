using System;

namespace DragonDogStudios.Exceptions
{
    public class ReadOnlyObjectInvalidCreationException : Exception
    {
        public ReadOnlyObjectInvalidCreationException() : base(
            "An immutable object can only be created via the static Create<T> method.")
        {
        }
    }

    public class ReadOnlyObjectEditException : Exception
    {
        public ReadOnlyObjectEditException() : base("This objects is flagged as ReadOnly and cannot be changed after it has been created.")
        {
        }
    }
}
