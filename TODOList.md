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
