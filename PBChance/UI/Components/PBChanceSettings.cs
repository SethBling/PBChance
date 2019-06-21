using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.UI;
using System.Net; //because WebClient



namespace PBChance.UI.Components
{
    public partial class PBChanceSettings : UserControl
    {
        public string sVersion { get; set; }
        public Boolean UsePercentOfAttempts { get; set; }
        public Boolean UseFixedAttempts { get; set; }
        public int AttemptCount { get; set; }
        public int MalusCount { get; set; }
        public int SplitclipCount { get; set; }
        public int TimediffCount { get; set; }
        public int SamplesCount { get; set; }
        public bool DisplayOdds { get; set; }
        public bool IgnoreRunCount { get; set; }
        public bool bDebug { get; set; }
        public int iOptimistic { get; set; }
        public bool bRebalance { get; set; }
        public bool bValueRuns { get; set; }
        public bool bExpSplitsvalue { get; set; }
        public int iMinTimes { get; set; }
        public int iUpdate { get; set; }
        public int iSplitsvalue { get; set; }
        public bool bInfoNext { get; set; }
        public bool bConsiderFails { get; set; }

        public bool bDispGoodPace { get; set; }
        public bool bGoodPaceTotal { get; set; }
        public bool bPaceWorst { get; set; }
        public int iMalusMax { get; set; }
        public int iPaceExtraGoalMS { get; set; }
        public int iPaceDigits { get; set; }

        public System.Data.DataTable DataGridViewV { get; set; }

        public Boolean bSurvival { get; set; }
        public Boolean bIgnoreSkipClip { get; set; }
        public Boolean bSkipSplitStroke { get; set; }
        public Boolean bDeviation { get; set; }

        public int iSkipNewest { get; set; }
        public int iCalcToSplit { get; set; }
        public int iCalctime { get; set; }
        public int iRndInfoEvery { get; set; }
        public int iRndInfoFor { get; set; }

        public int iAddBest { get; set; }

        public event EventHandler SettingChanged;

        public PBChanceSettings()
        {
            //bool bFixedAttempts;
            InitializeComponent();

            UsePercentOfAttempts = false;
            UseFixedAttempts = true;
            IgnoreRunCount = false;
            AttemptCount = 50;
            MalusCount = 30;
            SplitclipCount = 150;
            TimediffCount = 0;
            SamplesCount = 250000;
            iCalctime = 900;
            bSurvival = false;
            bDebug = false;
            iOptimistic = 0;
            bRebalance = false;
            bValueRuns = false;
            iMinTimes = 20;
            iUpdate = 1;
            iSplitsvalue = 100;
            iSkipNewest = 0;
            iCalcToSplit = 0;
            bExpSplitsvalue = false;
            bConsiderFails = true;
            bIgnoreSkipClip = true;
            iRndInfoEvery = 0; // once per segment, in the middle (e.g. Best Segment Time=2:00, for 10 seconds, it will display between 0:55-1:05)
            iRndInfoFor = 12;
            iAddBest = 25;
            bDispGoodPace = true;
            bGoodPaceTotal = false;
            iMalusMax = 1;
            iPaceExtraGoalMS = 0;
            bPaceWorst = false;
            iPaceDigits = 0;

            rdoPercentAttempt.DataBindings.Add("Checked", this, "UsePercentOfAttempts", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            rdoAbsAttempt.DataBindings.Add("Checked", this, "UseFixedAttempts", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            AttemptCountBox.DataBindings.Add("Value", this, "AttemptCount", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            DisplayOddsCheckbox.DataBindings.Add("Checked", this, "DisplayOdds", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            IgnoreRunCountBox.DataBindings.Add("Checked", this, "IgnoreRunCount", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            MalusCountBox.DataBindings.Add("Value", this, "MalusCount", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            SplitclipCountBox.DataBindings.Add("Value", this, "SplitclipCount", true, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            SamplesCountBox.DataBindings.Add("Value", this, "SamplesCount", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            TimediffCountBox.DataBindings.Add("Value", this, "TimediffCount", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            CalctimeCountBox.DataBindings.Add("Value", this, "iCalctime", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkSurvival.DataBindings.Add("Checked", this, "bSurvival", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkIgnoreSkipClip.DataBindings.Add("Checked", this, "bIgnoreSkipClip", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkSkipSplitStroke.DataBindings.Add("Checked", this, "bSkipSplitStroke", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkDeviation.DataBindings.Add("Checked", this, "bDeviation", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;

            MinTimesCountBox.DataBindings.Add("Value", this, "iMinTimes", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            UpdateCountBox.DataBindings.Add("Value", this, "iUpdate", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            SplitsvalueCountBox.DataBindings.Add("Value", this, "iSplitsvalue", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;

            SkipNewestCountBox.DataBindings.Add("Value", this, "iSkipNewest", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            CalcToSplitUpDown.DataBindings.Add("Value", this, "iCalcToSplit", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkExpSplitsvalue.DataBindings.Add("Checked", this, "bExpSplitsvalue", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkInfoNext.DataBindings.Add("Checked", this, "bInfoNext", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;

            RndInfoEveryCountBox.DataBindings.Add("Value", this, "iRndInfoEvery", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            RndInfoForCountBox.DataBindings.Add("Value", this, "iRndInfoFor", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkConsiderFails.DataBindings.Add("Checked", this, "bConsiderFails", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;

            AddBestCountBox.DataBindings.Add("Value", this, "iAddBest", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkDispGoodPace.DataBindings.Add("Checked", this, "bDispGoodPace", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkGoodPaceTotal.DataBindings.Add("Checked", this, "bGoodPaceTotal", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;

            chkPaceWorst.DataBindings.Add("Checked", this, "bPaceWorst", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            PaceExtraGoalMSCountBox.DataBindings.Add("Value", this, "iPaceExtraGoalMS", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            PaceDigitsCountBox.DataBindings.Add("Value", this, "iPaceDigits", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            MalusMaxCountBox.DataBindings.Add("Value", this, "iMalusMax", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;

            UseFixedAttempts = !UsePercentOfAttempts;
            UsePercentOfAttempts = !UseFixedAttempts;
        }

        private void OnSettingChanged(object sender, BindingCompleteEventArgs e)
        {
            SettingChanged?.Invoke(this, e);
            updateDataTable_Core2(dataGridView1, true);
        }

        public LayoutMode Mode { get; internal set; }

        internal XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            return parent;
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "Version", "1.3.9") ^
                SettingsHelper.CreateSetting(document, parent, "AttemptCount", AttemptCount) ^
                SettingsHelper.CreateSetting(document, parent, "UsePercentOfAttempts", UsePercentOfAttempts) ^
                SettingsHelper.CreateSetting(document, parent, "UseFixedAttempts", UseFixedAttempts) ^
                SettingsHelper.CreateSetting(document, parent, "DisplayOdds", DisplayOdds) ^
                SettingsHelper.CreateSetting(document, parent, "IgnoreRunCount", IgnoreRunCount) ^
                SettingsHelper.CreateSetting(document, parent, "MalusCount", MalusCount) ^
                SettingsHelper.CreateSetting(document, parent, "SplitclipCount", SplitclipCount) ^
                SettingsHelper.CreateSetting(document, parent, "TimediffCount", TimediffCount) ^
                SettingsHelper.CreateSetting(document, parent, "SamplesCount", SamplesCount) ^
                SettingsHelper.CreateSetting(document, parent, "iCalctime", iCalctime) ^
                SettingsHelper.CreateSetting(document, parent, "iOptimistic", iOptimistic) ^
                SettingsHelper.CreateSetting(document, parent, "chkRebalance", bRebalance) ^
                SettingsHelper.CreateSetting(document, parent, "chkSurvival", bSurvival) ^
                SettingsHelper.CreateSetting(document, parent, "chkInfoNext", bInfoNext) ^
                SettingsHelper.CreateSetting(document, parent, "bValueRuns", bValueRuns) ^
                SettingsHelper.CreateSetting(document, parent, "IgnoreSkipClip", bIgnoreSkipClip) ^
                SettingsHelper.CreateSetting(document, parent, "bSkipSplitStroke", bSkipSplitStroke) ^
                SettingsHelper.CreateSetting(document, parent, "Deviation", bDeviation) ^
                SettingsHelper.CreateSetting(document, parent, "iMinTimes", iMinTimes) ^
                SettingsHelper.CreateSetting(document, parent, "iUpdate", iUpdate) ^
                SettingsHelper.CreateSetting(document, parent, "iAddBest", iAddBest) ^
                SettingsHelper.CreateSetting(document, parent, "RndInfoEveryCountBox", iRndInfoEvery) ^
                SettingsHelper.CreateSetting(document, parent, "RndInfoForCountBox", iRndInfoFor) ^
                SettingsHelper.CreateSetting(document, parent, "chkConsiderFails", bConsiderFails) ^
                SettingsHelper.CreateSetting(document, parent, "bDispGoodPace", bDispGoodPace) ^
                SettingsHelper.CreateSetting(document, parent, "bGoodPaceTotal", bGoodPaceTotal) ^
                SettingsHelper.CreateSetting(document, parent, "chkPaceWorst", bPaceWorst) ^
                SettingsHelper.CreateSetting(document, parent, "PaceExtraGoalMSCountBox", iPaceExtraGoalMS) ^
                SettingsHelper.CreateSetting(document, parent, "MalusMaxCountBox", iMalusMax) ^
                SettingsHelper.CreateSetting(document, parent, "PaceDigitsCountBox", iPaceDigits) ^
                SettingsHelper.CreateSetting(document, parent, "iSplitsvalue", iSplitsvalue);
        }

        public void SetSettings(XmlNode settings) { SetSettingsCore(dataGridView1, settings); }
        public delegate void SetSettingsDelegate(DataGridView control, XmlNode settings);  // defines a delegate type
        public void SetSettingsCore(DataGridView control, XmlNode settings)
        {
            if (this.InvokeRequired)
                this.Invoke(new SetSettingsDelegate(SetSettingsCore), new object[] { control, settings });  // invoking itself
            else      // the "functional part", executing only on the main thread
            {
                sVersion = SettingsHelper.ParseString(settings["Version"]);
                AttemptCount = SettingsHelper.ParseInt(settings["AttemptCount"]);
                UsePercentOfAttempts = SettingsHelper.ParseBool(settings["UsePercentOfAttempts"]);
                UseFixedAttempts = SettingsHelper.ParseBool(settings["UseFixedAttempts"]);
                DisplayOdds = SettingsHelper.ParseBool(settings["DisplayOdds"]);
                IgnoreRunCount = SettingsHelper.ParseBool(settings["IgnoreRunCount"]);
                MalusCount = SettingsHelper.ParseInt(settings["MalusCount"]);
                SplitclipCount = SettingsHelper.ParseInt(settings["SplitclipCount"]);
                TimediffCount = SettingsHelper.ParseInt(settings["TimediffCount"]);
                SamplesCount = SettingsHelper.ParseInt(settings["SamplesCount"]);
                iCalctime = SettingsHelper.ParseInt(settings["iCalctime"]);
                iOptimistic = SettingsHelper.ParseInt(settings["iOptimistic"]);
                bSurvival = SettingsHelper.ParseBool(settings["chkSurvival"]);
                bRebalance = SettingsHelper.ParseBool(settings["chkRebalance"]);
                bIgnoreSkipClip = SettingsHelper.ParseBool(settings["IgnoreSkipClip"]);
                bSkipSplitStroke = SettingsHelper.ParseBool(settings["bSkipSplitStroke"]);
                bDeviation = SettingsHelper.ParseBool(settings["Deviation"]);
                bValueRuns = SettingsHelper.ParseBool(settings["bValueRuns"]);
                bInfoNext = SettingsHelper.ParseBool(settings["chkInfoNext"]);
                iMinTimes = SettingsHelper.ParseInt(settings["iMinTimes"]);
                iUpdate = SettingsHelper.ParseInt(settings["iUpdate"]);
                iAddBest = SettingsHelper.ParseInt(settings["iAddBest"]);
                iSplitsvalue = SettingsHelper.ParseInt(settings["iSplitsvalue"]);
                bExpSplitsvalue = SettingsHelper.ParseBool(settings["bExpSplitsvalue"]);
                iRndInfoEvery = SettingsHelper.ParseInt(settings["RndInfoEveryCountBox"]);
                iRndInfoFor = SettingsHelper.ParseInt(settings["RndInfoForCountBox"]);
                bConsiderFails = SettingsHelper.ParseBool(settings["chkConsiderFails"]);
                iPaceExtraGoalMS = SettingsHelper.ParseInt(settings["PaceExtraGoalMSCountBox"]);
                iMalusMax = SettingsHelper.ParseInt(settings["MalusMaxCountBox"]);
                iPaceDigits = SettingsHelper.ParseInt(settings["PaceDigitsCountBox"]);
                bPaceWorst = SettingsHelper.ParseBool(settings["chkPaceWorst"]);
                bDispGoodPace = SettingsHelper.ParseBool(settings["bDispGoodPace"]);
                bGoodPaceTotal = SettingsHelper.ParseBool(settings["bGoodPaceTotal"]);
            }
        }
        /*
        internal void SetSettings(XmlNode settings)
        {
            sVersion = SettingsHelper.ParseString(settings["Version"]);
            AttemptCount = SettingsHelper.ParseInt(settings["AttemptCount"]);
            UsePercentOfAttempts = SettingsHelper.ParseBool(settings["UsePercentOfAttempts"]);
            UseFixedAttempts = SettingsHelper.ParseBool(settings["UseFixedAttempts"]);
            DisplayOdds = SettingsHelper.ParseBool(settings["DisplayOdds"]);
            IgnoreRunCount = SettingsHelper.ParseBool(settings["IgnoreRunCount"]);
            MalusCount = SettingsHelper.ParseInt(settings["MalusCount"]);
            SplitclipCount = SettingsHelper.ParseInt(settings["SplitclipCount"]);
            TimediffCount = SettingsHelper.ParseInt(settings["TimediffCount"]);
            SamplesCount = SettingsHelper.ParseInt(settings["SamplesCount"]);
            iCalctime = SettingsHelper.ParseInt(settings["iCalctime"]);
            iOptimistic = SettingsHelper.ParseInt(settings["iOptimistic"]);
            bSurvival = SettingsHelper.ParseBool(settings["chkSurvival"]);
            bRebalance = SettingsHelper.ParseBool(settings["chkRebalance"]);
            bIgnoreSkipClip = SettingsHelper.ParseBool(settings["IgnoreSkipClip"]);
            bDeviation = SettingsHelper.ParseBool(settings["Deviation"]);
            bValueRuns = SettingsHelper.ParseBool(settings["bValueRuns"]);
            bInfoNext = SettingsHelper.ParseBool(settings["chkInfoNext"]);
            iMinTimes = SettingsHelper.ParseInt(settings["iMinTimes"]);
            iUpdate = SettingsHelper.ParseInt(settings["iUpdate"]);
            iAddBest = SettingsHelper.ParseInt(settings["iAddBest"]);
            iSplitsvalue = SettingsHelper.ParseInt(settings["iSplitsvalue"]);
            bExpSplitsvalue = SettingsHelper.ParseBool(settings["bExpSplitsvalue"]);
            iRndInfoEvery = SettingsHelper.ParseInt(settings["RndInfoEveryCountBox"]);
            iRndInfoFor = SettingsHelper.ParseInt(settings["RndInfoForCountBox"]);
            bConsiderFails = SettingsHelper.ParseBool(settings["chkConsiderFails"]);
            iPaceExtraGoalMS = SettingsHelper.ParseInt(settings["PaceExtraGoalMSCountBox"]);
            iMalusMax = SettingsHelper.ParseInt(settings["MalusMaxCountBox"]);
            iPaceDigits = SettingsHelper.ParseInt(settings["PaceDigitsCountBox"]);
            bPaceWorst = SettingsHelper.ParseBool(settings["chkPaceWorst"]);
            bDispGoodPace = SettingsHelper.ParseBool(settings["bDispGoodPace"]);
            bGoodPaceTotal = SettingsHelper.ParseBool(settings["bGoodPaceTotal"]);
        }
        */
        private void btnDebug_Click(object sender, EventArgs e)
        {
            bDebug = true;
            SettingChanged?.Invoke(this, e);
        }

        private void label26_DoubleClick(object sender, EventArgs e)
        {
            if (!bExpSplitsvalue)
                chkExpSplitsvalue.Visible = !chkExpSplitsvalue.Visible;
        }

        private void label15_DoubleClick(object sender, EventArgs e)
        {
            CalctimeCountBox.Visible = !CalctimeCountBox.Visible;
            lblCalctime1.Visible = !lblCalctime1.Visible;
            lblCalctime2.Visible = !lblCalctime2.Visible;
        }

        private void btnNewVersion_Click(object sender, EventArgs e)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            WebClient wc = new WebClient();
            try
            {
                string sVersion = wc.DownloadString("https://github.com/kasi777/PBChance/raw/master/PBChance/Version.txt");

                if (sVersion.Remove(10, sVersion.Length - 10) != "1.4.3     ")
                {
                    wc.DownloadFile("https://github.com/kasi777/PBChance/raw/master/PBChance.dll", "PBChance.dll");
                    MessageBox.Show("New Version available: Installed: 1.4.3, Available: " + sVersion.Remove(10, sVersion.Length - 10) +
                        "\n\r\n\rPBChance.dll is already downloaded into your LiveSplit directory. Move it into the Components directory to install it.\n\r\n\r" +
                        sVersion.Remove(0, 10) + "\n\r\n\rhttps://github.com/kasi777/PBChance");
                }
                else
                    MessageBox.Show("PBChance is already up to date. \n\r\n\rhttps://github.com/kasi777/PBChance");
            }
            catch
            {
                MessageBox.Show("Can't connect to https://github.com/kasi777/PBChance");
            }
        }
        
        private void btnUpdList_Click(object sender, EventArgs e) { updateDataTable_Core2(dataGridView1, true); }
        public delegate void updateDataTable_Delegate(DataGridView control, bool bStar);  // defines a delegate type
        public void updateDataTable_Core(DataGridView control, bool bStar)
        {
            
            if (this.InvokeRequired)
                this.Invoke(new updateDataTable_Delegate(updateDataTable_Core), new object[] { control, bStar });  // invoking itself
            else      // the "functional part", executing only on the main thread
            {
                control.DataSource = DataGridViewV;
                control.Columns[0].Width = 143;
                control.Columns[1].Width = 80; // 77 min
                control.Columns[2].Width = 87; // 87 min
                control.Columns[3].Width = 80; // 73 min
                if (DataGridViewV.Rows.Count > 2)
                {
                    if (bStar) //(DataGridViewV.Rows[0][3].ToString() != this.lblSampleSize.Text.Remove(0, 13))
                    {
                        this.lblSampleSize.Text = "Sample size: " + DataGridViewV.Rows[0][3] + " * ";
                        this.lblFaster.Text = "Faster: " + DataGridViewV.Rows[0][1] + " * ";
                        this.lblChance.Text = "Chance: " + (Convert.ToDouble(DataGridViewV.Rows[0][1]) / Convert.ToDouble(DataGridViewV.Rows[0][3])).ToString("0.000%") + "*";
                    }
                }
                else if (DataGridViewV.Rows.Count <= 2)
                {
                    if (bStar)
                    {
                        this.lblSampleSize.Text = "Sample size:" + DataGridViewV.Rows[0][1] + " * ";
                        this.lblFaster.Text = "Faster: " + "0*";
                        this.lblChance.Text = "Chance: " + "0%*";
                    }
                }
            }
        }
        public void updateDataTable_Core2(DataGridView control, bool bStar) // this.UIThreadSync(() => X)   this.Invoke(new MethodInvoker( X ))
        {
            this.UIThreadSync(() => control.DataSource = DataGridViewV);
            this.UIThreadSync(() => control.Columns[0].Width = 143);
            this.UIThreadSync(() => control.Columns[1].Width = 80);
            this.UIThreadSync(() => control.Columns[2].Width = 87);
            this.UIThreadSync(() => control.Columns[3].Width = 80);

            if (DataGridViewV.Rows.Count > 2)
                {
                    if (bStar) //(DataGridViewV.Rows[0][3].ToString() != this.lblSampleSize.Text.Remove(0, 13))
                    {
                        this.lblSampleSize.Text = "Sample size: " + DataGridViewV.Rows[0][3] + " * ";
                        this.lblFaster.Text = "Faster: " + DataGridViewV.Rows[0][1] + " * ";
                        this.lblChance.Text = "Chance: " + (Convert.ToDouble(DataGridViewV.Rows[0][1]) / Convert.ToDouble(DataGridViewV.Rows[0][3])).ToString("0.000%") + "*";
                    }
                }
                else if (DataGridViewV.Rows.Count <= 2)
                {
                    if (bStar)
                    {
                        this.lblSampleSize.Text = "Sample size:" + DataGridViewV.Rows[0][1] + " * ";
                        this.lblFaster.Text = "Faster: " + "0*";
                        this.lblChance.Text = "Chance: " + "0%*";
                    }
                }
        }
        /*
        private void updateDataTable(bool bStar)
        {
            dataGridView1.DataSource = DataGridViewV;
            dataGridView1.Columns[0].Width = 143;
            dataGridView1.Columns[1].Width = 80; // 77 min
            dataGridView1.Columns[2].Width = 87; // 87 min
            dataGridView1.Columns[3].Width = 80; // 73 min
            if (DataGridViewV.Rows.Count > 2)
            {
                if (bStar) //(DataGridViewV.Rows[0][3].ToString() != this.lblSampleSize.Text.Remove(0, 13))
                {
                    this.lblSampleSize.Text = "Sample size: " + DataGridViewV.Rows[0][3] + " * ";
                    this.lblFaster.Text = "Faster: " + DataGridViewV.Rows[0][1] + " * ";
                    this.lblChance.Text = "Chance: " + (Convert.ToDouble(DataGridViewV.Rows[0][1]) / Convert.ToDouble(DataGridViewV.Rows[0][3])).ToString("0.000%") + "*";
                }
            }
            else if (DataGridViewV.Rows.Count <= 2)
            {
                if (bStar)
                {
                    this.lblSampleSize.Text = "Sample size:" + DataGridViewV.Rows[0][1] + " * ";
                    this.lblFaster.Text = "Faster: " + "0*";
                    this.lblChance.Text = "Chance: " + "0%*";
                }
            }
        }
        */
        /*
        public delegate void viewUpdateDisplayDelegate();  // defines a delegate type
        public void viewUpdateDisplay2()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new viewUpdateDisplayDelegate(viewUpdateDisplay2), new object[] {  });  // invoking itself
            }
            else      // the "functional part", executing only on the main thread
            {
                if (DataGridViewV.Rows.Count > 2)
                {
                    this.lblSampleSize.Text = "Sample size: " + DataGridViewV.Rows[0][3];
                    this.lblFaster.Text = "Faster: " + DataGridViewV.Rows[0][1];
                    this.lblChance.Text = "Chance: " + (Convert.ToDouble(DataGridViewV.Rows[0][1]) / Convert.ToDouble(DataGridViewV.Rows[0][3])).ToString("0.000%");
                }
                else
                {
                    this.lblSampleSize.Text = "Sample size:" + DataGridViewV.Rows[0][1];
                    this.lblFaster.Text = "Faster: " + "0";
                    this.lblChance.Text = "Chance: " + "0%";
                }
            }
        }*/

        public void viewUpdateDisplay()
        {

            if (DataGridViewV.Rows.Count > 2)
            {
                this.lblSampleSize.Text = "Sample size: " + DataGridViewV.Rows[0][3];
                this.lblFaster.Text = "Faslter: " + DataGridViewV.Rows[0][1];
                this.lblChance.Text = "Chance: " + (Convert.ToDouble(DataGridViewV.Rows[0][1]) / Convert.ToDouble(DataGridViewV.Rows[0][3])).ToString("0.000%");
            }
            else
            {
                this.lblSampleSize.Text = "Sample size:" + DataGridViewV.Rows[0][1];
                this.lblFaster.Text = "Faster: " + "0";
                this.lblChance.Text = "Chance: " + "0%";
            }
        }

        //private void TabPages_Selected(Object sender, TabControlEventArgs e)
        //{

        //    System.Text.StringBuilder messageBoxCS = new System.Text.StringBuilder();
        //    messageBoxCS.AppendFormat("{0} = {1}", "TabPage", e.TabPage);
        //    messageBoxCS.AppendLine();
        //    messageBoxCS.AppendFormat("{0} = {1}", "TabPageIndex", e.TabPageIndex);
        //    messageBoxCS.AppendLine();
        //    messageBoxCS.AppendFormat("{0} = {1}", "Action", e.Action);
        //    messageBoxCS.AppendLine();
        //    MessageBox.Show(messageBoxCS.ToString(), "Selected Event");
        //}

        private void tabHistory_Enter(object sender, EventArgs e) { /*dataGridView1.DataSource = DataGridViewV;*/ tabHistory_Enter_Core(dataGridView1); }
        public delegate void tabHistory_Enter_Delegate(DataGridView control);  // defines a delegate type
        public void tabHistory_Enter_Core(DataGridView control)
        {
            if (this.InvokeRequired)
                this.Invoke(new tabHistory_Enter_Delegate(tabHistory_Enter_Core), new object[] { control });  // invoking itself
            else      // the "functional part", executing only on the main thread
                control.DataSource = DataGridViewV;
        }
    }
}

public static class FormInvokeExtension
{
    static public void UIThreadAsync(this Control control, Action code)
    {
        if (control.InvokeRequired)
        {
            control.BeginInvoke(code);
            return;
        }
        code.Invoke();
    }

    static public void UIThreadSync(this Control control, Action code)
    {
        if (control.InvokeRequired)
        {
            control.Invoke(code);
            return;
        }
        code.Invoke();
    }
}

