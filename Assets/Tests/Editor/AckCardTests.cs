using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CardData;
using CardGame;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AckCardTests
{
    GameObject root;
    GameManager manager;
    Dictionary<GameManager.P, CardSlot[]> minions;
    const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("ACK card tests");
        manager = NewObject("Manager").AddComponent<GameManager>();
        typeof(GameManager).GetProperty("Instance").SetValue(null, manager);
        Set(manager, "resolvingLocalTurn", (bool?)true);
        minions = Slots(GameManager.maxMinionSlots);
        Set(manager, "minionSlots", minions);
        Set(manager, "HandSlots", Slots(GameManager.maxHandSlots));
        manager.FieldSlot = NewObject("Field slot").AddComponent<FieldSlot>();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);
    }

    [Test]
    public void AllAckCardsAndTheirTokenVariantsResolveToScripts()
    {
        var extra = CDJsonUtils.JSONToList(Resources.Load<TextAsset>("CardData/extraCardData").text);
        var names = new HashSet<string>(extra.Cast<Dictionary<string, object>>()
            .Where(card => card.TryGetValue("ReviewNote", out var note) && (string)note == "ACK")
            .Select(card => (string)card["Název"]));
        int checkedCards = 0;
        foreach (var pair in CDJsonUtils.LoadCardDatabase())
        {
            string name = pair.Value.name;
            if (name.EndsWith(" (token)")) name = name[..^8];
            if (!names.Contains(name)) continue;
            Assert.That(pair.Value.scripts, Is.Not.Empty, pair.Value.name);
            foreach (string path in pair.Value.scripts)
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Resources/" + path + ".cs");
                Assert.That(script, Is.Not.Null, path);
                Assert.That(script.GetClass(), Is.Not.Null, path);
                Assert.That(typeof(CardScriptBase).IsAssignableFrom(script.GetClass()), Is.True, path);
            }
            checkedCards++;
        }
        Assert.That(checkedCards, Is.EqualTo(36));
    }

    [Test]
    public void RevealFieldCoversExistingAndNewHandCardsButNotDecks()
    {
        Card own = Card(1);
        Card opponent = Card(1);
        Card deck = Card(1);
        deck.Hidden = true;
        manager.AddCardToHand(GameManager.P.P1, own);
        manager.AddCardToHand(GameManager.P.P2, opponent);
        Assert.That(opponent.Hidden, Is.True);

        var field = NewObject("Reveal field").AddComponent<Field>();
        field.transform.SetParent(manager.FieldSlot.transform);
        field.gameObject.AddComponent<Akademická_spolupráce>();
        manager.RefreshHandVisibility();
        Card drawn = Card(33);
        manager.AddCardToHand(GameManager.P.P2, drawn);
        Assert.That(own.Hidden, Is.False);
        Assert.That(opponent.Hidden, Is.False);
        Assert.That(drawn.Hidden, Is.False);
        Assert.That(deck.Hidden, Is.True);

        // FieldSlot.RemoveActor detaches the field before deferred destruction.
        field.transform.SetParent(root.transform);
        manager.RefreshHandVisibility();
        Assert.That(own.Hidden, Is.False);
        Assert.That(opponent.Hidden, Is.True);
        Assert.That(drawn.Hidden, Is.True);
    }

    [Test]
    public void CarPrefersFirstEnemyHonzaAndBypassesTauntFrozenAndAttackLimit()
    {
        Minion car = Minion(GameManager.P.P1, 0, 53);
        Minion friend = Minion(GameManager.P.P1, 1, 54);
        Minion taunt = Minion(GameManager.P.P2, 0, 15);
        taunt.gameObject.AddComponent<Kelnatec_šedohřbetý>();
        Minion first = Minion(GameManager.P.P2, 1, 54);
        Minion second = Minion(GameManager.P.P2, 2, 54);
        car.Attack = 12;
        car.Frozen = true;
        Set(car, "attacksRemaining", 0);
        var script = car.gameObject.AddComponent<Honzovo_auto>();
        Assert.That(car.IsTargetValid(first), Is.False);
        Invoke(script, "OnTableActorStartOwnTurn", car, new GameActor.TurnEventArgs(true));
        Assert.That(first.Health, Is.EqualTo(88));
        Assert.That(second.Health, Is.EqualTo(100));
        Assert.That(friend.Health, Is.EqualTo(100));
        Assert.That(taunt.Health, Is.EqualTo(100));
        Assert.That(car.Health, Is.EqualTo(99));
        Assert.That(car.Frozen, Is.True);
        Assert.That(car.CanAttack, Is.False);
    }

    [Test]
    public void CarFallsBackToFriendlyHonzaAndDoesNothingWithoutOne()
    {
        Minion car = Minion(GameManager.P.P1, 0, 53);
        Minion friend = Minion(GameManager.P.P1, 1, 54);
        car.Attack = 12;
        Set(car, "attacksRemaining", 1);
        var script = car.gameObject.AddComponent<Honzovo_auto>();
        Invoke(script, "OnTableActorStartOwnTurn", car, new GameActor.TurnEventArgs(true));
        Assert.That(friend.Health, Is.EqualTo(88));
        Assert.That(car.CanAttack, Is.True);
        friend.transform.SetParent(root.transform);
        Invoke(script, "OnTableActorStartOwnTurn", car, new GameActor.TurnEventArgs(true));
        Assert.That(friend.Health, Is.EqualTo(88));
        Assert.That(car.Health, Is.EqualTo(99));
    }

    [Test]
    public void PololetniTestDoesNotHitMinionsAddedDuringDamageResolution()
    {
        Minion first = Minion(GameManager.P.P2, 0, 48);
        Minion summoned = null;
        first.OnDamaged += (_, _) => summoned = Minion(GameManager.P.P2, 1, 48);
        Card spell = Card(75);
        spell.backupOwner = GameManager.P.P1;
        var script = spell.gameObject.AddComponent<Pololetní_test>();
        Invoke(script, "OnSelfPlayed", spell, new CardScriptBase.TargetlessEventArgs(CardGame.Card.CardType.Spell));
        Assert.That(first.Health, Is.EqualTo(96));
        Assert.That(summoned, Is.Not.Null);
        Assert.That(summoned.Health, Is.EqualTo(100));
    }

    GameObject NewObject(string name)
    {
        var result = new GameObject(name);
        result.transform.SetParent(root.transform);
        return result;
    }

    Dictionary<GameManager.P, CardSlot[]> Slots(int count)
    {
        var result = new Dictionary<GameManager.P, CardSlot[]>();
        foreach (GameManager.P owner in new[] { GameManager.P.P1, GameManager.P.P2 })
        {
            result[owner] = Enumerable.Range(0, count).Select(index =>
            {
                CardSlot slot = NewObject("Slot").AddComponent<CardSlot>();
                slot.Initialize(owner, index);
                return slot;
            }).ToArray();
        }
        return result;
    }

    Card Card(int id)
    {
        Card card = NewObject("Card").AddComponent<Card>();
        Set(card, "<ID>k__BackingField", id);
        Set(card, "sr", card.GetComponent<Image>());
        return card;
    }

    Minion Minion(GameManager.P owner, int slot, int id)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Slot&Actor/Minion.prefab");
        Minion minion = UnityEngine.Object.Instantiate(prefab, minions[owner][slot].transform).GetComponent<Minion>();
        Set(minion, "original", Card(id));
        minion.audioSource = minion.GetComponent<AudioSource>();
        minion.MaxHealth = 100;
        minion.Health = 100;
        minion.Attack = 1;
        return minion;
    }

    static void Set(object instance, string field, object value)
    {
        for (Type type = instance.GetType(); type != null; type = type.BaseType)
        {
            FieldInfo info = type.GetField(field, PrivateInstance);
            if (info == null) continue;
            info.SetValue(instance, value);
            return;
        }
        throw new MissingFieldException(field);
    }

    static void Invoke(object script, string method, object sender, object args)
    {
        script.GetType().GetMethod(method, PrivateInstance).Invoke(script, new[] { sender, args });
    }
}
