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

        public Boolean bSurvival { get; set; }
        public Boolean bIgnoreSkipClip { get; set; }
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
            SamplesCount = 100000;
            iCalctime = 500;
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
            iRndInfoEvery = 600;
            iRndInfoFor = 10;
            iAddBest = 1;

            rdoPercentAttempt.DataBindings.Add("Checked", this, "UsePercentOfAttempts", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            rdoAbsAttempt.DataBindings.Add("Checked", this, "UseFixedAttempts", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            //rdoPercentAttempt.DataBindings.Add("Checked", this, "UsePercentOfAttempts", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            //rdoAbsAttempt.DataBindings.Add("Checked", this, "UseFixedAttempts", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            AttemptCountBox.DataBindings.Add("Value", this, "AttemptCount", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            DisplayOddsCheckbox.DataBindings.Add("Checked", this, "DisplayOdds", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            IgnoreRunCountBox.DataBindings.Add("Checked", this, "IgnoreRunCount", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            MalusCountBox.DataBindings.Add("Value", this, "MalusCount", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            SplitclipCountBox.DataBindings.Add("Value", this, "SplitclipCount", true, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            SamplesCountBox.DataBindings.Add("Value", this, "SamplesCount", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            TimediffCountBox.DataBindings.Add("Value", this, "TimediffCount", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            CalctimeCountBox.DataBindings.Add("Value", this, "iCalctime", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkSurvival.DataBindings.Add("Checked", this, "bSurvival", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            //chkRebalance.DataBindings.Add("Checked", this, "bRebalance", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            //RebalanceCountBox.DataBindings.Add("Value", this, "iOptimistic", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkIgnoreSkipClip.DataBindings.Add("Checked", this, "bIgnoreSkipClip", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
            chkDeviation.DataBindings.Add("Checked", this, "bDeviation", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;

            //chkValueRuns.DataBindings.Add("Checked", this, "bValueRuns", false, DataSourceUpdateMode.OnPropertyChanged).BindingComplete += OnSettingChanged;
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
            
            UseFixedAttempts = !UsePercentOfAttempts;
            UsePercentOfAttempts = !UseFixedAttempts;
        }

        private void OnSettingChanged(object sender, BindingCompleteEventArgs e)
        {
            SettingChanged?.Invoke(this, e);
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
            return SettingsHelper.CreateSetting(document, parent, "Version", "1.3.7") ^
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
                SettingsHelper.CreateSetting(document, parent, "Deviation", bDeviation) ^
                SettingsHelper.CreateSetting(document, parent, "iMinTimes", iMinTimes) ^
                SettingsHelper.CreateSetting(document, parent, "iUpdate", iUpdate) ^
                SettingsHelper.CreateSetting(document, parent, "iAddBest", iAddBest) ^
                SettingsHelper.CreateSetting(document, parent, "RndInfoEveryCountBox", iRndInfoEvery) ^
                SettingsHelper.CreateSetting(document, parent, "RndInfoForCountBox", iRndInfoFor) ^
                SettingsHelper.CreateSetting(document, parent, "chkConsiderFails", bConsiderFails) ^
                SettingsHelper.CreateSetting(document, parent, "iSplitsvalue", iSplitsvalue);
        }

        internal void SetSettings(XmlNode settings)
        {
            sVersion             = SettingsHelper.ParseString(settings["Version"]);
            AttemptCount         = SettingsHelper.ParseInt(settings["AttemptCount"]);
            UsePercentOfAttempts = SettingsHelper.ParseBool(settings["UsePercentOfAttempts"]);
            UseFixedAttempts     = SettingsHelper.ParseBool(settings["UseFixedAttempts"]);
            DisplayOdds          = SettingsHelper.ParseBool(settings["DisplayOdds"]);
            IgnoreRunCount       = SettingsHelper.ParseBool(settings["IgnoreRunCount"]);
            MalusCount           = SettingsHelper.ParseInt (settings["MalusCount"]);
            SplitclipCount       = SettingsHelper.ParseInt (settings["SplitclipCount"]);
            TimediffCount        = SettingsHelper.ParseInt (settings["TimediffCount"]);
            SamplesCount         = SettingsHelper.ParseInt (settings["SamplesCount"]);
            iCalctime            = SettingsHelper.ParseInt (settings["iCalctime"]);
            iOptimistic          = SettingsHelper.ParseInt (settings["iOptimistic"]);
            bSurvival            = SettingsHelper.ParseBool(settings["chkSurvival"]);
            bRebalance           = SettingsHelper.ParseBool(settings["chkRebalance"]);
            bIgnoreSkipClip      = SettingsHelper.ParseBool(settings["IgnoreSkipClip"]);
            bDeviation           = SettingsHelper.ParseBool(settings["Deviation"]);
            bValueRuns           = SettingsHelper.ParseBool(settings["bValueRuns"]);
            bInfoNext            = SettingsHelper.ParseBool(settings["chkInfoNext"]);
            iMinTimes            = SettingsHelper.ParseInt (settings["iMinTimes"]);
            iUpdate              = SettingsHelper.ParseInt (settings["iUpdate"]);
            iAddBest             = SettingsHelper.ParseInt (settings["iAddBest"]);
            iSplitsvalue         = SettingsHelper.ParseInt (settings["iSplitsvalue"]);
            bExpSplitsvalue      = SettingsHelper.ParseBool(settings["bExpSplitsvalue"]);
            iRndInfoEvery        = SettingsHelper.ParseInt (settings["RndInfoEveryCountBox"]);
            iRndInfoFor          = SettingsHelper.ParseInt (settings["RndInfoForCountBox"]);
            bConsiderFails       = SettingsHelper.ParseBool(settings["chkConsiderFails"]);
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            bDebug = true;
            SettingChanged?.Invoke(this, e);
        }
        
        private void label26_DoubleClick(object sender, EventArgs e)
        {
            if(!bExpSplitsvalue)
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

                if (sVersion.Remove(5, sVersion.Length - 5) != "1.3.7")
                {
                    wc.DownloadFile("https://github.com/kasi777/PBChance/blob/master/PBChance.dll", "pbchance.dll");
                    MessageBox.Show("New Version available! Installed: 1.3.7, Available: " + sVersion.Remove(5, sVersion.Length - 5) + 
                        "\n\r\n\rPBChance.dll is already downloaded into LiveSplit directory. Move it into the Components directory to install it.\n\r\n\r" +
                        sVersion.Remove(0,5) + "\n\r\n\rhttps://github.com/kasi777/PBChance");
                }
                else
                    MessageBox.Show("PBChance is allready up to date. \n\r\n\rhttps://github.com/kasi777/PBChance");
            }
            catch
            {
                MessageBox.Show("Can't connect to https://github.com/kasi777/PBChance");
            }
        }
    }
}
