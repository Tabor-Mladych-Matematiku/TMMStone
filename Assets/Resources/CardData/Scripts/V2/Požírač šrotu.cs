using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Požírač_šrotu : CardScriptBase
{
    //Minion events
    //protected override void OnBattleCry(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnBeforeAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnAfterAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnHealed(object sender, EventArgs e) { }
    //protected override void OnDamaged(object sender, EventArgs e) { }
    //protected override void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorStartTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorStartOwnTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnDeath(object sender, EventArgs e) { }

    //Card events
    //protected override void OnDiscard(object sender, EventArgs e){}
    //protected override void OnEndTurn(object sender, GameActor.TurnEventArgs e){}
    //protected override void OnStartTurn(object sender, GameActor.TurnEventArgs e){}
    //protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) { }

    //Other card events
    //protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){}
    protected override void OnMinionSummoned(Minion minion) {
        if (TryGetComponent(out Minion selfminion)
            && minion != selfminion
            && minion.Owner == selfminion.Owner
            && minion.cardTag == "Stroj")
        {
            selfminion.Buff(1, 1);
        }
    }
    //protected override void OnFieldPlayed(Field field, Card.CardPlayedEventArgs e) { }

    //States
    //protected override Tuple<int, Card.CardType[]> ManaCostMod</*On what this effect sits*/>()=> new(0, new[] {  });
}