using CardGame;

public class Míša__Mistr_kódu : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        for (int i = 0; i < 3; i++) GameManager.Instance.AddCardToHandByID(GetOwner(sender), 102);
    }
}
