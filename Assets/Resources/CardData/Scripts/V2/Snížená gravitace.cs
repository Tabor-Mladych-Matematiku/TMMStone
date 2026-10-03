using CardGame;

public class Snížená_gravitace : CardScriptBase
{
    protected override void OnMinionSummoned(Minion minion) => minion.Buff(1, 1);
}
