using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveSplit.UI;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Threading;


namespace PBChance.UI.Components
{
    class PBChanceComponent : IComponent
    {
        protected InfoTextComponent InternalComponent { get; set; }
        protected PBChanceSettings Settings { get; set; }
        protected LiveSplitState State;
        protected Random rand;
        protected string category;
        protected Thread thread;
        protected bool KeepAlive;
        protected double fLiveChanceAct;
        protected double fLiveChanceAvg;
        protected double fLastUpdate;
        protected bool bCheckUpdate;

        string IComponent.ComponentName => "PB Chance";

        IDictionary<string, Action> IComponent.ContextMenuControls => null;
        float IComponent.HorizontalWidth => InternalComponent.HorizontalWidth;
        float IComponent.MinimumHeight => InternalComponent.MinimumHeight;
        float IComponent.MinimumWidth => InternalComponent.MinimumWidth;
        float IComponent.PaddingBottom => InternalComponent.PaddingBottom;
        float IComponent.PaddingLeft => InternalComponent.PaddingLeft;
        float IComponent.PaddingRight => InternalComponent.PaddingRight;
        float IComponent.PaddingTop => InternalComponent.PaddingTop;
        float IComponent.VerticalHeight => InternalComponent.VerticalHeight;

        XmlNode IComponent.GetSettings(XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        Control IComponent.GetSettingsControl(LayoutMode mode)
        {
            Settings.Mode = mode;
            return Settings;
        }

        void IComponent.SetSettings(XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        double factorial(int x)
        {
            double factorial = 1;
            for (int i = 2; i <= x; i++)
                factorial *= i;
            return factorial;
        }

        public PBChanceComponent(LiveSplitState state)
        {
            State = state;
            InternalComponent = new InfoTextComponent("PB Chance", "0.0%")
            {
                AlternateNameText = new string[]
                {
                    "PB Chance",
                    "PB%:"
                }
            };
            Settings = new PBChanceSettings();
            Settings.SettingChanged += OnSettingChanged;
            rand = new Random();
            category = State.Run.GameName + State.Run.CategoryName;

            state.OnSplit += OnSplit;
            state.OnReset += OnReset;
            state.OnSkipSplit += OnSkipSplit;
            state.OnUndoSplit += OnUndoSplit;
            state.OnStart += OnStart;
            state.RunManuallyModified += OnRunManuallyModified;

            StartRecalculate(true);
        }

        private void OnRunManuallyModified(object sender, EventArgs e)
        {
            StartRecalculate(true);
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            StartRecalculate(true);
        }

        private void OnStart(object sender, EventArgs e)
        {
            StartRecalculate(true);
        }

        protected void OnUndoSplit(object sender, EventArgs e)
        {
            StartRecalculate(true);
        }

        protected void OnSkipSplit(object sender, EventArgs e)
        {
            StartRecalculate(true);
        }

        protected void OnReset(object sender, TimerPhase value)
        {
            StartRecalculate(true);
        }

        protected void OnSplit(object sender, EventArgs e)
        {
            StartRecalculate(true);
        }

        protected void StartRecalculate(bool bForceNewcalc)
        {
            if (thread != null)
                if (thread.ThreadState == ThreadState.Running)
                {
                    if (!bForceNewcalc) return;
                    KeepAlive = false;
                    thread.Join();
                }
            KeepAlive = true;
            bCheckUpdate = false;
            fLiveChanceAvg = 0;
            thread = new Thread(new ThreadStart(Recalculate));
            thread.Start();
        }

        protected void Recalculate()
        {
            int iSurvicalFailure = 0, iSurvicalSuccess = 0, iMaxAtempts, iCurrentSplitIndex = State.CurrentSplitIndex < 0 ? 0 : State.CurrentSplitIndex, iMaxSplit, i, iLastSegment, iAttempt; //, aktSurvival, sumSurvival = 0;
            double fSec;
            string text = ""; var watch = System.Diagnostics.Stopwatch.StartNew();
            System.String sWriteDebug1="", sWriteDebug2="";
            Time? split;

            if (Settings.SamplesCount == 0) // not configured, load default values
            {
                Settings.UsePercentOfAttempts = false;
                Settings.UseFixedAttempts = true;
                Settings.IgnoreRunCount = false;
                Settings.AttemptCount = 50;
                Settings.MalusCount = 30;
                Settings.SplitclipCount = 150;
                Settings.TimediffCount = 0;
                Settings.SamplesCount = 100000;
                Settings.iMinTimes = 20;
                Settings.iUpdate = 1;
                Settings.iSplitsvalue = 100;
            }

            if (Settings.iCalcToSplit == 0 || Settings.iCalcToSplit > State.Run.Count) iMaxSplit = State.Run.Count; else iMaxSplit = Settings.iCalcToSplit;

            // Array of the count of valid split times per split (without failing attemps)
            int[] lCountSplits = new int[iMaxSplit];

            // Get the current Personal Best, if it exists
            Time pb = State.Run.Last().PersonalBestSplitTime;

            if (pb[State.CurrentTimingMethod] == TimeSpan.Zero)
            {
                // No personal best, so any run will PB
                InternalComponent.InformationValue = "100%";
                return;
            }

            // Create the lists of split times
            List<Time?>[] splits = new List<Time?>[iMaxSplit];
            for (i = 0; i < iMaxSplit; i++)
                splits[i] = new List<Time?>();

            // Find the range of attempts to gather times from
            int firstAttempt = 1, lastAttempt = State.Run.AttemptHistory.Count - Settings.iSkipNewest;
            if (!Settings.IgnoreRunCount)
            {
                if (Settings.UseFixedAttempts) // Fixed number of attempts
                {
                    firstAttempt = lastAttempt - Settings.AttemptCount + 1;
                    if (firstAttempt < 1) firstAttempt = 1;
                }
                else // Percentage of attempts
                {
                    firstAttempt = lastAttempt - lastAttempt * (Settings.AttemptCount - 1 - Settings.iSkipNewest) / 100;
                    if (firstAttempt < 1) firstAttempt = 1;
                }
            }

            if (Settings.bDebug)
            {
                System.IO.File.WriteAllText(@"pbchance_Debug.txt", "AdjustedStartTime: " + State.AdjustedStartTime + " AttemptEnded: " + State.AttemptEnded + " AttemptStarted: " + State.AttemptStarted + " \r\nCurrentAttemptDuration: " + State.CurrentAttemptDuration + " CurrentComparison: " + State.CurrentComparison + " CurrentPhase: " + State.CurrentPhase + "\r\n");//" CurrentHotkeyProfile: " + State.CurrentHotkeyProfile.ToString()
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", "CurrentSplit: " + State.CurrentSplit + " CurrentSplitIndex: " + State.CurrentSplitIndex + " CurrentTime: " + State.CurrentTime + " \r\nCurrentTimingMethod: " + State.CurrentTimingMethod + " GameTimePauseTime: " + State.GameTimePauseTime + " IsGameTimeInitialized: " + State.IsGameTimeInitialized + " \r\nIsGameTimePaused: " + State.IsGameTimePaused + " Layout: " + State.Layout + " LayoutSettings: " + State.LayoutSettings + " \r\nLoadingTimes: " + State.LoadingTimes + " PauseTime: " + State.PauseTime + " Run: " + State.Run + " Settings: " + State.Settings + " \r\nStartTime: " + State.StartTime + " StartTimeWithOffset: " + State.StartTimeWithOffset + " TimePausedAt: " + State.TimePausedAt + "\r\n");
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", "Attempts: " + firstAttempt + " to " + lastAttempt + " Malus: " + Settings.MalusCount + " Splitclip: " + Settings.SplitclipCount + " Timediff: " + Settings.TimediffCount + " Samples: " + Settings.SamplesCount + " Survival: " + Settings.bSurvival + " Rebalancing: " + Settings.iOptimistic + "\r\n");
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", "bValueRuns: " + Settings.bValueRuns + " Min Times per Split: " + Settings.iMinTimes + " Automatic Update every: " + Settings.iUpdate + "s More Value on newer Splits: " + Settings.iSplitsvalue + " Skip newest Splits: " + Settings.iSkipNewest + "\r\n\r\n--- Failed Runs --- " + watch.ElapsedMilliseconds + "ms\r\n");
            }

            // Gather split times
            for (int a = firstAttempt; a <= lastAttempt; a++)
            {
                iLastSegment = -1;

                for (int segment = iCurrentSplitIndex; segment < iMaxSplit; segment++)
                {
                    if (State.Run[segment].SegmentHistory == null || State.Run[segment].SegmentHistory.Count == 0)
                    {
                        if (State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                        {
                            splits[segment].Add(State.Run[segment].BestSegmentTime); // no split times available, take the best split time, display a warning
                            InternalComponent.InformationValue = "E1 not times found in S" + (1 + segment) + " " + State.Run[segment].Name;
                        }
                        else
                        {
                            InternalComponent.InformationValue = "E2 not best time found in S" + (1 + segment) + " " + State.Run[segment].Name;
                            return;
                        }
                    }
                    else if (State.Run[segment].SegmentHistory.ContainsKey(a) && State.Run[segment].SegmentHistory[a][State.CurrentTimingMethod] > TimeSpan.Zero)
                    {
                        if (State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                        {
                            if ((State.Run[segment].SegmentHistory[a][State.CurrentTimingMethod].Value.TotalSeconds <= Settings.SplitclipCount * 0.01 * State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds))  // | (State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds < 60))
                            {
                                splits[segment].Add(State.Run[segment].SegmentHistory[a]); // add a valid split time
                                lCountSplits[segment]++;
                            }
                            else
                                if (Settings.bDebug) sWriteDebug2 += "Segment: " + segment.ToString("00") + " Attempt: " + a.ToString("0000") + " Factor: " + Math.Round(State.Run[segment].SegmentHistory[a][State.CurrentTimingMethod].Value.TotalSeconds / State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds, 2).ToString("0.00") + " Best: " + State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds.ToString("000.000") + " Time: " + State.Run[segment].SegmentHistory[a][State.CurrentTimingMethod].Value.TotalSeconds.ToString("0000.000") + " Name: " + State.Run[segment].Name + "\r\n";
                        }
                        else // no Best Time, then allways add
                        {
                            splits[segment].Add(State.Run[segment].SegmentHistory[a]);
                            lCountSplits[segment]++;
                        }
                        iLastSegment = segment;
                    }
                }
                if (iMaxSplit > 1 && ((iLastSegment < iMaxSplit - 1 && iLastSegment >= iCurrentSplitIndex) || (iLastSegment == -1 && iCurrentSplitIndex == 0)))
                { // Run didn't finish, add a failure for the last known split
                    if (iLastSegment == -1) iLastSegment = 0;
                    splits[iLastSegment + 1].Add(null);
                    iSurvicalFailure++;
                    if (Settings.bDebug) sWriteDebug1 += "#" + iSurvicalFailure.ToString("00") + ":  Attempt: " + a.ToString("00") + " Segment: " + (iLastSegment + 1).ToString("00") + " LastSegment: " + iLastSegment.ToString("00") + "\r\n";
                }
                else
                    if (iLastSegment >= 0) iSurvicalSuccess++;
            }

            if (Settings.bDebug)
            {
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1 + "\r\n--- Clipping segments --- " + watch.ElapsedMilliseconds + "ms\r\n" + sWriteDebug2);
                sWriteDebug1 = "\r\n--- Added times --- " + watch.ElapsedMilliseconds + "ms\r\n";
            }

            // add older split times, if there are not enough (Setting: at least X split times)
            for (int segment = iCurrentSplitIndex; segment < iMaxSplit; segment++)
                if (lCountSplits[segment] < Settings.iMinTimes) // splits[segment].Count contains also failures, lCountSplits[] contains no failures
                {
                    for (i = firstAttempt - 1; i >= 0 && lCountSplits[segment] < Settings.iMinTimes; i--)
                        if (State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                            if (State.Run[segment].SegmentHistory.ContainsKey(i) && State.Run[segment].SegmentHistory[i][State.CurrentTimingMethod] > TimeSpan.Zero)
                                if (State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                                    if ((State.Run[segment].SegmentHistory[i][State.CurrentTimingMethod].Value.TotalSeconds <= Settings.SplitclipCount * 0.01 * State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds))  // | (State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds < 60))
                                        if (State.Run[segment].SegmentHistory[i][State.CurrentTimingMethod].HasValue) // valid split time found
                                        { 
                                            splits[segment].Insert(0, State.Run[segment].SegmentHistory[i]);
                                            if (Settings.bDebug) sWriteDebug1 += "Segment: " + segment.ToString("00") + " Attempt: " + i.ToString("0000") + " Added Time: " + State.Run[segment].SegmentHistory[i][State.CurrentTimingMethod].Value.ToString() + " Count of Valid/Total Times: " + lCountSplits[segment].ToString("0000") + "/" + splits[segment].Count.ToString("0000") + " Name: " + State.Run[segment].Name + "\r\n";
                                            lCountSplits[segment]++;
                                        }
                    if (splits[segment].Count == 0 && State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                    { // no times found, but at least a best split time, display a warning, take the best split time
                        InternalComponent.InformationValue = "E3 no times found in S" + (1 + segment) + " " + State.Run[segment].Name;
                        splits[segment].Add(State.Run[segment].BestSegmentTime);
                    }
                }
            if (KeepAlive == false) return; // cancel calculation if new thread is requested

            if (Settings.bDebug) // Count of Times per Split
            {
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);
                sWriteDebug1 = "\r\n--- Count of Times per Split --- " + watch.ElapsedMilliseconds + "ms\r\n";
                for (int segment = iCurrentSplitIndex; segment < iMaxSplit; segment++)
                    sWriteDebug1+="Segment: " + segment.ToString("00") + " Count of Valid/Total Times: " + lCountSplits[segment].ToString("0000") + "/" + splits[segment].Count.ToString("0000") + " Name: " + State.Run[segment].Name + "\r\n";
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);

                //Write all Split Times
                sWriteDebug2 = "\r\n\r\n\r\n--- Detailed Split Times ---\r\n";
                for (int segment = iCurrentSplitIndex; segment < iMaxSplit; segment++)
                    for (int attempt = 0; attempt < splits[segment].Count; attempt++)
                        if (splits[segment][attempt].HasValue)
                            sWriteDebug2 += "Segment: " + segment + " Attempt:" + attempt.ToString("000") + " Time:" + splits[segment][attempt].Value[State.CurrentTimingMethod].Value + "\r\n";
            }
            
            // Calculate probability of PB
            if (Settings.bDebug) sWriteDebug1 = "\r\n--- First generated Route (" + Settings.SamplesCount + " Routes in total) --- " + watch.ElapsedMilliseconds + "ms\r\n";
            int iFaster = 0, iSlower = 0, iCountMalus = 0;
            double fSecStart = 0, fSecMalus, fTotalBestTime = 0;

            // Total Best Time
            for (int segment = iCurrentSplitIndex; segment < iMaxSplit; segment++)
                fTotalBestTime += State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000;

            // Get current time as a baseline
            Time test = State.CurrentTime;

            if (bCheckUpdate) iCurrentSplitIndex++; // background calculation, then calculate starting next split

            if (State.CurrentSplitIndex > 0)
            {
                if (test.RealTime == null) test = State.CurrentTime;
                test = State.Run[iCurrentSplitIndex - 1].SplitTime;
            }
            else
                test[State.CurrentTimingMethod] = TimeSpan.Zero;
            
            if (test[State.CurrentTimingMethod].HasValue && !bCheckUpdate) // split hasn't skipped
                fSecStart = test[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000;
            else // use the actual time, possible to read the skipping time?
                fSecStart = State.CurrentTime[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000;

            for (i = 0; i < Settings.SamplesCount; i++)
            {
                if (KeepAlive == false) return; // cancel calculation if new thread is requested
                fSecMalus = 0;
                fSec = 0;
                iAttempt = 0;

                // Add random split times for each remaining segment
                for (int segment = iCurrentSplitIndex; segment < iMaxSplit; segment++)
                {  
                    if (splits[segment].Count == 0) // This split contains no split times, so we cannot calculate a probability
                    {
                        InternalComponent.InformationValue = "E4 no times found in S" + (1 + segment) + " " + State.Run[segment].Name; // + " split contains no times"
                        return;
                    }

                    iMaxAtempts = 10000; // max tries to catch a valid time
                    do
                    {
                        iMaxAtempts--;
                        do { // select newer attempts more often
                            iAttempt = rand.Next(splits[segment].Count);
                        //} while (Math.Pow((attempt+1)/(splits[segment].Count * 1.0), Settings.iSplitsvalue * .05) < rand.Next(100) * .01); // exponential, maybe as an option, but this is slower
                        } while ((iAttempt + 1) / (splits[segment].Count * 1.0) < rand.Next(Settings.iSplitsvalue) * .01); // linear

                        split = splits[segment][iAttempt];

                        if (split == null) // split is a failure, add a malus
                        {
                            fSecMalus += Settings.MalusCount;
                            iCountMalus++;
                            if (Settings.bDebug && i == 0) sWriteDebug1 += "Segment: " + segment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Route: " + i + " Name: " + State.Run[segment].Name + " Failure " + iCountMalus + " add " + Settings.MalusCount + "s\r\n";
                        }
                    } while (split == null & iMaxAtempts > 0);
                    if (split != null) // found split times
                    {
                        //test += split.Value;
                        iAttempt++;

                        if (split.Value[State.CurrentTimingMethod].HasValue) // add the time
                            fSec += split.Value[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000;

                        if (Settings.bDebug && i == 0) sWriteDebug1 += "Segment: " + segment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Route: " + i + " Duration: " + split.Value.RealTime.ToString() + " = " + (split.Value.RealTime.Value.TotalMilliseconds / 1000).ToString("000.000") + "ms" + " --- BestTime=" + State.Run[segment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds.ToString("000.000") + " Name: " + State.Run[segment].Name + "\r\n";
                    }
                    else
                    {
                        InternalComponent.InformationValue = "E5 too many failure attempts S" + (1 + segment) + " " + State.Run[segment].Name; // + " split contains no times"
                        return;
                    }
                }
                if (Settings.bDebug && i == 0) System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);
                if (Settings.bDebug && i == 0) sWriteDebug1 = "\r\n--- Results of successfully runs and 10 failures if possible --- " + watch.ElapsedMilliseconds + "ms\r\n";

                // Check if the time is faster than pb
                if (fSec + fSecStart + fSecMalus <= pb[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000 + Settings.TimediffCount)
                {
                    iFaster++;
                    if (Settings.bDebug && iFaster <= 10) // write first ten faster times
                        sWriteDebug1 += "Run: " + i.ToString("00000") + " Total Time: (" + fSec.ToString("0000.000") + "+" + fSecStart.ToString() + "+" + fSecMalus.ToString("0000.000") + ") = " + (fSec + fSecStart + fSecMalus).ToString("0.000") + " < " + (pb[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000 + Settings.TimediffCount * fSec / (fSec + fSecStart)).ToString("0.00") + " = (" + pb[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000 + " + " + (Settings.TimediffCount * fSec / (fSec + fSecStart)).ToString("0.00") + ") success" + (Settings.iOptimistic == 0 ? "\r\n" : "");
                }
                else
                {
                    iSlower++;
                    if (Settings.bDebug && iSlower <= 10) // write first ten slower times
                        sWriteDebug1 += "Run: " + i.ToString("00000") + " Total Time: (" + fSec.ToString("0000.000") + "+" + fSecStart.ToString() + "+" + fSecMalus.ToString("0000.000") + ") = " + (fSec + fSecStart + fSecMalus).ToString("0.000") + " > " + (pb[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000 + Settings.TimediffCount * fSec / (fSec + fSecStart)).ToString("0.00") + " = (" + pb[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000 + " + " + (Settings.TimediffCount * fSec / (fSec + fSecStart)).ToString("0.00") + ") failure" + (Settings.iOptimistic == 0 ? "\r\n" : "");
                }
            }

            if (iFaster == 0 & iSlower == 0) // no more remaining times, check for a new pb
            {
                if (fSecStart < pb[State.CurrentTimingMethod].Value.TotalMilliseconds/1000)
                    InternalComponent.InformationValue = "100% PB";
                else
                    InternalComponent.InformationValue = "0%";
                return;
            }
            else // display results
            {
                double fProb = iFaster / (iFaster + iSlower + 0.0);
                if (bCheckUpdate) { // background calculation
                    if (fLiveChanceAvg>0) fLiveChanceAvg = (fLiveChanceAvg + fProb) * .5;
                    else fLiveChanceAvg = fProb;
                    //InternalComponent.InformationValue = fProb + "|" + fLiveChanceAvg + "|" + fLiveChanceAct;
                    if (fProb > fLiveChanceAvg || fLiveChanceAvg > fLiveChanceAct) // do not display, if chance is higher than the displaying chance //&& !(fProb==0 && fLiveChanceAvg==0 && fLiveChanceAct>0)
                        return;

                    if (fProb > 0)
                    {
                        if (fProb > .1 && fProb < .9) // round a place behind the comma, in the range of 10%-90%
                            fProb = Math.Round(fLiveChanceAvg, 3);
                        else
                            fProb = fLiveChanceAvg;
                    }
                }
                fLiveChanceAct = fProb;

                if (fProb == 0) text += "0%"; // Zero success, display 0% instead of 0.00%. 0.00% will be displayed if count of samples are >> 10,000 and only a few successes
                else text += (fProb * 100).ToString("0.00") + "%";

                if (Settings.DisplayOdds && fProb > 0) // Displaying odds
                    text += " (1 in " + Math.Round(1 / fProb, 1).ToString() + ")";

                if (Settings.bSurvival) // Calculate survival chance
                {
                    text += " / " + Math.Round(iSurvicalSuccess / (iSurvicalSuccess + iSurvicalFailure + .0) * 100, 0).ToString() + "%";
                    if (InternalComponent.InformationName != "PB / Survival Chance")
                        InternalComponent.InformationName = "PB / Survival Chance";
                } else if (InternalComponent.InformationName != "PB Chance")
                    InternalComponent.InformationName = "PB Chance";

                if (Settings.bDebug)
                {
                    sWriteDebug1 += "\r\n\r\n" + InternalComponent.InformationName + ": " + text + " SuccessCount: " + iFaster + " FailCount: " + iSlower + " MalusCount: " + iCountMalus + "\r\n\r\nExecution time: " + watch.ElapsedMilliseconds + "ms";
                    System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1 + sWriteDebug2);
                    Settings.bDebug = false;
                }
                
                InternalComponent.InformationValue = text;
            }
        }

        void IComponent.DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            PrepareDraw(state, LayoutMode.Horizontal);
            InternalComponent.DrawHorizontal(g, state, height, clipRegion);
        }

        void IComponent.DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            InternalComponent.PrepareDraw(state, LayoutMode.Vertical);
            PrepareDraw(state, LayoutMode.Vertical);
            InternalComponent.DrawVertical(g, state, width, clipRegion);
        }
        
        void PrepareDraw(LiveSplitState state, LayoutMode mode)
        {
            InternalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
            InternalComponent.PrepareDraw(state, mode);
        }

        void IComponent.Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            string newCategory = State.Run.GameName + State.Run.CategoryName;
            if (newCategory != category)
            {
                StartRecalculate(true);
                category = newCategory;
            }

            // check the update frequency
            if (Math.Abs(fLastUpdate - State.CurrentTime[State.CurrentTimingMethod].Value.TotalMilliseconds) >= Settings.iUpdate * 1000)
            {
                double fTimer=0;
                int iCurrentSplitIndex = State.CurrentSplitIndex - 1;
                fLastUpdate = State.CurrentTime[State.CurrentTimingMethod].Value.TotalMilliseconds;

                if (iCurrentSplitIndex > 0) // calculate the actual split time
                    if (State.Run[iCurrentSplitIndex].SplitTime[State.CurrentTimingMethod].HasValue)
                        //if (State.CurrentTime[State.CurrentTimingMethod].HasValue) // seems this is not necessary
                        if (iCurrentSplitIndex >= 0)
                                    fTimer = State.CurrentAttemptDuration.TotalMilliseconds - State.Run[iCurrentSplitIndex].SplitTime[State.CurrentTimingMethod].Value.TotalMilliseconds;
                            else fTimer = State.CurrentAttemptDuration.TotalMilliseconds;
                    else         fTimer = 999999999999999999999999999999999999999999999999999999.9; // determination of split time isn't possible now
                else             fTimer = State.CurrentAttemptDuration.TotalMilliseconds;

                // background calculation, if actual split time is slower than best split time, and the actual chance is > 0
                if (fTimer > State.Run[iCurrentSplitIndex + 1].BestSegmentTime[State.CurrentTimingMethod].Value.TotalMilliseconds && fLiveChanceAct > 0)
                    if (thread == null || thread.ThreadState != ThreadState.Running) // do it if the thread isn't running
                    {
                        KeepAlive = true;
                        bCheckUpdate = true; // tell Recalculate(), that's a background calculation
                        thread = new Thread(new ThreadStart(Recalculate));
                        thread.Start();
                    }
            }

            InternalComponent.Update(invalidator, state, width, height, mode);
        }

        void IDisposable.Dispose()
        {

        }
    }
}
