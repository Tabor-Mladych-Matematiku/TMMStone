using CardGame;

public class Extra_pár_rukou : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is Minion minion) minion.Attack *= 2;
    }

    protected override bool TargetValidate(TableActor target) => target is Minion;
}
