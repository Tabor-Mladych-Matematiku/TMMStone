using CardGame;

public class Standardizace : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is Minion minion) minion.Attack = 1;
    }

    protected override bool TargetValidate(TableActor target) =>
        target is Minion && target.Owner == GetComponent<Card>().Owner.Other();
}
