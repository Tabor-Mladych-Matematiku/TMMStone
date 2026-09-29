using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Pavel__Master_casina : CardScriptBase
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
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        Minion minion = sender as Minion;
        Card topcardOpponent = GameManager.Instance.decks[minion.Owner.Other()][0];
        Card topcardOwn = GameManager.Instance.decks[minion.Owner][0];
        if (topcardOwn.mana >= topcardOpponent.mana)
        {
            GameManager.Instance.AddCardToHand(
                minion.Owner,
                GameManager.Instance.decks[minion.Owner].PopFirst(),
                true);
            GameManager.Instance.AddCardToHand(
                minion.Owner,
                GameManager.Instance.decks[minion.Owner.Other()].PopFirst(),
                true);
        }
    }

    //Other card events
    //protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){}
    //protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e) { }
    //protected override void OnMinionPlayed(Minion minion, Card.CardPlayedEventArgs e) { }
    //protected override void OnFieldPlayed(Field field, Card.CardPlayedEventArgs e) { }

    //States
    //public override bool Taunt => true;
    //public override int AttackCount => 2;
    //protected override Func<Card, int> ManaCostModifier => (card) => 0;

}
