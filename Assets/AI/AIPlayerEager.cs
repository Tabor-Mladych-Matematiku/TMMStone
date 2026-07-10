using CardGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerEager : AIPlayerBase
{
    public override void OnTurnStart()
    {
        base.OnTurnStart();
        PlayerData ownPlayerData = GameManager.Instance.GetPlayerData(GameManager.Instance.PlayerOnTurn);
        for (int i = 0; i < ownPlayerData.hand.Count; i++)
        {
            Card card = ownPlayerData.hand[i];
            if (card&&card.mana <= ownPlayerData.mana)
            {
                Debug.Log("card mana:" + card.mana + " Player mana: " + ownPlayerData.mana);
                switch (card.cardType)
                {
                    case Card.CardType.Minion:
                        int freeIndex = GameManager.Instance.GetRandomFreeMinionSlot(GameManager.Instance.PlayerOnTurn);
                        if (freeIndex == -1)
                        {
                            Debug.Log("No free minion slot available.");
                            break;
                        }
                        Debug.Log("AIPlayerEager playing minion: " + card.cardname);

                        GameManager.Instance.OnAIPlayMinion(i, freeIndex);
                        i = 0;
                        break;
                    case Card.CardType.Spell:
                        if (!card.Targetted) { 
                            Debug.Log("AIPlayerEager playing untargeted spell: " + card.cardname);
                            GameManager.Instance.OnAICastSpell(i);
                            i = 0;
                        }
                        else
                        {
                            Debug.Log("AIPlayerEager skipping targeted spell: " + card.cardname);
                        }
                        break;
                    case Card.CardType.Field:
                        GameManager.Instance.OnAIPlayField(i);
                        i = 0;
                        break;
                    default:
                        break;
                }
            }
            if(i==0)ownPlayerData = GameManager.Instance.GetPlayerData(GameManager.Instance.PlayerOnTurn);//Reset if something happened to the hand or mana. It will reset extra time, but at least we stop it from reloading after every card.
        }

        Debug.Log("Ending turn.");
        GameManager.Instance.ToggleTurnOffline();

    }
}
