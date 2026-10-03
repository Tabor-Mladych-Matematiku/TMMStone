using CardGame;

public class Alfa_samec : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is Minion minion) minion.Buff(2, 3);
    }

    protected override bool TargetValidate(TableActor target) =>
        target is Minion minion && minion.HasTag("Zvíře");
}
