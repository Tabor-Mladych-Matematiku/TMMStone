# TMMStone

TMMStone is a two-player digital card game built in Unity. This document describes the intended digital rules, including mechanics still tracked for implementation in [TODOList.md](TODOList.md). Physical camp-game procedures and components are not part of the digital rules.

## Running the game

Opening the Unity project may require retrying package imports.

To test multiplayer, run one instance with **Build & Run** and another in Play mode. The instances must authenticate with distinct names; Play mode adds a random suffix to the default anonymous name. A lobby may be created without a name, must be public, and can start only after both players join.

The repository includes a known-valid deck based on the physical game and an experimental deck containing cards that may be incomplete. Available local deck JSON files can be selected in the lobby. If loading stalls, restart the affected instance.

## Game objective

Each hero starts with 30 health. Reduce the opposing hero to 0 health while keeping your own hero above 0.

The game ends immediately after a direct effect finishes if either hero is at 0 health. If that direct effect kills both heroes, the game is a draw. Otherwise the surviving player wins and later queued reactions, including death effects, do not resolve.

## Board and zones

Each player has:

- seven minion slots;
- six Effect slots;
- a deck and graveyard;
- a hand limited to ten cards;
- a hero and mana counter.

Both players share one Field slot. Playing a new Field moves the previous Field to its owner's graveyard.

Cards have a mana cost, name, type, rules text, optional tags, class and rarity. Minions also have attack and health. Flavor text has no gameplay effect.

## Starting a game

The first player is selected randomly. In debug mode, the server always starts.

The first player draws three cards. The second player draws four cards and receives a generated **Bod** in hand.

Each player then gets one simultaneous mulligan. The player selects any number of starting cards, puts the complete selection on the bottom of their deck, and draws the same number of replacements. A player cannot inspect a replacement before deciding whether to replace another starting card.

Both players start with zero maximum and current mana.

## Turn sequence

At the start of a turn:

1. Resolve start-of-turn triggers.
2. Ready the active player's minions. A Frozen minion loses Frozen instead of readying.
3. Increase that player's maximum mana by one, up to ten.
4. Refill current mana to maximum mana.
5. Draw one card.

Stop immediately if an earlier step ends the game. During the turn, the active player may play any number of affordable cards and attack with ready minions. End-of-turn triggers resolve after the player ends the turn and before the opponent's turn begins.

## Drawing, hand overflow and fatigue

An ordinary drawn card enters the player's hand. If it cannot enter a full hand, it is **Zlikvidována**: move it to the graveyard without triggering OnDiscard or other discard reactions.

Trying to draw from an empty deck deals fatigue damage instead. Fatigue starts at 1 and increases by 1 after every failed draw for that player.

**OnDraw** and **CastOnDraw** are distinct:

- OnDraw resolves an effect when the card is drawn, after which the card enters the hand normally.
- CastOnDraw plays the card for free instead of putting it into the hand and emits no spell-play event. Trojský kůň, Zaokrouhlovací chyba and Úchyt use CastOnDraw.

## Playing cards

### Minions

A minion requires an empty friendly slot. Its selected slot is reserved and the minion is placed there before its own OnPlay/BattleCry resolves. Other cards' OnPlay and OnSummon reactions occur afterward. This allows effects such as Síma's rotation to include the newly occupied slot.

The minion's OnPlay has already resolved even if later reactions immediately kill it. A minion summoned by an effect without being played from hand and paid for does not execute OnPlay. Newly played and summoned minions normally cannot attack until their controller's next turn.

If an effect tries to summon a minion onto a full board, nothing is summoned and no summon, death or graveyard event occurs.

### Spells

A Spell resolves its instructions in written order and then moves to its owner's graveyard unless stated otherwise.

A countered card is treated as never played: it emits no spell-play or other played-card events. This applies to effects such as Aplikace prediktivního modelu.

### Experiments

An Experiment is played as a Spell and emits OnSpellPlayed. Instead of resolving an immediate public effect, it creates a face-down Effect owned by its caster.

The Experiment waits for its trigger: an opponent action during the opponent's turn. It triggers automatically on the first qualifying action and cannot be deliberately deferred.

### Fields

A Field supplies a persistent effect from the shared Field slot and is replaced when another Field is played.

### Abilities

A deck may contain at most one Ability (`Schopnost`). It begins the game in its controller's Effect area, does not count toward the ordinary deck-card limit, and readies once per controller turn like a minion. Activating it pays its printed mana cost and exhausts it.

## Combat

A ready minion normally attacks once during its controller's turn and cannot attack a friendly character. When one minion attacks another, both deal their attack to each other simultaneously. A minion at 0 health dies and its attached card moves to the graveyard.

Some effects force attacks. Unless their text says otherwise, forced attacks preserve combat and attack events while bypassing Taunt and the normal attack allowance. Frozen and other explicit attack prohibitions still block them; a blocked forced attack does not consume Frozen.

## Effect resolution order

For a shared trigger, resolve sources in this order:

1. the Field;
2. the active player's minions from left to right;
3. the active player's Effects;
4. the opponent's minions from left to right from that opponent's perspective;
5. the opponent's Effects.

If resolution kills a minion, pause the current event batch and resolve deaths. The dying minion's OnDeath resolves before reactions to another minion dying. Resume the original batch afterward, skipping sources no longer in play.

Area effects snapshot their targets before resolving, so entities created during the effect are excluded. Simultaneous deaths and random candidates use stable player and slot identities on both network peers.

Instructions on a card resolve in written order. Inserting a card into a deck shuffles it unless the instruction explicitly places the card on top or bottom.

A self-repeating effect chain resolves at most 13 times. This is not a limit on unrelated queued effects; the resolution queue has a separate safety bound.

## Targeting and randomness

A random target is selected only from targets that would be legal if chosen normally. Random targets, cards, positions, shuffles, binary outcomes and integer ranges use synchronized gameplay randomness. Presentation-only randomness does not consume gameplay randomness.

## Keywords

### Card and character terms

- **Minion (`Jednotka`)**: a card with attack and health played to a minion slot.
- **Spell (`Kouzlo`)**: a card with an immediate effect.
- **Field (`Pole`)**: a persistent effect in the shared Field slot.
- **Hero (`Hrdina`)**: the player's damageable hero.
- **Character (`Postava`)**: a minion or hero.
- **Opponent (`Protivník`)**: the opposing player or hero as required by context.
- **Summon (`Vyvolej`)**: put a minion onto the board. Playing a minion from hand also summons it.

### Trigger and zone terms

- **OnPlay (`Při zahrání`)**: resolves after the minion reserves and enters its slot but before other cards react. It requires playing the card from hand and paying its cost, including a cost of zero.
- **OnDeath (`Při smrti`)**: resolves when a minion dies.
- **OnDraw (`Při líznutí`)**: resolves when drawn; the card then enters the hand normally.
- **CastOnDraw (`ZAHRAJ PŘI LÍZNUTÍ`)**: play the drawn card for free instead of putting it into hand, without spell-play events.
- **Discard / Burn (`Spal`)**: discard from hand using the ordinary Discard flow. The instruction still resolves if fewer cards are available than requested. For a multi-card discard, remove the complete chosen set before resolving discard reactions.
- **Destroy from zone (`Zlikviduj`)**: move a card from a deck, or a card unable to enter a full hand, to the graveyard without OnDeath or OnDiscard.
- **Resurrect (`Oživ`)**: summon a fresh copy of a minion represented in the graveyard with printed stats and no prior buffs, damage, statuses or attached effects. The dead card remains in the graveyard.

### Combat and status keywords

- **Taunt / Defender (`Obránce`)**: opposing minions must attack a Taunt minion if able. If several exist, choose among them.
- **Frozen (`Zmrazení`)**: the minion loses its next normal opportunity to ready and attack; Frozen is removed then.
- **Shield (`Štít`)**: prevent the next damage instance and consume Shield. Only one Shield can be active; granting another does nothing. Prevented damage triggers neither Lifesteal nor Poisonous.
- **Lifesteal**: when this source deals damage, heal its controller's hero by the full damage dealt, including overkill, up to maximum health.
- **Poisonous (`Jedovatý`)**: when this minion deals positive damage to another minion by any route—including combat, OnPlay or a triggered effect—kill the damaged minion. Zero damage and Shield-prevented damage do not poison.
- **Stealth (`Skrytý`)**: cannot be targeted by attacks or targeted effects, but can be affected by random and non-targeted effects. Stealth ends when the minion attacks.
- **Momentum (`Hybnost`)**: the stated effect is active only if its controller has already played a card during the current turn.
- **Inactive (`Neaktivní`)**: cannot attack or be affected by another game effect, is otherwise ignored, and only occupies its slot until its card-specific activation condition occurs.

### Modification keywords

- **Spell Damage +N (`Poškození kouzel +N`)**: add N damage to damage-dealing Spells played by that controller and to their Experiments. It does not generally increase OnDraw damage. Explicit card exceptions override this rule.
- **Silence (`Umlč`)**: reset attack and maximum health to printed values; clamp current health down to the printed maximum but do not otherwise heal; remove abilities, modifiers and statuses including Taunt, Frozen and Stealth; reset tags to printed values; and set maximum attacks per turn to one. A printed-zero-attacks minion such as Petr can attack beginning next turn. Board minions have no relevant mana cost to reset.
- **Control (`Ovládni`)**: move the existing minion to the rightmost empty slot on the new controller's side while preserving stats, damage, statuses and attached effects. It cannot attack immediately unless explicitly allowed. If the destination is full, remove it without death or discard effects.
- **Stat modifiers**: `+M/+N` adds M attack and N maximum/current health. Healing cannot exceed current maximum health.

## Deck construction

Deck construction is validated by the deck builder, not during gameplay. A normal deck contains exactly 30 cards and obeys these restrictions:

- no more than two copies of a normal card;
- no more than one Org card;
- no more than one Ability;
- only neutral cards and cards belonging to the selected class.

Before building, select **Matematik**, **Informatik**, **Fyzik**, or **Inženýr**. The builder hides cards belonging to the other classes.

When `DEBUG` is true, the builder also offers an unrestricted **Admin** class and disables normal validation restrictions so arbitrary test decks can be created.

## Implementation status

These are the canonical intended rules, not a claim that every mechanic and card is complete. Outstanding gameplay, card, networking, UI, editor and test work is tracked in [TODOList.md](TODOList.md).
