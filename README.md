Might require couple of retrys when loading the UnityProject properly.
Threw a couple of errors during package import but after retrying it worked.

To test this:
Build&Run it and also launch it in Playmode (The game needs two instances)

You MUST pick TWO distinct names before Authentication (You can run build with empty name and Play with empty name because in Play mode it automatically adds random number to the default anonymous name)

Creating lobby is fine without name.

(lobby has to be public - can be set after creating it)
Game can be started ONLY once both players are in lobby (Otherwise start button is gray)

Game can:
take turns (End turn button)
Play cards: Each card has a cost on top right corner. In the middle bottom of the screen is your manacounter - starts at 0 and each turn adds one up to the maximum of 10. Mana gets refilled each round and does not carry over.
If a card is a minion it can be placed on a Jednotka slot
After one turn each minion can attack once (drag&drop on an enemy target)
If card is spell it might require target or just be dropped somewhere on the board to take effect (Green highlight should aid visually when card can be played) 
If a card is Field (Pole) it can be dropped to the middle slot.

There are two decks one is a valid deck built some time ago for the physical version of the game and is fully functional.
THe other deck is the same but has some extra experimental cards. Most of them work at least partially, but not every card.

All rules can be found on our website - link on itch.

Sometimes gets stuck in loading if that happens just retry. (Did not figure out what causes it)