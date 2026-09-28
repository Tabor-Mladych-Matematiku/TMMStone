# TODO

- Test on Android

## Less Important

- BattleCry - Enbetter UI
- Spells - Make specific targetting - "Can target minions only, enemy minions only etc."
- Taunts
- Export JSON from website
- Import JSON in lobby selection
  - Display Deck Class in Lobby
  - Probably make a deck folder with JSONs
  - List them in Lobby selection and allow the selection of one.
- Do the rest of the cards

# Things to polish

- Network:
  - Loading screens and such - hide the network accesses.
  - "Your opponent left"
- Cards:
  - Make em move nicely - sortoff follow mouse in a cool way not instant snap.
  - Card movement animations in general.
  - Change picking a minion into like an arrow that points and then the minion attacks and stuff like that.
  - Highlight which cards are playable and which minions can attack.
  - Replay what opponent did
  - Stats text is should only extend horizontaly
- Prettier Menu
- Prettier EndGame (Its just a copy of the main menu which itself is garbage)
- LobbyUI:
  - Lobby musí mít jméno, nesmí v něm být cizí znaky - Is this true???
  - Loading screeny - Prokukujou životy - UnityUI je cursed
  - Join lobby via code. Quickjoin
- EditorExt:
  - Make a button to show the editor script in filesystem
  - Allow attaching a script to a card without creating a new one

# Notes

## Highlighted

- I think there are cases in which "highlighted slot" and "highlightedActor" may keep wrong data - fix?
- (They null themselves on leaving the mouse. But that may not be enough)

## Random

- We pass seed around but we should be SUPER mindfull to use it only on things that happen on both sides.
- Probably would be better to make some kind of Network random or something that we will pinky promise that it gets used only in the synched cases.

# Bugs

- Sometimes the relay does not join. Why?
- ServerOnTurn.OnValueChanged seems to happen sequentially first on the client who ended the turn and then on the other player.
- If the endturn/startturn effects kill the other player the caller kicks himself from the game and the ServerOnTurn.OnValueChanged does not get called on the opponent.
- Pokud si zabiješ algedraka vlastním kouzlem tak to nějak blbne - zkontrolovat co se stane s řetězením effektů a pofixovat to

# EditorExt

- TODO: cardMakyr,displayer

# Adaptive screen

- kinda
