using System;
using CardGame;

public class Táborový_lékař : CardScriptBase
{
    protected override void OnDeath(object sender, EventArgs e) =>
        GameManager.Instance.HPCounters[GetOwner(sender).Other()].Heal(5);
}
