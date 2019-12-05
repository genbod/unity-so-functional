using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Core
{
    public interface IPrintableValue
    {
        string GetValueToString(bool prettyPrint);
    }
}
