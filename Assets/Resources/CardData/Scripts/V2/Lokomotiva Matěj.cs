using CardGame;

public class Lokomotiva_Matěj : CardScriptBase
{
    public override bool Taunt => true;

    protected override void OnSelfSummoned(object sender, Minion.TargetedEventEventArgs e)
    {
        Minion minion = (Minion)sender;
        minion.Shielded = true;
        minion.Charge();
    }

    protected override void OnCombatDamageDealt(object sender, Minion.DamageDealtEventArgs e)
    {
        if (e.Amount > 0)
            GameManager.Instance.HPCounters[((Minion)sender).Owner].Face.Heal(e.Amount);
    }
}
