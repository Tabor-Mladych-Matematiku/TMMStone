using CardGame;

public class Kamarád_ze_Strahova : CardScriptBase
{
    public override bool Taunt => true;

    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) =>
        GameManager.Instance.HPCounters[GetOwner(sender)].Heal(6);
}
