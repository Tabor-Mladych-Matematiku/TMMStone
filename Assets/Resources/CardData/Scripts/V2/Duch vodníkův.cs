using System;
using CardGame;

public class Duch_vodníkův : CardScriptBase
{
    protected override void OnDeath(object sender, EventArgs e)
    {
        GameManager.Instance.AddCardToHandByID(GetOwner(sender), 212);
        GameManager.Instance.AddCardToHandByID(GetOwner(sender), 212);
    }
}
