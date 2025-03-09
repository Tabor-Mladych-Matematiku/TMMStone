using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Kofola : TargetableCardScriptBase
{
    //Minion events
    //protected override void OnAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnHealed(object sender, EventArgs e) { }
    //protected override void OnDamaged(object sender, EventArgs e) { }
    //protected override void OnMinionEndTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnMinionStartTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnDeath(object sender, EventArgs e) { }

    //Card events
    //protected override void OnDiscard(object sender, EventArgs e){}
    //protected override void OnEndTurn(object sender, GameActor.TurnEventArgs e){}
    //protected override void OnStartTurn(object sender, GameActor.TurnEventArgs e){}
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e) {
        if(e.Target is Minion minion)minion.Buff(1, 1);
    }

    //Other card events
    //protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){}
}