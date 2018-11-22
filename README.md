# PBChance
PBChance Component for LiveSplit

## What it is

PBChance is a LiveSplit component that displays the chance of obtaining a PB on your current run. Whenever you reset, split or the run becomes slow, it runs a variable number (100,000 recomended) of simulations by randomly drawing the remaining splits from your splits in previous attempts, and computes how many of those would result in a PB.

## Installation

1. Place PBChance.dll into the Components directory of your LiveSplit installation.
2. Open LiveSplit. Right click -> Edit Layout -> [Giant "+" Button] -> Information -> PB Chance
3. You can configure how many of your most recent attempts will be used to calculate the PB chance. Go to Layout Settings and click on the PB Chance tab. You can either have it use a percentage of your most recent attempts, or just a fixed number of your most recent attempts.
4. Read bellow for advanced settings.
5. Speedrun!

## Features

1. The calculation will perform in background in an own thread. Even if the number of calculations is set to a high number, LiveSplit will not be stopped during the calculation.
2. Autoupdate. The chance will be automatically updated if the chance drops during the run.
3. Debug file. To retrace the calculation. This includes: Headerdata, Failed Runs, clipped segment, added times, count of times per segment, first generated route, results of successfully runs and 10 failures if possible, the results, the detailed segment times/chance of selection, the execution time and the number of possible combinations.

## Advanced Settings

1. Consider at least # times per segment.
This will fill up times, if there are not enough because of the Setting "Use the most recent [x] attempts."
Valid values: 1-999 Suggestion: 10-25
2. Random sample survey # combinations.
The number of calculations. A lower number will calculate faster, a higher number gives a more stable result.
Valid values: 1-999,999 Suggestion: at least 100,000
3. Update automatically every # seconds when the chance drops.
During a run, PBChance compares the current segment time with the best segment time. If it's slower, and the chance is above zero, PBChance calculates in background the chance. If that chance becomes lower than the displayed one, the chance will be updated automatically. 0 means as fast as possible.
Valid values: 0-999 Suggestion: 1
4. Display survival chance
This will display in addition a percent number. The formula is simply [finished runs]/[total remaining runs].
5. Do not increase chance on "Skip Split"
Otherwise the chance will be recalculated similar as the command "Split".
6. Newer segment times are more important than old ones.
Newer attempts will be selected more often during a simulated run.
Valid values: 0-100 Suggestion: 100
7. Malus on failures # seconds.
For each discontinued run, a failed segment will be generated. It's added every time to a simulated run, if a failed segment is selected.
Valid values: 0-999 Suggestion: Should be set to a higher value if the reason for an interruption is essentially significant errors in a single segment (and lower in opposite case). Can be also set to 0, if "Display survival chance" is set. In that case, multiple these two numbers will result in a chance, same as an infinite malus.
8. Only consider segments, which are faster than {bestSegment[s] x # percent}
Very slow segment times will be eliminated. Should set to a number, which are clearly too slow for a normal run. Should be checked with the debug file (clipped segments), for the correct setting and result.
Valid values: 100-999 Suggestion: 150-200

Hint: The displaying chance on the start should be realistic. But rather too small than too high, for more motivation during a run. Try with these settings to optimize the chance estimation.

## Debuging settings (should be 0 during regular runs)

1. Timedifference # seconds.
This will simply adds to the pb-time.
Valid values: -86400-86400 (24h)
2. Skip the newest attempts #.
To see how high the chance was before.
Valid values: 0-99999
3. Only calc to segment #.
The calculation will stop at split #, if it's not set to 0.
4. Generate Debug File.
This will generate a debug file "pbchance_debug.txt", and will be saved in your LiveSplit folder. You can open it with any text editor. See Features 3. for more information.

## Pictures

[The component in action.](https://ibb.co/byjJzq)
<a href="https://ibb.co/byjJzq"><img src="https://thumb.ibb.co/byjJzq/PBChance-Settings4.png" alt="PBChance-Settings4" border="0"></a>

[The configuration screen.](https://ibb.co/j9x2QL)
<a href="https://ibb.co/j9x2QL"><img src="https://thumb.ibb.co/j9x2QL/PBChance-Settings2.png" alt="PBChance-Settings2" border="0" /></a>

## Troubleshooting

**It displays W3 no times found in S# [Segment name] for a moment*

PBChance can't find historical times, gives a warning and uses best split time instead. This can in certain circumstances happen, if the setting "Only consider segments, which are faster than best segment x # percent" is set too low.

**What's the difference between 0.00% and 0%?*

If it displays 0%, then there are zero success runs found. If it displays 0.00%, then there are a few success runs found, but the chance is rounded to 0.00%.
