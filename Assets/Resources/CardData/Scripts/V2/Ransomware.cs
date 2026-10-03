using CardGame;

public class Ransomware : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) =>
        DealSpellDamage(GameManager.Instance.HPCounters[GetOwner(sender)].Face, 2, sender);
}
