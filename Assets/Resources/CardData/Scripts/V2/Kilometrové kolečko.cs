using CardGame;

public class Kilometrové_kolečko : CardScriptBase
{
    protected override void OnMinionSummoned(Minion minion)
    {
        if (TryGetComponent(out Field _)) minion.Damage(1);
    }

}
