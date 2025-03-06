using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Pofidérní_bankéř : CardScriptBase
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
    //protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e) { }

    //Other card events
    protected override void OnSpellPlayed(Card card, Card.CardPlayedEventArgs e)
    {
        Minion minion = gameObject.GetComponent<Minion>();
        if (minion == null || minion.Owner != card.Owner) return;
        GameManager.Instance.DrawCard(minion.Owner);

    }
}