using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPrintableValue
{
    string GetValueToString(bool prettyPrint);
}
