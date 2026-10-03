using CardGame;

public class Dobití_baterie : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager.Instance.HPCounters[GetOwner(sender)].Heal(5);
        DrawCard(true, sender);
    }
}
