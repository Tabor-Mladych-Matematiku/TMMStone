using CardGame;

public class Šíření_ransomware : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager.P opponent = GetOwner(sender).Other();
        for (int i = 0; i < 3; i++) GameManager.Instance.AddCardToHandByID(opponent, 89);
    }
}
