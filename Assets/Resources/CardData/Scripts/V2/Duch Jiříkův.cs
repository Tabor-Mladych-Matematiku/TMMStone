using System.Linq;
using CardGame;
using CardData;

public class Duch_Jiříkův : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager.P owner = GetOwner(sender);
        Minion self = (Minion)sender;
        if (GameManager.Instance.GetAllMinionsOwnedBy(owner)
            .Any(minion => minion != self && minion.HasTag(CardTag.Duch)))
            GameManager.Instance.AddCardToHandByID(owner, 216);
    }
}
