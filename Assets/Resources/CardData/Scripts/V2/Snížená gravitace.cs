using CardGame;

public class Snížená_gravitace : CardScriptBase
{
    protected override void OnMinionSummoned(Minion minion)
    {
        if (TryGetComponent(out Field _)) 
            minion.Buff(1, 1);
    }

}
