using CardGame;

public class Zaokrouhlení : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is not Minion minion) return;
        int roundedHealth = (minion.Health + 5) / 10 * 10;
        minion.MaxHealth = roundedHealth;
        minion.Health = roundedHealth;
    }

    protected override bool TargetValidate(TableActor target) => target is Minion;
}
