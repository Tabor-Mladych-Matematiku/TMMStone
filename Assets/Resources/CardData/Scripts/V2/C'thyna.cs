using System;
using System.Linq;
using CardGame;

public class C_thyna : CardScriptBase
{
    protected override void OnDeath(object sender, EventArgs e)
    {
        GameManager.P owner = GetOwner(sender);
        if (GameManager.Instance.GetCardsInHandOwnedBy(owner).Any()) return;
        if (GameManager.Instance.decks[owner].Count != 0) return;
        if (GameManager.Instance.GetAllMinionsOwnedBy(owner).Any()) return;

        GameManager.Instance.HPCounters[owner.Other()].Health = 0;
    }
}
