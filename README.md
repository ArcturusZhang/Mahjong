# Mahjong

A working in progress LAN Japanese Mahjong game.

Some resources are fetched from On-line web game [Majsoul](http://www.majsoul.com/0/).

## TODO List

### 2018/10/29

1. Richi cannot be performed when a player's point is low (DONE)

1. Player cannot discard same tile or strongly related tiles after claimed an open (DONE)

1. Add item to RoundStatus how many richi stick are placed on the board (Done)

1. Rework RoundStatus, assign this information to a new Networked Object (Done)

1. Logic for rong and tsumo

1. Detailed data struct describing points transfer

1. Complete summary panel

1. Setting item whether allows false richi (claim richi when hand are not ready)

1. Add setting items to end game when all point are lost or not

1. UI completion

1. Assign hooks to players' properties (such as PlayerIndex), to assiociate them with correct position, etc.

1. Early abort game conditions

1. Graduately obsolete ResoureManager