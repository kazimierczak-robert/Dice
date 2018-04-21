# Dice
Network version of Dice game 

![alt text](https://github.com/kazimierczak-robert/Dice/blob/master/Screenshot.PNG)

# Overview
**Dice** is the project of network version of popular game "Dice" for 2-4 player for Windows.

# Description
The project consists of a desktop application acting as a server (player creating a single game) or a client (player joining the game). Required number of players: 2-4. The game uses network sockets and TCP protocol to send messages between clients and the server. After receiving a message from one of the players, the server sends messages to other players to update the game status. Only one server application and many clients can be started on one computer. The applications work only inside the local network. The game was realized in accordance with the rules on page https://www.kurnik.pl/kosci/zasady.phtml. Applications are created in C# in WFA.

# Tools
Microsoft Visual Studio 2017

# How to run/compile
To run/compile **Dice**, open solution file in Visual Studio (Dice.sln) and click 'Build' or/and 'Start without debugging'.

# Attributions
* https://stackoverflow.com/a/24814027
* https://stackoverflow.com/a/45163846
* https://stackoverflow.com/a/147149
* https://stackoverflow.com/a/14890323
* https://stackoverflow.com/a/13611139
* https://stackoverflow.com/a/2138994
* https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2008/7a2f3ay4(v=vs.90)

# License
MIT

# Credits
* Robert Kazimierczak
