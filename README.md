Ultrawide HUD Fix for Takeover 2



\#Installation



1. Install BepInEx 5 (the 64-bit version) into your Takeover 2 folder. This is the folder that has the game's .exe file in it.
2. Run the game once, then close it. This lets BepInEx set itself up.
3. Download UltrawideHud.dll from the Releases page and put it in the BepInEx\\plugins folder.
4. Start the game.

The first time you run the game with the mod, it creates a settings file at BepInEx\\config\\local.ultrawide.hud.cfg. You don't need to touch it unless you want to change something.





\#Usage



Choose the 16:9 equivalent of your resolution in the options. For example, 2560x1440 on a 3440x1440 monitor, 3840x2160 on a 5120x2160 monitor, etc.

Press F8 while playing to toggle between the spanned HUD and 16:9 constrained HUD.





\#Settings



Close the game before editing the settings file, found at BepInEx\\config\\local.ultrawide.hud.cfg.

All of these settings are optional.



SpanHud (default: true)
Whether the game launches with the HUD spanned by default. Set it to false to launch with the HUD constrained to 16:9. You can still switch while playing with the toggle key.



ToggleKey (default: F8)
The key that switches between the spanned and 16:9 constrained HUD. Rename this to a different key if you prefer.



MatchAt21:9 (default: 0.4)
How big the HUD is on a 21:9 screen, as a number from 0 to 1. A higher number makes the HUD smaller and moves it lower down the screen. A lower number makes it bigger and moves it up. Screens narrower than 21:9 use this value too.



MatchAt32:9 (default: 0.7)
The same thing, but for a 32:9 screen. Screens between 21:9 and 32:9 get a mix of the two numbers.



SpreadStrength (default: 1)
How far the HUD moves toward the edges, from 0 to 1. At 0 the HUD stays in the middle where it normally sits. At 1 it goes all the way to the edges. If the edges feel too far away, try a number like 0.6.





\#Troubleshooting



\-Nothing changed: open BepInEx\\LogOutput.log in a text editor and look for a line that says "Ultrawide HUD Fix 1.0.0 loaded." If it isn't there, the DLL is in the wrong folder, or you have a different BepInEx version.

\-The HUD is too big or too small: change MatchAt21:9 or MatchAt32:9.

\-Something is too close to the edge of the screen: lower SpreadStrength.

\-Some HUD parts don't move: parts that sit near the middle of the HUD are left where they are on purpose.





\#Building it yourself



1\. Install the .NET SDK, or Visual Studio with the ".NET desktop development" option. BepInEx must already be installed in your game folder.

2\. Run this, with your own game folder path:

&#x20;  dotnet build -c Release -p:GameDir="C:\\Path\\To\\Takeover2"

&#x20;  You can also edit GameDir inside UltrawideHud.csproj and build in Visual Studio.

3\. Copy the finished UltrawideHud.dll from bin\\Release\\net472\\ into BepInEx\\plugins\\.





\#Notes



This is an unofficial mod. It isn't connected to the game's developers, and it doesn't include any of the game's files. Don't use BepInEx in games with online anti-cheat, as it could get you banned. Use it at your own risk.

