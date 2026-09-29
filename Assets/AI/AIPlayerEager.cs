using CardGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerEager : AIPlayerBase
{
    [SerializeField]
    private bool enableTargetedSpellsAndAttacks = false;

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        PlayerData ownPlayerData = GameManager.Instance.GetPlayerData(GameManager.Instance.PlayerOnTurn);
        for (int i = 0; i < ownPlayerData.hand.Count; i++)
        {
            Card card = ownPlayerData.hand[i];
            if (card && GameManager.Instance.IsCardPlayable(card, GameManager.Instance.PlayerOnTurn))
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
                        if (card.Targetted)
                        {
                            GameManager.CharacterTargetIndex target = GameManager.Instance.GetRandomTargetForCard(card, GameManager.Instance.PlayerOnTurn);
                            if (!target.HasTarget)
                            {
                                Debug.LogWarning("AIPlayerEager found no valid target for: " + card.cardname);
                                break;
                            }

                            Debug.Log("AIPlayerEager playing targeted minion: " + card.cardname + " on target: " + target);
                            GameManager.Instance.OnAIPlayMinion(i, freeIndex, target);
                        }
                        else
                        {
                            Debug.Log("AIPlayerEager playing minion: " + card.cardname);
                            GameManager.Instance.OnAIPlayMinion(i, freeIndex, GameManager.CharacterTargetIndex.None);
                        }
                        i = 0;
                        break;
                    case Card.CardType.Spell:
                        if (!card.Targetted)
                        {
                            Debug.Log("AIPlayerEager playing untargeted spell: " + card.cardname);
                            GameManager.Instance.OnAICastSpell(i);
                            i = 0;
                        }
                        else if (enableTargetedSpellsAndAttacks)
                        {
                            GameManager.CharacterTargetIndex target = GameManager.Instance.GetRandomTargetForCard(card, GameManager.Instance.PlayerOnTurn);
                            if (target.HasTarget)
                            {
                                Debug.Log("AIPlayerEager playing targeted spell: " + card.cardname + " on target: " + target);
                                GameManager.Instance.OnAICastSpell(i, target.Value);
                                i = 0;
                            }
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
            if (i == 0) ownPlayerData = GameManager.Instance.GetPlayerData(GameManager.Instance.PlayerOnTurn);//Reset if something happened to the hand or mana. It will reset extra time, but at least we stop it from reloading after every card.
        }

        if (enableTargetedSpellsAndAttacks)
        {
            Minion[] attackers = new List<Minion>(GameManager.Instance.GetAllMinionsOwnedBy(GameManager.Instance.PlayerOnTurn)).ToArray();
            foreach (Minion attacker in attackers)
            {
                while (attacker != null && attacker.CanAttack && attacker.Attack > 0)
                {
                    GameManager.CharacterTargetIndex target = GameManager.Instance.GetRandomTargetForMinion(attacker, GameManager.Instance.PlayerOnTurn);
                    if (!target.HasTarget) break;

                    int sourceSlot = attacker.GetComponentInParent<CardSlot>().index % GameManager.maxMinionSlots;
                    GameManager.Instance.OnAIMinionAttack(sourceSlot, target.Value);
                }
            }
        }

        Debug.Log("Ending turn.");
        GameManager.Instance.ToggleTurnOffline();

    }
}
