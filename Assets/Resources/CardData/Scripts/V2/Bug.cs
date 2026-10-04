using CardGame;

public class Bug : CardScriptBase
{
    protected override void OnSelfSummoned(object sender, Minion.TargetedEventEventArgs e)
    {
        var minion = (Minion)sender;
        int previous = GameManager.Instance.Stats.GetSummonCount(minion.Owner, minion.CardID);
        minion.Buff(previous, previous);
    }
}
