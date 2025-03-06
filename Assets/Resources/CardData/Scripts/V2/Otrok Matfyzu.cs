using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Otrok_Matfyzu : CardScriptBase
{
    //Minion events
    //protected override void OnAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnHealed(object sender, EventArgs e) { }
    protected override void OnDamaged(object sender, EventArgs e) { GameManager.Instance.DrawCard(((Minion)sender).Owner); }
    //protected override void OnMinionEndTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnMinionStartTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnDeath(object sender, EventArgs e) { }
    //Card events
    //protected override void OnDiscard(object sender, EventArgs e){}

    //protected override void OnEndTurn(object sender, GameActor.TurnEventArgs e){}

    //protected override void OnStartTurn(object sender, GameActor.TurnEventArgs e){}

    //protected override void OnBattleCry(){}
    //protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){}
}