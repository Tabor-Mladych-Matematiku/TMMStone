using System;
using CardGame;

public class Táborový_hlídač : CardScriptBase
{
    public override bool Taunt => true;

    protected override void OnDeath(object sender, EventArgs e) =>
        GameManager.Instance.SummonMinion(GetOwner(sender), 188);
}
