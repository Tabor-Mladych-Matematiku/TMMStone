using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Kladivo_z_houby : TargetableCardScriptBase
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
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e) {
        DealSpellDamage((DamageableActor)e.Target, 3, sender);
        DrawCard(true, sender);
    }

    //Other card events
    //protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){}
    //protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e) { }
    //protected override void OnMinionPlayed(Minion minion, Card.CardPlayedEventArgs e) { }
    //protected override void OnFieldPlayed(Field field, Card.CardPlayedEventArgs e) { }

    //States
    //protected override Tuple<int, Card.CardType[]> ManaCostMod()=> new(0, new[] {  });
}