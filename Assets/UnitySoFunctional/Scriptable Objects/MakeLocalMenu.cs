﻿using System;

namespace DragonDogStudios.UnitySoFunctional.ScriptableObjects
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MakeLocalMenuAttribute : Attribute { }
}