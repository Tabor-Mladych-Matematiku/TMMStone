using CardGame;

public class Útěk_za_firewall : CardScriptBase
{
    private Face protectedHero;

    protected override void OnExperimentAfterAttack(Minion attacker, Minion.TargetedEventEventArgs e)
    {
        if (attacker.Owner == Experiment.Owner || !GameManager.Instance.IsOpponentTurn(Experiment.Owner)) return;
        if (e.target is not Face face || face.Owner != Experiment.Owner) return;

        Card sourceCard = Experiment.SourceCard;
        GameManager.P owner = Experiment.Owner;
        ConsumeExperiment();

        CardSlot slot = GameManager.Instance.GetFreeEffectSlot(owner);
        if (slot == null) return;
        Effect immunityEffect = sourceCard.PlaceEffect(slot);
        immunityEffect.ConvertToPersistentEffect();//TODO: this essentially converts it from Experiment to Immunity effect. The immunity should likely be its own Effect - like Firewall: Your hero is immune. At the end of the turn - remove this.
        immunityEffect.GetComponent<Útěk_za_firewall>().Activate(face);
    }

    private void Activate(Face face)
    {
        protectedHero = face;
        protectedHero.Immune = true;
    }

    protected override void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e)
    {
        if (protectedHero == null) return;
        protectedHero.Immune = false;//TODO: this could override other Immunity providing effect.
        protectedHero = null;
        ((Effect)sender).Destroy();
    }
}
