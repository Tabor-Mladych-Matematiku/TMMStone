using System.Collections.Generic;
using System.Linq;
using CardGame;
using UnityEngine;

public class Implementace_v_Java : CardScriptBase
{
    public override bool OnBeforePlayed(CardPlayContext context)
    {
        Card card = GetComponent<Card>();
        IEnumerable<Card> opponentHand = GameManager.Instance.GetCardsInHandOwnedBy(card.Owner.Other());
        Card[] hand;
        if (context.AutomaticallySelectingChoices)
        {
            // Automatic play runs on both peers. Let CardPlayContext make the
            // synchronized Unity-random selection from the complete hand.
            hand = opponentHand.ToArray();
        }
        else
        {
            // Pre-play UI runs only on the initiating peer. Never advance the
            // synchronized Unity RNG while constructing a local prompt.
            System.Random promptRandom = new();
            hand = opponentHand.OrderBy(_ => promptRandom.Next()).Take(3).ToArray();
        }
        CardChoiceRequest request = new() { Header = "Kterou kartu spaluješ?" };
        foreach (Card option in hand)
            request.Options.Add(new(option.GetComponentInParent<CardSlot>().index, option));
        return context.TrySelectChoices(request, out _);
    }

    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        if (e.Choices.Count != 1) return;
        GameManager.P opponent = GetOwner(sender).Other();
        Card card = GameManager.Instance.GetCardsInHandOwnedBy(opponent)
            .FirstOrDefault(candidate => candidate.GetComponentInParent<CardSlot>().index == e.Choices[0]);
        if (card != null) GameManager.Instance.Discard(card, opponent);
    }
}
