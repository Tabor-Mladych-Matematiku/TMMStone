using CardGame;

public class Prvostupňový_matemág : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is DamageableActor target) target.Damage(1);
    }

    protected override bool TargetValidate(TableActor target) => target is DamageableActor;
}
