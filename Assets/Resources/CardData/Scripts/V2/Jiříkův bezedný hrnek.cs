using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;
using System.Linq;

public class Jiříkův_bezedný_hrnek : CardScriptBase
{
    private readonly List<int> stolenMinionIDs = new();

    public override bool Taunt => true;


    //Minion events
    
    //protected override void OnBeforeAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnAfterAttack(object sender, Minion.TargetedEventEventArgs e) { }
    //protected override void OnHealed(object sender, EventArgs e) { }
    //protected override void OnDamaged(object sender, EventArgs e) { }
    //protected override void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorStartTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e) { }
    //protected override void OnTableActorStartOwnTurn(object sender, GameActor.TurnEventArgs e) { }
    protected override void OnDeath(object sender, EventArgs e)
    {
        GameManager.P enemy = ((Minion)sender).Owner.Other();
        foreach (int id in stolenMinionIDs)
        {
            if (GameManager.Instance.SummonMinion(enemy, id) == null) break;
        }
    }

    //Card events
    //protected override void OnDiscard(object sender, EventArgs e){}
    //protected override void OnEndTurn(object sender, GameActor.TurnEventArgs e){}
    //protected override void OnStartTurn(object sender, GameActor.TurnEventArgs e){}
    //protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) { }

    //Other card events
    //protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){}
    protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e)
    {
        if(!TryGetComponent(out Minion mug))return;
        if (spell.cardname != "Funfact" || spell.Owner != mug.Owner) return;

        Minion[] enemies = GameManager.Instance.GetAllMinionsOwnedBy(mug.Owner.Other()).ToArray();
        if (enemies.Length == 0) return;

        Minion stolen = enemies[RandomRange(0, enemies.Length)];
        stolenMinionIDs.Add(stolen.CardID);
        CardSlot slot = stolen.GetComponentInParent<CardSlot>();
        stolen.transform.SetParent(null);
        Destroy(stolen.gameObject);
        Destroy(slot.PopCard().gameObject);
    }
    //protected override void OnMinionPlayed(Minion minion, Card.CardPlayedEventArgs e) { }
    //protected override void OnFieldPlayed(Field field, Card.CardPlayedEventArgs e) { }

    //States
    //public override int AttackCount => 2;
    //protected override Func<Card, int> ManaCostModifier => (card) => 0;

}
