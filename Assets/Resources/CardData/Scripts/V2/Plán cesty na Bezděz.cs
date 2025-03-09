using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Plán_cesty_na_Bezděz : CardScriptBase
{
    //Minion events
    //protected override void OnAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnHealed(object sender, EventArgs e) { }
    //protected override void OnDamaged(object sender, EventArgs e) { }
    //protected override void OnMinionEndTurn(object sender, GameActor.TurnEventArgs e) { }
    protected override void OnTableActorStartOwnTurn(object sender, GameActor.TurnEventArgs e)
    {
        foreach (Minion minion in GameManager.Instance.AllMinions)
        {
            minion.Death();
        }
    }
    //protected override void OnDeath(object sender, EventArgs e) { }

    //Card events
    //protected override void OnDiscard(object sender, EventArgs e){}
    //protected override void OnEndTurn(object sender, GameActor.TurnEventArgs e){}
    //protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e) { }

    //Other card events
    //protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){}
}