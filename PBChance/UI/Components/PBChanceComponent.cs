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
        protected bool bRndInformationOn;
        protected bool bRndInformationActive;
        protected string sInformationName;
        protected string sInformationValue;
        //int[] lAliveSplits;

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

        double RoundExtended(double d, int iDecimals)
        {
            if (iDecimals>=0)
                return Math.Round(d, iDecimals);
            else
                return Math.Round(d * Math.Pow(10, iDecimals)) * Math.Pow(10, -iDecimals);
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
            if (!Settings.bIgnoreSkipClip)
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
            bRndInformationOn = false;
            fLiveChanceAvg = 0;
            thread = new Thread(new ThreadStart(Recalculate));
            thread.Start();
        }

        protected void Recalculate()
        {
            int iSurvivalFailure = 0, iSurvivalSuccess = 0, iNextSurvSuc = 0, iNextSurvFail = 0, iMaxAtempts, iCurrentSplitIndex, iMaxSplit, i, iAttempt, iSegment; //, aktSurvival, sumSurvival = 0;
            double fSec, fDeviation = 0, iSumMsActSplit = 0, iSurvToHereAttempt, iSurvToHereCount = 0;
            string text = ""; var watch = System.Diagnostics.Stopwatch.StartNew();
            System.String sWriteDebug1 = "", sWriteDebug2 = "", sWriteDebug3 = "";
            Time? split;

            //if (Settings.SamplesCount == 0) // not configured, load default values
            //{
            //    Settings.UsePercentOfAttempts = false;
            //    Settings.UseFixedAttempts = true;
            //    Settings.IgnoreRunCount = false;
            //    Settings.AttemptCount = 50;
            //    Settings.MalusCount = 30;
            //    Settings.SplitclipCount = 160;
            //    Settings.TimediffCount = 0;
            //    Settings.SamplesCount = 100000;
            //    Settings.iCalctime = 500;
            //    Settings.iMinTimes = 20;
            //    Settings.iUpdate = 1;
            //    Settings.iSplitsvalue = 100;
            //    Settings.iRndInfoEvery = 0;
            //    Settings.iRndInfoFor = 10;
            //}
            if (Settings.sVersion != "1.3.9")
                Settings.iRndInfoEvery = 0;

            iCurrentSplitIndex = (State.CurrentSplitIndex < 0 ? 0 : State.CurrentSplitIndex) + (bCheckUpdate ? 1 : 0);
            iMaxSplit = (Settings.iCalcToSplit == 0 || Settings.iCalcToSplit > State.Run.Count) ? State.Run.Count : Settings.iCalcToSplit;
            //if (iCurrentSplitIndex > iMaxSplit) return; // Last split do no check for an update

            // Array of the count of valid split times per split (without failing attemps)
            int[] lCountSplits = new int[iMaxSplit + 1];

            // Get the current Personal Best, if it exists
            Time pb = State.Run.Last().PersonalBestSplitTime;

            if (pb[State.CurrentTimingMethod] == TimeSpan.Zero || !pb[State.CurrentTimingMethod].HasValue)
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
                iFirstAttempt = iLastAttempt - (Settings.UseFixedAttempts ? Settings.AttemptCount - 1 : iLastAttempt * (Settings.AttemptCount - 1 - Settings.iSkipNewest) / 100);
            if (iFirstAttempt < 1) iFirstAttempt = 1;
            iSurvToHereAttempt = iLastAttempt;

            if (Settings.bDebug)
                System.IO.File.WriteAllText(@"pbchance_Debug.txt", "AdjustedStartTime: " + State.AdjustedStartTime + " AttemptEnded: " + State.AttemptEnded + " AttemptStarted: " + State.AttemptStarted + " \r\nCurrentAttemptDuration: " + State.CurrentAttemptDuration + " CurrentComparison: " + State.CurrentComparison + " CurrentPhase: " + State.CurrentPhase + "\r\n" //" CurrentHotkeyProfile: " + State.CurrentHotkeyProfile.ToString()
                    + "CurrentSplit: " + State.CurrentSplit + " CurrentSplitIndex: " + State.CurrentSplitIndex + " CurrentTime: " + State.CurrentTime
                    + "\r\nCurrentTimingMethod: " + State.CurrentTimingMethod + " GameTimePauseTime: " + State.GameTimePauseTime + " IsGameTimeInitialized: " + State.IsGameTimeInitialized
                    + "\r\nIsGameTimePaused: " + State.IsGameTimePaused + " Layout: " + State.Layout + " LayoutSettings: " + State.LayoutSettings
                    + "\r\nLoadingTimes: " + State.LoadingTimes + " PauseTime: " + State.PauseTime + " Run: " + State.Run + " Settings: " + State.Settings
                    + "\r\nStartTime: " + State.StartTime + " StartTimeWithOffset: " + State.StartTimeWithOffset + " TimePausedAt: " + State.TimePausedAt
                    + "\r\nAttempts: " + iFirstAttempt + " to " + iLastAttempt + " Malus: " + Settings.MalusCount + " Splitclip: " + Settings.SplitclipCount + " Timediff: " + Settings.TimediffCount + " Samples: " + Settings.SamplesCount + " Calctime: " + Settings.iCalctime + " Survival: " + Settings.bSurvival + " Rebalancing: " + Settings.iOptimistic
                    + "\r\nValueRuns: " + Settings.bValueRuns + " Min Times per Segment: " + Settings.iMinTimes + " Automatic Update every: " + Settings.iUpdate + "s More Value on newer Splits: " + Settings.iSplitsvalue + " Skip newest Splits: " + Settings.iSkipNewest
                    + "\r\nPersonal Best Time to beat: " + pb[State.CurrentTimingMethod].ToString() + " CalcToSplit: " + Settings.iCalcToSplit
                    + "\r\nConsider also fails: " + Settings.bConsiderFails + " Random Information every: " + Settings.iRndInfoEvery + " Random Information for: " + Settings.iRndInfoFor + "\r\n\r\n--- Failed Runs --- " + watch.ElapsedMilliseconds + "ms\r\n");

            //if (Settings.bDebug) // alle Daten in Datei schreiben
            //{
            //    sWriteDebug1 = "Segment Attempt Time\r\n";
            //    for (iAttempt = iFirstAttempt; iAttempt <= iLastAttempt; iAttempt++)
            //        for (iSegment = 0; iSegment < iMaxSplit; iSegment++)
            //        {
            //            if (State.Run[iSegment].SegmentHistory.ContainsKey(iAttempt) && State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod] > TimeSpan.Zero)
            //                sWriteDebug1 += iSegment + " " + iAttempt + " " + State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod].Value.TotalMilliseconds + "\r\n";
            //        }
            //    System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);
            //    sWriteDebug1 = "";
            //}

            // Gather split times
            int iFailState; // 0 = init, 1=Success, 2=Fail next Split, 3=Fail later Split
            for (iAttempt = iLastAttempt; iAttempt >= 0; iAttempt--)
            {
                iFailState = 0;

                for (iSegment = iMaxSplit - 1; iSegment >= 0; iSegment--)
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
                                if (iAttempt >= iFirstAttempt || lCountSplits[iSegment] < Settings.iMinTimes)
                                {
                                    splits[iSegment].Add(State.Run[iSegment].SegmentHistory[iAttempt]); // add a valid split time
                                    lCountSplits[iSegment]++;
                                    if (iSegment == iCurrentSplitIndex - 1) { iSurvToHereCount++; if (iSurvToHereAttempt > iAttempt) iSurvToHereAttempt = iAttempt; }
                                    if (Settings.bDebug && iAttempt < iFirstAttempt) sWriteDebug3 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Added Time: " + State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod].Value.ToString() + " Factor: " + Math.Round(State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod].Value.TotalSeconds / (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds + .0001), 2).ToString("0.00") + " Count of Valid/Total Times: " + lCountSplits[iSegment].ToString("0000") + "/" + splits[iSegment].Count.ToString("0000") + " Name: " + State.Run[iSegment].Name + "\r\n";
                                }

                                if (iSegment == iMaxSplit - 1)
                                    iFailState = 1; // Run is finished
                                else if (iFailState == 0) // Run didn't finish, add a failure for the last known split
                                {
                                    iFailState = (iSegment == iCurrentSplitIndex - 1) ? 2 : (iSegment > iCurrentSplitIndex - 1) ? 3 : 4; // Failure is set
                                    if (iAttempt >= iFirstAttempt || (lCountSplits[iSegment + 1] < Settings.iMinTimes && Settings.bConsiderFails))
                                    {
                                        splits[iSegment + 1].Add(null);
                                        //if (Settings.bDebug) sWriteDebug1 += "#" + iSurvivalFailure.ToString("00") + ":  Attempt: " + iAttempt.ToString("00") + " Segment: " + (iSegment + 1).ToString("00") + " LastSegment: " + iSegment.ToString("00") + " CountSplit: " + lCountSplits[iSegment].ToString("00") + "\r\n";
                                    }
                                }
                            }
                            else
                                //{ if (iAttempt >= iFirstAttempt || lCountSplits[iSegment] < Settings.iMinTimes) if (iSegment == iCurrentSplitIndex - 1) { iSurvToHereCount++; if (iSurvToHereAttempt > iAttempt) iSurvToHereAttempt = iAttempt; }
                                if (Settings.bDebug && (iAttempt >= iFirstAttempt || lCountSplits[iSegment] < Settings.iMinTimes)) sWriteDebug2 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Factor: " + Math.Round(State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod].Value.TotalSeconds / State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds, 2).ToString("0.00") + " Best: " + State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds.ToString("000.000") + " Time: " + State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod].Value.TotalSeconds.ToString("0000.000") + " Name: " + State.Run[iSegment].Name + "\r\n";
                        }
                        else // no Best Time, then allways add
                        {
                            if (iAttempt >= iFirstAttempt || lCountSplits[iSegment] < Settings.iMinTimes)
                            {
                                splits[iSegment].Add(State.Run[iSegment].SegmentHistory[iAttempt]);
                                InternalComponent.InformationValue = "W2 no best time found in S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                                lCountSplits[iSegment]++;
                                if (iSegment == iCurrentSplitIndex - 1) { iSurvToHereCount++; if (iSurvToHereAttempt > iAttempt) iSurvToHereAttempt = iAttempt; }
                                if (Settings.bDebug) sWriteDebug2 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Time: " + State.Run[iSegment].SegmentHistory[iAttempt][State.CurrentTimingMethod].Value.TotalSeconds.ToString("0000.000") + " Name: " + State.Run[iSegment].Name + " Warning 2: No best Time found\r\n";
                            }

                            if (iSegment == iMaxSplit - 1)
                                iFailState = 1; // Run is finished
                            else if (iFailState == 0) // Run didn't finish, add a failure for the last known split
                            {
                                iFailState = (iSegment == iCurrentSplitIndex - 1) ? 2 : (iSegment > iCurrentSplitIndex - 1) ? 3 : 4; // Failure is set
                                if (iAttempt >= iFirstAttempt || lCountSplits[iSegment + 1] < Settings.iMinTimes)
                                {
                                    splits[iSegment + 1].Add(null);
                                    if (Settings.bDebug) sWriteDebug1 += "#" + iSurvivalFailure.ToString("00") + ":  Attempt: " + iAttempt.ToString("00") + " Segment: " + (iSegment + 1).ToString("00") + " LastSegment: " + iSegment.ToString("00") + "\r\n";
                                }
                            }
                        }
                    }
                }

                if (iAttempt >= iFirstAttempt || lCountSplits[iCurrentSplitIndex] < Settings.iMinTimes)
                {
                    if (iFailState == 0 && iCurrentSplitIndex == 0) // Failure on first Split
                    {
                        splits[iCurrentSplitIndex].Add(null);
                        if (Settings.bDebug) sWriteDebug1 += "#" + iSurvivalFailure.ToString("00") + ":  Attempt: " + iAttempt.ToString("00") + " Segment: 00 LastSegment: -1\r\n";

                        if (iAttempt >= iFirstAttempt || (iNextSurvSuc < Settings.iMinTimes))
                        {
                            iNextSurvFail++;
                            iSurvivalFailure++;
                        }
                    }
                }
                if (iAttempt >= iFirstAttempt || (iNextSurvSuc < Settings.iMinTimes))//+++++++++++++++++++++++
                {
                    switch (iFailState)
                    {
                        case 1: // Run is finished
                            iNextSurvSuc++;
                            break;
                        case 2: // Failure on next Split
                            iNextSurvFail++;
                            break;
                        case 3: // Failure is in Future
                            iNextSurvSuc++;
                            break;
                        case 4: // Failure is in Past
                        default:
                            break;
                    }
                }
                if (iAttempt >= iFirstAttempt || (iSurvivalSuccess < Settings.iMinTimes))//+++++++++++++++++++++++
                {
                    switch (iFailState)
                    {
                        case 1: // Run is finished                            //iNextSurvSuc++;
                            iSurvivalSuccess++;
                            break;
                        case 2: // Failure on next Split
                            iSurvivalFailure++;
                            break;
                        case 3: // Failure is in Future
                            iSurvivalFailure++;
                            break;
                        case 4: // Failure is in Past
                        default:
                            break;
                    }
                }/*
                if (iAttempt >= iFirstAttempt)//|| Settings.bConsiderFails)//+++++++++++++++++++++++
                {
                    switch (iFailState)
                    {
                        case 1: // Run is finished
                            iNextSurvSuc++;
                            iSurvivalSuccess++;
                            break;
                        case 2: // Failure on next Split
                            iNextSurvFail++;
                            iSurvivalFailure++;
                            break;
                        case 3: // Failure is in Future
                            iNextSurvSuc++;
                            iSurvivalFailure++;
                            break;
                        case 4: // Failure is in Past
                        default:
                            break;
                    }
                }*/
            }

            if (Settings.iAddBest > 0)
                for (iSegment = 0; iSegment < iMaxSplit; iSegment++)
                    if (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                        splits[iSegment].Insert(splits[iSegment].Count * (100 - Settings.iAddBest) / 99, State.Run[iSegment].BestSegmentTime);

            if (Settings.bDebug)
            {
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1 + "\r\n--- Clipping segments --- " + watch.ElapsedMilliseconds + "ms\r\n" + sWriteDebug2);
                sWriteDebug1 = "\r\n--- Added times --- " + watch.ElapsedMilliseconds + "ms\r\n" + sWriteDebug3;
            }

            if (KeepAlive == false) return; // cancel calculation if new thread is requested

            if (Settings.bDebug) // Count of Times per Segment
            {
                double fNumberOfCombinations = 1;
                //System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug3);
                sWriteDebug1 += "\r\n--- Count of Times per Segment --- " + watch.ElapsedMilliseconds + "ms\r\n";
                for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                    sWriteDebug1 += "Segment: " + iSegment.ToString("00") + " Count of Valid/Total Times: " + lCountSplits[iSegment].ToString("0000") + "/" + splits[iSegment].Count.ToString("0000") + " Name: " + State.Run[iSegment].Name + "\r\n";
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);

                //Write all Segment Times
                sWriteDebug2 = "\r\n\r\n\r\n--- Detailed Segment Times ---\r\n";
                for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                {
                    sWriteDebug2 += " - " + State.Run[iSegment].Name + " -\r\n";
                    fNumberOfCombinations *= splits[iSegment].Count;
                    for (iAttempt = 0; iAttempt < splits[iSegment].Count; iAttempt++)
                    {
                        sWriteDebug2 += "Segment: " + iSegment + " Attempt:" + iAttempt.ToString("000") + " Chance: ";
                        sWriteDebug2 += ((100 - Settings.iSplitsvalue + Settings.iSplitsvalue * (splits[iSegment].Count - iAttempt) / (splits[iSegment].Count + 1.0)) / splits[iSegment].Count / (100 - Settings.iSplitsvalue * .5) * 100.0).ToString("00.0") + "% "; // linear
                        //sWriteDebug2 += (100 * (iAttempt + 1) / summ(splits[iSegment].Count)).ToString("00.0") + "% ";
                        if (splits[iSegment][iAttempt].HasValue)
                            sWriteDebug2 += "Time:" + splits[iSegment][iAttempt].Value[State.CurrentTimingMethod].Value + "/" + splits[iSegment][iAttempt].Value[State.CurrentTimingMethod].Value.TotalMilliseconds + " Factor: " + Math.Round(splits[iSegment][iAttempt].Value[State.CurrentTimingMethod].Value.TotalSeconds / (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalSeconds + .0001), 2).ToString("0.00\r\n");
                        else
                            sWriteDebug2 += "Failed\r\n";
                        //while (iAttempt / (splits[iSegment].Count * 1.0) < rand.Next(Settings.iSplitsvalue) * .01) ;
                    }
                }
                sWriteDebug2 += "\r\nNumber of combinations = " + fNumberOfCombinations + " = " + fNumberOfCombinations.ToString($"F{0}");
                sWriteDebug1 = "\r\n--- First generated Route (" + Settings.SamplesCount + " Routes in total) --- " + watch.ElapsedMilliseconds + "ms\r\n";
            }

            double fAvgTime = 0; // Calc next average Split Time
            if (Settings.bInfoNext || bRndInformationActive)
                if (iCurrentSplitIndex < iMaxSplit)
                {
                    i = 0;
                    for (iAttempt = 0; iAttempt < splits[iCurrentSplitIndex].Count; iAttempt++)
                        if (splits[iCurrentSplitIndex][iAttempt].HasValue)
                        {
                            i++;
                            fAvgTime += splits[iCurrentSplitIndex][iAttempt].Value[State.CurrentTimingMethod].Value.TotalMilliseconds;
                        }
                    fAvgTime = fAvgTime / i;
                }

            if ((Settings.bDeviation || bRndInformationActive) && iCurrentSplitIndex < iMaxSplit)
            {
                double fMean;
                if (Settings.bDebug) sWriteDebug2 += "\r\n\r\n--- Standard Deviation ---\r\n";
                for (iSegment = Settings.bDebug ? iMaxSplit - 1 : iCurrentSplitIndex; iSegment >= iCurrentSplitIndex; iSegment--)
                //for (iSegment = iCurrentSplitIndex; iSegment >= iCurrentSplitIndex; iSegment--)
                {
                    i = 0;
                    fDeviation = 0;
                    iSumMsActSplit = 0;
                    for (iAttempt = 0; iAttempt < splits[iSegment].Count; iAttempt++)
                        if (splits[iSegment][iAttempt].HasValue)
                        {
                            i++;
                            iSumMsActSplit += splits[iSegment][iAttempt].Value[State.CurrentTimingMethod].Value.TotalMilliseconds;
                            //if(Settings.bDebug) System.IO.File.AppendAllText(@"pbchance_Debug.txt", "+"+ splits[iSegment][iAttempt].Value[State.CurrentTimingMethod].Value.TotalMilliseconds+",");
                        }
                    fMean = iSumMsActSplit / i;
                    //if (Settings.bDebug) System.IO.File.AppendAllText(@"pbchance_Debug.txt", " MEAN=" + fMean + " iSumMs=" + iSumMsActSplit + " /i= "+ i);
                    for (iAttempt = 0; iAttempt < splits[iSegment].Count; iAttempt++)
                    {
                        if (splits[iSegment][iAttempt].HasValue)
                            fDeviation += (splits[iSegment][iAttempt].Value[State.CurrentTimingMethod].Value.TotalMilliseconds - fMean)
                                        * (splits[iSegment][iAttempt].Value[State.CurrentTimingMethod].Value.TotalMilliseconds - fMean);
                        //if (Settings.bDebug) System.IO.File.AppendAllText(@"pbchance_Debug.txt", " fDeviation=" + fDeviation);
                    }
                    fDeviation = Math.Pow(fDeviation / i, .5) * 0.001; // 0.5 Deviation / 1.0 Variation / Version 2 = fDeviation / (i-1)
                                                                       //if (Settings.bDebug) System.IO.File.AppendAllText(@"pbchance_Debug.txt", " Wurzel fDeviation /1000 =" + fDeviation);
                    if (Settings.bDebug) sWriteDebug2 += "Segment: " + iSegment.ToString("00") + " Deviation: " + fDeviation.ToString("0.00") + "\r\n";
                }
            }

            // Calculate probability of PB
            int iFaster = 0, iSlower = 0, iCountMalus = 0;
            double fSecStart = 0, fSecMalus, fTotalBestTime = 0;

            // Total Best Time
            for (iSegment = State.CurrentSplitIndex +1; iSegment < iMaxSplit; iSegment++)
                fTotalBestTime += State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000;

            // Get current time as a baseline
            Time test = State.CurrentTime;

            if (State.CurrentSplitIndex > 0)
            {
                if (test.RealTime == null) test = State.CurrentTime;
                test = State.Run[iCurrentSplitIndex - 1].SplitTime;
            }
            else
                test[State.CurrentTimingMethod] = TimeSpan.Zero;

            if (test[State.CurrentTimingMethod].HasValue && !bCheckUpdate) // split hasn't skipped
                fSecStart = test[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000;
            else
            {
                if (Settings.bIgnoreSkipClip && !bCheckUpdate)
                    return;
                else // use the actual time, possible to read the skipping time?
                    fSecStart = State.CurrentTime[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000;
            }

            //if (fTotalBestTime * 1000 + State.CurrentAttemptDuration.TotalMilliseconds <= pb[State.CurrentTimingMethod].Value.TotalMilliseconds + Settings.TimediffCount * 1000)  // PB possible
            //    //InternalComponent.InformationValue = fTotalBestTime * 1000 + State.CurrentAttemptDuration.TotalMilliseconds + "possible";
            //else
            //{
            //    InternalComponent.InformationValue = fTotalBestTime * 1000 + State.CurrentAttemptDuration.TotalMilliseconds + "not possible";  // Math.Round(fTotalBestTime + State.CurrentAttemptDuration.TotalMilliseconds / 1000) + "s / " + Math.Round((fTotalBestTime + State.CurrentAttemptDuration.TotalMilliseconds / 1000) / 60, 2) + "min";
            //    return;
            //}
            int k, iCheckDistance = Settings.SamplesCount > 1000 ? 1000 : Settings.SamplesCount;

            if (fTotalBestTime * 1000 + State.CurrentAttemptDuration.TotalMilliseconds <= pb[State.CurrentTimingMethod].Value.TotalMilliseconds + Settings.TimediffCount * 1000) // PB possible
                for (i = 0; (i < Settings.SamplesCount * (iFaster == 0 ? 10 : 1) || watch.ElapsedMilliseconds < Settings.iCalctime) && (fTotalBestTime * 1000 + State.CurrentAttemptDuration.TotalMilliseconds <= pb[State.CurrentTimingMethod].Value.TotalMilliseconds + Settings.TimediffCount * 1000); i=i+iCheckDistance)
                    for(k = 0; k < iCheckDistance; k++)
            {
                if (KeepAlive == false) return; // cancel calculation if new thread is requested
                fSecMalus = 0;
                fSec = 0;// *window.perfomance.now();
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
                                iAttempt = rand.Next(splits[iSegment].Count);
                            while (Math.Pow((splits[iSegment].Count - iAttempt) / (splits[iSegment].Count * 1.0), Settings.iSplitsvalue * .05) < rand.Next(100) * .01); // exponential, this is slower
                        else
                            do
                                iAttempt = rand.Next(splits[iSegment].Count);
                            while (100 - Settings.iSplitsvalue + (Settings.iSplitsvalue) * (splits[iSegment].Count - iAttempt) / (splits[iSegment].Count * 1.0) < rand.Next(100)); // linear
                                                                                                                                                                                   //while (iAttempt / (splits[iSegment].Count * 1.0) < rand.Next(Settings.iSplitsvalue) * .01) ; // linear
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
                        }
                        else
                        { // should never happen
                            if (State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                            {
                                InternalComponent.InformationValue = "W4 no time found in A" + iAttempt + " S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                                if (Settings.bDebug) sWriteDebug1 += "Segment: " + iSegment.ToString("00") + " Atempt: " + iAttempt + " Name: " + State.Run[iSegment].Name + " (Warning 3: no time found)\r\n";
                                fSec += State.Run[iSegment].BestSegmentTime[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000;
                            }
                            else
                            {
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

                if (Settings.bDebug && i == 0) System.IO.File.AppendAllText(@"pbchance_debug.txt", sWriteDebug1);
                if (Settings.bDebug && i == 0) sWriteDebug1 = "\r\n--- Results of 10 successfully runs and 10 failures if possible --- " + watch.ElapsedMilliseconds + "ms\r\n";

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
            else
            {
                iFaster = 0;
                iSlower = Settings.SamplesCount;
                //InternalComponent.InformationValue = fTotalBestTime * 1000 + State.CurrentAttemptDuration.TotalMilliseconds + "not possible";  // Math.Round(fTotalBestTime + State.CurrentAttemptDuration.TotalMilliseconds / 1000) + "s / " + Math.Round((fTotalBestTime + State.CurrentAttemptDuration.TotalMilliseconds / 1000) / 60, 2) + "min";
                //return;
            }

            if (iCurrentSplitIndex == iMaxSplit) // no more remaining times, check for a new pb
                InternalComponent.InformationValue = (fSecStart < pb[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000) ? bCheckUpdate ? InternalComponent.InformationValue : "100% PB" : (fSecStart == pb[State.CurrentTimingMethod].Value.TotalMilliseconds / 1000) ? "50% PB" : "0%";
            else // display results
            {
                double fProb = iFaster / (iFaster + iSlower + 0.0);
                
                if (bRndInformationActive)
                {// display a random information
                    Random random = new Random();
                    int iRandom;
                    //InternalComponent.InformationValue = "";
                    TryAgain:
                    {
                        iRandom = random.Next(1, 9);
                        //InternalComponent.InformationValue = InternalComponent.InformationValue + iRandom+ iCurrentSplitIndex;
                    } //while((iCurrentSplitIndex == 0 && iRandom == 7))// || (iFaster == 0 && iRandom == 8))

                    switch (iRandom)
                    {
                        case 1:
                            double fNumberOfCombinations = 1;
                            for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                                fNumberOfCombinations *= splits[iSegment].Count;
                            InternalComponent.InformationName = "Remaining Combinations";
                            InternalComponent.InformationValue = fNumberOfCombinations.ToString("E1");
                            break;
                        case 2:
                            InternalComponent.InformationName = "Sample Size";
                            InternalComponent.InformationValue = iFaster + iSlower + "";
                            break;
                        case 3:
                            InternalComponent.InformationName = "Survival in this Segment";
                            InternalComponent.InformationValue = (iNextSurvSuc / (iNextSurvSuc + iNextSurvFail + 0.0)).ToString("0.00%");
                            break;
                        case 4:
                            InternalComponent.InformationName = "Survival to the End";
                            InternalComponent.InformationValue = (iSurvivalSuccess / (iSurvivalSuccess + iSurvivalFailure + 0.0)).ToString("0.00%");
                            break;
                        case 5:
                            InternalComponent.InformationName = "Standard Deviation";
                            InternalComponent.InformationValue = fDeviation.ToString("0.00");
                            break;
                        case 6:
                            InternalComponent.InformationName = "Average Difference to Best";
                            InternalComponent.InformationValue = (fAvgTime * .001 - State.Run[iCurrentSplitIndex].BestSegmentTime[State.CurrentTimingMethod].Value.TotalMilliseconds * .001).ToString("0.0s");
                            break;
                        case 7:
                            if (iCurrentSplitIndex == 0) goto TryAgain;
                            InternalComponent.InformationName = "Until here have Survived";
                            InternalComponent.InformationValue = iSurvToHereCount + " of " + (iLastAttempt - iSurvToHereAttempt + 1);
                            break;
                        case 8:
                            if (iFaster == 0) goto TryAgain;
                            InternalComponent.InformationName = "Combinations per Second";
                            InternalComponent.InformationValue = (1000 * (iFaster + iSlower) / watch.ElapsedMilliseconds).ToString("0");
                            break;
                    }
                    bRndInformationActive = false;
                    return;
                }

                if (Settings.bSurvival && iCurrentSplitIndex < iMaxSplit) // Calculate survival chance
                    InternalComponent.InformationName = "PB Chance (" + Math.Round(iNextSurvSuc / (iNextSurvSuc + iNextSurvFail + .0) * 100, 0).ToString() + "%/"
                        + Math.Round(iSurvivalSuccess / (iSurvivalSuccess + iSurvivalFailure + .0) * 100, 0).ToString() + "%)";
                else if (InternalComponent.InformationName != "PB Chance")
                    InternalComponent.InformationName = "PB Chance";
                if (Settings.bDeviation) // Displaying Deviation
                    InternalComponent.InformationName += " (" + Math.Round(fDeviation, 2).ToString() + ")";
                if (Settings.bInfoNext && iCurrentSplitIndex < iMaxSplit)
                    if (State.Run[iCurrentSplitIndex].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                        InternalComponent.InformationName += " (" + (int)(fAvgTime * .001 - State.Run[iCurrentSplitIndex].BestSegmentTime[State.CurrentTimingMethod].Value.TotalMilliseconds * .001) + "s " + (iNextSurvFail / (iNextSurvSuc + iNextSurvFail + .0) * 100).ToString("0") + "%)";

                if (bCheckUpdate)
                { // background calculation
                    fLiveChanceAvg = (fLiveChanceAvg > 0) ? (fLiveChanceAvg + fProb) * .5 : fProb;

                    if (fProb > fLiveChanceAvg || fLiveChanceAvg > fLiveChanceAct) // do not display, if chance is higher than the displaying chance //&& !(fProb==0 && fLiveChanceAvg==0 && fLiveChanceAct>0)
                        return;

                    if (fProb > 0)
                        fProb = (fLiveChanceAvg > .1 && fLiveChanceAvg < .9) ? Math.Round(fLiveChanceAvg, 3) : fLiveChanceAvg; // round a place behind the comma, in the range of 10%-90%
                }
                fLiveChanceAct = fProb;

                // Zero success, display 0% instead of 0.00%. If it's <0.01%, it will display the number of success runs and total runs
                text += Settings.DisplayOdds && fProb < .0000100055 && fProb > 0 ? "" : (fProb > 0 && fProb < .0001 && !bCheckUpdate && !Settings.DisplayOdds) ? iFaster + " in " + (iFaster + iSlower) : fProb == 0 ? "0%" : (fProb * 100).ToString("0.00") + "%";

                if (Settings.DisplayOdds && fProb > 0 && (fProb >= 0.00000001 || bCheckUpdate)) // Displaying odds
                    if (bCheckUpdate)
                        text = "1 in " + RoundExtended(1 / fProb, fProb > 0.0000002 ? fProb > 0.000002 ? fProb > 0.00002 ? fProb > 0.0002 ? fProb > 0.002 ? fProb > .02 ? fProb > .2 ? 3 : 1 : 0 : -1 : -2 : -3 : -4 : -5).ToString(fProb > .000100055 ? fProb > .00100055 ? fProb > .0100055 ? fProb > .100055 ? fProb > .91 ? fProb > .9999 ? "0      " : "0.00   " : "0.00   " : "0.0   " : "0    " : "0  " : "0") + (fProb > .00000100055 ? fProb > .0000100055 ? "" : "     0%" : "   0%") + (fProb >= .1 ? fProb == 1 ? "" : "" : "  ") + text;
                    //                text = "1 in " + RoundExtended(1 / fProb, fProb > 0.0000002 ? fProb > 0.000002 ? fProb > 0.00002 ? fProb > 0.0002 ? fProb > 0.002 ? fProb > .02 ? fProb > .2 ? 3 : 1 : 0 : -1 : -2 : -3 : -4 : -5).ToString(fProb > .0100055 ? fProb > .100055 ? fProb > .91 ? fProb == 1 ? "0 " : "0.00  " : "0.00" : "0.0" : "0") + (fProb > .000100055 ? fProb > .00100055 ? fProb > .0100055 ? fProb > .100055 ? fProb > .91 ? fProb == 1 ? "    " : "" : "  " : "  " : "   " : " " : "") + "" + (fProb >= .1 ? fProb == 1 ? "" : "" : "  ") + text; //.ToString(fProb > .0100055 ? fProb > .100055 ? fProb > .91 ? fProb == 1 ? "0" : "0.000" : "0.00" : "0.0" : "0") + " | " + text; 
                    else
                        text = "1 in " + (1 / fProb).ToString(fProb > .000100055 ? fProb > .00100055 ? fProb > .0100055 ? fProb > .100055 ? fProb > .91 ? fProb >.9999 ? "0      " : "0.00   " : "0.00   " : "0.0   " : "0    " : "0  " : "0") + (fProb > .00000100055 ? fProb > .0000100055 ? "" : "     0%" : "   0%") + (fProb>=.1? fProb==1 ? "" : "" : "  ") + text;

                /*
                if (Settings.bSurvival && iCurrentSplitIndex < iMaxSplit) // Calculate survival chance
                {
                    //text += " / " + Math.Round(iSurvivalSuccess / (iSurvivalSuccess + iSurvivalFailure + .0) * 100, 0).ToString() + "%"; // Chance to finish the run
                    text += " / " + iSurvivalSuccess + "*" + iSurvivalFailure + "*" + iNextSurvSuc + "*" + iNextSurvFail; // Chance to finish the run
                                                                                                                          //+ (iCurrentSplitIndex+1 < iMaxSplit ? "|" + lCountSplits[iCurrentSplitIndex + 1] + "-" + splits[iCurrentSplitIndex + 1].Count + "-" + Math.Round(lCountSplits[iCurrentSplitIndex+1] / (splits[iCurrentSplitIndex+1].Count + .0) * 100, 0).ToString() :"") + ")%"; // next Split Chance
                                                                                                                          //if (InternalComponent.InformationName != "PB / Survival Chance") InternalComponent.InformationName = "PB / Survival Chance";
                }*/
                if (Settings.bDebug)
                {
                    sWriteDebug1 += "\r\n\r\n" + InternalComponent.InformationName + ": " + text + " SuccessCount: " + iFaster + " FailCount: " + iSlower + " MalusCount: " + iCountMalus + "\r\n\r\nExecution time: " + watch.ElapsedMilliseconds + "ms";
                    System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1 + sWriteDebug2);
                    Settings.bDebug = false;
                }

                InternalComponent.InformationValue = text;
            }
            sInformationName = InternalComponent.InformationName;
            sInformationValue = InternalComponent.InformationValue;
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
                double fTimer = 0, fBestSegmentTime = 0, fSplitTime = 0;
                int iCurrentSplitIndex = State.CurrentSplitIndex - 1;
                fLastUpdate = State.CurrentTime[State.CurrentTimingMethod].Value.TotalMilliseconds;

                if (iCurrentSplitIndex > 0) // calculate the actual split time
                    if (State.Run[iCurrentSplitIndex].SplitTime[State.CurrentTimingMethod].HasValue)
                        //if (State.CurrentTime[State.CurrentTimingMethod].HasValue) // seems this is not necessary
                        if (iCurrentSplitIndex >= 0)
                        {
                            fSplitTime = State.Run[iCurrentSplitIndex].SplitTime[State.CurrentTimingMethod].Value.TotalMilliseconds;
                            fTimer = State.CurrentAttemptDuration.TotalMilliseconds - fSplitTime;
                        }
                        else fTimer = State.CurrentAttemptDuration.TotalMilliseconds;
                    else fTimer = 999999999999999999999999999999999999999999999999999999.9; // determination of split time isn't possible now
                else fTimer = State.CurrentAttemptDuration.TotalMilliseconds;

                // background calculation, if actual split time is slower than best split time, and the actual chance is > 0
                //if (iCurrentSplitIndex > -1)
                if (State.Run[iCurrentSplitIndex + 1].BestSegmentTime[State.CurrentTimingMethod].HasValue)
                {
                    if (thread == null || thread.ThreadState != ThreadState.Running) // do it if the thread isn't running
                    {
                        fBestSegmentTime = State.Run[iCurrentSplitIndex + 1].BestSegmentTime[State.CurrentTimingMethod].Value.TotalMilliseconds;
                        if (fTimer > fBestSegmentTime && fLiveChanceAct > 0)
                        {
                            KeepAlive = true;
                            bCheckUpdate = true; // tell Recalculate(), that's a background calculation
                            bRndInformationOn = false;
                            thread = new Thread(new ThreadStart(Recalculate));
                            thread.Start();
                        }
                        else if (Settings.iRndInfoEvery>0)
                        {
                            if (fTimer > 10000 && (fTimer < fBestSegmentTime - 11000 || fLiveChanceAct == 0) && !bRndInformationOn && 
                                (fLastUpdate / 1000 - Settings.iRndInfoFor + Settings.iRndInfoEvery / 2) % Settings.iRndInfoEvery > Settings.iRndInfoEvery - Settings.iRndInfoFor)
                            { // display a random information
                                if (InternalComponent.InformationName.Remove(2, InternalComponent.InformationName.Length - 2) == "PB")
                                {
                                    bRndInformationOn = true;
                                    KeepAlive = true;
                                    bRndInformationActive = true;
                                    thread = new Thread(new ThreadStart(Recalculate));
                                    thread.Start();
                                }
                                else
                                {
                                    sInformationName = InternalComponent.InformationName;
                                    sInformationValue = InternalComponent.InformationValue;
                                    StartRecalculate(true);
                                }
                            }
                            else if (bRndInformationOn && ((fLastUpdate / 1000 - Settings.iRndInfoEvery / 2) % Settings.iRndInfoEvery > Settings.iRndInfoFor
                                || (fTimer > fBestSegmentTime - 10000 && fLiveChanceAct > 0)))
                            {
                                InternalComponent.InformationName = sInformationName;
                                InternalComponent.InformationValue = sInformationValue;
                                bRndInformationOn = false;
                            }
                        }
                        else
                        {
                            if (fTimer > fBestSegmentTime / 2 - 1000 * Settings.iRndInfoFor / 2 && fTimer > 10000 && fTimer < fBestSegmentTime - 10000 &&
                                fTimer < fBestSegmentTime / 2 + 1000 * Settings.iRndInfoFor / 2 && !bRndInformationOn)
                            { // display a random information
                                if (InternalComponent.InformationName.Remove(2, InternalComponent.InformationName.Length - 2) == "PB")
                                {
                                    bRndInformationOn = true;
                                    KeepAlive = true;
                                    bRndInformationActive = true;
                                    thread = new Thread(new ThreadStart(Recalculate));
                                    thread.Start();
                                }
                                else
                                {
                                    sInformationName = InternalComponent.InformationName;
                                    sInformationValue = InternalComponent.InformationValue;
                                    StartRecalculate(true);
                                }
                            }
                            else if ((fTimer > fBestSegmentTime / 2 + 1000 * Settings.iRndInfoFor / 2 || fTimer > fBestSegmentTime - 10000) && bRndInformationOn)
                            {
                                InternalComponent.InformationName = sInformationName;
                                InternalComponent.InformationValue = sInformationValue;
                                bRndInformationOn = false;
                            }
                        }
                    }
                }
            }

            InternalComponent.Update(invalidator, state, width, height, mode);
        }

        void IDisposable.Dispose()
        {

        }
    }
}
