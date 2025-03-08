using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Zkouškové_období : TargetableCardScriptBase
{

    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        DealSpellDamage((DamageableActor)e.Target, 10, sender);
    }
}