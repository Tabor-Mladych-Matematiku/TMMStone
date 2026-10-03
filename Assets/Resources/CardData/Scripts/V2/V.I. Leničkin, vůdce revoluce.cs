using CardGame;

public class V_I__Leničkin__vůdce_revoluce : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is not DamageableActor target) return;
        int difference = System.Math.Abs(
            GameManager.Instance.decks[GameManager.P.P1].Count
            - GameManager.Instance.decks[GameManager.P.P2].Count);
        target.Damage(difference);
    }

    protected override bool TargetValidate(TableActor target) => target is DamageableActor;
}
