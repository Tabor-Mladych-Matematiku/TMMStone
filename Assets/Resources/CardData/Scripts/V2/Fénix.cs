using System;
using CardGame;

public class Fénix : CardScriptBase
{
    protected override void OnDeath(object sender, EventArgs e)
    {
        Minion minion = (Minion)sender;
        GameManager.Instance.SummonMinion(minion.Owner, minion.CardID);
    }
}
