using CardGame;
using UnityEngine.UI;

public class Útěk_za_firewall : CardScriptBase
{
    private Face protectedHero;

    protected override void OnExperimentAfterAttack(Minion attacker, Minion.TargetedEventEventArgs e)
    {
        if (attacker.Owner == Experiment.Owner || !GameManager.Instance.IsOpponentTurn(Experiment.Owner)) return;
        if (e.target is not Face face || face.Owner != Experiment.Owner) return;

        Card sourceCard = Experiment.Original;
        GameManager.P owner = Experiment.Owner;
        ConsumeExperiment();

        CardSlot slot = GameManager.Instance.GetFreeEffectSlot(owner);
        if (slot == null) return;
        Effect immunityEffect = sourceCard.PlaceEffect(slot);
        ConvertToPersistentEffect(immunityEffect);
        immunityEffect.GetComponent<Útěk_za_firewall>().Activate(face);
    }
//TODO: this essentially converts it from Experiment to Immunity effect. The immunity should likely be its own Effect - like Firewall: Your hero is immune. At the end of the turn - remove this.
    private static void ConvertToPersistentEffect(Effect effect)
    {
        effect.isExperiment = false;
        CardArt.Load(CardArt.PlainAddress(effect.expansion, effect.CardName), sprite =>
        {
            if (effect != null && sprite != null) effect.GetComponent<Image>().sprite = sprite;
        });
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
