using CardGame;

public class Roztržitá_dozorkyně : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager.P owner = GetOwner(sender);
        GameManager.Instance.SummonMinion(owner.Other(), 206);
        GameManager.Instance.AddCardToHandByID(owner, 207);
    }
}
