# MultiplayerP4

[Watch the demo video](https://youtu.be/by8KGNBQgO8)

<div align = center>

## Features
</div>
I used Mirror to create the base for a Multiplayer 2 Person-Coop Video Game, where one player will see and interact with one side of the map, while the other will see and interact with the other side of the map. The main idea is to have both players have to deal with multiple puzzles where each one will directly affect each other. Using Mirror-only methods such as [Server], [SyncVar], [ClientRpc], [Command] in order to sync variables needed for all players for this exact reason.

<br>
<br>

**[Command]** is used to send local information of a player to the server, in this instance it as used to dictate if the lever was pulled or not and as such, know whether or not to toggle the sliding door or to identify if a cube's color has been interacted or not, and as such, cycle through the other colors.

**[ClientRpc]** is used to show ALL clients what is desired, such as the colors of all cubes, and sync them to prevent mistakes, activating all interactable cubes.

**[Server]** is used for all the logic behind the game, such as checking if the colors of the interactable cubes coincide with the other non-interactable cubes.

**[SyncVar]** is used to automatically synchronize the value of variables, such as the color index (since the colors are inside a list nd the order never changes) or a bool to check if a puzzle is solved.


<br>


<div align = center>

## Gallery

<br>
<img width="1456" height="725" alt="Map" src="https://github.com/user-attachments/assets/e353ad95-8c0a-4304-966e-c735d7b2f236" />

<br>
<img width="1450" height="710" alt="ColorPasswordCubesExample" src="https://github.com/user-attachments/assets/d09a34cb-d627-4032-873a-3006dba6eced" />

<br>
<img width="1456" height="714" alt="PlayerBEntrance" src="https://github.com/user-attachments/assets/fbaff616-2877-4010-b990-e82bea7fe462" />

<br>
<img width="1453" height="709" alt="ColorInputCubesStart" src="https://github.com/user-attachments/assets/7ec912f0-0922-4fe1-9ccd-0cc7d87daa97" />

<br>
<br>

</div>


## Unity Packages used
- [Low Poly Dungeons Lite]
- [Low Poly Simple Medieval Props]
- [Stylized Hand Painted Dungeon (Free)]
- [Mirror]


  <br>

  <!----------------------------------{ Thanks }--------------------------------->
[Low Poly Dungeons Lite]: https://assetstore.unity.com/packages/3d/environments/dungeons/low-poly-dungeons-lite-177937
[Low Poly Simple Medieval Props]: https://assetstore.unity.com/packages/3d/environments/dungeons/low-poly-dungeons-lite-177937
[Stylized Hand Painted Dungeon (Free)]: https://assetstore.unity.com/packages/3d/environments/stylized-hand-painted-dungeon-free-173934
[Mirror]: https://assetstore.unity.com/packages/tools/network/mirror-129321
