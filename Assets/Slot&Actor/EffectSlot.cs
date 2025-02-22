using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame
{
    public class EffectSlot : CardSlot
    {
        internal Effect GetEffect() => GetComponentInChildren<Effect>();
    }
}