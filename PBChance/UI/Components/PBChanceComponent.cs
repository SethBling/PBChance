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

        double summ(int x)
        {
            double fSumm = 0;
            for (int i = 1; i <= x; i++)
                fSumm += i;
            return fSumm;
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
            int iSurvivalFailure = 0, iSurvivalSuccess = 0, iMaxAtempts, iCurrentSplitIndex, iMaxSplit, i, iLastSegment, iAttempt, iSegment; //, aktSurvival, sumSurvival = 0;
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

            iCurrentSplitIndex = (State.CurrentSplitIndex < 0 ? 0 : State.CurrentSplitIndex) + (bCheckUpdate ? 1 : 0);
            iMaxSplit = (Settings.iCalcToSplit == 0 || Settings.iCalcToSplit > State.Run.Count) ? State.Run.Count : Settings.iCalcToSplit;

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
            int iFirstAttempt = 1, iLastAttempt = State.Run.AttemptHistory.Count - Settings.iSkipNewest;
            if (!Settings.IgnoreRunCount)
                iFirstAttempt = iLastAttempt - (Settings.UseFixedAttempts ? Settings.AttemptCount + 1: iLastAttempt * (Settings.AttemptCount - 1 - Settings.iSkipNewest) / 100);
            if (iFirstAttempt < 1) iFirstAttempt = 1;

            if (Settings.bDebug)
                System.IO.File.WriteAllText(@"pbchance_Debug.txt", "AdjustedStartTime: " + State.AdjustedStartTime + " AttemptEnded: " + State.AttemptEnded + " AttemptStarted: " + State.AttemptStarted + " \r\nCurrentAttemptDuration: " + State.CurrentAttemptDuration + " CurrentComparison: " + State.CurrentComparison + " CurrentPhase: " + State.CurrentPhase + "\r\n" //" CurrentHotkeyProfile: " + State.CurrentHotkeyProfile.ToString()
                    + "CurrentSplit: " + State.CurrentSplit + " CurrentSplitIndex: " + State.CurrentSplitIndex + " CurrentTime: " + State.CurrentTime
                    + "\r\nCurrentTimingMethod: " + State.CurrentTimingMethod + " GameTimePauseTime: " + State.GameTimePauseTime + " IsGameTimeInitialized: " + State.IsGameTimeInitialized
                    + "\r\nIsGameTimePaused: " + State.IsGameTimePaused + " Layout: " + State.Layout + " LayoutSettings: " + State.LayoutSettings
                    + "\r\nLoadingTimes: " + State.LoadingTimes + " PauseTime: " + State.PauseTime + " Run: " + State.Run + " Settings: " + State.Settings
                    + "\r\nStartTime: " + State.StartTime + " StartTimeWithOffset: " + State.StartTimeWithOffset + " TimePausedAt: " + State.TimePausedAt
                    + "\r\nAttempts: " + iFirstAttempt + " to " + iLastAttempt + " Malus: " + Settings.MalusCount + " Splitclip: " + Settings.SplitclipCount + " Timediff: " + Settings.TimediffCount + " Samples: " + Settings.SamplesCount + " Survival: " + Settings.bSurvival + " Rebalancing: " + Settings.iOptimistic
                    + "\r\nValueRuns: " + Settings.bValueRuns + " Min Times per Split: " + Settings.iMinTimes + " Automatic Update every: " + Settings.iUpdate + "s More Value on newer Splits: " + Settings.iSplitsvalue + " Skip newest Splits: " + Settings.iSkipNewest
                    + "\r\nPersonal Best Time to beat: " + pb[State.CurrentTimingMethod].ToString() + "\r\n\r\n--- Failed Runs --- " + watch.ElapsedMilliseconds + "ms\r\n"); 

            // Gather split times
            for (iAttempt = iFirstAttempt; iAttempt <= iLastAttempt; iAttempt++)
            {
                iLastSegment = -1;

                for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                {
                    if (State.Run[iSegment].SegmentHistory == null || State.Run[iSegment].SegmentHistory.Count == 0)
                    {
                        if (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                        {
                            splits[iSegment].Add(State.Run[iSegment].BestSegmentTime); // no split times available, take the best split time, display a warning
                            InternalComponent.InformationValue = "W1 no times found in S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                            if (Settings.bDebug) sWriteDebug2 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Best: " + State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds.ToString("000.000") + " Time: " + State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod].Value.TotalSeconds.ToString("0000.000") + " Name: " + State.Run[iSegment].Name + " (Warning 1: no historial times found)\r\n";
                            lCountSplits[iSegment]++;
                        }
                        else
                        {
                            InternalComponent.InformationValue = "E1 no (best) times found in S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                            System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1 + "\r\n--- Clipping segments --- " + watch.ElapsedMilliseconds + "ms\r\n" + sWriteDebug2);
                            return;
                        }
                    }
                    else if (State.Run[iSegment].SegmentHistory.ContainsKey(iAttempt) && State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod] > TimeSpan.Zero)
                    {
                        if (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                        {
                            if ((State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod].Value.TotalSeconds <= Settings.SplitclipCount * 0.01 * State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds))  // | (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds < 60))
                            {
                                splits[iSegment].Add(State.Run[iSegment].SegmentHistory[iAttempt]); // add a valid split time
                                lCountSplits[iSegment]++;
                            }
                            else
                                if (Settings.bDebug) sWriteDebug2 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Factor: " + Math.Round(State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod].Value.TotalSeconds / State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds, 2).ToString("0.00") + " Best: " + State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds.ToString("000.000") + " Time: " + State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod].Value.TotalSeconds.ToString("0000.000") + " Name: " + State.Run[iSegment].Name + "\r\n";
                        }
                        else // no Best Time, then allways add
                        {
                            splits[iSegment].Add(State.Run[iSegment].SegmentHistory[iAttempt]);
                            InternalComponent.InformationValue = "W2 no best time found in S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                            if (Settings.bDebug) sWriteDebug2 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Time: " + State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod].Value.TotalSeconds.ToString("0000.000") + " Name: " + State.Run[iSegment].Name + " Warning 2: No best Time found\r\n";
                            lCountSplits[iSegment]++;
                        }
                        iLastSegment = iSegment;
                    }
                }
                if (iMaxSplit > 1 && ((iLastSegment < iMaxSplit - 1 && iLastSegment >= iCurrentSplitIndex) || (iLastSegment == -1 && iCurrentSplitIndex == 0)))
                { // Run didn't finish, add a failure for the last known split
                    if (iLastSegment == -1) iLastSegment = 0;
                    splits[iLastSegment + 1].Add(null);
                    iSurvivalFailure++;
                    if (Settings.bDebug) sWriteDebug1 += "#" + iSurvivalFailure.ToString("00") + ":  Attempt: " + iAttempt.ToString("00") + " Segment: " + (iLastSegment + 1).ToString("00") + " LastSegment: " + iLastSegment.ToString("00") + "\r\n";
                }
                else
                    if (iLastSegment >= 0) iSurvivalSuccess++;
            }

            if (Settings.bDebug)
            {
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1 + "\r\n--- Clipping segments --- " + watch.ElapsedMilliseconds + "ms\r\n" + sWriteDebug2);
                sWriteDebug1 = "\r\n--- Added times --- " + watch.ElapsedMilliseconds + "ms\r\n";
            }

            // add older split times, if there are not enough (Setting: at least X split times)
            for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
            {
                //if (Settings.bDebug) sWriteDebug1 += "iSegment: " + iSegment.ToString("00") + " Count Valid/Min " + lCountSplits[iSegment] + " < " + Settings.iMinTimes + "\n";
                if (lCountSplits[iSegment] < Settings.iMinTimes) // splits[iSegment].Count contains also failures, lCountSplits[] contains no failures
                {
                    for (i = iFirstAttempt - 1; i >= 0 && lCountSplits[iSegment] < Settings.iMinTimes; i--)
                        if (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                            if (State.Run[iSegment].SegmentHistory.ContainsKey(i) && State.Run[iSegment].SegmentHistory[i][State.CurrentTimingMethod] > TimeSpan.Zero)
                                if (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                                    if ((State.Run[iSegment].SegmentHistory[i][State.CurrentTimingMethod].Value.TotalSeconds <= Settings.SplitclipCount * 0.01 * State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds))  // | (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds < 60))
                                        if (State.Run[iSegment].SegmentHistory[i][State.CurrentTimingMethod].HasValue) // valid split time found
                                        {
                                            splits[iSegment].Insert(0, State.Run[iSegment].SegmentHistory[i]);
                                            if (Settings.bDebug) sWriteDebug1 += "Segment: " + iSegment.ToString("00") + " Attempt: " + i.ToString("0000") + " Added Time: " + State.Run[iSegment].SegmentHistory[i][State.CurrentTimingMethod].Value.ToString() + " Factor: " + Math.Round(State.Run[iSegment].SegmentHistory[i][State.CurrentTimingMethod].Value.TotalSeconds / (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds + .0001), 2).ToString("0.00") + " Count of Valid/Total Times: " + lCountSplits[iSegment].ToString("0000") + "/" + splits[iSegment].Count.ToString("0000") + " Name: " + State.Run[iSegment].Name + "\r\n";
                                            lCountSplits[iSegment]++;
                                            if (lCountSplits[iSegment] == Settings.iMinTimes) i = 0;
                                        }
                    if (lCountSplits[iSegment] == 0 && State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                    { // no times found, but at least a best split time, display a warning, take the best split time
                        InternalComponent.InformationValue = "W3 no times found in S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                        splits[iSegment].Add(State.Run[iSegment].BestSegmentTime);
                        if (Settings.bDebug) sWriteDebug1 += "Segment: " + iSegment.ToString("00") + "          Added Best Time: " + State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].ToString() + " Count of Valid/Total Times: " + lCountSplits[iSegment].ToString("0000") + "/" + splits[iSegment].Count.ToString("0000") + " Name: " + State.Run[iSegment].Name + " (Warning 3: no historial times found)\r\n";
                        lCountSplits[iSegment]++;
                    }
                }
                if (lCountSplits[iSegment] == 0)
                { // no times found in older attempts
                    InternalComponent.InformationValue = "E2 no times found in S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                    System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);
                    return;
                }
            }
            if (KeepAlive == false) return; // cancel calculation if new thread is requested

            if (Settings.bDebug) // Count of Times per Split
            {
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);
                sWriteDebug1 = "\r\n--- Count of Times per Split --- " + watch.ElapsedMilliseconds + "ms\r\n";
                for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                    sWriteDebug1+="Segment: " + iSegment.ToString("00") + " Count of Valid/Total Times: " + lCountSplits[iSegment].ToString("0000") + "/" + splits[iSegment].Count.ToString("0000") + " Name: " + State.Run[iSegment].Name + "\r\n";
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);

                //Write all Split Times
                sWriteDebug2 = "\r\n\r\n\r\n--- Detailed Split Times ---\r\n";
                for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++) {
                    for (iAttempt = 0; iAttempt < splits[iSegment].Count; iAttempt++) {
                        sWriteDebug2 += "Segment: " + iSegment + " Attempt:" + iAttempt.ToString("000") + " Chance: ";
                        sWriteDebug2 += ((100-Settings.iSplitsvalue + Settings.iSplitsvalue * (iAttempt + 1) / (splits[iSegment].Count+1.0)) / splits[iSegment].Count / (100-Settings.iSplitsvalue*.5)*100.0).ToString("00.0") + "% "; // linear
                        //sWriteDebug2 += (100 * (iAttempt + 1) / summ(splits[iSegment].Count)).ToString("00.0") + "% ";
                        if (splits[iSegment][iAttempt].HasValue)
                            sWriteDebug2 += "Time:" + splits[iSegment][iAttempt].Value[State.CurrentTimingMethod].Value + " Factor: " + Math.Round(splits[iSegment][iAttempt].Value[State.CurrentTimingMethod].Value.TotalSeconds / (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds + .0001), 2).ToString("0.00\r\n");
                        else
                            sWriteDebug2 += "Failed\r\n";
                        while (iAttempt / (splits[iSegment].Count * 1.0) < rand.Next(Settings.iSplitsvalue) * .01) ;
                    }
                    sWriteDebug2 += "\r\n";
                }
                sWriteDebug1 = "\r\n--- First generated Route (" + Settings.SamplesCount + " Routes in total) --- " + watch.ElapsedMilliseconds + "ms\r\n";
            }

            // Calculate probability of PB
            int iFaster = 0, iSlower = 0, iCountMalus = 0;
            double fSecStart = 0, fSecMalus, fTotalBestTime = 0;

            // Total Best Time
            for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                fTotalBestTime += State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000;

            // Get current time as a baseline
            Time test = State.CurrentTime;

            //if (bCheckUpdate) iCurrentSplitIndex++; // background calculation, then calculate starting next split

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
                for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                {
                    if (splits[iSegment].Count == 0) // This split contains no split times, so we cannot calculate a probability
                    {
                        InternalComponent.InformationValue = "E3 no times found in S" + (1 + iSegment) + " " + State.Run[iSegment].Name; // + " split contains no times"
                        if (Settings.bDebug) System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);
                        return;
                    }
                
                    iMaxAtempts = 100000; // max tries to catch a valid time
                    do
                    {
                        iMaxAtempts--;
                        // select newer attempts more often
                        if (Settings.bExpSplitsvalue)
                            do // 0 Exp = X^0 (=0 linear = deactivated) 10 Exponential = X^0.5, 20 Exp = X^1 (=100 linear), 40 Exp = X^2, 60 Exp = X^3, 80 Exp = X^4, 100 Exp = X^5
                                iAttempt = rand.Next(splits[iSegment].Count) + 1;
                            while (Math.Pow(iAttempt/(splits[iSegment].Count * 1.0), Settings.iSplitsvalue * .05) < rand.Next(100) * .01); // exponential, this is slower
                        else
                            do
                                iAttempt = rand.Next(splits[iSegment].Count) + 1;
                            while (100-Settings.iSplitsvalue + (Settings.iSplitsvalue) * iAttempt / (splits[iSegment].Count * 1.0) < rand.Next(100)); // linear
                          //while (iAttempt / (splits[iSegment].Count * 1.0) < rand.Next(Settings.iSplitsvalue) * .01) ; // linear

                        iAttempt--;
                        split = splits[iSegment][iAttempt];

                        if (split == null) // split is a failure, add a malus
                        {
                            fSecMalus += Settings.MalusCount;
                            iCountMalus++;
                            if (Settings.bDebug && i == 0) sWriteDebug1 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Route: " + i + " Name: " + State.Run[iSegment].Name + " Failure " + iCountMalus + " add " + Settings.MalusCount + "s\r\n";
                        }
                    } while (split == null & iMaxAtempts > 0);

                    if (split != null) // found split times
                    {
                        if (split.Value[State.CurrentTimingMethod].HasValue)
                        { // add the time
                            fSec += split.Value[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000;
                            if (Settings.bDebug && i == 0) sWriteDebug1 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Route: " + i + " Duration: " + split.Value.RealTime.ToString() + " = " + (split.Value.RealTime.Value.TotalMilliseconds / 1000).ToString("000.000") + "ms" + " --- BestTime=" + State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds.ToString("000.000") + " Name: " + State.Run[iSegment].Name + "\r\n";
                        } else { // should never happen
                            if (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].HasValue) {
                                InternalComponent.InformationValue = "W4 no time found in A" + iAttempt + " S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                                if (Settings.bDebug) sWriteDebug1 += "Segment: " + iSegment.ToString("00") + " Atempt: " + iAttempt + " Name: " + State.Run[iSegment].Name + " (Warning 3: no time found)\r\n";
                                fSec += State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000;
                            } else {
                                InternalComponent.InformationValue = "E4 no (best) time found in A" + iAttempt + " S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                                return;
                            }
                        }
                    }
                    else
                    {
                        InternalComponent.InformationValue = "E5 too many failure attempts S" + (1 + iSegment) + " " + State.Run[iSegment].Name; // + " split contains no times"
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

            if (iCurrentSplitIndex == iMaxSplit) // no more remaining times, check for a new pb
                InternalComponent.InformationValue = (fSecStart < pb[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000) ? "100% PB" : (fSecStart == pb[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000) ? "50% PB" : "0%";
            else // display results
            {
                double fProb = iFaster / (iFaster + iSlower + 0.0);
                if (bCheckUpdate) { // background calculation
                    fLiveChanceAvg = (fLiveChanceAvg > 0) ? (fLiveChanceAvg + fProb) * .5 : fProb;

                    if (fProb > fLiveChanceAvg || fLiveChanceAvg > fLiveChanceAct) // do not display, if chance is higher than the displaying chance //&& !(fProb==0 && fLiveChanceAvg==0 && fLiveChanceAct>0)
                        return;

                    if (fProb > 0)
                        fProb = (fProb > .1 && fProb < .9) ? Math.Round(fLiveChanceAvg, 3) : fLiveChanceAvg; // round a place behind the comma, in the range of 10%-90%
                }
                fLiveChanceAct = fProb;

                // Zero success, display 0% instead of 0.00%. 0.00% will be displayed if count of samples are >> 10,000 and only a few successes
                text += (fProb == 0) ? "0%" : (fProb * 100).ToString("0.00") + "%";

                if (Settings.DisplayOdds && fProb > 0) // Displaying odds
                    text += " (1 in " + Math.Round(1 / fProb, 1).ToString() + ")";

                if (Settings.bSurvival && iCurrentSplitIndex < iMaxSplit) // Calculate survival chance
                {
                    text += " / " + Math.Round(iSurvivalSuccess / (iSurvivalSuccess + iSurvivalFailure + .0) * 100, 0).ToString() + "%"; // Chance to finish the run
                        //+ (iCurrentSplitIndex+1 < iMaxSplit ? "|" + lCountSplits[iCurrentSplitIndex + 1] + "-" + splits[iCurrentSplitIndex + 1].Count + "-" + Math.Round(lCountSplits[iCurrentSplitIndex+1] / (splits[iCurrentSplitIndex+1].Count + .0) * 100, 0).ToString() :"") + ")%"; // next Split Chance
                    if (InternalComponent.InformationName != "PB / Survival Chance") InternalComponent.InformationName = "PB / Survival Chance";
                } else if (InternalComponent.InformationName != "PB Chance") InternalComponent.InformationName = "PB Chance";

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
