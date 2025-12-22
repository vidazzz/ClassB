using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public enum LineCheckingType
{
    none = 0,
    Stats,
    Skill,
}

[Serializable]
public enum EffectType
{
    None = 0,
    ModifyStats,
    ModifyAfinity,
    Check,
    CheckItem,
    OffWork,
    RollBack,
}


