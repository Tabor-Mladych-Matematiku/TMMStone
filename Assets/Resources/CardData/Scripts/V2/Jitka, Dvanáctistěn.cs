using System;
using System.Linq;
using CardGame;

public class Jitka__Dvanáctistěn : CardScriptBase
{
    protected override void OnDeath(object sender, EventArgs e)
    {
        GameManager.P owner = GetOwner(sender);
        if (GameManager.Instance.graves[owner].Any(card => card.ID == 55))
            GameManager.Instance.SummonMinion(owner, 58);
    }
}
