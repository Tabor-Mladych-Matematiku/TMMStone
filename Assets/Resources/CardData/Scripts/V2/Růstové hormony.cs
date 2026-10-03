using CardGame;

public class Růstové_hormony : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is not Minion minion) return;
        minion.Attack = 10;
        minion.MaxHealth = 10;
        minion.Health = 10;
    }

    protected override bool TargetValidate(TableActor target) => target is Minion;
}
