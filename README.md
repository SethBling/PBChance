# PBChance
PBChance Component for LiveSplit. It displays the chance of obtaining a PB on your current run. Whenever you reset, split or the run becomes slow, it runs a variable number (1,000,000 recommended) of simulations by randomly drawing the remaining splits from your splits in previous attempts and computes how many of those would result in a PB.

![Title](/PBChance/images/PBChance%20Title.png)

## Installation

1. Download ![PBChance.dll](/PBChance.dll) and place it into the Components directory of your LiveSplit installation.
2. Open LiveSplit. Right click -> Edit Layout -> [Giant "+" Button] -> Information -> PB Chance
3. Read below for general, PB pace and display settings.
4. Speedrun!

## Features

1. The calculation will perform in the background in an own thread. Even if the number of calculations is set to a high number, LiveSplit will not be stopped during the calculation.
2. Autoupdate. The chance will be automatically updated if the chance drops during the run.
3. Additional informations can be displayed continuously or periodically.
4. Debug file. To retrace the calculation. This includes: Header data, Failed Runs, clipped segment, added times, count of times per segment, first generated route, results of first 10 successfully runs and 10 failures if possible, the results, the detailed segment times/chance of selection, the execution time and the number of possible combinations.
5. Check for the latest version and download it. Directly in the settings window.

## General Settings (Tab 1)
![Method1](/PBChance/images/PBChance%20Settings%201.4.2%20P1.png)

1. Number of attempts used
   1. You can configure how many of your most recent attempts will be used to calculate the PB chance. Go to Layout Settings and click on the PB Chance tab. You can either have it use a percentage of your most recent attempts, or just a fixed number of your most recent attempts. Valid values: 1-99,999 Suggestion: 50 attempts on runs > 2h, 100 attempts on runs < 1h.
   2. Consider at least # times per segment
This will fill up times, if there are not enough because of the Setting "Use the most recent [x] attempts."
Valid values: 1-99,999 Suggestion: about 1/3 of the number of attempts. Checkbox: Also include failed segments. It will consider all intervening failes. Otherwise, only the additional times are considered. The chance is lower when it's activated.

2. Sample Size
   1. Random sample survey # combinations
The minimum number of simulated runs. A lower number will calculates the result faster, a higher number gives a more stable result. However, it will calculate at least 900ms by default, so on most cases the sample size is already more than a million (check "Actual Sample size"). Valid values: 1-9,999,999 Suggestion: around 1,000,000

3. Settings affect the PB Chance
   1. Penalty on failures # seconds
For each discontinued run, a failed segment will be generated. It's added every time to a simulated run, if a failed segment is selected. Valid values: 0-999 Suggestion: Start with 30. Should be set to a higher value if the reason for an interruption is essentially significant errors in a single segment (and lower in the opposite case). Can be also set to 0, if "Display survival chance" is set. In that case, multiply these two numbers will result in a chance, same as an infinite malus.
   2. Max fails #, then the penality is infinite
If a simulated run contains more then # fails, the whole run is considered as a fail.
The number of selecting a failure in a simulated run can be limited now. If it is set to 1, only the first time the penalty time will be added. If a failed segment will be selected two times, the simulated run will be count as a failure, no matter how good it would be. In addition, if it's set to a low number, the number of combinations per second will increase significantly by two to three times. Valid values: 0-999 Suggestion: 1
   3. Only consider segments, which are faster than {bestSegment[s] x # percent}
Very slow segment times will be eliminated. Should set to a number, which is clearly too slow for a normal run. Should be checked with the debug file (clipped segments), for the correct setting and result.
Valid values: 100-999 Suggestion: 150-200
   4. Newer segment times are more important than old ones
Newer attempts will be selected more often during a simulated run.
Valid values: 0-100 Suggestion: 100
   5. Add the best time for each segment
Valid values: 0-100. Suggestion: about 25
If the value is above 0, the best time will be added once for each segment, regardless of other settings. The larger the set value, the more often the best time will be selected. Point 8. is relevant for this. Then there is always a chance, if the remaining best time is faster than the Pb time, even a few runs are selected. Can be adjusted to optimize the chance estimation.

Hint: The displaying chance on the start should be realistic. You can try with these settings to optimize the chance estimation. Keep in mind the chance will automatically grow up if you play better. Generate and open the text file pbchance_debug.txt (recommended a fixed font like currier) for understanding the calculation of PBChance. It is also recommended to read the overview picture below.

## PB Pace Settings (Tab 2)
![Method1](/PBChance/images/PBChance%20Settings%201.4.2%20P2c.png)

1. Display Pace to PB
"Pace to PB" is defined as the average segment time, for all successfully simulated runs. So it displays a good, typical segment/split time, if it is on PB pace. It's not a static information, so if a pb isn't possible, it can't display, and if it's only barely possible, the times will be going harder to beat.
2. Split time instead of segment time
If you want to display the total time, check "Split time instead of Segment Time".
3. Worst segments instead of average in a successful pace
If a successful run is found, it remembers the worst time per segment. It can display the worst segment, but it was successful in a simulated run. Use it, if you want to know the slowest time you have to beat to continue. Use the average segment time, if you want to know an average segment time for a run to PB.
4. Extra goal: # milliseconds faster then  PB
An extra goal can be specified for the PB pace. Use it, if you want to PB a better time, e.g. your PB is currently 1:30:20 and you want 1:30:00, then you can set the difference time to 20000 milliseconds. The regular PB Chance remains untouched for this setting, but you can see the chance in the data-list under "PB Regular/Extra Goal".
5. Display Resolution: # Digits of milliseconds
The number of digits for a pace can be specified between 0-3 (1 second up to 1-millisecond resolution).
6. Refresh list
Check in the table for "total" - this is the sample size. This information can be used to know the information in the table is complete. If not, the sample size to low, click on "Refresh List". Every time, you change a setting, the list will be automatically quick-updated, for a fast response, but not the whole sample size. After the calculation is complete - it takes about one second after a change in the settings - the results will no longer be changed when clicking on "refresh list" twice or more.
7. Data table
You can make a data-table of a complete run based on (1.). Go into the settings and scroll down. The table will be updated automatically, however you can click on "Refresh List" for a more accurate result. It will display the "Goal Time" (see 1.) in total and separately by the segments. In addition, it displays the best possible time, you can compare to this. So, if you want to beat a certain pb-time, you can also adjust the setting "Time difference" for this, to shift the Goal PB Time what you want.
Faster/Extra Goal/Total. Faster means the number of successful runs, Extra Goal means the same, but considering the setting in (4.), Total is the sample size.

## Display Settings (Tab 3)
![Method1](/PBChance/images/PBChance%20Settings%201.4.2%20P3.png)

1. Display Options
   1. Update automatically every # seconds when the chance drops
During a run, PBChance compares the current segment time with the best segment time. If it's slower, and the chance is above zero, PBChance calculates the new chance in the background as if you were splitting. If that chance becomes lower than the displayed one, the chance will be updated automatically. 0 means as fast as possible.
Valid values: 0-999 Suggestion: 1
   2. Do not increase chance on "Skip Split"
Otherwise the chance will be recalculated similar to the command "Split".
   3. Display additional Split info
The first number is the difference of average split time and the best split time. The second number is the chance, to fail on the actual split.
   4. Display survival chance
This will display in addition two percent numbers. The first number is the chance to survive on actual segment, the second number is the chance to survive to the end. It considers the settings "Use most recent...attempts" and "Consider at least...attempts".
   5. Display standard deviation
Formula: Root of the sum of the square difference of actual segment time to average segment time.
   6. Display every # seconds a random information for # seconds.
It will display a randomly chosen information (Remaining Combinations/Sample Size/Survive in this Segment/Survive to the End/Standard Deviation/Average Difference to Best/Until here have Survived/Combinations per Second) at periodic intervals. For example, you can set to display every 60s the information for 10s. Then, it will start to display at 0:00:30 until 0:00:40, next will be 0:01:30 - 0:01:40 etc. Alternatively, you can set it to every 0s, then it will display a information once per segment (in the middle). For example, display every 0 seconds ... for 10 seconds, and the best segment time is 2:00, then it will display between 0:55 and 1:05 in this segment. In any case it will stop to display, if the chance is decreasing, and between splits with a gap of 10 seconds. It displays one of following informations:
      1. Remaining Combinations
      2. Sample Size
      3. Survival in this Segment
      4. Survival to the End
      5. Standard Deviation
      6. Average Difference to Best
      7. Until here have Survived
      8. Combinations per Second
      9. Worth to continue
      10. Run is currently
      11. Pace to PB
      12. Avg Dif to Best (All/Suc)
      13. Worst Pace to PB
      14. Avg/Worst Pace to PB

2. Debugging settings (should be 0 during regular runs)
   1. Time difference # seconds
This will simply add to the pb-time.
Valid values: -86400-86400 (24h)
   2. Skip the newest attempts #
To see how high the chance was before.
Valid values: 0-99999
   3. Only calc to segment #
The calculation will stop at segment #, if it's not set to 0.
   4. Generate Debug File
This will generate a debug file "pbchance_debug.txt", and will be saved in your LiveSplit folder. You can open it with any text editor. See Features 4. for more information.

## Check New Version

When you are online, you can check for the newest version. If a newer version is available, you will be notified and PBChance automatically downloads PBChance.dll into your LiveSplit directory. To install the latest version, close LiveSplit and move PBChance.dll from the LiveSplit directory into the Components subdirectory. Your current PBChance.dll will be overwritten.

## Pictures

<a href="https://ibb.co/byjJzq"><img src="https://thumb.ibb.co/byjJzq/PBChance-Settings4.png" alt="PBChance-Settings4" border="0"></a>
[The component in action](https://ibb.co/byjJzq)

<a href="https://ibb.co/bBt2mF2"><img src="https://i.ibb.co/bBt2mF2/PBChance-Overview2.png" alt="PBChance-Overview2" border="0"></a>[Overview](https://ibb.co/bBt2mF2)
                                                                                                                                      
## Troubleshooting

*It displays W3 no times found in S# [Segment name] for a moment*

PBChance can't find historical times, gives a warning and uses best split time instead. This can in certain circumstances happen, if the setting "Only consider segments, which are faster than the best segment x # percent" is set too low.

*What's the difference between 0.00% and 0%?*

If it displays 0%, then there are zero success runs found. If it displays 0.00%, then there are a few successful runs found, but the chance is rounded to 0.00%. However, it will display # in #, if you split it. These are the odds, and the chance is <0.01%. For example, 5 in 125000 means, 5 runs are faster than pb, 125000 runs are tested (it's equal to a 0.004% chance).

*I'm starting to speedrun, and it displays 100%. Why that?*

This chance isn't really calculated. It's assumed that the run will be finished, even if the run is restarted. As a result, the best personal time will be set, therefore a PB. Only then does PB Chance work with a real probability calculation. 

*It tells me there is a 0% chance, although it would theoretically be possible, so it can't be correct*

As long as a number of segments are pending, mathematically, this is still possible, but it is simply too unlikely that a randomly chosen run will be successful. Keep in mind that the number of possible combinations is incredibly large. In another example, it is theoretically possible to roll a 6 10 times in a row, but you will never manage to do it in your whole life because it is simply too unrealistic.

*I've found a hidden setting. Does it work?*

Yes.
