using CardGame;

public class Rozebrání_na_zdrojový_kód : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is Minion minion) minion.Death();
        DrawCard(false, sender);
        DrawCard(false, sender);
    }

    protected override bool TargetValidate(TableActor target) => target is Minion;
}
