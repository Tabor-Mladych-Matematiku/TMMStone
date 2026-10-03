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

### Shared mechanics to implement before the remaining cards

- [ ] Route all gameplay randomness through one network-synchronized, owner-aware service: targets, cards, positions, shuffles, binary outcomes and integer ranges. Keep presentation randomness separate. Elis is the only approved outcome replacement and lets her controller choose legal outcomes for that player's requests, including random targets. Multiple friendly Elis instances provide one replacement; remove each source on silence or leaving play without disabling another surviving source.
- [ ] Normalize random candidate lists and area-effect/death ordering to stable network player and slot identities. `AllCharacters` currently enumerates local P1 minions, local P2 minions, then the local and opposing heroes; this reverses player identity between peers. Verify Teorie chaosu assigns each roll to the same character on both peers and that deck presentation in `Deck.Add` does not consume gameplay randomness.
- [ ] Build a reusable choice flow for Tomík, Síma, optional resurrection and other multi-step effects. Support previews, repeated upgrades, direction choices, target validation and cancellation. Reserve mana during selection and charge it only after resolution; cancellation releases the reservation. Finalize the commitment boundary before implementing irreversible choices.
- [ ] Implement shared control transfer: move to the new controller's rightmost empty minion slot, preserving the minion's stats. By default it cannot attack after transfer unless the effect explicitly allows it. If the destination board is full, destroy the minion without death effects for either player. Finalize the API flag's meaning and handling of statuses/durations.
- [ ] Implement bounce by removing the board minion without death effects and moving its attached card to the specified player's hand. Minion buffs, damage, statuses and attached effects do not survive. If the hand is full, put the returned card in the graveyard without `OnDiscard`. Keep this separate from drawing/discarding and from destroying an excess newly generated copy.
- [ ] Implement fresh-stat resurrection and effect summons using printed card stats, without previous buffs, damage, statuses or attached effects. Effect summons do not execute on-play effects. Finalize grave-entry consumption and ordinary resurrection placement; Fénix specifically prefers its old slot, then a random empty friendly slot, and fails if none exists.
- [ ] Replace recursive effect dispatch with a deterministic queue. Snapshot area-effect targets before resolution so generated minions/cards are excluded. Bound self-repeating effect chains to 13 executions and add a queue-size safety limit; choose that safety limit separately rather than treating 13 as the total allowed number of unrelated effects.
- [ ] Evaluate lethal outcomes after one direct effect finishes and before processing queued follow-up effects. If that effect directly kills both heroes, including Aktivní student's draw/fatigue batch, declare a draw. If it kills only one, declare the other the winner and skip later triggers, including death cascades and healing that would revive a hero from lethal.
- [ ] Resolve damage-received triggers before the damaged minion's death, including lethal hits, once per damage instance; prevented damage does not trigger them. Apply this consistently to Otrok Matfyzu, Princezna Uhelnice and Krysí král. Put earlier simultaneous deaths in the graveyard before resolving later death effects so Sabča/Jitka can find each other there.
- [ ] Add a damage-dealt event carrying source, recipient, combat/effect context and damage actually dealt. Duch matematikův freezes after positive combat damage while attacking or defending, even if Frozen; effect damage does not freeze and silence disables the ability. Týnka prevents only combat retaliation while attacking, not other triggered damage.
- [ ] Reuse Honzovo auto's shared forced-combat behavior for Cyberdyne Systems T-800 and Sáňky samochodky: bypass Taunt, Frozen and normal attack allowance while preserving combat and attack events. Add character-target support for Sáňky's possible hero target; keep normal player-declared attacks restricted.
- [ ] Implement Shield for the next damage instance from any source, consuming it on prevention. Implement Lifesteal using the full damage dealt, including overkill, capped only by the healing recipient's maximum health; Shield-prevented damage heals nothing. Finalize repeated Shield grants and Poisonous damage-source coverage.
- [ ] Finish the shared keyword rules still needed by unreviewed cards: Stealth targeting/break conditions, silence, Momentum activation, Charge aura removal, spell-damage ordering and modifier interactions. Keep genuinely undecided card-specific behavior marked for review.
- [ ] Use explicit ending-player context for end-turn triggers and expiry, independent of the newly selected active player. Cover Petr, Jana Z., Jiříkova přednáška, Forexový obchodník, Pavel, Finanční podvod and Dole v dole. Track spell plays for the entire turn independently of Matemagická aréna's presence.
- [ ] Add effect-based cost overrides for the next Org card, the next experiment and the opponent's next-turn spells. Preserve immediate activation of Pavel's +5 spell-cost effect and remove it at that opponent's turn end; remove Finanční podvod at the current turn's end. Finalize precedence of zero-cost multipliers and other cost modifiers.
- [ ] Support a shared deck and shared fatigue for EVIL Jiřík, Pán chaosu: point both players' deck entries at the same persistent pile, make deck operations transparently affect it, and make replaying the merge a no-op. Drawn cards belong to the drawing player. Define fatigue initialization and ambiguous top-card exchanges when both deck entries already alias one pile.
- [ ] Add regressions for paired-peer random ordering, cancelled mana reservations, control into a full board, bounce overflow without discard, lethal damage-received triggers, simultaneous graveyard checks, Shield/Lifesteal, and direct-effect draws versus later lethal death cascades.

### Deferred designs

- [ ] Leave Chybný rozpis směn unimplemented until explicitly resumed; queued consecutive turns and interactions between multiple copies remain deferred.
- [ ] Design a digital challenge, potentially trivia, for Petr, Tělocvikář and related physical-challenge cards before implementing their challenge flow. Redesign Aplikovaný Darwinismus rather than introducing a second gameplay-randomness replacement alongside Elis.
- [ ] Implement Tomík, Zeusus only after the repeated-upgrade and multi-target choice flow exists. Implement Síma only after direction selection and the exact rotation mapping are defined.
- [ ] Finish the remaining identity rules for transforms, captured minions and zone changes. Control preserves existing stats; resurrection resets them. Preserve explicit card exceptions, including the mug returning captives to the opponent of its current controller and releasing nothing on silence or bounce.

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
