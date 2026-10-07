using CardGame;

public class V_I__Leničkin__vůdce_revoluce : CardScriptBase
{
    TableActor target;
    public override bool OnBeforePlayed(CardPlayContext context) =>
        context.TrySelectTarget(TargetValidate, out target);

    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        if (target is not DamageableActor damageableTarget) return;
        int difference = System.Math.Abs(
            GameManager.Instance.decks[GameManager.P.P1].Count
            - GameManager.Instance.decks[GameManager.P.P2].Count);
        damageableTarget.Damage(difference);
    }

    protected override bool TargetValidate(TableActor target) => target is DamageableActor;
}
