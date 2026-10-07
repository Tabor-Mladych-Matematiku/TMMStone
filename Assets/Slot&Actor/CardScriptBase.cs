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
    protected sealed override void StartTurnBinder(Card card)
    {
        card.OnSelfPlayed += OnSelfPlayed;
        card.TargetValidator = TargetValidate;
    }
    public override bool OnBeforePlayed(CardPlayContext context) => context.TrySelectTarget(TargetValidate, out _);
    protected virtual void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e) { }
    protected abstract override bool TargetValidate(TableActor target);
}
public abstract class CardScriptBase : MonoBehaviour
{
    //TODO: stop implemented methods from being virtual. Add separete nonvirtual ones that run virtual ones at the end
    public virtual bool Taunt => false;
    public virtual bool Spellproof => false;
    public virtual int AttackCount => 1;
    public virtual bool ImmuneToAttackDamage => false;
    public virtual bool OnBeforePlayed(CardPlayContext context) => true;
    protected virtual bool TargetValidate(TableActor target) => true;
    private int dynamicAttackModifier;
    public int DynamicAttackModifier
    {
        get => dynamicAttackModifier;
        set
        {
            if (dynamicAttackModifier == value) return;
            dynamicAttackModifier = value;
            if (TryGetComponent(out Minion minion)) minion.RefreshAttack();
        }
    }
    private int spelldamage = 0;
    /// <summary>
    /// Returns an additive modifier for this card's mana cost. This does not set
    /// the final cost; the returned value is added to the card's base mana cost.
    /// </summary>
    protected virtual Func<Card, int> ManaCostModifier => null;
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
        card.OnSelfPlayed += (sender, args) => OnSelfPlayed(sender, new(args.cardType,args.Choices));//OnSelfPlayed is called when the card is played from hand and triggers before OnPlayed
    }
    public void Awake()
    {
        GameManager.Instance.OnPlayed += OnPlayed;
        GameManager.Instance.OnSummoned += _OnMinionSummoned;
        GameManager.Instance.BeforeSpellPlayed += _OnBeforeSpellPlayed;
        GameManager.Instance.BeforeAttackDeclared += _OnBeforeAttackDeclared;
        GameManager.Instance.AfterAttackResolved += _OnAfterAttackResolved;
        GameManager.Instance.MinionDied += _OnAnyMinionDied;
        GameManager.Instance.MinionPlayedForReactions += _OnAnyMinionPlayed;
        GameManager.Instance.BoardChanged += OnBoardChanged;
        
        if (TryGetComponent(out Card card))
        {
            Func<Card, int> manaCostModifier = ManaCostModifier;
            if (manaCostModifier != null)
                card.manacostmod.Add(candidate => candidate == card ? manaCostModifier(candidate) : 0);
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
            minion.OnCombatDamageDealt += OnCombatDamageDealt;
            minion.OnDeath += HandleDeath;
            minion.OnRemovedWithoutDeath += (_, _) => UnsubscribeFromGameManager();
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
    protected virtual void OnSelfSummoned(object sender, Minion.SummonedEventArgs e) { }
    protected virtual void OnBeforeAttack(object sender, Minion.TargetedEventEventArgs e) { }
    protected virtual void OnAfterAttack(object sender, Minion.TargetedEventEventArgs e) { }
    protected virtual void OnCombatDamageDealt(object sender, Minion.DamageDealtEventArgs e) { }
    protected virtual void OnHealed(object sender, EventArgs e) { }
    protected virtual void OnDamaged(object sender, EventArgs e) { }
    /// <summary>
    /// At the end of ANY turn
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e)
    {
        if (GetOwner(sender) == e.Player) OnTableActorEndOwnTurn(sender, e);
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
        if (GetOwner(sender) == e.Player) OnTableActorStartOwnTurn(sender, e);
        OnTableActorStartTurn(sender, e);
    }

    /// <summary>
    /// At the start of your turn
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnTableActorStartOwnTurn(object sender, GameActor.TurnEventArgs e) { }
    protected virtual void OnDeath(object sender, EventArgs e) { }
    private void HandleDeath(object sender, EventArgs e)
    {
        OnDeath(sender, e);
        UnsubscribeFromGameManager();
    }

    private void OnDestroy()
    {
        UnsubscribeFromGameManager();
    }

    private void UnsubscribeFromGameManager()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnPlayed -= OnPlayed;//Cleanup
        GameManager.Instance.OnSummoned -= _OnMinionSummoned;
        GameManager.Instance.BeforeSpellPlayed -= _OnBeforeSpellPlayed;
        GameManager.Instance.BeforeAttackDeclared -= _OnBeforeAttackDeclared;
        GameManager.Instance.AfterAttackResolved -= _OnAfterAttackResolved;
        GameManager.Instance.MinionDied -= _OnAnyMinionDied;
        GameManager.Instance.MinionPlayedForReactions -= _OnAnyMinionPlayed;
        GameManager.Instance.BoardChanged -= OnBoardChanged;
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
    protected virtual void OnBoardChanged() { }
    protected virtual void OnBeforeSpellPlayed(Card spell, GameManager.CancelableCardEventArgs e) { }
    protected virtual void OnExperimentBeforeSpellPlayed(Card spell, GameManager.CancelableCardEventArgs e) { }
    protected virtual void OnExperimentBeforeAttack(Minion attacker, Minion.TargetedEventEventArgs e) { }
    protected virtual void OnExperimentAfterAttack(Minion attacker, Minion.TargetedEventEventArgs e) { }
    protected virtual void OnExperimentMinionDied(Minion minion) { }
    protected virtual void OnExperimentMinionPlayed(Minion minion) { }

    private bool IsActiveExperiment(out Effect effect) => TryGetComponent(out effect) && effect.isExperiment;

    private void _OnBeforeSpellPlayed(object sender, GameManager.CancelableCardEventArgs e)
    {
        OnBeforeSpellPlayed((Card)sender, e);
        if (IsActiveExperiment(out _)) OnExperimentBeforeSpellPlayed((Card)sender, e);
    }

    private void _OnBeforeAttackDeclared(object sender, Minion.TargetedEventEventArgs e)
    {
        if (IsActiveExperiment(out _)) OnExperimentBeforeAttack((Minion)sender, e);
    }

    private void _OnAfterAttackResolved(object sender, Minion.TargetedEventEventArgs e)
    {
        if (IsActiveExperiment(out _)) OnExperimentAfterAttack((Minion)sender, e);
    }

    private void _OnAnyMinionDied(object sender, EventArgs e)
    {
        if (IsActiveExperiment(out _)) OnExperimentMinionDied((Minion)sender);
    }

    private void _OnAnyMinionPlayed(object sender, EventArgs e)
    {
        if (IsActiveExperiment(out _)) OnExperimentMinionPlayed((Minion)sender);
    }
    public class TargetlessEventArgs : EventArgs
    {
        public TargetlessEventArgs(Card.CardType cardType, IReadOnlyList<int> choices = null)
        {
            this.cardType = cardType;
            Choices = choices ?? Array.Empty<int>();
        }

        public Card.CardType cardType { get; private set; }
        public IReadOnlyList<int> Choices { get; private set; }
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
        Minion[] minions = GameManager.Instance.GetAllMinionsOwnedBy(owner)
            .Concat(GameManager.Instance.GetAllMinionsOwnedBy(owner.Other()))
            .ToArray();
        return minions.Length == 0 ? null : minions[RandomRange(0, minions.Length)];
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
    protected int CalculateSpellDamage(int damage, object sender) => damage + GetOwnersSpellDamage(GetOwner(sender));
    protected Effect Experiment => GetComponent<Effect>();
    protected void ConsumeExperiment()
    {
        if (Experiment != null && Experiment.isExperiment) Experiment.Destroy();
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

public sealed class CardChoiceOption
{
    public CardChoiceOption(int value, string text) { Value = value; Text = text; }
    public CardChoiceOption(int value, Card card) { Value = value; Card = card; }
    public int Value { get; }
    public string Text { get; }
    public Card Card { get; }
}

public sealed class CardChoiceRequest
{
    public string Header;
    public List<CardChoiceOption> Options = new();
    public int RequiredSelections = 1;
    public bool MarkSelections;
    public bool AutoSelectAll;
}

public sealed class CardPlayContext
{
    private readonly Card card;
    private readonly List<(TableActor actor, int index)> targets = new();
    private readonly List<(TableActor actor, int index)> resolvedTargets = new();
    private readonly List<int[]> choiceGroups = new();
    private int targetCursor;
    private int choiceCursor;
    private readonly bool automaticallySelectChoices;

    internal CardPlayContext(Card card, int cardIndex, int minionSlotIndex, bool automaticallySelectChoices = false)
    {
        this.card = card;
        CardIndex = cardIndex;
        MinionSlotIndex = minionSlotIndex;
        this.automaticallySelectChoices = automaticallySelectChoices;
    }

    public Card Card => card;
    public int CardIndex { get; }
    public int MinionSlotIndex { get; }
    public bool AutomaticallySelectingChoices => automaticallySelectChoices;
    public bool WaitingForInput { get; internal set; }
    public IReadOnlyList<int> TargetIndices => resolvedTargets.Select(target => target.index).ToArray();
    public IReadOnlyList<int[]> ChoiceGroups => choiceGroups;

    internal void ResetReadPosition()
    {
        targetCursor = 0;
        choiceCursor = 0;
        resolvedTargets.Clear();
        WaitingForInput = false;
    }

    internal void AddInitialTarget(TableActor actor, int index)
    {
        if (actor != null && index >= 0) targets.Add((actor, index));
    }

    public bool TrySelectTarget(Func<TableActor, bool> validator, out TableActor target)
    {
        while (targetCursor < targets.Count)
        {
            (TableActor actor, _) = targets[targetCursor++];
            if (card.IsTargetValid(actor, validator))
            {
                resolvedTargets.Add((actor, targets[targetCursor - 1].index));
                target = actor;
                return true;
            }
        }

        target = null;
        WaitingForInput = true;
        card.RequestTarget(this, validator);
        return false;
    }

    public bool TrySelectChoices(CardChoiceRequest request, out IReadOnlyList<int> choices)
    {
        if (choiceCursor < choiceGroups.Count)
        {
            choices = choiceGroups[choiceCursor++];
            return true;
        }

        int required = Mathf.Min(request.RequiredSelections, request.Options.Count);
        if (required == 0 || request.AutoSelectAll || automaticallySelectChoices)
        {
            IEnumerable<CardChoiceOption> options = automaticallySelectChoices
                ? request.Options.OrderBy(_ => UnityEngine.Random.value)
                : request.Options;
            int[] automatic = options.Take(required).Select(option => option.Value).ToArray();
            choiceGroups.Add(automatic);
            choiceCursor++;
            choices = automatic;
            return true;
        }

        choices = Array.Empty<int>();
        WaitingForInput = true;
        card.RequestChoices(this, request);
        return false;
    }

    internal void AddTarget(TableActor actor, int index) => targets.Add((actor, index));
    internal void AddChoices(int[] choices) => choiceGroups.Add(choices ?? Array.Empty<int>());
}
