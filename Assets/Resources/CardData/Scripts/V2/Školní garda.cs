using System;
using CardGame;

public class Školní_garda : CardScriptBase
{
    public override bool Taunt => true;

    protected override void OnDeath(object sender, EventArgs e)
    {
        GameManager.Instance.HPCounters[GetOwner(sender)].Heal(6);
    }
}
