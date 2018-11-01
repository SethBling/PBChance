# PBChance
PBChance Component for LiveSplit

## What it is:

PBChance is a LiveSplit component that displays the chance of obtaining a PB on your current run. Whenever you reset or split, it runs a variable number (100,000 recomended) of simulations by randomly drawing the remaining splits from your splits in previous attempts, and computes how many of those would result in a PB.

## Installation:

1. Place PBChance.dll into the Components directory of your LiveSplit installation.
2. Open LiveSplit. Right click -> Edit Layout -> [Giant "+" Button] -> Information -> PB Chance
3. You can configure how many of your most recent attempts will be used to calculate the PB chance. Go to Layout Settings and click on the PB Chance tab. You can either have it use a percentage of your most recent attempts, or just a fixed number of your most recent attempts.
4. For advanced setting read bellow.
5. Speedrun!

## Features

1. The calculation will perform in background in an own thread. Even if the number of calculations is set to a high number, LiveSplit will not be stopped during the calculation.
2. Debug file. To retrace the calculation. This includes: Headerdata, Failed Runs, clipped splits, added times, count of times per split, first generated route, results of successfully runs and 10 failures if possible, the result, the detailed split times and the execution time.

## Advanced Settings:

1. Consider at least # times per split.
This will fill up times, if there are not enough because of the Setting "Use the most recent [x] attempts."
Valid values: 1-999 Suggestion: 10-25
2. Random sample survey # combinations.
The number of calculations. A lower number will calculate faster, a higher number gives a more stable result.
Valid values: 1-999,999 Suggestion: 100,000
3. Update automatically every # seconds when the chance drops
During a run, PBChance compares the current split time with the best split time. If it's slower, and the chance is above zero, PBChance calculate in background the chance. If that chance becomes lower than the displayed one, the chance will be updated automatically every # seconds. 0 means as fast as possible.
Valid values: 0-999 Suggestion: 1
4. Display survival chance
This will display in addition a percent number. The formula is simply [successfully runs]/[total remaining runs].
5. Newer split times are more important than old ones
Newer attempts will be selected more often during a simulated run.
Valid values: 0-100 Suggestion: 100
6. Malus on failures # seconds
For each discontinued run, a failed split will be generated. It's added every time to a simulated run, if a failed split is selected.
Valid values: 0-999 Suggestion: it depends. Can be set to 0, if "Display survical chance" is set.
7. Only consider splits, which are faster than {bestSplit[s] x # percent}
Very slow split times will be eliminated. Should set to a number, which are clearly to slow for a normal run. Should be checked with the debug file (clipped splits), for the correct setting and result.
Valid values: 100-999 Suggestion: 150-250

## Debuging settings (should be 0 during regular runs)

1. Timedifference # seconds
This will simply adds to the pb-time.
Valid values: -86400-86400 (24h)
2. Skip the newest attempts #
To see how high the chance was before.
Valid values: 0-99999
3. Only calc to split #
The calculation will stop at split #, if it's not set to 0.
4. Generate Debug File
This will generate a debug file "pbchance_debug.txt", and will be saved in your LiveSplit folder. You can open it with any text editor. See Features 2. for more information.

## Pictures:

[The component in action.](http://i.imgur.com/YIjln5P.png)

[The configuration screen.](http://i.imgur.com/CgUuB46.png)

## Troubleshooting:

**It displays E# no times found / too many failure attempts in S# [Segment name]*

PBChance can't find historical times. This can in certain circumstances happen, if the Setting "Only consider splits, which are faster than {best Split x X percent" is set too low. Please set it to a higher number, eg. 200%. Skip the newest attempts should be set to 0.
