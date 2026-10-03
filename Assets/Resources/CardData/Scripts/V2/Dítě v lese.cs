using CardGame;

public class Dítě_v_lese : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        if (sender is Minion minion) minion.Damage(4);
    }
}
