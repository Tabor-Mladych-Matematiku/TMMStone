using System.Linq;
using CardData;
using CardGame;

public class Oprava_na_koleni : CardScriptBase
{
    public override bool OnBeforePlayed(CardPlayContext context)
    {
        Card card = GetComponent<Card>();
        Card[] machines = GameManager.Instance.graves[card.Owner]
            .Where(candidate => candidate.cardType == Card.CardType.Minion && candidate.HasTag(CardTag.Stroj))
            .ToArray();
        CardChoiceRequest request = new()
        {
            Header = "Které stroje opravíš?",
            RequiredSelections = System.Math.Min(3, machines.Length),
            MarkSelections = true,
            AutoSelectAll = machines.Length <= 3
        };
        foreach (Card machine in machines)
            request.Options.Add(new(GameManager.Instance.graves[card.Owner].IndexOf(machine), machine));
        return context.TrySelectChoices(request, out _);
    }

    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager.P owner = GetOwner(sender);
        Card[] graveSnapshot = GameManager.Instance.graves[owner].ToArray();
        foreach (int index in e.Choices)
        {
            if (index < 0 || index >= graveSnapshot.Length) continue;
            Card card = graveSnapshot[index];
            if (card.cardType != Card.CardType.Minion || !card.HasTag(CardTag.Stroj)) continue;
            if (GameManager.Instance.SummonMinion(owner, card.ID) == null) break;
        }
    }
}
