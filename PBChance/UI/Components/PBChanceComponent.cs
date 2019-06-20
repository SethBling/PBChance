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
//using System.Diagnostics;

namespace PBChance.UI.Components
{
    class PBChanceComponent : IComponent
    {
        protected InfoTextComponent InternalComponent { get; set; }
        protected PBChanceSettings Settings { get; set; }
        protected LiveSplitState State;
        protected Random rand;
        protected string category;
        protected Thread thread;//, thread1, thread2;
        protected bool KeepAlive;//, thread1running;
        protected double fLiveChanceAct;
        protected double fLiveChanceAvg;
        protected double fLastUpdate;
        protected bool bCheckUpdate;
        protected bool bRndInformationOn;
        protected bool bRndInformationActive;
        protected double fNumberOfCombinations;
        protected double fDeviation;
        protected double fAvgTime;
        protected long iMinCalcTime;
        protected bool bCalcComplete;
        protected string sInformationName;
        protected string sInformationValue;
        private int iFaster, iSlower, iCountMalus, iFasterPace;
        //private List<Time?>[] splitsExt;// = new List<Time?>[1000];
        private double fSecStart, fProbStart, fTotalBestTime;
        private int iCurrentSplitIndex, iMaxSplit;
        //private static Semaphore _pool;// = new Semaphore(0, 10);
        //private Mutex mut = new Mutex();
        //int[] lAliveSplits;
        protected double[] lSumPBTimes;
        protected double[] lActPBTimes;
        protected double[] lWorstPBTimes;
        protected int[] lCountSplits;
        protected int[] lCountTimedSplits;
        protected int[] lCountSkippedSplits;
        protected int[] lOldestAttempt;
        protected List<Time?>[] splits;
        protected List<Time?>[] splitsExt;
        protected Time pb;
        protected int iFirstAttempt = 1, iLastAttempt, iSurvivalFailure, iSurvivalSuccess, iSurvivalFailurePast, iNextSurvSuc, iNextSurvFail ,
            iSurvToHereAttempt, iSurvToHereCount;
        protected TimingMethod tmTimingMethod;
        System.String sWriteDebug1 = "", sWriteDebug2 = "", sWriteDebug3 = "";
        System.Diagnostics.Stopwatch watch;

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
            if (iDecimals >= 0)
                return Math.Round(d, iDecimals);
            else
                return Math.Round(d * Math.Pow(10, iDecimals)) * Math.Pow(10, -iDecimals);
        }

        string getLargeNumber(double fIn)
        {
            string[] varA = new string[] { "Kilo", "Mega", "Giga", "Tera", "Peta", "Exa", "Zetta", "Yotta", "Oct", "Non" };
            string[] varB = new string[] { "", "Un", "Duo", "Tre", "Quattuor", "Quinqua", "Se", "Septen", "Octo", "Nonven" };
            string[] varC = new string[] { "", "Un", "Duo", "Tres", "Quattuor", "Quinqua", "Ses", "Septem", "Octo", "Nonvem" };
            string[] varD = new string[] { "", "Un", "Duo", "Tres", "Quattuor", "Quinqua", "Ses", "Septen", "Octo", "Nonven" };
            string[] varMeta = new string[] { "", "dec", "vigint", "trigint", "quadragint", "quinquagint", "sexagint", "septuagint", "octogint", "nonagint", "cent", "decicent", "viginticent", "trigintacent", "quadragintacent", "Quinquagintacent", "Sexagintacent", "Septuagintacent", "Octogintacent", "Nonagintacent", "Ducent" };

            long iStellen, i;
            string sZahl = "";
            double iNumber;

            iStellen = (long)Math.Log10(fIn);
            iNumber = Math.Round(fIn / Math.Pow(10, (iStellen - iStellen % 3)), iStellen % 3 == 0 ? 1 : 0);
            i = ((iStellen - 3) / 30);

            if (iStellen < 3)
                sZahl = iNumber.ToString();
            else if (iStellen < 27)
                sZahl = iNumber + " " + varA[(iStellen - 3) / 3] + varMeta[i];
            else if (iStellen < 33)
                sZahl = iNumber + " " + varA[(iStellen - 3) / 3] + varMeta[i] + "illion";
            else if (iStellen < 63)
                sZahl = iNumber + " " + varB[(iStellen - 33) / 3] + varMeta[i] + "illion";
            else if (iStellen < 93)
                sZahl = iNumber + " " + varC[(iStellen - 63) / 3] + varMeta[i] + "illion";
            else if (iStellen < 630)
                sZahl = iNumber + " " + varD[(iStellen - i * 30 - 3) / 3] + varMeta[i] + "illion";
            else
                sZahl = iNumber.ToString("E1");

            return sZahl + " E" + (iStellen / 3 * 3);
        }

        string secondsToTime(double fIn, int iPrecision, bool bAutoHours)
        {
            if (bAutoHours && fIn<60*60)
                if (iPrecision == 0)
                    return TimeSpan.FromSeconds(fIn).ToString(@"mm\:ss");
                else
                    return TimeSpan.FromSeconds(fIn).ToString(@"mm\:ss\" + (iPrecision > 0 ? "." + (iPrecision > 2 ? "fff" : iPrecision == 2 ? "ff" : "f") : ""));
            else
                if (iPrecision == 0)
                    return TimeSpan.FromSeconds(fIn).ToString(@"h\:mm\:ss");
                else
                    return TimeSpan.FromSeconds(fIn).ToString(@"h\:mm\:ss\" + (iPrecision > 0 ? "." + (iPrecision>2?"fff": iPrecision==2?"ff":"f"):""));
            
            //return (fIn >= 3600 ? ((int)(fIn / 3600)).ToString("00:") : "") + ((int)(fIn / 60) % 60).ToString("00:") + (fIn % 60).ToString("00") + 
            //    (iPrecision > 0 ? ":" + (((iPrecision==3?1000: iPrecision==2?100:10) * fIn) % (iPrecision == 3 ? 1000 : iPrecision == 2 ? 100 : 10)).ToString((iPrecision == 3 ? "000" : iPrecision == 2 ? "00" : "0")) : "");
        }

        public PBChanceComponent(LiveSplitState state)
        {
            State = state;
            InternalComponent = new InfoTextComponent("PB Chance", "Starting")
            {
                AlternateNameText = new string[]
                {
                    "PB Chance",
                    "PB Ch.",
                    "PB"
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

            iMinCalcTime = 1900;
            StartRecalculate(true);
        }

        private void OnRunManuallyModified(object sender, EventArgs e)
        {
            iMinCalcTime = (State.CurrentSplitIndex < 1) ? 1900 : Settings.iCalctime;
            StartRecalculate(true);
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            iMinCalcTime = Settings.iCalctime;
            StartRecalculate(true);
        }

        private void OnStart(object sender, EventArgs e)
        {
            iMinCalcTime = 1900;
            StartRecalculate(true);
        }

        protected void OnUndoSplit(object sender, EventArgs e)
        {
            iMinCalcTime = (State.CurrentSplitIndex < 1) ? 1900 : Settings.iCalctime;
            StartRecalculate(true);
        }

        protected void OnSkipSplit(object sender, EventArgs e)
        {
            iMinCalcTime = (State.CurrentSplitIndex < 1) ? 1900 : Settings.iCalctime;
            if (Settings.bSkipSplitStroke) { InternalComponent.InformationValue = "-"; return; }
            if (!Settings.bIgnoreSkipClip) StartRecalculate(true);
        }

        protected void OnReset(object sender, TimerPhase value)
        {
            iMinCalcTime = 1900;
            StartRecalculate(true);
        }

        protected void OnSplit(object sender, EventArgs e)
        {
            iMinCalcTime = Settings.iCalctime;
            StartRecalculate(true);
        }

        protected void StartRecalculate(bool bForceNewcalc)
        {
            if (thread != null)
                if (thread.ThreadState == System.Threading.ThreadState.Running)
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
            /*
            KeepAlive = true;
            bCheckUpdate = false;
            bRndInformationOn = false;
            Recalculate();*/
        }
        
        private void PrepareSplits()
        {
            int iLastKnownSplit, iSegment, iFailState;
            // 0 = init, 1=Success, 2=Fail next Split, 3=Fail later Split


            // Create the lists of split times
            //List<Time?>[] splits = new List<Time?>[iMaxSplit];
            //for (int i = 0; i < iMaxSplit; i++)
            //    splits[i] = new List<Time?>();

            //iLastAttempt = State.Run.AttemptHistory.Count - Settings.iSkipNewest;
            //if (!Settings.IgnoreRunCount)
            //    iFirstAttempt = iLastAttempt - (Settings.UseFixedAttempts ? Settings.AttemptCount - 1 : iLastAttempt * (Settings.AttemptCount - 1 - Settings.iSkipNewest) / 100);
            //if (iFirstAttempt < 1) iFirstAttempt = 1;
            //iSurvToHereAttempt = iLastAttempt;

            for (int iAttempt = iLastAttempt; iAttempt >= 0; iAttempt--)
            {
                iFailState = 0;
                iLastKnownSplit = 0;
                for (iSegment = iMaxSplit - 1; iSegment >= 0; iSegment--)
                {
                    if (State.Run[iSegment].SegmentHistory == null || State.Run[iSegment].SegmentHistory.Count == 0)
                    {
                        if (State.Run[iSegment].BestSegmentTime[tmTimingMethod].HasValue)
                        {
                            splits[iSegment].Add(State.Run[iSegment].BestSegmentTime); // no split times available, take the best split time, display a warning
                            InternalComponent.InformationValue = "W1 no times found in S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                            if (Settings.bDebug) sWriteDebug2 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Best: " + State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalSeconds.ToString("000.000") + " Time: " + State.Run[iSegment].SegmentHistory[iAttempt][tmTimingMethod].Value.TotalSeconds.ToString("0000.000") + " Name: " + State.Run[iSegment].Name + " (Warning 1: no historial times found)\r\n";
                            lCountSplits[iSegment]++;
                            lOldestAttempt[iSegment] = iAttempt;
                        }
                        else
                        {
                            InternalComponent.InformationValue = "E1 no (best) times found in S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                            System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1 + "\r\n--- Clipping segments --- " + watch.ElapsedMilliseconds + "ms\r\n" + sWriteDebug2);
                            return;
                        }
                    }
                    else if (State.Run[iSegment].SegmentHistory.ContainsKey(iAttempt) && State.Run[iSegment].SegmentHistory[iAttempt][tmTimingMethod] > TimeSpan.Zero)
                    {
                        if (State.Run[iSegment].BestSegmentTime[tmTimingMethod].HasValue)
                        {
                            if ((State.Run[iSegment].SegmentHistory[iAttempt][tmTimingMethod].Value.TotalSeconds <= Settings.SplitclipCount * 0.01 * State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalSeconds))  // | (State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalSeconds < 60))
                            {
                                if (iAttempt >= iFirstAttempt || lCountSplits[iSegment] < Settings.iMinTimes)
                                {
                                    splits[iSegment].Add(State.Run[iSegment].SegmentHistory[iAttempt]); // add a valid split time
                                    lCountSplits[iSegment]++;
                                    lOldestAttempt[iSegment] = iAttempt;

                                    while (iLastKnownSplit > iSegment + 1)
                                        lCountSkippedSplits[iLastKnownSplit--]++;
                                    lCountTimedSplits[iSegment]++;
                                    iLastKnownSplit = iSegment;


                                    if (iSegment == iCurrentSplitIndex - 1) { iSurvToHereCount++; if (iSurvToHereAttempt > iAttempt) iSurvToHereAttempt = iAttempt; }
                                    if (Settings.bDebug && iAttempt < iFirstAttempt) sWriteDebug3 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Added Time: " + State.Run[iSegment].SegmentHistory[iAttempt][tmTimingMethod].Value.ToString() + " Factor: " + Math.Round(State.Run[iSegment].SegmentHistory[iAttempt][tmTimingMethod].Value.TotalSeconds / (State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalSeconds + .0001), 2).ToString("0.00") + " Count of Valid/Total Times: " + lCountSplits[iSegment].ToString("0000") + "/" + splits[iSegment].Count.ToString("0000") + " Name: " + State.Run[iSegment].Name + "\r\n";

                                    //while (iLastSetSegment > 0 && iSegment < iLastSetSegment - 1)
                                    //{ lCountSkippedSplits[iLastSetSegment--]++;
                                    //    //sWriteDebug1 +="Atp" + iAttempt + " Segm " + iLastSetSegment + " skipped \n";
                                    //}
                                    //iLastSetSegment = iSegment;
                                }

                                if (iSegment == iMaxSplit - 1)
                                    iFailState = 1; // Run is finished
                                else if (iFailState == 0) // Run didn't finish, add a failure for the last known split
                                {
                                    iFailState = (iSegment == iCurrentSplitIndex - 1) ? 2 : (iSegment > iCurrentSplitIndex - 1) ? 3 : 4; // Failure is set
                                    if (iAttempt >= iFirstAttempt || (lCountSplits[iSegment + 1] < Settings.iMinTimes && Settings.bConsiderFails))
                                    {
                                        splits[iSegment + 1].Add(null);
                                        if (Settings.bDebug) sWriteDebug1 += "#" + iSurvivalFailure.ToString("00") + ":  Attempt: " + iAttempt.ToString("00") + " Segment: " + (iSegment + 1).ToString("00") + " LastSegment: " + iSegment.ToString("00") + " CountSplit: " + lCountSplits[iSegment].ToString("00") + "\r\n";
                                    }
                                }
                            }
                            else
                                //{ if (iAttempt >= iFirstAttempt || lCountSplits[iSegment] < Settings.iMinTimes) if (iSegment == iCurrentSplitIndex - 1) { iSurvToHereCount++; if (iSurvToHereAttempt > iAttempt) iSurvToHereAttempt = iAttempt; }
                                if (Settings.bDebug && (iAttempt >= iFirstAttempt || lCountSplits[iSegment] < Settings.iMinTimes)) sWriteDebug2 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Factor: " + Math.Round(State.Run[iSegment].SegmentHistory[iAttempt][tmTimingMethod].Value.TotalSeconds / State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalSeconds, 2).ToString("0.00") + " Best: " + State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalSeconds.ToString("000.000") + " Time: " + State.Run[iSegment].SegmentHistory[iAttempt][tmTimingMethod].Value.TotalSeconds.ToString("0000.000") + " Name: " + State.Run[iSegment].Name + "\r\n";
                        }
                        else // no Best Time, then allways add
                        {
                            if (iAttempt >= iFirstAttempt || lCountSplits[iSegment] < Settings.iMinTimes)
                            {
                                splits[iSegment].Add(State.Run[iSegment].SegmentHistory[iAttempt]);
                                InternalComponent.InformationValue = "W2 no best time found in S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                                lCountSplits[iSegment]++;
                                lOldestAttempt[iSegment] = iAttempt;
                                if (iSegment == iCurrentSplitIndex - 1) { iSurvToHereCount++; if (iSurvToHereAttempt > iAttempt) iSurvToHereAttempt = iAttempt; }
                                if (Settings.bDebug) sWriteDebug2 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Time: " + State.Run[iSegment].SegmentHistory[iAttempt][tmTimingMethod].Value.TotalSeconds.ToString("0000.000") + " Name: " + State.Run[iSegment].Name + " Warning 2: No best Time found\r\n";
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
                        case 1: // Run is finished
                            iSurvivalSuccess++;
                            break;
                        case 2: // Failure on next Split
                            iSurvivalFailure++;
                            break;
                        case 3: // Failure is in Future
                            iSurvivalFailure++;
                            break;
                        case 4: // Failure is in Past
                            iSurvivalFailurePast++;
                            break;
                        default:
                            break;
                    }
                }
            }

            if (Settings.iAddBest > 0)
                for (iSegment = 0; iSegment < iMaxSplit; iSegment++)
                    if (State.Run[iSegment].BestSegmentTime[tmTimingMethod].HasValue)
                        splits[iSegment].Insert(splits[iSegment].Count * (100 - Settings.iAddBest) / 99, State.Run[iSegment].BestSegmentTime);
        }

        private int CheckForPB()
        {
            int iAttempt, iCountActMalus, iMaxAtempts = 10000, iSegment, i, iMalusRange = 2 * Settings.MalusCount + 1;
            uint iPseudoRnd = 0;
            double fSec = 0, fSecMalus = 0, fToBeat, fToBeatGoal;
            Time? split;
            int k, iCheckDistance = Settings.SamplesCount > 1000 ? 1000 : Settings.SamplesCount;
            iFaster = 0; iSlower = 0; iFasterPace = 0;
            fToBeat = -fSecStart + pb[tmTimingMethod].Value.TotalMilliseconds / 1000 + Settings.TimediffCount;
            fToBeatGoal= fToBeat - Settings.iPaceExtraGoalMS / 1000.0;
            //mut.WaitOne();
            ////iFaster = 0; iSlower = 0;
            for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                 lSumPBTimes[iSegment] = 0;
            //mut.ReleaseMutex();

            //for(int iStep = 1; iStep <= (bRndInformationActive || bCheckUpdate ? 1 : 1001); iStep++)
            //{

            if (Settings.bDebug) sWriteDebug1 = "\r\n--- First generated Route (" + Settings.SamplesCount + " Routes in total) --- " + watch.ElapsedMilliseconds + "ms\r\n";

            if (fTotalBestTime * 1000 + State.CurrentAttemptDuration.TotalMilliseconds <= pb[tmTimingMethod].Value.TotalMilliseconds + Settings.TimediffCount * 1000) // PB possible
                for (i = 0; /*KeepAlive &&*/ (i < Settings.SamplesCount * (iFaster == 0 ? 10 : 1) || watch.ElapsedMilliseconds < iMinCalcTime) && 
                    (fTotalBestTime * 1000 + State.CurrentAttemptDuration.TotalMilliseconds <= pb[tmTimingMethod].Value.TotalMilliseconds + Settings.TimediffCount * 1000); i = i + iCheckDistance)
                    for (k = 0; k < iCheckDistance; k++)
                    {
                        if (KeepAlive == false) return 0; // cancel calculation if new thread is requested
                        fSecMalus = 0;
                        iCountActMalus = 0;
                        fSec = 0;
                        //Buffer.BlockCopy(lZeroPBTimes, iCurrentSplitIndex, lActPBTimes, iCurrentSplitIndex, iMaxSplit - iCurrentSplitIndex);
                        //for (int j = 0; j < iMaxSplit; j++)
                        //    lActPBTimes[j] = 0;
                        //iAttempt = 0;
                        iMaxAtempts = 100000; // max tries to catch a valid time

                        if (Settings.bDebug || Settings.bExpSplitsvalue || Settings.iSplitsvalue != 100)
                        {
                            // Add random split times for each remaining segment
                            for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit && iCountActMalus <= Settings.iMalusMax; iSegment++)
                            {
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
                                        if ((iPseudoRnd++ % splits[iSegment].Count) > lCountSkippedSplits[iSegment])
                                        //if (rand.Next(lCountTimedSplits[iSegment] + lCountSkippedSplits[iSegment]) >= lCountSkippedSplits[iSegment])
                                        {
                                            //fSecMalus += Settings.MalusCount;
                                            iCountActMalus++;
                                            fSecMalus += rand.Next(2 * Settings.MalusCount + 1);
                                            if (Settings.bDebug && iFaster == 0 && iSlower == 0) sWriteDebug1 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Route: " + i + " Name: " + State.Run[iSegment].Name + " Failure " + iCountMalus + " add " + Settings.MalusCount + "s\r\n";
                                        }
                                } while (split == null && iMaxAtempts > 0 && iCountActMalus <= Settings.iMalusMax);

                                if (split != null) // found split times
                                {
                                    if (split.Value[tmTimingMethod].HasValue)
                                    { // add the time
                                        fSec += split.Value[tmTimingMethod].Value.TotalMilliseconds / 1000;
                                        lActPBTimes[iSegment] = split.Value[tmTimingMethod].Value.TotalMilliseconds;
                                        if (Settings.bDebug && iFaster == 0 && iSlower == 0) sWriteDebug1 += "Segment: " + iSegment.ToString("00") + " Attempt: " + iAttempt.ToString("0000") + " Route: " + i + " Duration: " + split.Value[tmTimingMethod].ToString() + " = " + (split.Value[tmTimingMethod].Value.TotalMilliseconds / 1000).ToString("000.000") + "ms" + " --- BestTime=" + State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalSeconds.ToString("000.000") + " Name: " + State.Run[iSegment].Name + "\r\n";
                                    }
                                    else
                                    { // should never happen
                                        if (State.Run[iSegment].BestSegmentTime[tmTimingMethod].HasValue)
                                        {
                                            InternalComponent.InformationValue = "W4 no time found in A" + iAttempt + " S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                                            if (Settings.bDebug) sWriteDebug1 += "Segment: " + iSegment.ToString("00") + " Atempt: " + iAttempt + " Name: " + State.Run[iSegment].Name + " (Warning 3: no time found)\r\n";
                                            fSec += State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalMilliseconds / 1000;
                                        }
                                        else
                                        {
                                            InternalComponent.InformationValue = "E4 no (best) time found in A" + iAttempt + " S" + (1 + iSegment) + " " + State.Run[iSegment].Name;
                                            return 4;
                                        }
                                    }
                                }
                                else if(iCountActMalus <= Settings.iMalusMax)
                                {
                                    InternalComponent.InformationValue = "E5 too many failure attempts S" + (1 + iSegment) + " " + State.Run[iSegment].Name; // + " split contains no times"
                                    return 5;
                                }
                            }
                            if (Settings.bDebug && iFaster == 0 && iSlower == 0) { System.IO.File.AppendAllText(@"pbchance_debug.txt", sWriteDebug1); sWriteDebug1 = "\r\n--- Results of 10 successfully runs and 10 failures if possible --- " + watch.ElapsedMilliseconds + "ms\r\n"; }

                            // Check if the time is faster than pb
                            if (fSec + fSecStart + iCountActMalus * Settings.MalusCount <= pb[tmTimingMethod].Value.TotalMilliseconds / 1000 + Settings.TimediffCount && iCountActMalus <= Settings.iMalusMax)
                            //if (fSec + fSecMalus <= fToBeat && iCountActMalus <= Settings.iMalusMax)
                                {
                                    iFaster++;
                                if (Settings.bDebug && iFaster <= 10) // write first ten faster times
                                    sWriteDebug1 += "Run: " + i.ToString("00000") + " Total Time: (" + fSec.ToString("0000.000") + "+" + fSecStart.ToString() + "+" + (iCountMalus * Settings.MalusCount).ToString("0000.000") + ") = " + (fSec + fSecStart + (iCountMalus * Settings.MalusCount)).ToString("0.000") + " < " + (pb[tmTimingMethod].Value.TotalMilliseconds / 1000 + Settings.TimediffCount * fSec / (fSec + fSecStart)).ToString("0.00") + " = (" + pb[tmTimingMethod].Value.TotalMilliseconds / 1000 + " + " + (Settings.TimediffCount * fSec / (fSec + fSecStart)).ToString("0.00") + ") success" + (Settings.iOptimistic == 0 ? "\r\n" : "");
                                //if (fSec + fSecStart + fSecMalus <= pb[tmTimingMethod].Value.TotalMilliseconds / 1000 + Settings.TimediffCount - Settings.iPaceExtraGoalMS / 1000.0)
                                if (fSec + fSecMalus <= fToBeatGoal)
                                {
                                        iFasterPace++;
                                    for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                                    {
                                        lSumPBTimes[iSegment] += lActPBTimes[iSegment];
                                        if (lWorstPBTimes[iSegment] < lActPBTimes[iSegment])
                                            lWorstPBTimes[iSegment] = lActPBTimes[iSegment];
                                    }
                                }
                            }
                            else
                            {
                                iSlower++;
                                if (Settings.bDebug && iSlower <= 10) // write first ten slower times
                                    sWriteDebug1 += "Run: " + i.ToString("00000") + " Total Time: (" + fSec.ToString("0000.000") + "+" + fSecStart.ToString() + "+" + (iCountMalus * Settings.MalusCount).ToString("0000.000") + ") = " + (fSec + fSecStart + (iCountMalus * Settings.MalusCount)).ToString("0.000") + " > " + (pb[tmTimingMethod].Value.TotalMilliseconds / 1000 + Settings.TimediffCount * fSec / (fSec + fSecStart)).ToString("0.00") + " = (" + pb[tmTimingMethod].Value.TotalMilliseconds / 1000 + " + " + (Settings.TimediffCount * fSec / (fSec + fSecStart)).ToString("0.00") + ") failure" + (Settings.iOptimistic == 0 ? "\r\n" : "");
                            }
                        }


                        else // no debug, optimized fast version
                        {
                            for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                            {
                                {
newTryBecauseThisIsFaster:
                                    iAttempt = rand.Next(splitsExt[iSegment].Count);
                                    //iAttempt = iPseudoRnd++ % splitsExt[iSegment].Count;
                                    split = splitsExt[iSegment][iAttempt];

                                    if (split == null)
                                    {// split is a failure, add a malus
                                        if ((iPseudoRnd++ % splits[iSegment].Count) > lCountSkippedSplits[iSegment])
                                        {
                                            iCountActMalus++;
                                            fSecMalus += iPseudoRnd % iMalusRange; //rand.Next(iMalusRange);
                                            if (iCountActMalus > Settings.iMalusMax)
                                            {
                                                iSlower++;
                                                goto InterruptedRunBecauseToMuchFailures;
                                            }
                                        }
                                        goto newTryBecauseThisIsFaster;
                                    }
                                } //while (split == null /*&& iMaxAtempts > 0*/);
                                fSec += split.Value[tmTimingMethod].Value.TotalMilliseconds / 1000; // add the time  
                                lActPBTimes[iSegment] = split.Value[tmTimingMethod].Value.TotalMilliseconds;
                            }
                            // Check if the time is faster than pb
                            //if (fSec + fSecStart + fSecMalus <= pb[tmTimingMethod].Value.TotalMilliseconds / 1000 + Settings.TimediffCount)
                            if (fSec + fSecMalus <= fToBeat)
                            {
                                iFaster++;
                                //if (fSec + fSecStart + fSecMalus <= pb[tmTimingMethod].Value.TotalMilliseconds / 1000 + Settings.TimediffCount - Settings.iPaceExtraGoalMS / 1000.0)
                                if (fSec + fSecMalus <= fToBeatGoal)
                                {
                                    iFasterPace++;
                                    for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                                    {
                                        lSumPBTimes[iSegment] += lActPBTimes[iSegment];
                                        if (lWorstPBTimes[iSegment] < lActPBTimes[iSegment])
                                            lWorstPBTimes[iSegment] = lActPBTimes[iSegment];
                                    }
                                }
                            }
                            else
                                iSlower++;
                        }
InterruptedRunBecauseToMuchFailures:
                        iCountMalus += iCountActMalus;
                    }
            else
            {
                iFaster = 0;
                iSlower = Settings.SamplesCount;
            }

            if (Settings.bDebug) System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);

            bCalcComplete = true;
            return 0;
        }

        protected void Recalculate()
        {
            int i, iAttempt, iSegment;
            double /*fDeviation = 0,*/ iSumMsActSplit = 0;
            //string text = "";
            bCalcComplete = false;
            watch = System.Diagnostics.Stopwatch.StartNew();
            System.String sWriteDebug1 = "", sWriteDebug2 = "", sWriteDebug3 = "";
            iSurvivalFailure = 0; iSurvivalSuccess = 0; iSurvivalFailurePast = 0; iNextSurvSuc = 0; iNextSurvFail = 0;iSurvToHereAttempt = 0; iSurvToHereCount = 0;
            tmTimingMethod = State.CurrentTimingMethod;
            //Random random = new Random(); InternalComponent.InformationValue = random.Next(1, 10000) + "";
            if (Settings.sVersion != "1.4.3")
            {
                Settings.iCalctime = 750;
                Settings.bDispGoodPace = true;
                Settings.bGoodPaceTotal = true;
                Settings.iMalusMax = 1;
                Settings.iPaceExtraGoalMS = 0;
                Settings.iPaceDigits = 0;
                Settings.iRndInfoFor = 12;
                Settings.bPaceWorst = false;
                Settings.sVersion = "1.4.3";
            }

            iCurrentSplitIndex = (State.CurrentSplitIndex < 0 ? 0 : State.CurrentSplitIndex) + (bCheckUpdate ? 1 : 0);
            iMaxSplit = (Settings.iCalcToSplit == 0 || Settings.iCalcToSplit > State.Run.Count) ? State.Run.Count : Settings.iCalcToSplit;
            //if (iCurrentSplitIndex > iMaxSplit) return; // Last split do no check for an update

            // Array of the count of valid split times per split (without failing attemps)
            lCountSplits = new int[iMaxSplit + 1];
            lCountTimedSplits = new int[iMaxSplit + 1];
            lCountSkippedSplits = new int[iMaxSplit + 1];
            lOldestAttempt = new int[iMaxSplit + 1];

            // Sum the Times, if this route is a PB
            lActPBTimes = new double[iMaxSplit + 1];
            lSumPBTimes = new double[iMaxSplit + 1];
            lWorstPBTimes = new double[iMaxSplit + 1];

            // Initialize Arrays
            for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
            {
                lSumPBTimes[iSegment] = 0;
                lWorstPBTimes[iSegment] = 0;// Int32.MaxValue;
            }

            // Get the current Personal Best, if it exists
            pb = State.Run.Last().PersonalBestSplitTime;

            if (pb[tmTimingMethod] == TimeSpan.Zero || !pb[tmTimingMethod].HasValue)
            {
                // No personal best, so any run will PB
                InternalComponent.InformationValue = "100%";
                return;
            }

            // Create the lists of split times
            splits = new List<Time?>[iMaxSplit];
            for (i = 0; i < iMaxSplit; i++)
                splits[i] = new List<Time?>();

            // Find the range of attempts to gather times from
            iLastAttempt = State.Run.AttemptHistory.Count - Settings.iSkipNewest;
            if (!Settings.IgnoreRunCount)
                iFirstAttempt = iLastAttempt - (Settings.UseFixedAttempts ? Settings.AttemptCount - 1 : iLastAttempt * (Settings.AttemptCount - 1 - Settings.iSkipNewest) / 100);
            if (iFirstAttempt < 1) iFirstAttempt = 1;
            iSurvToHereAttempt = iLastAttempt;

            if (Settings.bDebug)
                System.IO.File.WriteAllText(@"pbchance_Debug.txt", "AdjustedStartTime: " + State.AdjustedStartTime + " AttemptEnded: " + State.AttemptEnded + " AttemptStarted: " + State.AttemptStarted + " \r\nCurrentAttemptDuration: " + State.CurrentAttemptDuration + " CurrentComparison: " + State.CurrentComparison + " CurrentPhase: " + State.CurrentPhase + "\r\n" //" CurrentHotkeyProfile: " + State.CurrentHotkeyProfile.ToString()
                    + "CurrentSplit: " + State.CurrentSplit + " CurrentSplitIndex: " + State.CurrentSplitIndex + " CurrentTime: " + State.CurrentTime
                    + "\r\nCurrentTimingMethod: " + tmTimingMethod + " GameTimePauseTime: " + State.GameTimePauseTime + " IsGameTimeInitialized: " + State.IsGameTimeInitialized
                    + "\r\nIsGameTimePaused: " + State.IsGameTimePaused + " Layout: " + State.Layout + " LayoutSettings: " + State.LayoutSettings
                    + "\r\nLoadingTimes: " + State.LoadingTimes + " PauseTime: " + State.PauseTime + " Run: " + State.Run + " Settings: " + State.Settings
                    + "\r\nStartTime: " + State.StartTime + " StartTimeWithOffset: " + State.StartTimeWithOffset + " TimePausedAt: " + State.TimePausedAt
                    + "\r\nAttempts: " + iFirstAttempt + " to " + iLastAttempt + " Malus: " + Settings.MalusCount + " Splitclip: " + Settings.SplitclipCount + " Timediff: " + Settings.TimediffCount + " Samples: " + Settings.SamplesCount + " Calctime: " + Settings.iCalctime + " Survival: " + Settings.bSurvival + " Rebalancing: " + Settings.iOptimistic
                    + "\r\nValueRuns: " + Settings.bValueRuns + " Min Times per Segment: " + Settings.iMinTimes + " Automatic Update every: " + Settings.iUpdate + "s More Value on newer Splits: " + Settings.iSplitsvalue + " Skip newest Splits: " + Settings.iSkipNewest
                    + "\r\nPersonal Best Time to beat: " + pb[tmTimingMethod].ToString() + " CalcToSplit: " + Settings.iCalcToSplit
                    + "\r\nConsider also fails: " + Settings.bConsiderFails + " Random Information every: " + Settings.iRndInfoEvery + " Random Information for: " + Settings.iRndInfoFor + "\r\n\r\n--- Failed Runs --- " + watch.ElapsedMilliseconds + "ms\r\n");

            //if (Settings.bDebug) // alle Daten in Datei schreiben
            //{
            //    sWriteDebug1 = "Segment Attempt Time\r\n";
            //    for (iAttempt = iFirstAttempt; iAttempt <= iLastAttempt; iAttempt++)
            //        for (iSegment = 0; iSegment < iMaxSplit; iSegment++)
            //        {
            //            if (State.Run[iSegment].SegmentHistory.ContainsKey(iAttempt) && State.Run[iSegment].SegmentHistory[iAttempt][tmTimingMethod] > TimeSpan.Zero)
            //                sWriteDebug1 += iSegment + " " + iAttempt + " " + State.Run[iSegment].SegmentHistory[iAttempt][tmTimingMethod].Value.TotalMilliseconds + "\r\n";
            //        }
            //    System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);
            //    sWriteDebug1 = "";
            //}

            PrepareSplits();

            if (Settings.bDebug)
            {
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1 + "\r\n--- Clipping segments --- " + watch.ElapsedMilliseconds + "ms\r\n" + sWriteDebug2);
                sWriteDebug1 = "\r\n--- Added times --- " + watch.ElapsedMilliseconds + "ms\r\n" + sWriteDebug3;
                for (iSegment = 0; iSegment < iMaxSplit; iSegment++) // Count Skipped Splits
                    sWriteDebug1 += "Segm: " + iSegment.ToString("00") + " Skipped: " + lCountSkippedSplits[iSegment].ToString("0000") + " Timed: " + lCountTimedSplits[iSegment].ToString("0000") + " Fails: " + (splits[iSegment].Count - lCountTimedSplits[iSegment]).ToString("0000") + " Count: " + splits[iSegment].Count.ToString("0000") + " lOldestAttempt: " + lOldestAttempt[iSegment].ToString("0000") + " AttemptDifference: " + (iLastAttempt - lOldestAttempt[iSegment]).ToString("0000") + "\n";
            }

            if (KeepAlive == false) return; // cancel calculation if new thread is requested

            fNumberOfCombinations = 1; // calculate number of active combinations
            for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                if (splits[iSegment].Count == 0) // This split contains no split times, so we cannot calculate a probability
                {
                    InternalComponent.InformationValue = "E3 no times found in S" + (1 + iSegment) + " " + State.Run[iSegment].Name; // + " split contains no times"
                    if (Settings.bDebug) System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);
                    return;
                }
                else fNumberOfCombinations *= (splits[iSegment].Count + lCountSkippedSplits[iSegment]);

            if (Settings.bDebug) // Count of Times per Segment
            {
                //System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug3);
                sWriteDebug1 += "\r\n--- Count of Times per Segment --- " + watch.ElapsedMilliseconds + "ms\r\n";
                for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                {
                    double fAvg = 0;
                    if (iSegment < iMaxSplit)
                    {
                        fAvg = 0;
                        i = 0;
                        for (iAttempt = 0; iAttempt < splits[iSegment].Count; iAttempt++)
                            if (splits[iSegment][iAttempt].HasValue)
                            {
                                i++;
                                fAvg += splits[iSegment][iAttempt].Value[tmTimingMethod].Value.TotalMilliseconds;
                            }
                        fAvg = fAvg / i;
                    }
                    sWriteDebug1 += "Segment: " + iSegment.ToString("00") + " Count of Valid/Total Times: " + lCountSplits[iSegment].ToString("0000") + "/" + splits[iSegment].Count.ToString("0000")
                        + " Best: " + State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalSeconds.ToString("000.000")
                        + " Avg To Best: " + (fAvg * .001 - State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalMilliseconds * .001).ToString("00.0s")
                        + " Name: " + State.Run[iSegment].Name + "\r\n";
                }
                System.IO.File.AppendAllText(@"pbchance_Debug.txt", sWriteDebug1);

                //Write all Segment Times
                sWriteDebug2 = "\r\n\r\n\r\n--- Detailed Segment Times ---\r\n";
                for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                {
                    sWriteDebug2 += " - " + State.Run[iSegment].Name + " -\r\n";
                    for (iAttempt = 0; iAttempt < splits[iSegment].Count; iAttempt++)
                    {
                        sWriteDebug2 += "Segment: " + iSegment + " Attempt:" + iAttempt.ToString("000") + " Chance: ";
                        sWriteDebug2 += ((100 - Settings.iSplitsvalue + Settings.iSplitsvalue * (splits[iSegment].Count - iAttempt) / (splits[iSegment].Count + 1.0)) / splits[iSegment].Count / (100 - Settings.iSplitsvalue * .5) * 100.0).ToString("00.0") + "% "; // linear
                        //sWriteDebug2 += (100 * (iAttempt + 1) / summ(splits[iSegment].Count)).ToString("00.0") + "% ";
                        if (splits[iSegment][iAttempt].HasValue)
                            sWriteDebug2 += "Time:" + splits[iSegment][iAttempt].Value[tmTimingMethod].Value + "/" + splits[iSegment][iAttempt].Value[tmTimingMethod].Value.TotalMilliseconds + " Factor: " + Math.Round(splits[iSegment][iAttempt].Value[tmTimingMethod].Value.TotalSeconds / (State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalSeconds + .0001), 2).ToString("0.00\r\n");
                        else
                            sWriteDebug2 += "Failed\r\n";
                        //while (iAttempt / (splits[iSegment].Count * 1.0) < rand.Next(Settings.iSplitsvalue) * .01) ;
                    }
                }
                sWriteDebug2 += "\r\nNumber of combinations = " + fNumberOfCombinations + " = " + fNumberOfCombinations.ToString($"F{0}");
                //sWriteDebug1 = "\r\n--- First generated Route (" + Settings.SamplesCount + " Routes in total) --- " + watch.ElapsedMilliseconds + "ms\r\n";
            }

            /*double*/ fAvgTime = 0; // Calc next average Split Time
            if (Settings.bInfoNext || bRndInformationActive)
                if (iCurrentSplitIndex < iMaxSplit)
                {
                    i = 0;
                    for (iAttempt = 0; iAttempt < splits[iCurrentSplitIndex].Count; iAttempt++)
                        if (splits[iCurrentSplitIndex][iAttempt].HasValue)
                        {
                            i++;
                            fAvgTime += splits[iCurrentSplitIndex][iAttempt].Value[tmTimingMethod].Value.TotalMilliseconds;
                        }
                    fAvgTime = fAvgTime / i;
                }

            if ((Settings.bDeviation || bRndInformationActive) && iCurrentSplitIndex < iMaxSplit) // Calc Standard Deviation
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
                            iSumMsActSplit += splits[iSegment][iAttempt].Value[tmTimingMethod].Value.TotalMilliseconds;
                            //if(Settings.bDebug) System.IO.File.AppendAllText(@"pbchance_Debug.txt", "+"+ splits[iSegment][iAttempt].Value[tmTimingMethod].Value.TotalMilliseconds+",");
                        }
                    fMean = iSumMsActSplit / i;
                    //if (Settings.bDebug) System.IO.File.AppendAllText(@"pbchance_Debug.txt", " MEAN=" + fMean + " iSumMs=" + iSumMsActSplit + " /i= "+ i);
                    for (iAttempt = 0; iAttempt < splits[iSegment].Count; iAttempt++)
                    {
                        if (splits[iSegment][iAttempt].HasValue)
                            fDeviation += (splits[iSegment][iAttempt].Value[tmTimingMethod].Value.TotalMilliseconds - fMean)
                                        * (splits[iSegment][iAttempt].Value[tmTimingMethod].Value.TotalMilliseconds - fMean);
                        //if (Settings.bDebug) System.IO.File.AppendAllText(@"pbchance_Debug.txt", " fDeviation=" + fDeviation);
                    }
                    fDeviation = Math.Pow(fDeviation / i, .5) * 0.001; // 0.5 Deviation / 1.0 Variation / Version 2 = fDeviation / (i-1)
                                                                       //if (Settings.bDebug) System.IO.File.AppendAllText(@"pbchance_Debug.txt", " Wurzel fDeviation /1000 =" + fDeviation);
                    if (Settings.bDebug) sWriteDebug2 += "Segment: " + iSegment.ToString("00") + " Deviation: " + fDeviation.ToString("0.00") + "\r\n";
                }
            }

            // Total Best Time
            fTotalBestTime = 0;
            for (iSegment = State.CurrentSplitIndex + 1; iSegment < iMaxSplit; iSegment++)
                fTotalBestTime += State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalMilliseconds / 1000.0;
            fTotalBestTime -= Settings.TimediffCount;

            // Get current time as a baseline
            Time test = State.CurrentTime;
            if (State.CurrentSplitIndex > 0)
            {
                if (test[tmTimingMethod] == null) test = State.CurrentTime;
                test = State.Run[iCurrentSplitIndex - 1].SplitTime;
            }
            else
                test[tmTimingMethod] = TimeSpan.Zero;
            if (!bCheckUpdate)
                if (test[tmTimingMethod].HasValue) // split hasn't skipped
                    fSecStart = test[tmTimingMethod].Value.TotalMilliseconds / 1000;
                else
                {
                    if (Settings.bSkipSplitStroke) { InternalComponent.InformationValue = "-"; return; }
                    if (Settings.bIgnoreSkipClip)
                        return;
                    else // use the actual time, possible to read the skipping time?
                        fSecStart = State.CurrentTime[tmTimingMethod].Value.TotalMilliseconds / 1000;
                }
            
            // Create extended split times for faster calculation
            splitsExt = new List<Time?>[iMaxSplit];
            {
                int j, iCount;
                for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                {
                    splitsExt[iSegment] = new List<Time?>();
                    iCount = 0;
                    for (i = 0; i < splits[iSegment].Count; i++)
                    {
                        iCount++;
                        for (j = 0; j < iCount; j++)
                            splitsExt[iSegment].Add(splits[iSegment][splits[iSegment].Count - i - 1]);
                    }
                }
            }

            //thread1 = new Thread(new ThreadStart(CheckForPB));
            //thread1.Start();

            //thread2 = new Thread(new ThreadStart(CheckForPB));
            //thread2.Start();

            //if (thread1 != null)
            //    if ((thread.ThreadState & (System.Threading.ThreadState.Stopped | System.Threading.ThreadState.Unstarted)) == 0)
            //        if (thread1.IsAlive)
            //            thread1.Join();

            ////thread2 = new Thread(new ThreadStart(CheckForPB));
            ////thread2.Start();

            //if (thread2 != null)
            //    if ((thread2.ThreadState & (System.Threading.ThreadState.Stopped | System.Threading.ThreadState.Unstarted)) == 0)
            //        if (thread2.IsAlive)
            //            thread2.Join();

            //InternalComponent.InformationValue = "H";
            CheckForPB();
            //InternalComponent.InformationValue = "I";
            bCalcComplete = true;
            DisplayResults();
        }


        protected void DisplayResults()
        {
            int iSegment;

            if (iFasterPace > 0)
                for (iSegment = 0; iSegment < iMaxSplit; iSegment++)
                {
                    lSumPBTimes[iSegment] /= 1000.0 * iFasterPace;
                    lWorstPBTimes[iSegment] /= 1000.0;
                }

            if (iCurrentSplitIndex == iMaxSplit) // no more remaining times, check for a new pb
            {
                InternalComponent.InformationName = "PB Chance";
                InternalComponent.InformationValue = (fSecStart < pb[tmTimingMethod].Value.TotalMilliseconds / 1000) ? bCheckUpdate ? InternalComponent.InformationValue : "100% PB" : (fSecStart == pb[tmTimingMethod].Value.TotalMilliseconds / 1000) ? "50% PB" : "0%";
            }
            else // display results
            {
                double fProb = iFaster / (iFaster + iSlower + 0.0);
                if (State.CurrentSplitIndex == 0 && !bCheckUpdate) // memorize Startingchance
                    fProbStart = fProb;

                if (bRndInformationActive)
                {
                    displayRndInformation();
                    bRndInformationActive = false;
                    return;
                }

                if (bCheckUpdate)
                { // background calculation
                    fLiveChanceAvg = (fLiveChanceAvg > 0) ? (fLiveChanceAvg + fProb) * .5 : fProb;

                    if (fProb > fLiveChanceAvg || fLiveChanceAvg > fLiveChanceAct) // do not display, if chance is higher than the displaying chance //&& !(fProb==0 && fLiveChanceAvg==0 && fLiveChanceAct>0)
                        return;

                    if (fProb > 0)
                        fProb = (fLiveChanceAvg > .1 && fLiveChanceAvg < .9) ? Math.Round(fLiveChanceAvg, 3) : fLiveChanceAvg; // round a place behind the comma, in the range of 10%-90%
                }
                fLiveChanceAct = fProb;

                // Display additional informations
                if (!bCheckUpdate || iFaster == 0 || InternalComponent.InformationName.Remove(2, InternalComponent.InformationName.Length - 2) != "PB")
                {
                    if (Settings.bSurvival && iCurrentSplitIndex < iMaxSplit) // Calculate survival chance
                        InternalComponent.InformationName = "PB Chance (" + Math.Round(iNextSurvSuc / (iNextSurvSuc + iNextSurvFail + .0) * 100, 0).ToString() + "%/"
                            + Math.Round(iSurvivalSuccess / (iSurvivalSuccess + iSurvivalFailure + .0) * 100, 0).ToString() + "%)";
                    else if (InternalComponent.InformationName != "PB Chance")
                        InternalComponent.InformationName = "PB Chance";
                    if (Settings.bDeviation) // Displaying Deviation
                        InternalComponent.InformationName += " (" + Math.Round(fDeviation, 2).ToString() + ")";
                    if (Settings.bInfoNext && iCurrentSplitIndex < iMaxSplit)
                        if (State.Run[iCurrentSplitIndex].BestSegmentTime[tmTimingMethod].HasValue)
                            InternalComponent.InformationName += " (" + (int)(fAvgTime * .001 - State.Run[iCurrentSplitIndex].BestSegmentTime[tmTimingMethod].Value.TotalMilliseconds * .001) + "s " + (iNextSurvFail / (iNextSurvSuc + iNextSurvFail + .0) * 100).ToString("0") + "%)";
                    if (Settings.bDispGoodPace && iFaster > 0)
                        if (!Settings.bGoodPaceTotal || iCurrentSplitIndex < 1)
                            InternalComponent.InformationName += " - " + secondsToTime(lSumPBTimes[iCurrentSplitIndex], Settings.iPaceDigits, true);
                        else
                            if (State.Run[iCurrentSplitIndex - 1].SplitTime[tmTimingMethod].HasValue)
                                InternalComponent.InformationName += " - " + secondsToTime(lSumPBTimes[iCurrentSplitIndex] + State.Run[iCurrentSplitIndex - 1].SplitTime[tmTimingMethod].Value.TotalMilliseconds / 1000, Settings.iPaceDigits, true);
                    if (Settings.bPaceWorst && iFaster > 0)
                        if (!Settings.bGoodPaceTotal || iCurrentSplitIndex < 1)
                            InternalComponent.InformationName += " - " + secondsToTime(Settings.bPaceWorst ? lWorstPBTimes[iCurrentSplitIndex] : lSumPBTimes[iCurrentSplitIndex], Settings.iPaceDigits, true);
                        else
                            if (State.Run[iCurrentSplitIndex - 1].SplitTime[tmTimingMethod].HasValue)
                                InternalComponent.InformationName += " - " + secondsToTime((Settings.bPaceWorst ? lWorstPBTimes[iCurrentSplitIndex] : lSumPBTimes[iCurrentSplitIndex]) + State.Run[iCurrentSplitIndex - 1].SplitTime[tmTimingMethod].Value.TotalMilliseconds / 1000, Settings.iPaceDigits, true);
                }

                // Zero success, display 0% instead of 0.00%. If it's <0.01%, it will display the number of success runs and total runs
                string text = Settings.DisplayOdds && fProb < .0000100055 && fProb > 0 ? "" : (fProb > 0 && fProb < .0001 && !bCheckUpdate && !Settings.DisplayOdds) ? iFaster + " in " + (iFaster + iSlower) : fProb == 0 ? "0%" : (fProb * 100).ToString("0.00") + "%";

                if (Settings.DisplayOdds && fProb > 0 && (fProb >= 0.00000001 || bCheckUpdate)) // Displaying odds
                    if (bCheckUpdate)
                        text = "1 in " + RoundExtended(1 / fProb, fProb > 0.0000002 ? fProb > 0.000002 ? fProb > 0.00002 ? fProb > 0.0002 ? fProb > 0.002 ? fProb > .02 ? fProb > .2 ? 3 : 1 : 0 : -1 : -2 : -3 : -4 : -5).ToString(fProb > .000100055 ? fProb > .00100055 ? fProb > .010055 ? fProb > .100055 ? fProb > .91 ? fProb > .9999 ? "0      " : "0.00   " : "0.00   " : "0.0   " : "0    " : "0  " : "0") + (fProb > .00000100055 ? fProb > .0000100055 ? "" : "     0%" : "   0%") + (fProb >= .1 ? fProb == 1 ? "" : "" : "  ") + text;
                    //                text = "1 in " + RoundExtended(1 / fProb, fProb > 0.0000002 ? fProb > 0.000002 ? fProb > 0.00002 ? fProb > 0.0002 ? fProb > 0.002 ? fProb > .02 ? fProb > .2 ? 3 : 1 : 0 : -1 : -2 : -3 : -4 : -5).ToString(fProb > .0100055 ? fProb > .100055 ? fProb > .91 ? fProb == 1 ? "0 " : "0.00  " : "0.00" : "0.0" : "0") + (fProb > .000100055 ? fProb > .00100055 ? fProb > .0100055 ? fProb > .100055 ? fProb > .91 ? fProb == 1 ? "    " : "" : "  " : "  " : "   " : " " : "") + "" + (fProb >= .1 ? fProb == 1 ? "" : "" : "  ") + text; //.ToString(fProb > .0100055 ? fProb > .100055 ? fProb > .91 ? fProb == 1 ? "0" : "0.000" : "0.00" : "0.0" : "0") + " | " + text; 
                    else
                        text = "1 in " + (1 / fProb).ToString(fProb > .000100055 ? fProb > .00100055 ? fProb > .0100055 ? fProb > .100055 ? fProb > .91 ? fProb > .9999 ? "0      " : "0.00   " : "0.00   " : "0.0   " : "0    " : "0  " : "0") + (fProb > .00000100055 ? fProb > .0000100055 ? "" : "     0%" : "   0%") + (fProb >= .1 ? fProb == 1 ? "" : "" : "  ") + text;

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
                updateDataTable();
            }

            sInformationName = InternalComponent.InformationName;
            sInformationValue = InternalComponent.InformationValue;
            bCalcComplete = false;
        }
        
        private void updateDataTable()
        {
            double fSum = State.CurrentAttemptDuration.TotalMilliseconds / 1000,
                   fSumWorst = State.CurrentAttemptDuration.TotalMilliseconds / 1000;
            int iSegment;
            System.Data.DataTable dataTable1 = new System.Data.DataTable();

            dataTable1.Rows.Clear();
            dataTable1.Columns.Clear();
            dataTable1.Columns.Add("Name", typeof(string), null);
            dataTable1.Columns.Add("Avg Pace", typeof(string), null);
            dataTable1.Columns.Add("Worst Pace", typeof(string), null);
            dataTable1.Columns.Add("Best", typeof(string), null);
            //dataTable1.Rows.Add(new string[] { "PB chance", (100 * fProb).ToString("0.00") + "%" });

            if (iFasterPace > 0)
            {
                //for (iSegment = iMaxSplit; iSegment > iCurrentSplitIndex && iSegment > 0; iSegment--)
                //    lSumPBTimes[iSegment] -= lSumPBTimes[iSegment - 1];
                iMaxSplit = (Settings.iCalcToSplit == 0 || Settings.iCalcToSplit > State.Run.Count) ? State.Run.Count : Settings.iCalcToSplit;
                for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                {
                    if(lSumPBTimes.Length>iSegment) fSum += lSumPBTimes[iSegment] / 1;
                    if(lWorstPBTimes.Length > iSegment) fSumWorst += lWorstPBTimes[iSegment];
                }
                dataTable1.Rows.Add(new string[] { "Faster/Extra Goal/Total", iFaster.ToString("#,##0"), iFasterPace.ToString("#,##0"), (iFaster+iSlower).ToString("#,##0") });
                dataTable1.Rows.Add(new string[] { "PB Regular/Extra Goal", (1.0*iFaster / (iFaster + iSlower)).ToString("0.00%"), (iFasterPace / (iFaster + iSlower * 1.0)).ToString("0.00%") });
                dataTable1.Rows.Add(new string[] { "Total Time", secondsToTime(fSum, 3, false), secondsToTime(fSumWorst, 3, false), secondsToTime(fTotalBestTime, 3, false) });
                for (iSegment = iCurrentSplitIndex; iSegment < iMaxSplit; iSegment++)
                    if (State.Run[iSegment].BestSegmentTime[tmTimingMethod].HasValue && lSumPBTimes.Length > iSegment && lWorstPBTimes.Length > iSegment)
                        dataTable1.Rows.Add(new string[] { State.Run[iSegment].Name,
                            secondsToTime(lSumPBTimes[iSegment] / 1,1, false),
                            secondsToTime(lWorstPBTimes[iSegment],1, false),
                            secondsToTime(State.Run[iSegment].BestSegmentTime[tmTimingMethod].Value.TotalMilliseconds/1000,1, false) });
            }
            else
                dataTable1.Rows.Add(new string[] { "No PB found", "0", (iFaster + iSlower).ToString("#,##0"), secondsToTime(fTotalBestTime, 3, false) });

            Settings.DataGridViewV = dataTable1;
            Settings.viewUpdateDisplay();
        }

        void displayRndInformation()
        {
            double fProb = iFaster / (iFaster + iSlower + 0.0);
            Random random = new Random();
            int iRandom;

            TryAgain:
            {
                iRandom = random.Next(1, 16);
                if (random.Next(1, 4) == 1 && !Settings.bDispGoodPace)
                    iRandom = 11; // 25% chance Pace to PB, if it isn't displayed
            }
                //iRandom = 8;

            switch (iRandom)
            {
                case 1:
                    if (random.Next(0, 100) < 50) goto TryAgain; // half chance
                    InternalComponent.InformationName = "Remaining Combinations";
                    InternalComponent.InformationValue = getLargeNumber(fNumberOfCombinations);
                    break;
                case 2:
                    InternalComponent.InformationName = "Sample Size";
                    InternalComponent.InformationValue = (iFaster + iSlower).ToString("#,##0") + " in " + (((double)watch.ElapsedMilliseconds) / 1000).ToString("0.00s");
                    break;
                case 3:
                    InternalComponent.InformationName = "Survival in this Segment";
                    InternalComponent.InformationValue = /*iNextSurvSuc + " of " + (iNextSurvSuc + iNextSurvFail) + " = " +*/ (iNextSurvSuc / (iNextSurvSuc + iNextSurvFail + 0.0)).ToString("0.00%");
                    break;
                case 4:
                    InternalComponent.InformationName = "Survival to the End";
                    InternalComponent.InformationValue = /*iSurvivalSuccess + "/" + (iSurvivalSuccess + iSurvivalFailure + iSurvivalFailurePast) + " = " +*/ (iSurvivalSuccess / (iSurvivalSuccess + iSurvivalFailure + 0.0)).ToString("0.00%");
                    break;
                case 5:
                    if (random.Next(0, 100) < 50) goto TryAgain; // half chance
                    InternalComponent.InformationName = "Standard Deviation";
                    InternalComponent.InformationValue = fDeviation.ToString("0.00s");
                    break;
                case 6:
                    InternalComponent.InformationName = "Average Difference to Best";
                    InternalComponent.InformationValue = (fAvgTime * .001 - State.Run[iCurrentSplitIndex].BestSegmentTime[tmTimingMethod].Value.TotalMilliseconds * .001).ToString("0.0s");
                    break;
                case 7:
                    if (iCurrentSplitIndex == 0)
                        goto TryAgain;
                    InternalComponent.InformationName = "Until here has Survived";
                    InternalComponent.InformationValue = /*iSurvToHereCount + " of " + (iLastAttempt - iSurvToHereAttempt + 1) + " = " +*/ (iSurvToHereCount / (iLastAttempt - iSurvToHereAttempt + 1.0)).ToString("0.00%");
                    break;
                case 8:
                    if (watch.ElapsedMilliseconds >= 0) goto TryAgain; // case 2 doing similar, so skip it
                    InternalComponent.InformationName = "Combinations per Second";
                    InternalComponent.InformationValue = ((double)(1000 * (iFaster + iSlower + 0.0) / watch.ElapsedMilliseconds)).ToString("#,##0") /*+ "*" + iFaster + "*" + iSlower + "*" + watch.ElapsedMilliseconds*/;
                    break;
                case 9:
                    if (fProbStart == 0 || iFaster == 0) goto TryAgain;
                    double f = fProb / fProbStart * pb[tmTimingMethod].Value.TotalMilliseconds / (pb[tmTimingMethod].Value.TotalMilliseconds - State.CurrentAttemptDuration.TotalMilliseconds);
                    InternalComponent.InformationName = "Worth to continue";
                    InternalComponent.InformationValue = (f > 2 ? "sure" : f > 1.5 ? "yes" : f > 1.0 ? "think so" : f > 0.75 ? "equal" : f > .5 ? "maybe not" : "no") + " " + f.ToString("[x0.0]");
                    //InternalComponent.InformationValue = f.ToString(", x0.0 - ") + pb[tmTimingMethod].Value.TotalMilliseconds.ToString("0 - ") + State.CurrentAttemptDuration.TotalMilliseconds.ToString("0 - ") + (fProb / fProbStart).ToString("0");
                    break;
                case 10:
                    if (random.Next(0, 100) < 66) goto TryAgain; // 2/3 chance
                    if (fProbStart == 0) goto TryAgain;
                    double fFact = Math.Pow(10, Math.Log10(25 / fProbStart) / iMaxSplit); // 25% goal
                    fFact = Math.Pow(fFact, iCurrentSplitIndex);
                    f = fProb / fFact / fProbStart * pb[tmTimingMethod].Value.TotalMilliseconds / (pb[tmTimingMethod].Value.TotalMilliseconds - fSecStart);
                    InternalComponent.InformationName = "Run is currently";
                    InternalComponent.InformationValue = f > 1 ? (f > 2 ? "very good" : f > 1.5 ? "good" : f > 1 ? "above average" : f > 0.5 ? "average" : f > .25 ? "below average" : "bad") + " " + f.ToString("[x0]") : f.ToString("0%");
                    //InternalComponent.InformationValue = f1.ToString("x0.0 ") + fFact.ToString("x0.0 ") + f.ToString(", x0.0");
                    break;
                case 11:
                    if (iFaster == 0) goto TryAgain;
                    InternalComponent.InformationName = "Pace to PB";
                    if (iCurrentSplitIndex > 0)
                    {
                        if(State.Run[iCurrentSplitIndex - 1].SplitTime[tmTimingMethod].HasValue) 
                            InternalComponent.InformationValue = secondsToTime(lSumPBTimes[iCurrentSplitIndex], 0, true) + " / " + secondsToTime(lSumPBTimes[iCurrentSplitIndex] + State.Run[iCurrentSplitIndex - 1].SplitTime[tmTimingMethod].Value.TotalMilliseconds / 1000, 0, true);
                        else
                            goto TryAgain;
                    }
                    else
                        InternalComponent.InformationValue = secondsToTime(lSumPBTimes[0], 0, true) + " / " + secondsToTime((lSumPBTimes[0]), 0, true);
                    break;
                case 12:
                    if (random.Next(0, 100) < 33) goto TryAgain; // 1/3 chance
                    InternalComponent.InformationName = "Avg Dif to Best (All/Suc)";
                    InternalComponent.InformationValue = (fAvgTime * .001 - State.Run[iCurrentSplitIndex].BestSegmentTime[tmTimingMethod].Value.TotalMilliseconds * .001).ToString("0.0s") + 
                        " / " + (lSumPBTimes[iCurrentSplitIndex] - State.Run[iCurrentSplitIndex].BestSegmentTime[tmTimingMethod].Value.TotalMilliseconds * .001).ToString("0.0s");
                    break;
                case 13:
                    if (random.Next(0, 100) < 20) goto TryAgain; // 1/5 chance
                    if (iFaster == 0) goto TryAgain;
                    InternalComponent.InformationName = "Worst Pace to PB";
                    if (iCurrentSplitIndex > 0)
                    {
                        if (State.Run[iCurrentSplitIndex - 1].SplitTime[tmTimingMethod].HasValue)
                            InternalComponent.InformationValue = secondsToTime(lWorstPBTimes[iCurrentSplitIndex], 0, true) + " / " + secondsToTime(lWorstPBTimes[iCurrentSplitIndex] + State.Run[iCurrentSplitIndex - 1].SplitTime[tmTimingMethod].Value.TotalMilliseconds / 1000, 0, true);
                        else
                            goto TryAgain;
                    }
                    else
                        InternalComponent.InformationValue = secondsToTime(lWorstPBTimes[0], 0, true) + " / " + secondsToTime((lWorstPBTimes[0]), 0, true);
                    break;
                case 14:
                    if (random.Next(0, 100) < 25) goto TryAgain; // 1/4 chance
                    if (iFaster == 0) goto TryAgain;
                    InternalComponent.InformationName = "Avg/Worst Pace to PB";
                    InternalComponent.InformationValue = secondsToTime(lSumPBTimes[iCurrentSplitIndex], 0, true) + " / " + secondsToTime((lWorstPBTimes[iCurrentSplitIndex]), 0, true);
                    break;
                case 15:
                    if (iCurrentSplitIndex == 0) goto TryAgain; // 1/2 chance
                    InternalComponent.InformationName = "Considered Runs (here/end)";
                    InternalComponent.InformationValue = (iLastAttempt - iSurvToHereAttempt + 1) + "/" + (iSurvivalSuccess + iSurvivalFailure + iSurvivalFailurePast) + " of " + State.Run.AttemptHistory.Count;
                    //InternalComponent.InformationValue = (iLastAttempt-lOldestAttempt[iCurrentSplitIndex]) + " Ö " + (iLastAttempt - lOldestAttempt[iMaxSplit-1]) + " Ö " + (iLastAttempt - iSurvToHereAttempt + 1) + "/" + (iSurvivalSuccess + iSurvivalFailure + iSurvivalFailurePast) + " of " + State.Run.AttemptHistory.Count;
                    //lOldestAttempt[iSegment]
                    break;
                case 16:
                    if (random.Next(0, 100) < 25) goto TryAgain; // 1/2 chance
                    InternalComponent.InformationName = "Github.com/kasi777/PBChance";
                    InternalComponent.InformationValue = "";
                    break;
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
            /*if (bCalcComplete)
                DisplayResults();
            else*/ if (Math.Abs(fLastUpdate - State.CurrentTime[tmTimingMethod].Value.TotalMilliseconds) >= Settings.iUpdate * 1000)
            {
                double fTimer = 0, fBestSegmentTime = 0, fSplitTime = 0;
                int iCurrentSplitIndex = State.CurrentSplitIndex;
                fLastUpdate = State.CurrentTime[tmTimingMethod].Value.TotalMilliseconds;

                if (iCurrentSplitIndex >= 1) // calculate the actual split time
                    if (State.Run[iCurrentSplitIndex - 1].SplitTime[tmTimingMethod].HasValue)
                    //if (State.CurrentTime[tmTimingMethod].HasValue) // seems this is not necessary
                    {
                        fSplitTime = State.Run[iCurrentSplitIndex - 1].SplitTime[tmTimingMethod].Value.TotalMilliseconds;
                        fTimer = State.CurrentAttemptDuration.TotalMilliseconds - fSplitTime;
                    }
                    else fTimer = State.CurrentAttemptDuration.TotalMilliseconds; // determination of split time isn't possible now
                else fTimer = State.CurrentAttemptDuration.TotalMilliseconds;
                
                //InternalComponent.InformationName = secondsToTime(fTimer/1000,3); // Test actual Timer

                // background calculation, if actual split time is slower than best split time, and the actual chance is > 0
                //if (iCurrentSplitIndex > -1)
                if (State.Run[iCurrentSplitIndex].BestSegmentTime[tmTimingMethod].HasValue)
                {
                    if (thread == null || thread.ThreadState != System.Threading.ThreadState.Running) // do it if the thread isn't running
                    //if(bCalcComplete)
                    {
                        fBestSegmentTime = State.Run[iCurrentSplitIndex].BestSegmentTime[tmTimingMethod].Value.TotalMilliseconds;
                        if (fTimer > fBestSegmentTime && fLiveChanceAct > 0)
                        {
                            KeepAlive = true;
                            bCheckUpdate = true; // tell Recalculate(), that's a background calculation
                            bRndInformationOn = false;
                            thread = new Thread(new ThreadStart(Recalculate));
                            thread.Start();
                            //Recalculate();
                        }
                        else if (Settings.iRndInfoEvery > 0)
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
                                    //Recalculate();
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
                                    //Recalculate();
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
