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
    protected virtual void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {

    }
}
public abstract class CardScriptBase : MonoBehaviour
{
    //TODO: stop implemented methods from being virtual. Add separete nonvirtual ones that run virtual ones at the end
    private int spelldamage = 0;
    //protected Func<Card, int> manacostmod { get; private set; }
    public delegate Func<Card, int> ManaModsModifier(GameObject gameObject);
    protected virtual Dictionary<Type, ManaModsModifier> ManaCostMods =>new();
    protected static Dictionary<Type, ManaModsModifier> CreateManaCostModDict(
    params (Type type, int cost, Card.CardType[] cardTypes)[] mods)
    {
        Dictionary<Type, ManaModsModifier> result = new ();

        foreach (var (type, cost, cardTypes) in mods)
        {
            result[type] = (gameObject)=> (card) => (cardTypes.Length == 0 || cardTypes.Contains(card.cardType)) && card.Owner == gameObject.GetComponent<TableActor>().Owner ? cost : 0;
        }

        return result;
    }
    private bool CreateManacostMod<T>(out Func<Card, int> manacostmod) where T:GameActor {
        bool overridePresent = ManaCostMods.TryGetValue(typeof(T), out var manacostData);
        manacostmod = overridePresent ? manacostData(gameObject) : null;
        return overridePresent;
    }
    protected virtual int SetSpellDamage() => 0;
    protected int RandomRange(int startInc, int endExc) => UnityEngine.Random.Range(startInc, endExc);
    protected virtual void StartTurnBinder(Card card)
    {
        card.OnSelfPlayed += (sender, args) => OnSelfPlayed(sender, new(args.cardType));//OnSelfPlayed is called when the card is played from hand and triggers before OnPlayed
    }
    public void Start()
    {
        GameManager.Instance.OnPlayed += OnPlayed;
        GameManager.Instance.OnSummoned += _OnMinionSummoned;
        
        if (TryGetComponent(out Card card))
        {
            StartTurnBinder(card);
            card.OnStartTurn += OnStartTurn;
            card.OnEndTurn += OnEndTurn;
            card.OnDiscardEvent += OnDiscard;
            return;
        }
        if (TryGetComponent(out TableActor tableactor))
        {
            tableactor.OnStartTurn += _OnTableActorStartTurn;
            tableactor.OnEndTurn += OnTableActorEndTurn;
            spelldamage = SetSpellDamage();
        }
        if (TryGetComponent(out Minion minion))
        {
            if (CreateManacostMod<Minion>(out var manacostmod)) minion.manacostmod.Add(manacostmod);

            minion.OnSelfSummoned += OnSelfSummoned;
            minion.OnBeforeAttack += OnBeforeAttack;
            minion.OnAfterAttack += OnAfterAttack;
            minion.OnDeath += OnDeath;
            minion.OnHealed += OnHealed;
            minion.OnDamaged += OnDamaged;
            return;
        }
        else if (TryGetComponent(out Field field))
        {
            if (CreateManacostMod<Field>(out var manacostmod)) field.manacostmod.Add(manacostmod);
        }
        else if (TryGetComponent(out Effect effect)) {
            if (CreateManacostMod<Effect>(out var manacostmod)) effect.manacostmod.Add(manacostmod);
        }
        //TODO: add all the other effects
    }
    //Minion events
    /// <summary>
    /// Called after the minion entered on the board and battlecried
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnSelfSummoned(object sender, Minion.TargetedEventEventArgs e) { }
    protected virtual void OnBeforeAttack(object sender, Minion.TargetedEventEventArgs e) { }
    protected virtual void OnAfterAttack(object sender, Minion.TargetedEventEventArgs e) { }
    protected virtual void OnHealed(object sender, EventArgs e) { }
    protected virtual void OnDamaged(object sender, EventArgs e) { }
    /// <summary>
    /// At the end of ANY turn
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e)
    {
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
    protected virtual void OnTableActorStartTurn(object sender, GameActor.TurnEventArgs e){}

    private void _OnTableActorStartTurn(object sender, GameActor.TurnEventArgs e)
    {
        if (GetOwner(sender) == GameManager.Instance.PlayerOnTurn) OnTableActorStartOwnTurn(sender, e);
        OnTableActorStartTurn(sender, e);
    }

    /// <summary>
    /// At the start of your turn
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnTableActorStartOwnTurn(object sender, GameActor.TurnEventArgs e) { }
    protected virtual void OnDeath(object sender, EventArgs e) { }
    private void OnDestroy()
    {
        GameManager.Instance.OnPlayed -= OnPlayed;//Cleanup
        GameManager.Instance.OnSummoned -= _OnMinionSummoned;
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
    private void OnPlayed(object sender, Card.CardPlayedEventArgs e)
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
        OnCardPlayed(sender, e);
    }
    protected virtual void OnCardPlayed(object sender, Card.CardPlayedEventArgs e) { }
    protected virtual void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e) { }
    protected virtual void OnMinionPlayed(Minion minion, Card.CardPlayedEventArgs e) { }
    protected virtual void OnFieldPlayed(Field field, Card.CardPlayedEventArgs e) { }
    private void _OnMinionSummoned(object minion,EventArgs e)=> OnMinionSummoned((Minion)minion);
    /// <summary>
    /// Will also proc on oneself
    /// </summary>
    /// <param name="minion"></param>
    protected virtual void OnMinionSummoned( Minion minion) { }
    public class TargetlessEventArgs : EventArgs
    {
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
    protected void DrawCard(bool owner, object sender) => GameManager.Instance.DrawCard(owner ? ((GameActor)sender).Owner : ((GameActor)sender).Owner.Other());
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
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="action"></param>
    protected void ToRandomEnemyCharacterDo(object sender, Action<DamageableActor> action)
    {
        var characters = GameManager.Instance.GetAllCharactersOwnedBy(GetOwner(sender).Other());
        action(characters.ElementAt(RandomRange(0, characters.Count())));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="action"></param>
    protected void ToRandomEnemyMinionDo(object sender, Action<Minion> action)
    {
        var characters = GameManager.Instance.GetAllMinionsOwnedBy(GetOwner(sender).Other());
        action(characters.ElementAt(RandomRange(0, characters.Count())));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="action"></param>
    protected void ToRandomFriendlyCharacterDo(object sender, Action<DamageableActor> action)
    {
        var characters = GameManager.Instance.GetAllCharactersOwnedBy(GetOwner(sender));
        action(characters.ElementAt(RandomRange(0, characters.Count())));
    }
    /// <summary>
    /// Gets a random minion. Needs sender for network synchronisation
    /// </summary>
    /// <param name="sender"></param>
    /// <returns></returns>
    protected Minion GetRandomMinion(object sender)
    {
        GameManager.P owner = GetOwner(sender);
        var minions = GameManager.Instance.GetAllMinionsOwnedBy(owner).Concat(GameManager.Instance.GetAllMinionsOwnedBy(owner.Other()));
        return minions.ElementAt(RandomRange(0, minions.Count()));
    }


    /// <summary>
    /// Use with caution as this is casting into GameActor
    /// </summary>
    /// <param name="sender"></param>
    /// <returns></returns>
    protected GameManager.P GetOwner(object sender) => ((GameActor)sender).Owner;
    protected void DealSpellDamage(DamageableActor target, int damage, object sender)
    {
        target.Damage(damage + GetOwnersSpellDamage(GetOwner(sender)));
    }
    public int GetOwnersSpellDamage(GameManager.P owner)
    {
        int spellDamage = 0;
        foreach (Minion m in GameManager.Instance.GetAllMinionsOwnedBy(owner))
        {
            foreach (CardScriptBase script in m.GetComponents<CardScriptBase>())
            {
                spellDamage += script.spelldamage;
            }

        }
        return spellDamage;
    }
    protected void PlaceEffect(object sender, GameManager.P who)
    {
        CardSlot effectSlot = GameManager.Instance.GetFreeEffectSlot(who);
        if (effectSlot != null) ((Card)sender).PlaceEffect(effectSlot);
    }
    protected void Summon(object sender, GameManager.P who,int what) {
        
    
    }

}
