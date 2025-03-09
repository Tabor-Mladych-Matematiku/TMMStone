using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Michal__Hlavní_matemág : CardScriptBase
{
    const int NahlaLobotomieID = 78;
    protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e)
    {
        if (TryGetComponent(out Minion minion))
        {
            GameManager.P owner = minion.Owner;
            if (spell.Owner == owner)
            {
                GameManager.Instance.AddCardToHandByID(owner, NahlaLobotomieID);
            }
        }
    }

}