using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Duch_matematikův : CardScriptBase
{
    //Minion events
    protected override void OnAfterAttack(object sender, Minion.TargetedEventEventArgs e) {
        ((DamageableActor)e.target).Frozen = true;
    }
    //protected override void OnHealed(object sender, EventArgs e) { }
    //protected override void OnDamaged(object sender, EventArgs e) { }
    //protected override void OnMinionEndTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnMinionStartTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnDeath(object sender, EventArgs e) { }

    //Card events
    //protected override void OnDiscard(object sender, EventArgs e){}
    //protected override void OnEndTurn(object sender, GameActor.TurnEventArgs e){}
    //protected override void OnStartTurn(object sender, GameActor.TurnEventArgs e){}
    //protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e) { }

    //Other card events
    //protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){}
    //protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e) { }
    //protected override void OnMinionPlayed(Minion minion, Card.CardPlayedEventArgs e) { }
    //protected override void OnFieldPlayed(Field field, Card.CardPlayedEventArgs e) { }
}