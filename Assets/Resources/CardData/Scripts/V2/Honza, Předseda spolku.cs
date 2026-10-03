using CardGame;

public class Honza__Předseda_spolku : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager.Instance.SummonMinion(GetOwner(sender).Other(), 53);
    }
}
