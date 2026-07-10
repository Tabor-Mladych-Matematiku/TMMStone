using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;
using System.Linq;

public class Dolní_šichta : CardScriptBase
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
    const int UranID = 239;
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) {
        int survivingMinionsCount = 0;
        void LogDeath(object s, EventArgs args) { survivingMinionsCount--; }
        List<Minion> minions = GameManager.Instance.AllMinions.ToList();//This should ensure that it targets only the minions that are currently on the board, and not any that are summoned during the resolution of this spell
        foreach (var minion in minions)
        {
            if (minion != null)
            { //If one of them died while this spell is resolving, we don't want to crash
                survivingMinionsCount++;
                minion.OnDeath += LogDeath;
                DealSpellDamage(minion, 2,sender);
                minion.OnDeath -= LogDeath;
            }
        }
        for (int i = 0; i < survivingMinionsCount; i++)
            GameManager.Instance.AddCardToHandByID(GetOwner(sender), UranID, true);
        
    }

    //Other card events
    //protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){}
    //protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e) { }
    //protected override void OnMinionPlayed(Minion minion, Card.CardPlayedEventArgs e) { }
    //protected override void OnFieldPlayed(Field field, Card.CardPlayedEventArgs e) { }
}