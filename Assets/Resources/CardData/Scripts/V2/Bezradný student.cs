using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System.Linq;
using System;

public class Bezradný_student : CardScriptBase
{


    //Minion events
    //protected override void OnBattleCry(object sender, Minion.TargetedEventEventArgs e) { }
    protected override void OnBeforeAttack(object sender, Minion.TargetedEventEventArgs e)
    {
        if (RandomRange(0, 2) != 0) return;

        DamageableActor originalTarget = e.target as DamageableActor;
        DamageableActor[] alternatives = GameManager.Instance
            .GetAllCharactersOwnedBy(GetOwner(sender).Other())
            .Where(target => target != originalTarget)
            .ToArray();
        if (alternatives.Length > 0)
            e.target = alternatives[RandomRange(0, alternatives.Length)];
    }
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
    //public override bool Taunt => true;
    //protected override Func<Card, int> ManaCostModifier => (card) => 0;

}
