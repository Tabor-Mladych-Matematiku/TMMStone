using System;
using CardGame;

public class Leguán : CardScriptBase
{
    public Minion David { get; set; }

    protected override void OnDeath(object sender, EventArgs e)
    {
        if (David != null && David.Alive() && David.GetComponentInParent<CardSlot>() != null)
            David.Death();
    }
}
