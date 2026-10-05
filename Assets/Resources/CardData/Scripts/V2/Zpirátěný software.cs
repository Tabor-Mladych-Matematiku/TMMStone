using CardGame;

public class Zpirátěný_software : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager.P owner = GetOwner(sender);
        GameManager.Instance.SummonMinion(owner, 314);
        GameManager.Instance.AddCardToDeckAtRandomByID(owner, ((Card)sender).ID);
    }
}
