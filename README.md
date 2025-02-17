Might require retrying when loading the UNityProject properly.
Threw an error during package import but after retrying it worked.

To test this:
Build&Run it and also launch it in Playmode (The game needs two instances)

You MUST pick TWO distinct names that are not empty before Authentication (Yes yes I will add relevent checks later)

Creating lobby might be fine without name.

(lobby has to be public - can be set after creating it)
Game can be started ONLY once both players are in lobby (Nothing will break but there is no visual indication only debug message. that you need 2 players)

Game can: take turns (End turn button)
Place minions when on turn on your Jednotka slots.

Note: not all cards are minions - therefore not every card can be played.

I shipped the game without card art because thats like MASSIVE file which
A) should not be on git
B) I ought not share willy nilly.
I am eyeing the Adressables requirement and will see if I can use it for the release.

And of course after reviewing it after deadline I found out that the default card face gets nullified by accident.
Master has it fixed. Its like one line of code - without it you cannot really test placing the cards.