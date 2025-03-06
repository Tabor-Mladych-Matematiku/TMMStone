using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Jiříkova_přednáška : CardScriptBase
{
    //Minion events
    //protected override void OnBeforeAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnAfterAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnHealed(object sender, EventArgs e) { }
    //protected override void OnDamaged(object sender, EventArgs e) { }
    protected override void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e) {
        GameManager.Instance.DrawCard(GameManager.Instance.PlayerOnTurn);
    }
    //protected override void OnTableActorStartTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorStartOwnTurn(object sender, GameActor.TurnEventArgs e) { }
}