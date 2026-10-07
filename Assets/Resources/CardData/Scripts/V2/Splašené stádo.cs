using CardGame;

public class Splašené_stádo : CardScriptBase
{
    protected override void OnSelfSummoned(object sender, Minion.SummonedEventArgs e)
    {
        Minion minion = (Minion)sender;
        int earlierCards = System.Math.Max(0, GameManager.Instance.Stats.GetCardsPlayedThisTurn(minion.Owner) - 1);
        if (earlierCards > 0) minion.Buff(2 * earlierCards, 2 * earlierCards);
    }
}
