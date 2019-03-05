# PBChance
PBChance Component for LiveSplit

## What it is

PBChance is a LiveSplit component that displays the chance of obtaining a PB on your current run. Whenever you reset, split or the run becomes slow, it runs a variable number (100,000 recomended) of simulations by randomly drawing the remaining splits from your splits in previous attempts, and computes how many of those would result in a PB.

## Installation

1. Download PBChance.dll, and place it into the Components directory of your LiveSplit installation.
2. Open LiveSplit. Right click -> Edit Layout -> [Giant "+" Button] -> Information -> PB Chance
3. You can configure how many of your most recent attempts will be used to calculate the PB chance. Go to Layout Settings and click on the PB Chance tab. You can either have it use a percentage of your most recent attempts, or just a fixed number of your most recent attempts. Suggestion: number of attempts doing in a month.
4. Read bellow for advanced settings.
5. Speedrun!

## Features

1. The calculation will perform in background in an own thread. Even if the number of calculations is set to a high number, LiveSplit will not be stopped during the calculation.
2. Autoupdate. The chance will be automatically updated if the chance drops during the run.
3. Debug file. To retrace the calculation. This includes: Headerdata, Failed Runs, clipped segment, added times, count of times per segment, first generated route, results of successfully runs and 10 failures if possible, the results, the detailed segment times/chance of selection, the execution time and the number of possible combinations.

## Advanced Settings

1. Consider at least # times per segment.
This will fill up times, if there are not enough because of the Setting "Use the most recent [x] attempts."
Valid values: 1-999 Suggestion: number of attempts doing in two weeks. Checkbox: Also include failed segments. It will consider all intervening failes. Otherwise, only the additional times are considered. The chance is lower when it's activated.
2. Random sample survey # combinations.
The number of calculations. A lower number will calculate faster, a higher number gives a more stable result.
Valid values: 1-999,999 Suggestion: at least 100,000
3. Update automatically every # seconds when the chance drops.
During a run, PBChance compares the current segment time with the best segment time. If it's slower, and the chance is above zero, PBChance calculates the new chance in the background as if you were splitting. If that chance becomes lower than the displayed one, the chance will be updated automatically. 0 means as fast as possible.
Valid values: 0-999 Suggestion: 1
4. Display additional Split info. First number is the difference of average split time and the best split time. Second number is the chance, to fail on actual split.
5. Display survival chance
This will display in addition a percent number. The formula is simply [finished runs]/[total remaining runs]. Additional informations are: finished runs / unfinished runs / success on next split / failure on next split
6. Display standard deviation.
7. Display every # seconds a random information for # seconds. It will display a random choosen information (Remaining Combinations/Sample Size/Survive in this Segment/Survive to the End/Standard Deviation/Average Difference to Best/Times in this Segment) at periodic intervals. For example, you can set to display every 60s the information for 10s. Then, it will start do display at 0:00:30 until 0:00:40, next will be 0:01:30 - 0:01:40 etc. It will stop to display, if the chance is decreasing.
8. Do not increase chance on "Skip Split"
Otherwise the chance will be recalculated similar as the command "Split".
9. Newer segment times are more important than old ones.
Newer attempts will be selected more often during a simulated run.
Valid values: 0-100 Suggestion: 100
10. Malus on failures # seconds.
For each discontinued run, a failed segment will be generated. It's added every time to a simulated run, if a failed segment is selected.
Valid values: 0-999 Suggestion: Should be set to a higher value if the reason for an interruption is essentially significant errors in a single segment (and lower in opposite case). Can be also set to 0, if "Display survival chance" is set. In that case, multiple these two numbers will result in a chance, same as an infinite malus.
11. Only consider segments, which are faster than {bestSegment[s] x # percent}
Very slow segment times will be eliminated. Should set to a number, which are clearly too slow for a normal run. Should be checked with the debug file (clipped segments), for the correct setting and result.
Valid values: 100-999 Suggestion: 150-200

Hint: The displaying chance on the start should be realistic. But rather too small than too high, for more motivation during a run. Try with these settings to optimize the chance estimation. Generate and open the text file pbchance_debug.txt (recommended a fixed font like currier) for understanding the calculation of PBChance. It is also recommended to read the overview picture below.

## Debuging settings (should be 0 during regular runs)

1. Timedifference # seconds.
This will simply adds to the pb-time.
Valid values: -86400-86400 (24h)
2. Skip the newest attempts #.
To see how high the chance was before.
Valid values: 0-99999
3. Only calc to segment #.
The calculation will stop at segment #, if it's not set to 0.
4. Generate Debug File.
This will generate a debug file "pbchance_debug.txt", and will be saved in your LiveSplit folder. You can open it with any text editor. See Features 3. for more information.

## Pictures

<a href="https://ibb.co/byjJzq"><img src="https://thumb.ibb.co/byjJzq/PBChance-Settings4.png" alt="PBChance-Settings4" border="0"></a>
[The component in action](https://ibb.co/byjJzq)

<a href="https://ibb.co/yPWNRQs"><img src="https://i.ibb.co/yPWNRQs/PBChance-Settings7.png" alt="PBChance-Settings7" border="0" /></a>[The configuration screen](https://ibb.co/yPWNRQs)

<a href="https://ibb.co/bBt2mF2"><img src="https://i.ibb.co/bBt2mF2/PBChance-Overview2.png" alt="PBChance-Overview2" border="0"></a>[Overview](https://ibb.co/bBt2mF2)
                                                                                                                                      
## Troubleshooting

**It displays W3 no times found in S# [Segment name] for a moment*

PBChance can't find historical times, gives a warning and uses best split time instead. This can in certain circumstances happen, if the setting "Only consider segments, which are faster than best segment x # percent" is set too low.

**What's the difference between 0.00% and 0%?*

If it displays 0%, then there are zero success runs found. If it displays 0.00%, then there are a few success runs found, but the chance is rounded to 0.00%. However, it will display # in #, if you split it. These are the odds, and the chance is <0.01%. For example, 5 in 125000 means, 5 runs are faster than pb, 125000 runs are tested (it's equal to a 0.004% chance).

***I'm starting to speedrun, and it displays 100%. Why that?

This chance isn't really calculated. It's assumed that the run will be completed. Even if the run is restarted. As a result, a best time will be set. Only then does PB Chance work with a real probability calculation. 
