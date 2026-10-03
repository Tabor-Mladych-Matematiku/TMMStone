using CardGame;

public class Vodní_pistolka_s_tekutým_dusíkem : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is not Minion minion) return;
        if (minion.Frozen) minion.Death();
        else minion.Frozen = true;
    }

    protected override bool TargetValidate(TableActor target) => target is Minion;
}
