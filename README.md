GAME INFO

In this match3 game, you need to match at least 3 cells horizontally or vertically.

Column and row counts can be determined via the GameCreator game object. By default it is set as 8 by 8.

Nonspawnable column indexes can also be determined via the GameCreator game object. New objects will not drop in columns selected as nonspawnable.

The size of the board is determined according to the size of the screen.

The initial board objects are created randomly however they are prevented from forming a match.

The object which are spawned and fell during the game are determined as randomly.

A new move can be made before the end of the previous move in the game. "Move" includes both matching and the filling the empty cells.

If the new move affects the board that will be formed as a result of the previous incomplete move, matches on the board are determined when both of the moves are finished.
