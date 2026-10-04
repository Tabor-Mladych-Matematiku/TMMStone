using CardGame;

public class Genetická_modifikace : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        var mana = GameManager.Instance.ManaCounters[GetOwner(sender)];
        if (mana.MaxMana >= 10)
        {
            DrawCard(true, sender);
            return;
        }
        int current = mana.Mana;
        mana.MaxMana++;
        mana.Mana = current;
    }
}
