using System;
using System.Linq;
using CardGame;

public class Sabča__Dvacetistěn : CardScriptBase
{
    protected override void OnDeath(object sender, EventArgs e)
    {
        GameManager.P owner = GetOwner(sender);
        if (GameManager.Instance.graves[owner].Any(card => card.ID == 56))
            GameManager.Instance.SummonMinion(owner, 58);
    }
}
