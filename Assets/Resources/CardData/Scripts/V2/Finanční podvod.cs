using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Finanční_podvod : CardScriptBase
{
    private void Awake()
    {
        manacostmod = (card) => card.cardType == Card.CardType.Spell &&card.Owner==gameObject.GetComponent<Effect>().Owner ? -1 : 0;
    }
    //Minion events
    //protected override void OnBeforeAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnAfterAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnHealed(object sender, EventArgs e) { }
    //protected override void OnDamaged(object sender, EventArgs e) { }
    //protected override void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorStartTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e) { }
    protected override void OnTableActorStartOwnTurn(object sender, GameActor.TurnEventArgs arg)
    {
        if(sender is Effect e) e.Destroy();
    }
    //protected override void OnDeath(object sender, EventArgs e) { }

    //Card events
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        PlaceEffect(sender);
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