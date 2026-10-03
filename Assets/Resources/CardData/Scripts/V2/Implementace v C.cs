using CardGame;

public class Implementace_v_C : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is Minion minion) minion.Death();
        GameManager.Instance.SummonMinion(GetOwner(sender), 314);
    }

    protected override bool TargetValidate(TableActor target) => target is Minion;
}
