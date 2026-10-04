using System.Collections.Generic;
using System.Linq;
using CardGame;

public class EVIL_Jiřík__Dozorce_chaosu__druhá_fáze : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager manager = GameManager.Instance;
        GameManager.P first = GetOwner(sender);
        GameManager.P second = first.Other();
        CardPile firstFormerDeck = manager.decks[first].Pile;
        CardPile secondFormerDeck = manager.decks[second].Pile;
        IEnumerable<Card> graveCards = ReferenceEquals(manager.graves[first].Pile, manager.graves[second].Pile)
            ? manager.graves[first]
            : manager.graves[first].Concat(manager.graves[second]);
        CardPile sharedDraw = new(graveCards);
        sharedDraw.Cards.Shuffle();

        manager.decks[first].UsePile(sharedDraw);
        manager.decks[second].UsePile(sharedDraw);
        manager.graves[first].UsePile(firstFormerDeck);
        manager.graves[second].UsePile(secondFormerDeck);
    }
}
