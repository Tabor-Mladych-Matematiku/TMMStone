using CardGame;

public class Nevysprchovaný_účastník : CardScriptBase
{
    protected override void OnDeath(object sender, System.EventArgs e) =>
        GameManager.Instance.HPCounters[GetOwner(sender).Other()].Face.Damage(2);
}
