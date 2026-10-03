using System;
using CardGame;

public class Městečko_Mnichov : CardScriptBase
{
    protected override void OnDeath(object sender, EventArgs e) =>
        GameManager.Instance.AddCardToHandByID(GetOwner(sender), 184);
}
