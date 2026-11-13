Ultrawide support for Takeover 2. Fixes the off-screen HUD and spreads it out to the edges of 21:9 and 32:9 monitors.
<!-- Add before/after screenshots here, for example:
<img src="images/before.png" width="48%"> <img src="images/after.png" width="48%">
Features
HUD fix: Brings the HUD back on screen at any ultrawide aspect ratio.
Spread HUD: Moves health bars, score and counters out toward the left and right edges. Press F8 to switch back to the original layout.
Real resolutions: The 16:9 options in the menu now fit your monitor, so there's no need for Alt+Enter.
Adjustable: HUD size and spread can be tuned in a small settings file.
Install
Install BepInEx 5 (64-bit) into the Takeover 2 folder, next to the game's .exe, and run the game once.
Download `t2ultra.dll` from the Releases page.
Put it in `BepInEx\plugins`.
Start the game and pick a 16:9 resolution with the height you want. On a 21:9 monitor, 1920x1080 gives a 21:9 picture that is 1080 pixels tall.
Settings
A settings file is created at `BepInEx\config\t2ultra.cfg` the first time you run the game. Close the game before editing it.
SpanHud: start with the HUD spread out (`true`) or in its original layout (`false`).
ToggleKey: key that switches between the two (default F8).
MatchAt21:9 / MatchAt32:9: HUD size on each screen shape, from 0 to 1. Higher = smaller HUD.
SpreadStrength: how far the HUD moves toward the edges, from 0 to 1. Try 0.6 if the edges feel too far.
<details>
<summary>Troubleshooting</summary>
Nothing changed: open `BepInEx\LogOutput.log` and look for `t2ultra 1.0.0 loaded.` If it's missing, the DLL is in the wrong folder or BepInEx isn't the 5.x 64-bit version.
HUD too big or small: change `MatchAt21:9` or `MatchAt32:9`.
Something is too close to the edge: lower `SpreadStrength`.
Some HUD parts don't move: parts near the middle of the HUD are left in place on purpose.


21:9 (Spanned HUD)
<img width="5120" height="2160" alt="219span" src="https://github.com/user-attachments/assets/5af257e3-2bf2-4b9b-93e2-ef6dff8a1221" />

21:9 Constrained HUD
<img width="5120" height="2160" alt="219" src="https://github.com/user-attachments/assets/5f33c5cc-3f73-49ee-8c3b-3a43f91b4990" />
32:9
<img width="5120" height="1440" alt="329" src="https://github.com/user-attachments/assets/7aee6f57-a2f9-4770-94c2-e491293c3fbd" />

