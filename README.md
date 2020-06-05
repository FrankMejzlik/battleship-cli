# Battleships
Simlpe Battleship game with an interactive command line UI.

# Overview
This application is mostly driven by the actions that happen in the UI. The UI registers some event, it notifies the logic behind, the logic handles it and commands the UI what to do next. Occasionaly the logic can send direct commands to the UI (things like Shutdown etc.).

This diagram shows the big picture of the wholeapplication. The names visualised in it correspond to the classes/enums/methods inside the code so it should help you to better understand and orient in the code.
![Battleships diagram](img/battleships_diagram.png)

# Build
Open the Visual Studio solution, build and run using the desired configuration.