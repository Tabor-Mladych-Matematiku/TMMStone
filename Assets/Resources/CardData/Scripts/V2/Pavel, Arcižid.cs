using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;
using System.Linq;

public class Pavel__Arcižid : CardScriptBase
{

    protected override Dictionary<Type, ManaModsModifier> ManaCostMods => CreateManaCostModDict(
        (typeof(Effect), 5, new[] { Card.CardType.Spell })
        );
    //Minion events
    //protected override void OnBeforeAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnAfterAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnHealed(object sender, EventArgs e) { }
    //protected override void OnDamaged(object sender, EventArgs e) { }
    //protected override void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorStartTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e) { }
    protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs arg)
    {
        if (sender is Effect e) e.Destroy();
    }
    //protected override void OnDeath(object sender, EventArgs e) { }

    //Card events
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        PlaceEffect(sender, GetOwner(sender).Other());
    }
    //protected override void OnDiscard(object sender, EventArgs e){}
    //protected override void OnEndTurn(object sender, GameActor.TurnEventArgs e){}
    //protected override void OnStartTurn(object sender, GameActor.TurnEventArgs e){}

    //Other card events
    //protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){}
    //protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e) { }
    //protected override void OnMinionPlayed(Minion minion, Card.CardPlayedEventArgs e) { }
    //protected override void OnFieldPlayed(Field field, Card.CardPlayedEventArgs e) { }
}