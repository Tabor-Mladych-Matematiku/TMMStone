using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Elektrikář_z_ČVUTu : CardScriptBase
{
    TableActor target;
    public override bool OnBeforePlayed(CardPlayContext context) =>
        context.TrySelectTarget(TargetValidate, out target);

    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        Minion targetMinion = (Minion)target;
        int oldHealth = targetMinion.Health;
        int oldAttack = targetMinion.Attack;
        targetMinion.Attack = oldHealth;
        targetMinion.MaxHealth = oldAttack;
        targetMinion.Health = oldAttack;
    }

    protected override bool TargetValidate(TableActor target)=>target is Minion;

    //Targetable card events
    //protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e) { }
    //protected override bool TargetValidate(TableActor target)=>target is Minion;
    //protected override bool TargetValidate(TableActor target)=>true;

    //Minion events
    
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
    //protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e) { }
    //protected override void OnMinionPlayed(Minion minion, Card.CardPlayedEventArgs e) { }
    //protected override void OnFieldPlayed(Field field, Card.CardPlayedEventArgs e) { }

    //States
    //protected override Tuple<int, Card.CardType[]> ManaCostMod</*On what this effect sits*/>()=> new(0, new[] {  });
}
