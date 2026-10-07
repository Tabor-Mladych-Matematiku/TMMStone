using CardGame;

public class David__Neohrožený_dobrodruh : CardScriptBase
{
    protected override void OnSelfSummoned(object sender, Minion.SummonedEventArgs e)
    {
        Minion david = (Minion)sender;
        david.Charge();
        Minion iguana = GameManager.Instance.SummonMinion(david.Owner, 127);
        if (iguana != null && iguana.TryGetComponent(out Leguán script)) script.David = david;
        GameManager.Instance.SummonMinion(david.Owner.Other(), 126);
    }
}
