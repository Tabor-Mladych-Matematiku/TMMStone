using CardGame;

public class Prvostupňový_matemág : CardScriptBase
{
    TableActor target;
    public override bool OnBeforePlayed(CardPlayContext context) =>
        context.TrySelectTarget(TargetValidate, out target);

    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        if (target is DamageableActor damageableTarget) damageableTarget.Damage(1);
    }

    protected override bool TargetValidate(TableActor target) => target is DamageableActor;
}
