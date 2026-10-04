using CardGame;

public class Velká_mýdlová_bouře : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        ManaCounter mana = GameManager.Instance.ManaCounters[GetOwner(sender).Other()];
        if (mana.MaxMana == 0) return;
        int current = mana.Mana;
        if (current >= mana.MaxMana) current--;
        mana.MaxMana--;
        mana.Mana = current;
    }
}
