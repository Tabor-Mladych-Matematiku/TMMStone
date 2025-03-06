using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;
using UnityEngine.TextCore.Text;
using System.Linq;

/// <summary>
/// This will ensure I don't forget to set Targetable on future scripts
/// </summary>
public abstract class TargetableCardScriptBase : CardScriptBase
{
    sealed protected override void StartTurnBinder(Card card)
    {
        card.OnSelfPlayed += OnSelfPlayed;//OnSelfPlayed is called when the card is played from hand and triggers before OnPlayed
    }
    protected virtual void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e) { 
        
    }
}

public abstract class CardScriptBase : MonoBehaviour
{
    protected int spelldamage = 0;
    protected int RandomRange(int startInc,int endExc) => UnityEngine.Random.Range(startInc, endExc);
    protected virtual void StartTurnBinder(Card card)
    {
        card.OnSelfPlayed += (sender, args) => OnSelfPlayed(sender, new(args.cardType));//OnSelfPlayed is called when the card is played from hand and triggers before OnPlayed
    }
    public virtual void Start()
    {
        GameManager.Instance.OnPlayed+= OnPlayed;
        if (TryGetComponent(out Card card))
        {
            StartTurnBinder(card);
            card.OnStartTurn += OnStartTurn;
            card.OnEndTurn += OnEndTurn;
            card.OnDiscardEvent += OnDiscard;
            return;
        }
        if (TryGetComponent(out Minion minion))
        {
            minion.OnBeforeAttack += OnBeforeAttack;
            minion.OnAfterAttack += OnAfterAttack;
            minion.OnDeath += OnDeath;
            minion.OnStartTurn += OnTableActorStartTurn;
            minion.OnEndTurn += OnTableActorEndTurn;
            minion.OnHealed += OnHealed;
            minion.OnDamaged += OnDamaged;
            return;
        }
        if(TryGetComponent(out Field field))
        {
            field.OnStartTurn += OnTableActorStartTurn;
            field.OnEndTurn += OnTableActorEndTurn;
        }
        //TODO: add all the other effects
    }
    //Minion events
    protected virtual void OnBeforeAttack(object sender, Minion.TargetedEventEventArgs e) { }
    protected virtual void OnAfterAttack(object sender, Minion.TargetedEventEventArgs e) { }
    protected virtual void OnHealed(object sender, EventArgs e) { }
    protected virtual void OnDamaged(object sender, EventArgs e) { }
    /// <summary>
    /// At the end of ANY turn
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e) {
        if (GetOwner(sender) == GameManager.Instance.PlayerOnTurn) OnTableActorEndOwnTurn(sender, e);
    }
    /// <summary>
    /// At the end of your turn
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e) { }
    /// <summary>
    /// At the start of ANY turn
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnTableActorStartTurn(object sender, GameActor.TurnEventArgs e) {
        if (GetOwner(sender) == GameManager.Instance.PlayerOnTurn)OnMinionStartOwnTurn(sender, e);
    }
    /// <summary>
    /// At the start of your turn
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnMinionStartOwnTurn(object sender, GameActor.TurnEventArgs e) {}
    protected virtual void OnDeath(object sender, EventArgs e) {}
    private void OnDestroy()
    {
        GameManager.Instance.OnPlayed -= OnPlayed;//Cleanup
    }
    //Card events
    protected virtual void OnDiscard(object sender, EventArgs e) { }

    protected virtual void OnEndTurn(object sender, GameActor.TurnEventArgs e) { }
    /// <summary>
    /// While in hand or in deck. Not on Minion or other
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnStartTurn(object sender, GameActor.TurnEventArgs e) { }
    protected virtual void OnPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        //Card card = (Card)sender;
        switch (e.cardType)
        {
            case Card.CardType.Minion:
                OnMinionPlayed((Minion)sender, e);
                break;
            case Card.CardType.Field:
                OnFieldPlayed((Field)sender, e);
                break;
            case Card.CardType.Spell:
                OnSpellPlayed((Card)sender, e);
                break;
            default:
                throw new NotImplementedException();
        }
    }
    protected virtual void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e) { }
    protected virtual void OnMinionPlayed(Minion minion, Card.CardPlayedEventArgs e) { }
    protected virtual void OnFieldPlayed(Field field, Card.CardPlayedEventArgs e) { }
    public class TargetlessEventArgs : EventArgs {
        public TargetlessEventArgs(Card.CardType cardType)
        {
            this.cardType = cardType;
        }

        public Card.CardType cardType { get; private set; }
    }
    protected virtual void OnSelfPlayed(object sender, TargetlessEventArgs e) { }
    //Shorthands
    /// <summary>
    /// Draws a card for the sender.Owner if owner is true
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="sender"></param>
    protected void DrawCard(bool owner, object sender) => GameManager.Instance.DrawCard(owner?((GameActor)sender).Owner: ((GameActor)sender).Owner.Other());
    /// <summary>
    /// Does an action for both players starting with the owner of the object - GameActor
    /// </summary>
    /// <param name="action"></param>
    /// <param name="sender">Assumed GameActor</param>
    protected void ForEachPlayerDo(Action<GameManager.P> action, object sender) => ForEachPlayerDo(action, ((GameActor)sender).Owner);
    protected void ForEachPlayerDo(Action<GameManager.P> action, GameManager.P owner)
    {
        action(owner);
        action(owner.Other());
    }
    /// <summary>
    /// Do action for all characters opposing the sender
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="action"></param>
    protected void ToRandomEnemyCharacterDo(object sender,Action<DamageableActor> action)
    {
        var characters = GameManager.Instance.GetAllCharactersOwnedBy(GetOwner(sender).Other());
        action(characters.ElementAt(RandomRange(0, characters.Count())));
    }
    /// <summary>
    /// Do action for all minions opposing the sender
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="action"></param>
    protected void ToRandomEnemyMinionDo(object sender, Action<Minion> action)
    {
        var characters = GameManager.Instance.GetAllMinionsOwnedBy(GetOwner(sender).Other());
        action(characters.ElementAt(RandomRange(0, characters.Count())));
    }
    /// <summary>
    /// Use with caution as this is casting into GameActor
    /// </summary>
    /// <param name="sender"></param>
    /// <returns></returns>
    protected GameManager.P GetOwner(object sender) => ((GameActor)sender).Owner;
    protected void DealSpellDamage(DamageableActor target,int damage,object sender)
    {
        target.Damage(damage + GetOwnersSpellDamage(GetOwner(sender)));
    }
    public int GetOwnersSpellDamage(GameManager.P owner)
    {
        int spellDamage = 0;
        foreach (Minion m in GameManager.Instance.GetAllMinionsOwnedBy(owner))
        {
            foreach (var script in m.GetComponents< CardScriptBase>())
            {
                spellDamage += script.spelldamage;
            }

        }
        return spellDamage;
    }

}
