using CardGame;

public class Adam__Bastlíř : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager.Instance.SummonMinion(GetOwner(sender), 155);
        GameManager.Instance.SummonMinion(GetOwner(sender), 155);
    }
}
