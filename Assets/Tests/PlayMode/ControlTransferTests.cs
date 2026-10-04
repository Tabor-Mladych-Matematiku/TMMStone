using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CardGame;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlTransferTests
{
    GameObject root;
    GameManager manager;
    Dictionary<GameManager.P, CardSlot[]> slots;
    readonly List<Minion> createdMinions = new();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Control transfer test");
        manager = Child("Manager").AddComponent<GameManager>();
        manager.enabled = false; // No scene startup; exercise synchronous game logic only.
        Set(manager, "resolvingLocalTurn", (bool?)true);
        slots = new Dictionary<GameManager.P, CardSlot[]>();
        manager.graves = new Dictionary<GameManager.P, Grave>();
        foreach (GameManager.P player in new[] { GameManager.P.P1, GameManager.P.P2 })
        {
            slots[player] = Enumerable.Range(0, GameManager.maxMinionSlots).Select(index =>
            {
                CardSlot slot = Child("Slot").AddComponent<CardSlot>();
                slot.Initialize(player, index);
                return slot;
            }).ToArray();
            manager.graves[player] = Child("Grave").AddComponent<Grave>();
        }
        Set(manager, "minionSlots", slots);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (Minion minion in createdMinions)
            if (minion != null) UnityEngine.Object.DestroyImmediate(minion.gameObject);
        createdMinions.Clear();
        UnityEngine.Object.DestroyImmediate(root);
    }

    [TestCase(GameManager.P.P1, GameManager.P.P2)]
    [TestCase(GameManager.P.P2, GameManager.P.P1)]
    public void TransferPreservesInstanceAndStatsAndUsesRightmostFreeSlot(GameManager.P from, GameManager.P to)
    {
        Minion minion = CreateMinion(from, 0);
        Card card = slots[from][0].GetComponentInChildren<Card>(true);
        CreateMinion(to, 6);
        minion.Buff(3, 4);
        minion.Damage(2);
        minion.Frozen = true;
        minion.Charge();
        var taunt = minion.gameObject.AddComponent<Kelnatec_šedohřbetý>();
        int summons = 0;
        int plays = 0;
        int deaths = 0;
        manager.OnSummoned += (_, _) => summons++;
        manager.OnPlayed += (_, _) => plays++;
        minion.OnDeath += (_, _) => deaths++;

        Assert.That(manager.TakeControl(minion, to), Is.True);
        Assert.That(slots[from][0].Occupied, Is.False);
        Assert.That(slots[to][5].GetMinion(), Is.SameAs(minion));
        Assert.That(slots[to][5].GetComponentInChildren<Card>(true), Is.SameAs(card));
        Assert.That(card.Owner, Is.EqualTo(to));
        Assert.That(minion.Owner, Is.EqualTo(to));
        Assert.That(minion.Attack, Is.EqualTo(5));
        Assert.That(minion.Health, Is.EqualTo(12));
        Assert.That(minion.MaxHealth, Is.EqualTo(14));
        Assert.That(minion.Frozen, Is.True);
        Assert.That(minion.GetComponent<Kelnatec_šedohřbetý>(), Is.SameAs(taunt));
        Assert.That(minion.HasTaunt, Is.True);
        Assert.That(minion.CanAttack, Is.False);
        Assert.That(card.gameObject.activeSelf, Is.False);
        Assert.That(summons + plays + deaths, Is.Zero);
    }

    [Test]
    public void ExplicitImmediateAttackDoesNotBypassFrozen()
    {
        Minion ready = CreateMinion(GameManager.P.P1, 0);
        Assert.That(manager.TakeControl(ready, GameManager.P.P2, allowImmediateAttack: true), Is.True);
        Assert.That(ready.CanAttack, Is.True);

        Minion frozen = CreateMinion(GameManager.P.P1, 1);
        frozen.Frozen = true;
        Assert.That(manager.TakeControl(frozen, GameManager.P.P2, allowImmediateAttack: true), Is.True);
        Assert.That(frozen.Frozen, Is.True);
        Assert.That(frozen.CanAttack, Is.False);
    }

    [Test]
    public void FullBoardRemovesWithoutDeathDiscardOrDelayedCardTriggers()
    {
        Minion minion = CreateMinion(GameManager.P.P1, 0);
        Card card = slots[GameManager.P.P1][0].GetComponentInChildren<Card>(true);
        minion.gameObject.AddComponent<Algedrak>();
        for (int i = 0; i < GameManager.maxMinionSlots; i++)
            CreateMinion(GameManager.P.P2, i);
        int deaths = 0;
        int discards = 0;
        minion.OnDeath += (_, _) => deaths++;
        card.OnDiscardEvent += (_, _) => discards++;

        Assert.That(manager.TakeControl(minion, GameManager.P.P2), Is.False);
        Assert.That(slots[GameManager.P.P1][0].Occupied, Is.False);
        Assert.That(manager.GetAllMinionsOwnedBy(GameManager.P.P2).Count(), Is.EqualTo(GameManager.maxMinionSlots));
        Assert.That(manager.graves[GameManager.P.P1].Contains(card), Is.True);
        Assert.That(manager.graves[GameManager.P.P2].Count, Is.Zero);
        Assert.That(deaths + discards, Is.Zero);

        // Destroy is deferred: global listeners must already be detached in this frame.
        var played = (EventHandler<Card.CardPlayedEventArgs>)typeof(GameManager)
            .GetField("OnPlayed", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager);
        played?.Invoke(card, new Card.CardPlayedEventArgs(Card.CardType.Spell, null));
        Assert.That(minion.Attack, Is.EqualTo(2));
    }

    [Test]
    public void TakingAnAlreadyFriendlyMinionIsANoOpEvenOnAFullBoard()
    {
        Minion minion = CreateMinion(GameManager.P.P1, 0);
        minion.Charge();
        for (int i = 1; i < GameManager.maxMinionSlots; i++)
            CreateMinion(GameManager.P.P1, i);
        Assert.That(manager.TakeControl(minion, GameManager.P.P1), Is.True);
        Assert.That(slots[GameManager.P.P1][0].GetMinion(), Is.SameAs(minion));
        Assert.That(minion.CanAttack, Is.True);
    }

    [Test]
    public void WalkingHouseChangesSidesOnlyAtItsControllersTurnStartWithoutSummoning()
    {
        Minion house = CreateMinion(GameManager.P.P1, 0);
        house.gameObject.AddComponent<Chaloupka_na_kuří_nožce>();
        int summons = 0;
        manager.OnSummoned += (_, _) => summons++;
        house.StartTurn(false);
        Assert.That(house.Owner, Is.EqualTo(GameManager.P.P1));
        house.StartTurn(true);
        Assert.That(slots[GameManager.P.P2][6].GetMinion(), Is.SameAs(house));
        Assert.That(house.HasTaunt, Is.True);
        Assert.That(house.CanAttack, Is.False);
        house.StartTurn(true); // A lazy board traversal can visit the new slot as well.
        Assert.That(house.Owner, Is.EqualTo(GameManager.P.P2));
        Set(manager, "resolvingLocalTurn", (bool?)false);
        house.StartTurn(false);
        Assert.That(slots[GameManager.P.P1][6].GetMinion(), Is.SameAs(house));
        Assert.That(house.CanAttack, Is.False);
        Assert.That(summons, Is.Zero);
    }

    Minion CreateMinion(GameManager.P owner, int index)
    {
        GameObject cardObject = Child("Card", typeof(RectTransform));
        cardObject.SetActive(false);
        Card card = cardObject.AddComponent<Card>();
        slots[owner][index].PlaceCard(card);

        GameObject body = Child("Minion", typeof(RectTransform), typeof(Image));
        body.SetActive(false);
        body.transform.SetParent(slots[owner][index].transform, false);
        Minion minion = body.AddComponent<Minion>();
        Image graphic = body.GetComponent<Image>();
        Set(minion, "graphic", graphic);
        Set(minion, "HighlightRim", graphic);
        Set(minion, "AttackLabel", Child("Attack", typeof(RectTransform)).AddComponent<TextMeshProUGUI>());
        Set(minion, "HealthLabel", Child("Health", typeof(RectTransform)).AddComponent<TextMeshProUGUI>());
        Set(minion, "original", card);
        body.SetActive(true);
        minion.MaxHealth = 10;
        minion.Health = 10;
        minion.Attack = 2;
        createdMinions.Add(minion);
        return minion;
    }

    GameObject Child(string name, params Type[] components)
    {
        var result = new GameObject(name, components);
        result.transform.SetParent(root.transform, false);
        return result;
    }

    static void Set(object instance, string name, object value)
    {
        for (Type type = instance.GetType(); type != null; type = type.BaseType)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) continue;
            field.SetValue(instance, value);
            return;
        }
        throw new MissingFieldException(name);
    }
}
