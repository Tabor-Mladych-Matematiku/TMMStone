using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;


public abstract class CardScriptBase : MonoBehaviour
{
    public virtual void Start()
    {
        Card card =  gameObject.GetComponent<Card>();
        card.OnPlayed += OnPlayed;
        card.OnStartTurn += OnStartTurn;
        card.OnEndTurn += OnEndTurn;
        card.OnDiscardEvent += OnDiscard;
        //TODO: add all the other effects
    }

    protected virtual void OnDiscard(object sender, EventArgs e){}

    protected virtual void OnEndTurn(object sender, GameActor.TurnEventArgs e){}

    protected virtual void OnStartTurn(object sender, GameActor.TurnEventArgs e){}
    protected virtual void OnPlayed(object sender, Card.CardPlayedEventArgs e){}

}
