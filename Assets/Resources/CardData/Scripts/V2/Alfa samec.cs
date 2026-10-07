using CardGame;
using CardData;

public class Alfa_samec : CardScriptBase
{
    TableActor target;
    public override bool OnBeforePlayed(CardPlayContext context) =>
        context.TrySelectTarget(TargetValidate, out target);

    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        if (target is Minion minion) minion.Buff(2, 3);
    }

    protected override bool TargetValidate(TableActor target) =>
        target is Minion minion && minion.HasTag(CardData.CardTag.Zvíře);
}
