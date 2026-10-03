using CardGame;

public class Kilometrové_kolečko : CardScriptBase
{
    protected override void OnMinionSummoned(Minion minion) => minion.Damage(1);
}
