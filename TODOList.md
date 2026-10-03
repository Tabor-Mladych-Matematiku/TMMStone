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
- [ ] Add the opening mulligan: after the initial 3/4-card draw and second-player Bod are assigned, each player may once select any number of starting cards, put the complete selection on the bottom of their deck simultaneously, and draw the same number of replacements. Do not reveal replacements between individual selections.
- [ ] Randomly select the first player. When `DEBUG` is true, always make the server the first player instead.
- [ ] Finish implementing the remaining cards.

### Shared mechanics to implement before the remaining cards

- [ ] Implement the general Ability (`Schopnost`) system: place the ability in the effect area at game start, ready it once per controller turn like a minion, pay its printed mana cost and exhaust it when activated, exclude it from the normal deck-card limit, and allow at most one ability per deck.
- [ ] Complete the general Experiment flow. Playing an Experiment is a spell play and emits OnSpellPlayed, but its immediate result is a face-down Effect owned by the caster. That Effect waits for its condition and triggers automatically in response to the specified opponent action during the opponent's turn; it cannot be deferred after its first valid trigger.
- [ ] Enforce the canonical start-of-turn order: resolve start-of-turn triggers; ready the active player's minions (Frozen is removed instead of readying that minion); gain one maximum mana up to 10; refill current mana to maximum; then draw one card. Stop immediately if an earlier step ends the game.
- [ ] Enforce canonical triggered-effect ordering. Resolve the field first; then the active player's minions left-to-right; active player's Effects; opponent's minions left-to-right from that opponent's perspective; and opponent's Effects. Pause the current event batch for deaths. Resolve a dying minion's OnDeath before “when another minion dies” reactions, then resume only with sources still in play.
- [ ] Correct minion play timing. Reserve/place the minion in its selected board slot first, then resolve its own OnPlay/BattleCry before emitting other cards' OnPlay/OnSummon reactions. This lets Síma include its own occupied slot in a rotation while ensuring its own OnPlay has completed even if later reactions kill it. A summon that was not played from hand after paying its cost does not run OnPlay.
- [ ] Treat a countered card as not played: do not emit spell-play or other played-card events. Apply this to Aplikace prediktivního modelu and any future counter effects.
- [ ] Standardize `Zlikvidovat` for cards that cannot enter a full hand: move the card to the graveyard without OnDiscard or other discard reactions.
- [ ] Implement `Zlikviduj` as an explicit zone operation for destroying a card from a deck or a card that cannot enter a full hand. Move it to the graveyard without OnDeath, OnDiscard or other discard reactions, and use this term consistently in UI/rules text.
- [ ] Implement `Neaktivní`: the minion cannot attack or be affected by any other game effect, is otherwise ignored, and only occupies its board slot until a card-specific activation condition makes it active. Tomáš, skutečný předseda spolku currently uses this keyword; replace or define its physical rock-climbing activation separately.
- [ ] Treat `Spal` as ordinary discard from hand and use the existing Discard infrastructure. A card may still resolve a burn instruction when the hand contains fewer requested cards. For a multi-card burn, remove the complete chosen set before emitting any discard reactions, so one burned card cannot affect another card burned by the same instruction.
- [ ] Route all gameplay randomness through one network-synchronized, owner-aware service: targets, cards, positions, shuffles, binary outcomes and integer ranges. Keep presentation randomness separate. Elis is the only approved outcome replacement and lets her controller choose legal outcomes for that player's requests, including random targets. Multiple friendly Elis instances provide one replacement; remove each source on silence or leaving play without disabling another surviving source.
- [ ] Normalize random candidate lists and area-effect/death ordering to stable network player and slot identities. `AllCharacters` currently enumerates local P1 minions, local P2 minions, then the local and opposing heroes; this reverses player identity between peers. Verify Teorie chaosu assigns each roll to the same character on both peers and that deck presentation in `Deck.Add` does not consume gameplay randomness.
- [ ] Build a reusable choice flow for Tomík, Síma, optional resurrection and other multi-step effects. Stage every upgrade, target and board permutation without gameplay changes until all choices are complete and validated. Reserve mana for multi-step choices, allow cancellation before commitment, and charge after resolution. Tomík requires distinct targets, visible selection markers and a cap on target upgrades equal to the eligible target count; every damage upgrade affects all selected targets. Single-target Přímá úměra needs no extra reservation/amount-selection UI. Petr's activated challenge spends mana when issued. Petr's card has an effect that is different from Tomík's and Síma's - its not OnPlay but on click. When Petr's ability is avaiilable - (he is on board and you have enough mana) he should aside from green border should have a red arrow pointing at it from the bottom - to signify he can be tapped. You can reuse that red arrow for the targetting done by Tomík and Elis.
- [x] Add `GameManager.TakeControl(minion, newController, allowImmediateAttack = false)`: move the existing minion and attached card to the rightmost empty destination slot, preserving stats, statuses and attached effects without replay/summon events. Disable attacks by default; explicit immediate attack still respects Frozen. A full destination removes the original without death or discard effects and detaches its event listeners immediately. Same-controller calls are no-ops.
- [ ] Implement bounce by removing the board minion without death effects and moving its attached card to the specified player's hand. Minion buffs, damage, statuses and attached effects do not survive. If the hand is full, put the returned card in the graveyard without `OnDiscard`. Keep this separate from drawing/discarding and from destroying an excess newly generated copy.
- [ ] Implement resurrection as a fresh copy with printed stats and no previous buffs, damage, statuses or attached effects; the dead card remains in the graveyard. Use an available friendly slot. A full board creates neither a copy nor a new graveyard entry. Effect summons do not execute on-play effects. Fénix prefers its old slot, then a random empty friendly slot. Klonovací přístroj is consumed on its first eligible death.
- [ ] Replace recursive effect dispatch with a deterministic queue. Snapshot area-effect targets before resolution so generated minions/cards are excluded. Bound self-repeating effect chains to 13 total executions (verify Fénix + Kilometrové kolečko and Rekurze for an off-by-one error) and add a queue-size safety limit; choose that safety limit separately rather than treating 13 as the total allowed number of unrelated effects.
- [ ] Evaluate lethal outcomes after one direct effect finishes and before processing queued follow-up effects. If that effect directly kills both heroes, including Aktivní student's draw/fatigue batch, declare a draw. If it kills only one, declare the other the winner and skip later triggers, including death cascades and healing that would revive a hero from lethal.
- [ ] Resolve damage-received triggers before the damaged minion's death, including lethal hits, once per damage instance; prevented damage does not trigger them. Apply consistently to Otrok Matfyzu, Princezna Uhelnice and Krysí král. Process simultaneous deaths deterministically so Sabča/Jitka can find earlier deaths in their own controller's graveyard; the opponent's graveyard does not qualify. Rekurze repeats only for deaths directly caused by its damage wave.
- [ ] Add a damage-dealt event carrying source, recipient, combat/effect context and damage actually dealt. Poisonous kills another minion whenever its source minion deals positive damage by any route, including combat, OnPlay and triggered effects; zero attack/damage and Shield-prevented damage do not poison. Duch matematikův freezes after positive combat damage while attacking or defending, even if Frozen; effect damage does not freeze and silence disables the ability. Týnka prevents only combat retaliation while attacking, not other triggered damage.
- [ ] Reuse Honzovo auto's shared forced-combat behavior for Cyberdyne Systems T-800 and Sáňky samochodky: bypass Taunt, Frozen and normal attack allowance while preserving combat/attack events. Explicit prohibitions such as Pepča's opposite-slot aura block forced attacks too. Add character-target support for Sáňky and consistent opposing-slot mapping across peers.
- [ ] Implement Shield for the next damage instance from any source, consuming it on prevention. Only one Shield may be active at a time; granting another one, such as with Žlutá helma, does nothing. Lifesteal uses full damage dealt including overkill, capped only by the healing recipient's maximum health. Shield-prevented damage causes neither Lifesteal healing nor Poisonous death.
- [ ] Finish the shared keyword rules still needed by unreviewed cards: Stealth cannot be targeted by attacks or targeted effects and ends when the minion attacks; silence resets attack and maximum health to the printed values, clamps current health to the printed maximum without otherwise healing it, removes all other effects and statuses including Taunt, Frozen and Stealth, resets tags to their printed values, and sets maximum attacks per turn to 1 (including printed-zero attackers such as Petr, which may attack starting next turn). Cost modifiers need no board-minion reset. Also finish Momentum activation, Charge aura removal, and spell-damage ordering/modifier interactions. Spell damage applies to played damage-dealing spells and Experiments, but not to OnDraw effects; record explicit card exceptions separately. Keep genuinely undecided card-specific behavior marked for review.
- [ ] Use explicit ending-player context for end-turn triggers and expiry, independent of the newly selected active player. Cover Petr, Jana Z., Jiříkova přednáška, Forexový obchodník, Pavel, Finanční podvod and Dole v dole. Track spell plays for the entire turn independently of Matemagická aréna's presence.
- [ ] Add effect-based cost overrides for the next Org card, the next experiment and the opponent's next-turn spells. Apply a zero-cost override last: it wins over all surcharges, discounts and fixed-cost effects. Preserve immediate activation of Pavel's +5 spell-cost effect and remove it at that opponent's turn end; remove Finanční podvod at the current turn's end.
- [ ] Support a shared deck and shared fatigue for EVIL Jiřík, Pán chaosu: point both players' deck entries at the same persistent pile, make deck operations transparently affect it, and make replaying the merge a no-op. Drawn cards belong to the drawing player. Define fatigue initialization and ambiguous top-card exchanges when both deck entries already alias one pile.
- [ ] Add regressions for paired-peer random ordering, cancelled mana reservations, control into a full board, bounce overflow without discard, lethal damage-received triggers, simultaneous graveyard checks, Shield/Lifesteal, and direct-effect draws versus later lethal death cascades.

- [ ] Implement the reviewed copy rules: Michalův matematický model appends a spell copy to the deck, Zpirátěný software shuffles a copy back, and Filip creates copies in the opponent's hand. Originals reach the graveyard normally. Excess generated hand copies are destroyed without discard events.
- [ ] Support separate OnDraw and CastOnDraw flows. OnDraw resolves an effect when the card is drawn and then the card enters the hand normally; no current card is known to use it, but keep the semantic distinction. CastOnDraw plays the card for free instead of putting it into hand and emits no spell-play event. Trojský kůň is CastOnDraw: apply spell damage and defer reinsertion until its replacement-draw chain finishes. Zaokrouhlovací chyba and Úchyt are also CastOnDraw and avoid spell-play listeners.
- [ ] Implement Bug summons using `cards.json` ID 314, with 1/1 printed stats and the summon-scaling effect imported from `Tokeny/Bug.jpg`; its current summon does not count toward its own scaling. Generate Implementace v Pythonu using token ID 102.
- [ ] Resolve snapshotted attack-interception effects even if an earlier interception killed the attacker. Each triggered Schrödingerova kočka is consumed even if the attacker is already dead when it resolves. Once legally declared, an Animal's attack is not cancelled by Lenička dying during a before-attack event. Track each Leguán's specific David summoner rather than matching a card name.
- [ ] Implement Sv. Jana's bounce for other Org minions only; she remains on the board. Move the affected minions' attached cards rather than creating copies, then apply her next-Org zero-cost Effect.
- [ ] Reset current/max health, attack, enchantments and statuses for Zanedbání odporu vzduchu. Allow Grantová komise to exceed maximum current mana. Prohození proměnných ignores spell-damage bonuses. Lock out external deck mutations during Vyhledávací automat's staged selection.

### Deferred designs

- [ ] Leave David, Správce systému unimplemented for now; its deck-size/hand-limit setup rules and duplicate behavior remain deferred.
- [ ] Leave Chybný rozpis směn unimplemented until explicitly resumed; queued consecutive turns and interactions between multiple copies remain deferred.
- [ ] Design a digital challenge, potentially trivia, for Petr, Tělocvikář and related physical-challenge cards before implementing their challenge flow. Redesign Aplikovaný Darwinismus rather than introducing a second gameplay-randomness replacement alongside Elis.
- [ ] Implement Tomík after the staged upgrade/target-selection flow exists. Implement Síma using a simultaneous rotation of previously occupied positions, skipping gaps; for sparse receiving sides use the free slot nearest the chosen direction. Preserve the one-minion and two-minion examples and encode direction/slot order consistently on both peers.
- [ ] Finish the remaining identity rules for transforms, captured minions and zone changes. Control preserves existing stats; resurrection resets them. Preserve explicit card exceptions, including the mug returning captives to the opponent of its current controller and releasing nothing on silence or bounce.

## Decks and card data

- [ ] Export card JSON from the website in a format the game can consume.
- [ ] Add class selection before deck construction: Matematik, Informatik, Fyzik or Inženýr. Show neutral cards plus cards belonging to the selected class and filter out the other three classes. When `DEBUG` is true, also offer an unrestricted Admin class.
- [ ] Enforce deck-construction rules in the deck builder, not gameplay code: exactly 30 cards; at most two copies of a normal card; at most one Org card; at most one Ability (`Schopnost`); and only neutral cards or cards of the selected class. Disable validation restrictions when `DEBUG` is true so debug decks remain unrestricted.
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
