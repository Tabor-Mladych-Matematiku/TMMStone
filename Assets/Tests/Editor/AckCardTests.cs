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
    public void CardTagsAreParsedIntoAUniqueEnumSet()
    {
        HashSet<CardTag> tags = CDJsonUtils.ParseTags("Stroj, Zvíře, Stroj");

        Assert.That(tags, Is.EquivalentTo(new[] { CardTag.Stroj, CardTag.Zvíře }));
        Assert.Throws<FormatException>(() => CDJsonUtils.ParseTags("Neznámý štítek"));
    }

    [Test]
    public void LoadedCardsRetainOnlyTypedTags()
    {
        var database = CDJsonUtils.LoadCardDatabase();

        Assert.That(database[155].tags, Is.EquivalentTo(new[] { CardTag.Stroj, CardTag.Zvíře }));
        Assert.That(database[1].tags, Is.Empty);
    }

    [Test]
    public void ImplementedAckBatchAndTokenVariantsResolveToScripts()
    {
        // Review approval does not mean implementation: later review rounds add ACKs.
        var implementedIds = new HashSet<int> {
            15, 21, 22, 23, 24, 25, 27, 28, 29, 32, 33, 34, 36, 38, 45, 46,
            47, 48, 49, 50, 51, 52, 54, 55, 56, 58, 59, 64, 72, 73, 74, 75,
            76, 78, 17, 174,
            69, 79, 80, 81, 82, 84, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97,
            101, 103, 107, 108, 110, 113, 115, 123, 128, 129, 133, 134, 135,
            137, 145, 146, 147, 148, 150, 153, 157, 163, 164, 167, 169, 172,
            176, 177, 178, 179, 180, 182, 183, 187, 188, 193, 196, 199, 203,
            204, 205, 208, 209, 211, 214, 215, 217, 219, 312,
            42, 65, 118, 119, 120, 122, 124, 138, 139, 156, 194, 216,
            225, 226, 227, 228, 314,
            30, 43, 87, 125, 127, 131, 132, 136, 152, 158, 159, 160, 161, 162, 165
        };
        int checkedCards = 0;
        foreach (var pair in CDJsonUtils.LoadCardDatabase())
        {
            if (!implementedIds.Contains(pair.Key)) continue;
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
        Assert.That(checkedCards, Is.EqualTo(133));
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
    public void LordOfChaosCreatesOneSharedDrawPile()
    {
        CreateDeckAndGraveZones();
        Card first = Card(1);
        Card second = Card(33);
        manager.decks[GameManager.P.P1].Add(first);
        manager.decks[GameManager.P.P2].Add(second);

        Card lord = Card(17);
        Type lordScriptType = typeof(Honzovo_auto).Assembly.GetType("EVIL_Jiřík__Pán_chaosu", true);
        Component lordScript = lord.gameObject.AddComponent(lordScriptType);
        Invoke(lordScript, "OnSelfPlayed", lord,
            new CardScriptBase.TargetlessEventArgs(CardGame.Card.CardType.Minion));

        Assert.That(manager.decks[GameManager.P.P1].Pile,
            Is.SameAs(manager.decks[GameManager.P.P2].Pile));
        Assert.That(manager.decks[GameManager.P.P1], Is.EquivalentTo(new[] { first, second }));
        manager.decks[GameManager.P.P1].PopFirst();
        Assert.That(manager.decks[GameManager.P.P2].Count, Is.EqualTo(1));
        lord.backupOwner = GameManager.P.P2;
        Invoke(lordScript, "OnSelfPlayed", lord,
            new CardScriptBase.TargetlessEventArgs(CardGame.Card.CardType.Minion));
        Assert.That(manager.decks[GameManager.P.P1].Count, Is.EqualTo(1));
    }

    [Test]
    public void ChaosWardenExchangesGravesAndDrawPilesWithoutDuplicatingAliasedPile()
    {
        CreateDeckAndGraveZones();
        Card formerDeck = Card(1);
        Card firstGrave = Card(33);
        Card secondGrave = Card(48);
        manager.decks[GameManager.P.P1].Add(formerDeck);
        Card lord = Card(17);
        Type lordScriptType = typeof(Honzovo_auto).Assembly.GetType("EVIL_Jiřík__Pán_chaosu", true);
        Invoke(lord.gameObject.AddComponent(lordScriptType), "OnSelfPlayed", lord,
            new CardScriptBase.TargetlessEventArgs(CardGame.Card.CardType.Minion));
        manager.graves[GameManager.P.P1].Add(firstGrave);
        manager.graves[GameManager.P.P2].Add(secondGrave);

        Card warden = Card(314);
        Type wardenScriptType = typeof(Honzovo_auto).Assembly.GetType(
            "EVIL_Jiřík__Dozorce_chaosu__druhá_fáze", true);
        Component wardenScript = warden.gameObject.AddComponent(wardenScriptType);
        Invoke(wardenScript, "OnSelfPlayed", warden,
            new CardScriptBase.TargetlessEventArgs(CardGame.Card.CardType.Minion));

        Assert.That(manager.decks[GameManager.P.P1].Pile,
            Is.SameAs(manager.decks[GameManager.P.P2].Pile));
        Assert.That(manager.decks[GameManager.P.P1], Is.EquivalentTo(new[] { firstGrave, secondGrave }));
        Assert.That(manager.graves[GameManager.P.P1].Pile,
            Is.SameAs(manager.graves[GameManager.P.P2].Pile));
        Assert.That(manager.graves[GameManager.P.P1], Is.EquivalentTo(new[] { formerDeck }));

        warden.backupOwner = GameManager.P.P2;
        Invoke(wardenScript, "OnSelfPlayed", warden,
            new CardScriptBase.TargetlessEventArgs(CardGame.Card.CardType.Minion));
        Assert.That(manager.decks[GameManager.P.P1].Count, Is.EqualTo(1));
    }

    [Test]
    public void CarPrefersFirstEnemyHonzaAndBypassesTauntAndAttackLimit()
    {
        Minion car = Minion(GameManager.P.P1, 0, 53);
        Minion friend = Minion(GameManager.P.P1, 1, 54);
        Minion taunt = Minion(GameManager.P.P2, 0, 15);
        taunt.gameObject.AddComponent<Kelnatec_šedohřbetý>();
        Minion first = Minion(GameManager.P.P2, 1, 54);
        Minion second = Minion(GameManager.P.P2, 2, 54);
        car.Attack = 12;
        Set(car, "attacksRemaining", 0);
        var script = car.gameObject.AddComponent<Honzovo_auto>();
        Assert.That(car.IsTargetValid(first), Is.False);
        Invoke(script, "OnTableActorStartOwnTurn", car, new GameActor.TurnEventArgs(true));
        Assert.That(first.Health, Is.EqualTo(88));
        Assert.That(second.Health, Is.EqualTo(100));
        Assert.That(friend.Health, Is.EqualTo(100));
        Assert.That(taunt.Health, Is.EqualTo(100));
        Assert.That(car.Health, Is.EqualTo(99));
        Assert.That(car.Frozen, Is.False);
        Assert.That(car.CanAttack, Is.False);
    }

    [Test]
    public void FrozenStopsForcedAttackWithoutBeingConsumed()
    {
        Minion car = Minion(GameManager.P.P1, 0, 53);
        Minion target = Minion(GameManager.P.P2, 0, 54);
        car.Attack = 12;
        car.Frozen = true;

        car.ForceAttack(target);

        Assert.That(target.Health, Is.EqualTo(100));
        Assert.That(car.Health, Is.EqualTo(100));
        Assert.That(car.Frozen, Is.True);
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

    [TestCase(6, 3, 5, 3)]
    [TestCase(6, 6, 5, 5)]
    [TestCase(6, 7, 5, 6)]
    [TestCase(1, 4, 0, 3)]
    [TestCase(0, 3, 0, 3)]
    public void ManaStormRemovesEmptySlotsFirstAndPreservesBonusMana(int maximum, int current, int expectedMax, int expectedCurrent)
    {
        ManaCounter mana = Mana(GameManager.P.P2, maximum, current);
        Card spell = Card(216);
        spell.backupOwner = GameManager.P.P1;
        var script = spell.gameObject.AddComponent<Velká_mýdlová_bouře>();
        Invoke(script, "OnSelfPlayed", spell, new CardScriptBase.TargetlessEventArgs(CardGame.Card.CardType.Spell));
        Assert.That(mana.MaxMana, Is.EqualTo(expectedMax));
        Assert.That(mana.Mana, Is.EqualTo(expectedCurrent));
    }

    [TestCase(2, 1)]
    [TestCase(9, 11)]
    public void GeneticModificationAddsAnEmptySlotWithoutChangingCurrentMana(int maximum, int current)
    {
        ManaCounter mana = Mana(GameManager.P.P1, maximum, current);
        Card spell = Card(118);
        spell.backupOwner = GameManager.P.P1;
        Invoke(spell.gameObject.AddComponent<Genetická_modifikace>(), "OnSelfPlayed", spell,
            new CardScriptBase.TargetlessEventArgs(CardGame.Card.CardType.Spell));
        Assert.That(mana.MaxMana, Is.EqualTo(maximum + 1));
        Assert.That(mana.Mana, Is.EqualTo(current));
    }

    [TestCase(0)]
    [TestCase(7)]
    public void DirectProportionSpendsAllCurrentManaAndAddsSpellDamageEvenAtZero(int current)
    {
        ManaCounter mana = Mana(GameManager.P.P1, 5, current);
        var professor = Minion(GameManager.P.P1, 0, 80).gameObject.AddComponent<Profesor_matematiky>();
        professor.Awake();
        Minion target = Minion(GameManager.P.P2, 0, 48);
        Card spell = Card(65);
        spell.backupOwner = GameManager.P.P1;
        Invoke(spell.gameObject.AddComponent<Přímá_úměra>(), "OnSelfPlayed", spell,
            new CardGame.Card.CardPlayedEventArgs(CardGame.Card.CardType.Spell, target));
        Assert.That(mana.Mana, Is.Zero);
        Assert.That(mana.MaxMana, Is.EqualTo(5));
        Assert.That(target.Health, Is.EqualTo(100 - current - 2));
    }

    [TestCase(false, false, 5)]
    [TestCase(false, true, 5)]
    [TestCase(true, false, 7)]
    public void GrantRequiresAnOwnExperimentAndDoesNotCapRestoredMana(bool ownExperiment, bool enemyExperiment, int expected)
    {
        var effects = Slots(GameManager.maxEffSlots);
        Set(manager, "EffSlots", effects);
        foreach (var owner in new[] { GameManager.P.P1, GameManager.P.P2 })
        {
            Effect effect = NewObject("Effect").AddComponent<Effect>();
            effect.transform.SetParent(effects[owner][0].transform);
            effect.isExperiment = owner == GameManager.P.P1 ? ownExperiment : enemyExperiment;
        }
        ManaCounter mana = Mana(GameManager.P.P1, 5, 5);
        Card card = Card(120);
        card.backupOwner = GameManager.P.P1;
        Invoke(card.gameObject.AddComponent<Grantová_komise>(), "OnSelfPlayed", card,
            new CardScriptBase.TargetlessEventArgs(CardGame.Card.CardType.Minion));
        Assert.That(mana.Mana, Is.EqualTo(expected));
        Assert.That(mana.MaxMana, Is.EqualTo(5));
    }

    [Test]
    public void LeničkaGrantsChargeOnlyToNewFriendlyAnimalsAndDoesNotRevokeIt()
    {
        Minion oldAnimal = Minion(GameManager.P.P1, 0, 48);
        oldAnimal.cardTags.Add(CardTag.Zvíře);
        Minion source = Minion(GameManager.P.P1, 1, 124);
        var script = source.gameObject.AddComponent<Lenička__Matka_přírody>();
        script.Awake();
        manager.InvokeSummoned(source);
        Assert.That(oldAnimal.CanAttack, Is.False);
        Minion animal = Minion(GameManager.P.P1, 2, 155);
        animal.cardTags.UnionWith(new[] { CardTag.Zvíře, CardTag.Stroj });
        Minion enemy = Minion(GameManager.P.P2, 0, 48);
        enemy.cardTags.Add(CardTag.Zvíře);
        Minion nonAnimal = Minion(GameManager.P.P1, 3, 48);
        manager.InvokeSummoned(animal);
        manager.InvokeSummoned(enemy);
        manager.InvokeSummoned(nonAnimal);
        Assert.That(animal.CanAttack, Is.True);
        Assert.That(enemy.CanAttack, Is.False);
        Assert.That(nonAnimal.CanAttack, Is.False);
        UnityEngine.Object.DestroyImmediate(source.gameObject);
        Assert.That(animal.CanAttack, Is.True);
        Minion later = Minion(GameManager.P.P1, 4, 48);
        later.cardTags.Add(CardTag.Zvíře);
        manager.InvokeSummoned(later);
        Assert.That(later.CanAttack, Is.False);
    }

    [TestCase(0)]
    [TestCase(2)]
    [TestCase(6)]
    public void ExplosivesHitOnlyImmediateNeighborsWithoutCrossingGapsOrEdges(int targetSlot)
    {
        Minion target = Minion(GameManager.P.P2, targetSlot, 48);
        int adjacentSlot = targetSlot == 6 ? 5 : targetSlot + 1;
        Minion neighbor = Minion(GameManager.P.P2, adjacentSlot, 48);
        int distantSlot = targetSlot == 0 ? 6 : 0;
        Minion distant = Minion(GameManager.P.P2, distantSlot, 48);
        Minion newNeighbor = null;
        if (targetSlot == 2)
            target.OnDamaged += (_, _) => newNeighbor = Minion(GameManager.P.P2, 1, 48);
        Card spell = Card(156);
        spell.backupOwner = GameManager.P.P1;
        Invoke(spell.gameObject.AddComponent<Doma_namíchaná_trhavina>(), "OnSelfPlayed", spell,
            new Card.CardPlayedEventArgs(CardGame.Card.CardType.Spell, target));
        Assert.That(target.Health, Is.EqualTo(95));
        Assert.That(neighbor.Health, Is.EqualTo(98));
        Assert.That(distant.Health, Is.EqualTo(100));
        if (newNeighbor != null) Assert.That(newNeighbor.Health, Is.EqualTo(100));
    }

    [Test]
    public void BugCountsPreviousFriendlySummonsEvenAfterTheyLeaveTheBoard()
    {
        Minion first = Minion(GameManager.P.P1, 0, 314);
        Invoke(first.gameObject.AddComponent<Bug>(), "OnSelfSummoned", first, new Minion.TargetedEventEventArgs());
        manager.InvokeSummoned(first);
        Assert.That(first.Attack, Is.EqualTo(1));
        first.transform.SetParent(root.transform);
        Minion enemy = Minion(GameManager.P.P2, 0, 314);
        Invoke(enemy.gameObject.AddComponent<Bug>(), "OnSelfSummoned", enemy, new Minion.TargetedEventEventArgs());
        manager.InvokeSummoned(enemy);
        Assert.That(enemy.Attack, Is.EqualTo(1));
        Minion second = Minion(GameManager.P.P1, 1, 314);
        Invoke(second.gameObject.AddComponent<Bug>(), "OnSelfSummoned", second, new Minion.TargetedEventEventArgs());
        int countDuringReaction = 0;
        manager.OnSummoned += (_, _) => countDuringReaction = manager.Stats.GetSummonCount(GameManager.P.P1, 314);
        manager.InvokeSummoned(second);
        Assert.That(second.Attack, Is.EqualTo(2));
        Assert.That(second.Health, Is.EqualTo(101));
        Assert.That(second.MaxHealth, Is.EqualTo(101));
        Assert.That(countDuringReaction, Is.EqualTo(2));
    }

    [Test]
    public void ShieldPreventsAndConsumesExactlyOnePositiveDamageInstance()
    {
        Minion shielded = Minion(GameManager.P.P1, 0, 158);
        var shieldScript = shielded.gameObject.AddComponent<Čarobot_2000>();
        Invoke(shieldScript, "OnSelfSummoned", shielded, new Minion.TargetedEventEventArgs());
        shielded.Shielded = true; // Re-granting an active Shield must not stack.
        int damageEvents = 0;
        shielded.OnDamaged += (_, _) => damageEvents++;

        shielded.Damage(7);
        Assert.That(shielded.Health, Is.EqualTo(100));
        Assert.That(damageEvents, Is.Zero);

        shielded.Damage(7);
        Assert.That(shielded.Health, Is.EqualTo(93));
        Assert.That(damageEvents, Is.EqualTo(1));
    }

    [Test]
    public void DynamicAttackModifierRemainsSeparateFromPermanentBuffs()
    {
        Minion minion = Minion(GameManager.P.P1, 0, 43);
        var script = minion.gameObject.AddComponent<Okamie>();
        script.DynamicAttackModifier = 5;
        Assert.That(minion.Attack, Is.EqualTo(6));

        minion.Buff(2, 0);
        Assert.That(minion.Attack, Is.EqualTo(8));

        script.DynamicAttackModifier = 0;
        Assert.That(minion.Attack, Is.EqualTo(3));
    }

    [Test]
    public void DeathReleasesSlotAndMovesCardToGraveBeforeDeathEffectsRun()
    {
        CardSlot slot = minions[GameManager.P.P1][0];
        Set(slot, "minionDestroyPlayer", slot.gameObject.AddComponent<AudioSource>());
        manager.graves = new Dictionary<GameManager.P, Grave>
        {
            [GameManager.P.P1] = NewObject("Grave").AddComponent<Grave>(),
            [GameManager.P.P2] = NewObject("Enemy Grave").AddComponent<Grave>()
        };
        Card original = Card(30);
        slot.PlaceCard(original);
        original.gameObject.SetActive(false);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Slot&Actor/Minion.prefab");
        Minion dying = UnityEngine.Object.Instantiate(prefab, slot.transform).GetComponent<Minion>();
        Set(dying, "original", original);
        dying.MaxHealth = 1;
        dying.Health = 1;
        bool slotWasFree = false;
        bool cardWasInGrave = false;
        dying.OnDeath += (_, _) =>
        {
            slotWasFree = !slot.Occupied;
            cardWasInGrave = manager.graves[GameManager.P.P1].Contains(original);
        };

        dying.Death();

        Assert.That(slotWasFree, Is.True);
        Assert.That(cardWasInGrave, Is.True);
    }

    [Test]
    public void SpellproofBlocksOnlySpellTargeting()
    {
        Minion frog = Minion(GameManager.P.P2, 0, 160);
        frog.gameObject.AddComponent<Robohypnožába>();
        Card spell = Card(132);
        spell.cardType = CardGame.Card.CardType.Spell;
        Card battlecry = Card(125);
        battlecry.cardType = CardGame.Card.CardType.Minion;

        Assert.That(spell.IsTargetValid(frog), Is.False);
        Assert.That(battlecry.IsTargetValid(frog), Is.True);
    }

    [Test]
    public void StampedeCountsOnlyEarlierCardsFromItsOwnersCurrentTurn()
    {
        manager.Stats.RecordCardPlayed(GameManager.P.P1);
        manager.Stats.RecordCardPlayed(GameManager.P.P1);
        manager.Stats.RecordCardPlayed(GameManager.P.P1); // Splašené stádo itself
        manager.Stats.RecordCardPlayed(GameManager.P.P2);
        Minion stampede = Minion(GameManager.P.P1, 0, 131);

        Invoke(stampede.gameObject.AddComponent<Splašené_stádo>(), "OnSelfSummoned",
            stampede, new Minion.TargetedEventEventArgs());

        Assert.That(stampede.Attack, Is.EqualTo(5));
        Assert.That(stampede.Health, Is.EqualTo(104));
        Assert.That(stampede.MaxHealth, Is.EqualTo(104));
        manager.Stats.ResetCardsPlayedThisTurn(GameManager.P.P1);
        Assert.That(manager.Stats.GetCardsPlayedThisTurn(GameManager.P.P1), Is.Zero);
        Assert.That(manager.Stats.GetCardsPlayedThisTurn(GameManager.P.P2), Is.EqualTo(1));
    }

    [TestCase(1, 98)]
    [TestCase(2, 96)]
    public void EscapingExperimentsUsesMomentumDamage(int cardsPlayedThisTurn, int expectedHealth)
    {
        for (int i = 0; i < cardsPlayedThisTurn; i++) manager.Stats.RecordCardPlayed(GameManager.P.P1);
        Card spell = Card(132);
        spell.backupOwner = GameManager.P.P1;
        Minion target = Minion(GameManager.P.P2, 0, 48);

        Invoke(spell.gameObject.AddComponent<Útěk_experimentů>(), "OnSelfPlayed", spell,
            new Card.CardPlayedEventArgs(CardGame.Card.CardType.Spell, target));

        Assert.That(target.Health, Is.EqualTo(expectedHealth));
    }

    [Test]
    public void LaboratoryTeacherDiscountIsConsumedEvenWhenExperimentIsCountered()
    {
        var effectSlots = Slots(GameManager.maxEffSlots);
        Set(manager, "EffSlots", effectSlots);
        Effect discount = NewObject("Teacher discount").AddComponent<Effect>();
        discount.transform.SetParent(effectSlots[GameManager.P.P1][0].transform);
        var script = discount.gameObject.AddComponent<Učitel_v_laboratoři>();
        script.Awake();
        Card experiment = Card(139);
        experiment.backupOwner = GameManager.P.P1;
        experiment.mana = 7;
        Set(experiment, "<IsExperiment>k__BackingField", true);
        manager.AddCardToHand(GameManager.P.P1, experiment);

        Assert.That(manager.GetManaCost(experiment), Is.Zero);
        Invoke(script, "OnBeforeSpellPlayed", experiment,
            new GameManager.CancelableCardEventArgs(experiment) { Cancel = true });
        Assert.That(manager.GetManaCost(experiment), Is.EqualTo(7));
    }

    ManaCounter Mana(GameManager.P owner, int maximum, int current)
    {
        var counter = NewObject("Mana").AddComponent<ManaCounter>();
        var images = (Image[])typeof(ManaCounter).GetField("ManaCrystalImage", PrivateInstance).GetValue(counter);
        for (int i = 0; i < images.Length; i++) images[i] = NewObject("Crystal").AddComponent<Image>();
        counter.MaxMana = maximum;
        counter.Mana = current;
        manager.ManaCounters ??= new Dictionary<GameManager.P, ManaCounter>();
        manager.ManaCounters[owner] = counter;
        return counter;
    }

    void CreateDeckAndGraveZones()
    {
        manager.decks = new Dictionary<GameManager.P, Deck>();
        manager.graves = new Dictionary<GameManager.P, Grave>();
        foreach (GameManager.P owner in new[] { GameManager.P.P1, GameManager.P.P2 })
        {
            manager.decks[owner] = NewObject("Deck").AddComponent<Deck>();
            manager.graves[owner] = NewObject("Grave").AddComponent<Grave>();
        }
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
