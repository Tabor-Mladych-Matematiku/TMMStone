# TODO

## Bugs

- [x] Harden Relay joining with bounded retries, startup checks, and failure propagation back to the lobby.
- [x] Make turn changes resolve consistently on both peers instead of `ServerOnTurn.OnValueChanged` running at observably different times.
- [x] Stop turn resolution after an end-of-turn or start-of-turn effect finishes the match.
- [x] Stop a dead Algedrak from reacting to the owner's spell that killed it.
- [x] Prevent `highlightedSlot` and `highlightedActor` from retaining or clearing stale targets incorrectly.

## Gameplay and cards

- [x] Add BattleCry target selection and cancellation.
- [x] Define and enforce specific target constraints for every targeted spell, such as friendly minions, enemy minions, or any character.
- [x] Implement Taunt.
- [ ] Finish implementing the remaining cards.

### Mechanics to define before implementing the remaining cards

- [ ] Add one network-synchronized random service for random targets, random cards, random positions, binary outcomes, and random integer ranges. It must support replacement effects such as Elis's player choice. Elis is a special case and is the only card capable of such replacement. She affects only her player's randomness and so they should not conflict with each other.
- [ ] Define the shared choice/selection flow used by Tomík, Síma, resurrection effects, and other multi-step cards: preview, cancel, payment timing, target validation, and when the action becomes committed. Question: can a player cancel after paying or after any irreversible resolution has started?
Answer: Truly paying should not happen until the effect is truly resolved - only after the effect is resolved its payed so cancelation only cancels the reservation.
- [ ] Define control transfer: move a controlled minion to the rightmost empty slot; if there is no space, destroy it without death effects. Question: should temporary control effects and attack availability reset on transfer?
Answer: By default attack availability should be set to false unless specified otherwise (TakeControl function should have a default parameter "reset attack" that is false by default)
- [ ] Define bounce and hand overflow: return to the relevant owner's hand without death effects; if the hand is full, specify whether the card is destroyed, exiled, or discarded. Question: should a bounced minion lose buffs, damage, statuses, and attached effects? Answer: card that is bounced is destroyed - ends up on grave, does not cause OnDiscard. Minion buffs dont remain. Just put the card attached to it to the hand of the player
- [ ] Define effect resolution safety: snapshot area-of-effect targets, stop processing after lethal game state, resolve simultaneous deaths deterministically, and enforce the 13-iteration recursion/death-effect limit. Question: should the 13 limit be global per top-level effect or shared across nested effects? Answer: one effect should be able to cause itself only 13 times - thats more of a rule how to write scripts rather than a mechanical check. But there should be some kind of resolution stopping check. Currently everything is just recursive. So we'd have to track lenght or depth of recursion. preferably though we oughta turn all effects into a queue so they resolve somewhat reliably and we can add a limit on the queue size. Probably do the queue.
- [ ] Define turn sequencing and queued turns, including the identity of the ending player for end-turn effects, next-turn modifiers, and effects that grant multiple consecutive turns. Question: when a card changes turn order, does the current turn always finish first? Dont implement that card yet (I think this is related to that one card that lets your opponent play twice and they lets you play twice)
- [ ] Define card identity and stat reset for transform, resurrect, summon, captured minions, and cards moved between zones. Question: which effects preserve buffs or temporary statuses, if any? Answer: ressurect works just like summon - stats as printed on the card. stealing minions steals them as is - same stats. 
- [ ] Define damage, healing, Lifesteal, Shield, Poisonous, Stealth, Charge, Taunt, Frozen, Momentum, and spell-damage ordering as shared keyword mechanics. Questions: does Shield prevent any damage instance or only combat damage; does Poisonous apply to effect damage; and does Lifesteal use prevented/overkill-adjusted damage? Answer: Shield prevents any single instance of damage. Your Poisonous question is phrased weird but it should just ensure that the minion dies. Lifesteal uses overkill damage too, but shield stops it completely. In essence if you hit something and it has now negative HP it had taken full damage although it would have died sooner. Shield stops you from dealing that damage.  
- [ ] Define full-hand, empty-deck, fatigue, and simultaneous lethal outcomes. Question: should a single effect that kills both heroes always produce a draw, while later queued effects are skipped? Answer: Yes. If "deal 4 damage to all characters" kills both players its a draw. If it kills one player but another one would be killed by some cascading OnDeath effects it is a win for the player who had not died yet

## Decks and card data

- [ ] Export card JSON from the website in a format the game can consume.
- [ ] Support selecting imported deck JSON in the lobby.
  - [x] Store deck JSON files in a dedicated folder.
  - [x] Discover and list the available decks.
  - [x] Allow each player to select a local deck before starting.
  - [ ] Display the selected deck's class in the lobby.

## Networking and lobby

- [x] Add lobby UI controls for the existing join-by-code service method.
- [x] Add a lobby UI control for the existing Quick Join service method.
- [x] Decide and enforce lobby-name validation rules, including whether foreign characters are allowed.

## UI and graphics

- [ ] Hide network operations behind appropriate loading screens and transitions.
- [ ] Prevent health values from showing through loading screens.
- [ ] Make dragged cards follow the pointer smoothly instead of snapping directly to it.
- [ ] Add card movement animations.
- [ ] Replace direct minion dragging during attacks with a targeting arrow or similar interaction.
- [ ] Clearly indicate playable cards and minions that can attack.
- [ ] Show the opponent's actions instead of applying them without visible playback.
- [ ] Keep card stat text constrained horizontally.
- [ ] Improve the main menu presentation.
- [ ] Create a dedicated end-game presentation instead of reusing the main menu layout.
- [ ] Verify and improve adaptive layouts across supported screen sizes.

## Editor tooling

- [x] Add a button that reveals a card's editor script in the filesystem.
- [x] Allow `Blank.cs` or any existing Resources script to be attached to a card without creating a new one.
- [x] Support generating normal and targeted card scripts from one shared template.
- [ ] Finish the EditorExt card maker and card displayer.

## Testing

- [ ] Run a two-player Relay integration test covering host creation, client joining, and failed joins.
- [ ] Verify on two peers that end-of-turn and start-of-turn lethal effects show the correct result before shutdown.
- [ ] Test the current build on Android.

## Notes

### Random synchronization

The shared seed must only be consumed by operations that run identically on both game instances. Random calls made by UI, presentation, or other local-only behavior will cause the game states to diverge.

A dedicated network-aware random service would make this rule explicit and provide one controlled place for synchronized random operations.
