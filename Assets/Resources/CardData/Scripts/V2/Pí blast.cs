using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Pí_blast : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e) {
        DamageableActor target = (DamageableActor)e.Target;
        DealSpellDamage(target, 3,sender);
        target.Frozen = true;
    }
}