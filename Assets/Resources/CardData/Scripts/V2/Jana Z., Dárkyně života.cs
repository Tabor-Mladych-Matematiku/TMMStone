using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Jana_Z___Dárkyně_života : CardScriptBase
{
    //Minion events
    //protected override void OnBeforeAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnAfterAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnHealed(object sender, EventArgs e) { }
    //protected override void OnDamaged(object sender, EventArgs e) { }
    //protected override void OnMinionEndTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnMinionStartTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnDeath(object sender, EventArgs e) { }

    //Card events
    //protected override void OnDiscard(object sender, EventArgs e){}
    //protected override void OnEndTurn(object sender, GameActor.TurnEventArgs e){}
    //protected override void OnStartTurn(object sender, GameActor.TurnEventArgs e){}
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) {
        foreach (DamageableActor character in GameManager.Instance.AllCharacters) {
            character.Heal(int.MaxValue);//Why bother trying to figure out max health when you can just set it to max int
        }
    }

    //Other card events
    //protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){}
    //protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e) { }
    //protected override void OnMinionPlayed(Minion minion, Card.CardPlayedEventArgs e) { }
    //protected override void OnFieldPlayed(Field field, Card.CardPlayedEventArgs e) { }
}