using CardGame;

public class Deprese_z_nepodstatnosti : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is Minion minion) minion.Death();
    }

    protected override bool TargetValidate(TableActor target) =>
        target is Minion minion && minion.Attack <= 3;
}
