using CardGame;

public class Ježibaba : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) =>
        GameManager.Instance.SummonMinion(GetOwner(sender).Other(), 194);
}
