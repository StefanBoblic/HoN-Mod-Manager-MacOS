﻿using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Ionic.Zip;
using Ionic.Zlib;
using HonModManagerForMac.Interfaces;
using System.Collections.ObjectModel;
using HonModManagerForMac.Model;
using HonModManagerForMac.Utils;

namespace HonModManagerForMac;

public partial class MainPage : ContentPage
{
    private readonly IFolderPicker _folderPicker;

    //Mod Manager version
    private const int IconHeight = 48;
    private const int IconWidth = 48;

    //dictionary with all mods currently set to enabled via the UI
    //key: mod name, value: version string
    private readonly Dictionary<string, string> displayNamesList = new Dictionary<string, string>();
    private int enabledModsCount;

    //collection of all mods loaded by UpdateList()
    public ObservableCollection<Modification> ModsList { get; set; } = new ObservableCollection<Modification>();

    private string runGameArguments = "";
    private string runGameFile = "";
    private string gameVersion = "";

    private Version Version => Assembly.GetExecutingAssembly().GetName().Version;

    //#region " Version String Matching "

    //private static bool VersionsMatch(string Requirement, string Version)
    //{
    //    if (string.IsNullOrEmpty(Requirement)) Requirement = "*";
    //    if (string.IsNullOrEmpty(Version)) Version = "0";

    //    var VStrings = Requirement.Split('-');
    //    if (VStrings.Length > 2) return false;
    //    var VParts = Tools.StrArrayToIntArray(Version.Split('.'));
    //    if (VStrings.Length == 1)
    //    {
    //        var Parts = Tools.StrArrayToIntArray(VStrings[0].Split('.'));
    //        for (var i = 0; i <= Math.Min(VParts.Length - 1, Parts.Length - 1); i++)
    //            if (Parts[i] != int.MinValue && Parts[i] != VParts[i])
    //                return false;
    //        if (Parts.Length > VParts.Length)
    //            for (var i = VParts.Length; i <= Parts.Length - 1; i++)
    //                if (Parts[i] != 0)
    //                    return false;
    //        return true;
    //    }
    //    else
    //    {
    //        if (VStrings[0] == "") VStrings[0] = "*";
    //        if (VStrings[1] == "") VStrings[1] = "*";

    //        var Parts = Tools.StrArrayToIntArray(VStrings[0].Split('.'));
    //        for (var i = 0; i <= Math.Min(VParts.Length - 1, Parts.Length - 1); i++)
    //            if (Parts[i] != int.MinValue)
    //            {
    //                if (Parts[i] > VParts[i]) return false;
    //                if (Parts[i] < VParts[i]) break;
    //            }

    //        Parts = Tools.StrArrayToIntArray(VStrings[1].Split('.'));
    //        for (var i = 0; i <= Math.Min(VParts.Length - 1, Parts.Length - 1); i++)
    //            if (Parts[i] != int.MinValue)
    //            {
    //                if (Parts[i] < VParts[i]) return false;
    //                if (Parts[i] > VParts[i]) break;
    //            }

    //        return true;
    //    }
    //}

    //#endregion

    //#region " Mod Updating Business "

    //this list will hold references to all running mod updaters
    private readonly List<ModUpdater> modUpdatersList = new List<ModUpdater>();

    //private bool m_updatingMode;


    public MainPage(IFolderPicker folderPicker)
    {
        InitializeComponent();
        _folderPicker = folderPicker;
    }

    private async void OnPickFolderClicked(object sender, EventArgs e)
    {
        var pickedFolder = await _folderPicker.PickFolder();
    }

    void myListView_ItemSelected(System.Object sender, Microsoft.Maui.Controls.SelectedItemChangedEventArgs e)
    {

    }
    //private void EnterUpdatingMode()
    //{
    //    RefreshModDisplayToolStripMenuItem.Enabled = false;
    //    ChangeHoNPathToolStripMenuItem.Enabled = false;
    //    EnterHoNPathmanuallyToolStripMenuItem.Enabled = false;
    //    ApplyModsToolStripMenuItem.Enabled = false;
    //    ApplyModsAndLaunchHoNToolStripMenuItem.Enabled = false;
    //    DeleteToolStripMenuItem.Enabled = false;
    //    m_updatingMode = true;
    //    CancelAllUpdatesToolStripMenuItem.Visible = true;

    //    ForgetAllZIPs();

    //    myUpdatingTimer.Start();
    //}

    //private async void LeaveUpdatingMode()
    //{
    //    myUpdatingTimer.Stop();

    //    CancelAllUpdatesToolStripMenuItem.Visible = false;
    //    m_updatingMode = false;
    //    RefreshModDisplayToolStripMenuItem.Enabled = true;
    //    ChangeHoNPathToolStripMenuItem.Enabled = true;
    //    EnterHoNPathmanuallyToolStripMenuItem.Enabled = true;
    //    ApplyModsToolStripMenuItem.Enabled = true;
    //    ApplyModsAndLaunchHoNToolStripMenuItem.Enabled = GameHelper.GameFilePath != "";
    //    DeleteToolStripMenuItem.Enabled = true;

    //    myStatusLabel.Text = "Updating complete.";

    //    if (m_modUpdaters.Count > 0)
    //    {
    //        var myList = m_modUpdaters.ToArray();
    //        var SortKeys = new string[m_modUpdaters.Count];
    //        var i = 0;
    //        foreach (var tModUpdater in m_modUpdaters)
    //        {
    //            SortKeys[i] = tModUpdater.SortKey;
    //            i += 1;
    //        }

    //        Array.Sort(SortKeys, myList);

    //        var myReport = "";
    //        var LastStatus = "";
    //        foreach (var tMod in myList)
    //            if (tMod.UpdateDownloaded)
    //            {
    //                if (myReport != "") myReport += Environment.NewLine;
    //                myReport += "- " + tMod.Mod.Name + ": " + tMod.StatusString;
    //            }
    //            else
    //            {
    //                if (LastStatus != tMod.StatusString)
    //                {
    //                    if (myReport != "") myReport += Environment.NewLine + Environment.NewLine;
    //                    myReport += tMod.StatusString + ":";
    //                    LastStatus = tMod.StatusString;
    //                }

    //                myReport += Environment.NewLine + "- " + tMod.Mod.Name;
    //            }

    //        m_modUpdaters.Clear();
    //        foreach (var tMod in m_mods) tMod.Updater = null;

    //        await DisplayAlert(myReport, "Update Report", "OK");

    //        UpdateList();
    //    }
    //    else
    //    {
    //        await DisplayAlert("None of your mods are updatable.", "Update Report", "OK");
    //    }
    //}

    //private static Image UpdatingIcon(Image b)
    //{
    //    //paints updating.png on top of the given image
    //    Graphics.FromImage(b).DrawImageUnscaled(Resources.updating, 0, 0);
    //    return b;
    //}

    //#endregion

    //#region " GetZip [Managing of open ZIP files] "

    ////note that the open zip files managed here are intended to be read only

    ////key: full file path, value: corresponding open Ionic.Zip.ZipFile
    //private static readonly Dictionary<string, ZipFile> OpenZIPs = new Dictionary<string, ZipFile>();

    ////returns an open ZipFile for the given full file path
    //private static ZipFile GetZip(string Name)
    //{
    //    if (OpenZIPs.ContainsKey(Name)) return OpenZIPs[Name];
    //    var tZip = ZipFile.Read(Name);
    //    if (tZip != null) OpenZIPs.Add(Name, tZip);
    //    return tZip;
    //}

    ////closes an open ZipFile releasing the file lock
    //private static void ForgetZip(string Name)
    //{
    //    if (OpenZIPs.ContainsKey(Name))
    //    {
    //        OpenZIPs[Name].Dispose();
    //        OpenZIPs.Remove(Name);
    //    }
    //}

    ////closes all open ZipFiles
    //private static void ForgetAllZIPs()
    //{
    //    foreach (var OpenZIP in OpenZIPs) OpenZIP.Value.Dispose();
    //    OpenZIPs.Clear();
    //}


    ////extract a file from an open ZipFile and returns it as stream
    //private static Stream GetZippedFile(ZipFile z, string Filename)
    //{
    //    ZipEntry tZipEntry;
    //    tZipEntry = z[Filename];
    //    if (tZipEntry == null) return null;
    //    var tStream = new MemoryStream();
    //    tZipEntry.Extract(tStream);
    //    tStream.Seek(0, SeekOrigin.Begin);
    //    return tStream;
    //}

    //private static Stream GetZippedFile(List<ZipFile> ziplist, string Filename)
    //{
    //    foreach (var zip in ziplist)
    //    {
    //        var ret = GetZippedFile(zip, Filename);
    //        if (ret != null) return ret;
    //    }

    //    return null;
    //}

    //#endregion

    //#region " Program Load/Unload "

    //private string m_appliedGameVersion;
    //private Dictionary<string, string> m_appliedMods;
    //private Dictionary<string, string> m_enabledMods;
    //private bool m_firstActivation = true;

    ////private void MainForm_KeyDown(object sender, KeyEventArgs e)
    ////{
    ////    if (e.KeyCode == Keys.A && e.Control)
    ////        foreach (ListViewItem Item in myListView.Items)
    ////            Item.Selected = true;
    ////}

    //private async void MainForm_Load(object sender, EventArgs e)
    //{
    //    int i;

    //    //process command line arguments
    //    GameHelper.GameDir = RegistryHelper.GetRegistryEntry("hondir");
    //    if (string.IsNullOrEmpty(GameHelper.GameDir))
    //    {
    //        GameHelper.SetModsDir();
    //        var CLArgs = Environment.GetCommandLineArgs();
    //        for (i = 1; i <= CLArgs.GetUpperBound(0); i++) InstallMod(CLArgs[i]);
    //    }

    //    //todo: if there already is another modman process running, send it a message to update its display

    //    try
    //    {
    //        //try to disallow multiple instances (probably only working under windows as well)
    //        if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1) Environment.Exit(0);
    //    }
    //    catch
    //    {
    //    }

    //    //initialize stuff
    //    string s;
    //    s = RegistryHelper.GetRegistryEntry("showversions");
    //    ShowVersionsInMainViewToolStripMenuItem.Checked = s == "yes";

    //    ReadDisplayNames();

    //    SetGameDir(GameHelper.DetectGameDir());
    //    GameHelper.CheckVersion(GameHelper.GameDir);
    //    m_gameVersion = GameHelper.Version.ToString();

    //    //restore window position
    //    s = RegistryHelper.GetRegistryEntry("left");
    //    if (s != "")
    //    {
    //        int l;
    //        if (int.TryParse(s, out l))
    //            Left = l;
    //    }

    //    s = RegistryHelper.GetRegistryEntry("top");
    //    if (s != "")
    //    {
    //        int t;
    //        if (int.TryParse(s, out t))
    //            Top = t;
    //    }

    //    s = RegistryHelper.GetRegistryEntry("width");
    //    if (s != "")
    //    {
    //        int w;
    //        if (int.TryParse(s, out w))
    //            Width = w;
    //    }

    //    s = RegistryHelper.GetRegistryEntry("height");
    //    if (s != "")
    //    {
    //        int h;
    //        if (int.TryParse(s, out h))
    //            Height = h;
    //    }

    //    //restore view
    //    i = -1;
    //    s = RegistryHelper.GetRegistryEntry("view");
    //    if (s != "") int.TryParse(s, out i);
    //    switch (i)
    //    {
    //        case 3:
    //            myListView.View = View.List;
    //            ListToolStripMenuItem.Checked = true;
    //            TilesToolStripMenuItem.Checked = false;
    //            SmallIconsToolStripMenuItem.Checked = false;
    //            break;
    //        case 2:
    //            myListView.View = View.SmallIcon;
    //            ListToolStripMenuItem.Checked = false;
    //            TilesToolStripMenuItem.Checked = false;
    //            SmallIconsToolStripMenuItem.Checked = true;
    //            break;
    //        case 0:
    //        case 4:
    //            myListView.View = View.Tile;
    //            ListToolStripMenuItem.Checked = false;
    //            TilesToolStripMenuItem.Checked = true;
    //            SmallIconsToolStripMenuItem.Checked = false;
    //            break;
    //    }

    //    //restore command line arguments
    //    m_runGameArguments = RegistryHelper.GetRegistryEntry("clargs");

    //    //restore file extension association
    //    if (string.IsNullOrEmpty(GameHelper.GameDir) && Environment.OSVersion.Platform == PlatformID.Win32NT)
    //    {
    //        s = RegistryHelper.GetRegistryEntry("fileextension");
    //        if (s == "yes")
    //        {
    //            RegistryHelper.RegisterFileExtension();
    //        }
    //        else if (s == "")
    //        {
    //            bool answer = await DisplayAlert("Do you want to associate the .honmod file extension with the HoN Mod Manager?" +
    //                    Environment.NewLine + Environment.NewLine +
    //                    "You can change this setting in the Options menu at any time.", "HoN_ModMan",
    //                    "Yes", "No");
    //            if (answer)
    //                RegistryHelper.RegisterFileExtension();
    //            else
    //                RegistryHelper.SetRegistryEntry("fileextension", "no");
    //        }
    //    }
    //    else
    //    {
    //        RegisterhonmodFileExtensionToolStripMenuItem.Enabled = false;
    //    }
    //}


    //private async void MainForm_Activated(object sender, EventArgs e)
    //{
    //    if (m_firstActivation)
    //    {
    //        m_firstActivation = false;

    //        //try to be helpful and suggest to directly apply mods if a patch was released since last mod applying
    //        if (m_gameVersion != "" && m_appliedGameVersion != "" & m_gameVersion != m_appliedGameVersion)
    //        {
    //            bool answer = await DisplayAlert("The HoN install was patched since you last applied the mods. For the mods to work correctly you need to apply them again. Do you want to do that right now?",
    //                     "Game Patch Detected", "Yes", "No");

    //            if (answer)
    //            {
    //                if (ApplyMods(true))
    //                {
    //                    bool launchHoN = await DisplayAlert("Great Success! Launch HoN now?", "Success", "Yes", "No");
    //                    //then suggest to start hon
    //                    if (launchHoN)
    //                    {
    //                        try
    //                        {
    //                            Environment.CurrentDirectory = GameHelper.GameDir;
    //                            Process.Start(GameHelper.GameFilePath, m_runGameArguments);
    //                            Close();
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                            await DisplayAlert("Could not launch HoN:" + Environment.NewLine + Environment.NewLine +
    //                                ex.Message, "HoN_ModMan", "Ok");
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    //private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    //{
    //    RegistryHelper.SetRegistryEntry("hondir", GameHelper.GameDir);
    //    RegistryHelper.SetRegistryEntry("clargs", m_runGameArguments);
    //    RegistryHelper.SetRegistryEntry("view", Convert.ToString(Convert.ToInt32(myListView.View)));
    //    if (ShowVersionsInMainViewToolStripMenuItem.Checked)
    //        RegistryHelper.SetRegistryEntry("showversions", "yes");
    //    else
    //        RegistryHelper.SetRegistryEntry("showversions", "no");
    //    StoreDisplayNames();

    //    //store window position
    //    if (WindowState == FormWindowState.Normal)
    //    {
    //        RegistryHelper.SetRegistryEntry("left", Left.ToString());
    //        RegistryHelper.SetRegistryEntry("top", Top.ToString());
    //        RegistryHelper.SetRegistryEntry("width", Width.ToString());
    //        RegistryHelper.SetRegistryEntry("height", Height.ToString());
    //    }

    //    //check whether applied == enabled
    //    if (e.CloseReason == CloseReason.UserClosing && !m_appliedMods.DeepCompareDictionary(m_enabledMods))
    //    {
    //        bool userResponse = await DisplayAlert("The enabled mods don't match the applied mods - do you want to apply mods now?",
    //                "Save Changes?", "Yes", "No");
    //        if (userResponse)
    //        {
    //            if (!ApplyMods(false)) e.Cancel = true;
    //        }
    //        else if (!userResponse)
    //        {
    //            e.Cancel = true;
    //        }
    //    }
    //}

    //private void ReadDisplayNames()
    //{
    //    m_displayNames.Clear();
    //    var s = RegistryHelper.GetRegistryEntry("displaynames").Split(Convert.ToChar(10));
    //    if (s.Length % 2 == 0)
    //        for (var i = 0; i <= s.GetUpperBound(0); i += 2)
    //        {
    //            s[i + 1] = s[i + 1].Trim();
    //            if (s[i + 1] != "") m_displayNames[s[i]] = s[i + 1];
    //        }
    //}

    //private void StoreDisplayNames()
    //{
    //    var tOutput = "";
    //    var First = true;
    //    foreach (var tName in m_displayNames)
    //    {
    //        if (!First) tOutput += Convert.ToChar(10);
    //        tOutput += tName.Key + Convert.ToChar(10) + tName.Value;
    //        First = false;
    //    }

    //    RegistryHelper.SetRegistryEntry("displaynames", tOutput);
    //}

    ////sets the specified dir as game dir and reloads mods and game version
    //private void SetGameDir(string NewGameDir)
    //{
    //    GameHelper.GameDir = NewGameDir;
    //    GameHelper.SetModsDir();
    //    GetAppliedMods();
    //    m_enabledMods = m_appliedMods.DeepCopyDictionary();
    //    //GetGameVersion();
    //    UpdateList();
    //}


    //private void GetAppliedMods()
    //{
    //    m_appliedMods = new Dictionary<string, string>();
    //    m_appliedGameVersion = "";
    //    if (GameHelper.GameDir == "") return;


    //    ZipFile tZip;
    //    try
    //    {
    //        tZip = ZipFile.Read(Path.Combine(GameHelper.ModsDir, "resources999.s2z"));
    //    }
    //    catch
    //    {
    //        return;
    //    }

    //    if (tZip == null) return;


    //    try
    //    {
    //        var Lines = tZip.Comment.Replace(Convert.ToChar(13).ToString(), "").Split(Convert.ToChar(10));
    //        //make sure the first few lines match our output format
    //        if (!(Lines[0].StartsWith("HoN Mod Manager v") && Lines[0].EndsWith(" Output") & (Lines[1] == "")))
    //            return;


    //        if (Lines[2].StartsWith("Game Version: "))
    //            m_appliedGameVersion = Lines[2].Substring("Game Version: ".Length);

    //        //find start of mod list
    //        var i = 2;
    //        while (Lines[i] != "Applied Mods: ") i += 1;
    //        i += 1;

    //        //collect mods
    //        do
    //        {
    //            var j = Lines[i].LastIndexOf(" (v");
    //            if (j >= 0 && Lines[i].EndsWith(")"))
    //                m_appliedMods[FixModName(Lines[i].Substring(0, j))] = Lines[i].Substring(j + 3,
    //                    Lines[i].Length - (j + 3) -
    //                    1);
    //            i += 1;
    //        } while (!(i == Lines.Length));
    //    }
    //    catch
    //    {
    //    }
    //    finally
    //    {
    //        tZip.Dispose();
    //    }
    //}

    //#endregion

    //#region " GUI "

    //private void UpdateEnabledCountLabel()
    //{
    //    if (GameHelper.GameDir == "")
    //    {
    //        myInfoLabel.Text = "";
    //        myInfoLabel.Visible = false;
    //    }
    //    else
    //    {
    //        myInfoLabel.Text = m_enabledCount + "/" + m_mods.Count + " mods enabled";
    //        myInfoLabel.Visible = true;
    //    }
    //}

    //private async void UnapplyAllModsToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    var OutPath = Path.Combine(GameHelper.ModsDir, "resources999.s2z");
    //    if (!File.Exists(OutPath))
    //    {
    //        await DisplayAlert("Currently no mods are applied!", "Unapply All Mods", "OK");
    //        return;
    //    }
    //    bool unapplyMods = await DisplayAlert("Are you sure?", "Unapply All Mods", "Yes", "No");
    //    if (unapplyMods)
    //    {
    //        File.Delete(OutPath);
    //        m_appliedMods.Clear();
    //        m_enabledMods.Clear();

    //        foreach (var tMod in m_mods)
    //        {
    //            tMod.Disabled = true;
    //            if (tMod.Icon != null)
    //            {
    //                var tBitmap = new Bitmap(tMod.Icon);
    //                if (tMod.Disabled) DisableIcon(tBitmap);
    //                if (tMod.IsUpdating) UpdatingIcon(tBitmap);
    //                myImageList.Images[tMod.ImageListIdx] = tBitmap;
    //            }
    //        }

    //        foreach (ListViewItem LVI in myListView.Items)
    //        {
    //            if (LVI.ImageIndex == 1) LVI.ImageIndex = 2;
    //            if (LVI.ImageIndex == -1) LVI.ImageIndex = 0;
    //        }

    //        myListView.Refresh();
    //        m_enabledCount = 0;
    //        // UpdateEnabledCountLabel();

    //        myListView_SelectedIndexChanged(null, null);
    //    }
    //}

    ////puts a general status message into the status bar
    //private void UpdateStatusLabel()
    //{
    //    if (GameHelper.GameDir == "")
    //    {
    //        myStatusLabel.Text = "Not attached to a valid HoN install.";
    //    }
    //    else
    //    {
    //        if (m_updatingMode)
    //        {
    //            var FinishedCount = 0;
    //            foreach (var tModUpdater in m_modUpdaters)
    //                if (tModUpdater.Status >= ModUpdaterStatus.NoUpdateInformation)
    //                    FinishedCount += 1;
    //            myStatusLabel.Text = "Updating... (" + FinishedCount + " of " + m_modUpdaters.Count + " done)";
    //        }
    //        else
    //        {
    //            myStatusLabel.Text = "Ready.";
    //        }
    //    }
    //}

    ////puts a mod specific status message into the status bar
    //private void UpdateStatusLabel(Modification tMod)
    //{
    //    if (tMod.IsUpdating)
    //        myStatusLabel.Text = tMod.Name + " v" + tMod.Version + " [Update progress: " +
    //                             tMod.Updater.StatusString +
    //                             "]";
    //    else
    //        myStatusLabel.Text = tMod.Name + " v" + tMod.Version;
    //}

    //#region " Updating related "

    //private void UpdateThisModToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    foreach (ListViewItem Item in myListView.SelectedItems)
    //    {
    //        var tMod = (Modification)Item.Tag;
    //        if (tMod.UpdateCheck != "" && tMod.UpdateDownload != "")
    //        {
    //            if (!m_updatingMode) EnterUpdatingMode();
    //            if (!tMod.IsUpdating && tMod.Updater == null)
    //            {
    //                if (tMod.Icon != null)
    //                {
    //                    var tBitmap = new Bitmap(tMod.Icon);
    //                    if (tMod.Disabled) DisableIcon(tBitmap);
    //                    UpdatingIcon(tBitmap);
    //                    myImageList.Images[tMod.ImageListIdx] = tBitmap;
    //                }
    //                else
    //                {
    //                    if (tMod.Disabled)
    //                        Item.ImageIndex = 2;
    //                    else
    //                        Item.ImageIndex = 1;
    //                }

    //                myListView.RedrawItems(Item.Index, Item.Index, true);

    //                tMod.Updater = new ModUpdater(tMod);
    //                if (myListView.SelectedItems.Count == 1) UpdateStatusLabel(tMod);

    //                m_modUpdaters.Add(tMod.Updater);
    //            }
    //        }
    //    }
    //}

    //private void CancelUpdateToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    if (!m_updatingMode) return;

    //    foreach (ListViewItem Item in myListView.SelectedItems)
    //    {
    //        var tMod = (Modification)Item.Tag;
    //        if (tMod.UpdateCheck != "" && tMod.UpdateDownload != "")
    //            if (tMod.IsUpdating)
    //                tMod.Updater.Abort();
    //    }
    //}

    ////This timer ticks when updating threads are running and finishes the updating once all threads are done
    //private void myUpdatingTimer_Tick(object sender, EventArgs e)
    //{
    //    if (!m_updatingMode) return;


    //    var FoundBusyUpdate = false;
    //    foreach (var tModUpdater in m_modUpdaters)
    //        if (tModUpdater.Status < ModUpdaterStatus.NoUpdateInformation)
    //        {
    //            FoundBusyUpdate = true;
    //        }
    //        else
    //        {
    //            if (!tModUpdater.Reaped)
    //            {
    //                //an update is done, fix the icon!
    //                var tMod = tModUpdater.Mod;
    //                int i;
    //                for (i = 0; i <= myListView.Items.Count; i++)
    //                    if (ReferenceEquals(myListView.Items[i].Tag, tMod))
    //                        break;
    //                if (tMod.Icon != null)
    //                {
    //                    var tBitmap = new Bitmap(tMod.Icon);
    //                    if (tMod.Disabled) DisableIcon(tBitmap);
    //                    myImageList.Images[tMod.ImageListIdx] = tBitmap;
    //                }
    //                else
    //                {
    //                    if (i < myListView.Items.Count)
    //                    {
    //                        if (tMod.Disabled)
    //                            myListView.Items[i].ImageIndex = 0;
    //                        else
    //                            myListView.Items[i].ImageIndex = -1;
    //                    }
    //                }

    //                if (i < myListView.Items.Count) myListView.RedrawItems(i, i, true);
    //                tModUpdater.Reaped = true;
    //            }
    //        }

    //    if (!FoundBusyUpdate)
    //    {
    //        LeaveUpdatingMode();
    //    }
    //    else
    //    {
    //        if (myListView.SelectedItems.Count == 1)
    //            UpdateStatusLabel((Modification)myListView.SelectedItems[0].Tag);
    //        else
    //            UpdateStatusLabel();
    //    }
    //}

    //private void UpdateAllModsToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    if (!m_updatingMode) EnterUpdatingMode();
    //    foreach (var tMod in m_mods)
    //        if (tMod.UpdateCheck != "" && (tMod.UpdateDownload != "") & (tMod.Updater == null))
    //        {
    //            if (tMod.Icon != null)
    //            {
    //                var tBitmap = new Bitmap(tMod.Icon);
    //                if (tMod.Disabled) DisableIcon(tBitmap);
    //                UpdatingIcon(tBitmap);
    //                myImageList.Images[tMod.ImageListIdx] = tBitmap;
    //            }
    //            else
    //            {
    //                int i;
    //                for (i = 0; i <= myListView.Items.Count; i++)
    //                    if (ReferenceEquals(myListView.Items[i].Tag, tMod))
    //                        break;
    //                if (tMod.Disabled)
    //                    myListView.Items[i].ImageIndex = 2;
    //                else
    //                    myListView.Items[i].ImageIndex = 1;
    //            }

    //            tMod.Updater = new ModUpdater(tMod);
    //            m_modUpdaters.Add(tMod.Updater);
    //        }

    //    myListView.Refresh();
    //    UpdateStatusLabel();
    //}

    //private void CancelAllUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    foreach (var tModUpdater in m_modUpdaters) tModUpdater.Abort();
    //}

    //#endregion

    //#region " ListView && Enabling/Disabling "

    //private void myListView_DoubleClick(object sender, EventArgs e)
    //{
    //    if (cmdToggleDisabled.Visible && cmdToggleDisabled.Enabled) cmdToggleDisabled_Click(null, null);
    //}

    //private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    foreach (ListViewItem Item in myListView.Items) Item.Selected = true;
    //}

    //private void myListView_KeyDown(object sender, KeyEventArgs e)
    //{
    //    if (e.KeyCode == Keys.Enter && cmdToggleDisabled.Visible & cmdToggleDisabled.Enabled)
    //        cmdToggleDisabled_Click(null, null);
    //}

    //private void myListView_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    if (myListView.SelectedItems.Count != 1)
    //    {
    //        lblName.Text = "";
    //        lblDescription.Text = "";
    //        lblDisabled.Visible = false;
    //        cmdToggleDisabled.Visible = false;
    //        if (myListView.SelectedItems.Count == 0)
    //            UpdateStatusLabel();
    //        else
    //            myStatusLabel.Text = myListView.SelectedItems.Count + " mods selected.";
    //    }
    //    else
    //    {
    //        var tMod = (Modification)myListView.SelectedItems[0].Tag;
    //        lblName.Text = tMod.Name;
    //        lblDescription.Text = "";
    //        if (tMod.Author != "") lblDescription.Text += "by " + tMod.Author + Environment.NewLine;
    //        lblDescription.Text += Environment.NewLine + tMod.Description;
    //        if (tMod.WebLink != "")
    //        {
    //            lblDescription.Text += Environment.NewLine + Environment.NewLine + "Visit Website";
    //            lblDescription.LinkArea = new LinkArea(lblDescription.Text.Length - "Visit Website".Length,
    //                "Visit Website".Length);
    //        }
    //        else
    //        {
    //            lblDescription.LinkArea = new LinkArea(0, 0);
    //        }

    //        if (tMod.Disabled)
    //        {
    //            lblDisabled.Text = "This mod is disabled.";
    //            lblDisabled.ForeColor = Color.Red;
    //            cmdToggleDisabled.Text = "&Enable";
    //        }
    //        else
    //        {
    //            lblDisabled.Text = "This mod is enabled.";
    //            lblDisabled.ForeColor = Color.Green;
    //            cmdToggleDisabled.Text = "&Disable";
    //        }

    //        lblDisabled.Visible = true;
    //        cmdToggleDisabled.Visible = true;
    //        UpdateStatusLabel(tMod);
    //    }
    //}

    //private void cmdToggleDisabled_Click(object sender, EventArgs e)
    //{
    //    if (myListView.SelectedItems.Count == 1)
    //    {
    //        if (((Modification)myListView.SelectedItems[0].Tag).Disabled)
    //            EnableSelected();
    //        else
    //            DisableSelected();
    //    }
    //}

    //private async void EnableSelected()
    //{
    //    var ToDo = new List<ListViewItem>();
    //    var Done = new List<ListViewItem>();
    //    foreach (ListViewItem Item in myListView.SelectedItems) ToDo.Add(Item);
    //    bool Change;

    //    do
    //    {
    //        foreach (var Item in Done) ToDo.Remove(Item);
    //        Done.Clear();

    //        Change = false;

    //        foreach (var Item in ToDo)
    //        {
    //            var tMod = Item.Tag as Modification;

    //            if (tMod.Enabled) continue;

    //            //check whether it can be enabled and output the reason if not

    //            //wrong modman / hon version
    //            if (!Tools.IsNewerVersion(tMod.MMVersion, Version.ToString()))
    //            {
    //                if (myListView.SelectedItems.Count == 1 && false)
    //                {
    //                    MessageBox.Show(
    //                        "This mod was written for HoN Mod Manager v" + tMod.MMVersion +
    //                        " or newer. You can obtain the newest version by visiting the website (see \"Help\" menu).",
    //                        "Cannot enable", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //                    return;
    //                }

    //                continue;
    //            }
    //            /*
    //            if (m_gameVersion != "" && tMod.AppVersion != "" && !VersionsMatch(tMod.AppVersion, m_gameVersion))
    //            {
    //                if (myListView.SelectedItems.Count == 1 && false)
    //                {
    //                    MessageBox.Show(
    //                        "This mod was written for Heroes of Newerth v" + tMod.AppVersion +
    //                        " and is thus not compatible with your current install.", "Cannot enable",
    //                        MessageBoxButtons.OK, MessageBoxIcon.Error);
    //                    return;
    //                }
    //                else
    //                {
    //                    continue;
    //                }
    //            }*/

    //            //requirements not met
    //            var Found = true;
    //            foreach (var tReq in tMod.Requirements)
    //            {
    //                Found = false;
    //                foreach (var tMod2 in m_mods)
    //                    if (tMod2.FixedName == tReq.Key)
    //                        if (tMod2.Enabled &&
    //                            VersionsMatch(tReq.Value.Substring(0, tReq.Value.IndexOf(' ')), tMod2.Version))
    //                        {
    //                            Found = true;
    //                            break;
    //                        }

    //                if (!Found)
    //                {
    //                    if (myListView.SelectedItems.Count == 1)
    //                    {
    //                        var tVersion = tReq.Value.Substring(0, tReq.Value.IndexOf(' '));
    //                        if (tVersion != "") tVersion = " v" + tVersion;
    //                        await DisplayAlert("This mod requires \"" + tReq.Value.Substring(tReq.Value.IndexOf(' ') + 1) + "\"" +
    //                            tVersion + " to be present and enabled." + Environment.NewLine +
    //                            "Visit this mod's website to find out where to obtain the required mod.",
    //                            "Cannot enable", "OK");
    //                        return;
    //                    }

    //                    break;
    //                }
    //            }

    //            if (!Found) continue;

    //            //incompatibilities postulated by this mod
    //            Found = false;
    //            foreach (var tInc in tMod.Incompatibilities)
    //            {
    //                foreach (var tMod2 in m_mods)
    //                    if (tMod2.Enabled && tMod2.FixedName == tInc.Key)
    //                        if (VersionsMatch(tInc.Value.Substring(0, tInc.Value.IndexOf(' ')), tMod2.Version))
    //                        {
    //                            if (myListView.SelectedItems.Count == 1)
    //                            {
    //                                var tVersion = tInc.Value.Substring(0, tInc.Value.IndexOf(' '));
    //                                if (tVersion != "") tVersion = " v" + tVersion;
    //                                await DisplayAlert("This mod is incompatible with \"" +
    //                                    tInc.Value.Substring(tInc.Value.IndexOf(' ') + 1) + "\"" + tVersion +
    //                                    ". You cannot have both enabled at the same time.", "Cannot enable", "OK");
    //                                return;
    //                            }

    //                            Found = true;
    //                            break;
    //                        }

    //                if (Found) break;
    //            }

    //            if (Found) continue;

    //            //incompatibilities postulated by other mods
    //            Found = false;
    //            foreach (var tMod2 in m_mods)
    //                if (tMod2.Enabled)
    //                {
    //                    foreach (var tInc in tMod2.Incompatibilities)
    //                        if (tMod.FixedName == tInc.Key)
    //                            if (VersionsMatch(tInc.Value.Substring(0, tInc.Value.IndexOf(' ')), tMod.Version))
    //                            {
    //                                if (myListView.SelectedItems.Count == 1)
    //                                {
    //                                    await DisplayAlert("This mod is incompatible with \"" + tMod2.Name + "\" v" +
    //                                        tMod2.Version + ". You cannot have both enabled at the same time.", "Cannot enable", "OK");
    //                                    return;
    //                                }

    //                                Found = true;
    //                                break;
    //                            }

    //                    if (Found) break;
    //                }

    //            if (Found) continue;

    //            //are we creating an ApplyFirst cycle?
    //            tMod.Enabled = true;
    //            var tMod3 = FindCycle();
    //            tMod.Enabled = false;
    //            if (tMod3 != null)
    //            {
    //                if (myListView.SelectedItems.Count == 1)
    //                {
    //                    await DisplayAlert(
    //                        "Enabling this mod would create a cycle in mod priorities. Try disabling other mods modifying similar aspects if you want to enable this one.",
    //                        "Cannot enable", "OK");
    //                    return;
    //                }

    //                continue;
    //            }
    //            ///' end of validity checks ''''

    //            Change = true;
    //            Done.Add(Item);

    //            m_enabledMods.Add(tMod.FixedName, tMod.Version);
    //            tMod.Disabled = false;
    //            Item.ForeColor = SystemColors.WindowText;

    //            if (tMod.Icon != null)
    //            {
    //                var tBitmap = new Bitmap(tMod.Icon);
    //                if (tMod.IsUpdating) UpdatingIcon(tBitmap);
    //                myImageList.Images[tMod.ImageListIdx] = tBitmap;
    //            }
    //            else
    //            {
    //                if (tMod.IsUpdating)
    //                    Item.ImageIndex = 1;
    //                else
    //                    Item.ImageIndex = -1;
    //            }

    //            if (myListView.SelectedItems.Count == 1)
    //            {
    //                lblDisabled.Text = "This mod is enabled.";
    //                lblDisabled.ForeColor = Color.Green;
    //                cmdToggleDisabled.Text = "&Disable";
    //            }

    //            m_enabledCount += 1;
    //            myListView.RedrawItems(Item.Index, Item.Index, true);
    //            //myListView.Refresh()
    //        }
    //    } while (!!Change);

    //    UpdateEnabledCountLabel();
    //}

    //private async void DisableSelected()
    //{
    //    var ToDo = new List<ListViewItem>();
    //    var Done = new List<ListViewItem>();
    //    foreach (ListViewItem Item in myListView.SelectedItems) ToDo.Add(Item);
    //    bool Change;

    //    do
    //    {
    //        foreach (var Item in Done) ToDo.Remove(Item);
    //        Done.Clear();

    //        Change = false;

    //        foreach (var Item in ToDo)
    //        {
    //            var tMod = (Modification)Item.Tag;

    //            if (tMod.Disabled) continue;

    //            //check whether another mod requires this mod
    //            var Found = false;
    //            foreach (var tMod2 in m_mods)
    //                if (tMod2.Enabled)
    //                {
    //                    foreach (var tReq in tMod2.Requirements)
    //                        if (tMod.FixedName == tReq.Key)
    //                        {
    //                            if (myListView.SelectedItems.Count == 1)
    //                            {
    //                                await DisplayAlert(
    //                                    "The mod \"" + tMod2.Name +
    //                                    "\" requires this mod to be enabled. Disable it first if you want to disable this one.",
    //                                    "Cannot disable", "OK");
    //                                return;
    //                            }

    //                            Found = true;
    //                            break;
    //                        }

    //                    if (Found) break;
    //                }

    //            if (Found) continue;
    //            ///' end of validity checks ''''

    //            Change = true;
    //            Done.Add(Item);

    //            m_enabledMods.Remove(tMod.FixedName);
    //            tMod.Disabled = true;
    //            Item.ForeColor = Color.Red;

    //            if (tMod.Icon != null)
    //            {
    //                var tBitmap = new Bitmap(tMod.Icon);
    //                DisableIcon(tBitmap);
    //                if (tMod.IsUpdating) UpdatingIcon(tBitmap);
    //                myImageList.Images[tMod.ImageListIdx] = tBitmap;
    //            }
    //            else
    //            {
    //                if (tMod.IsUpdating)
    //                    Item.ImageIndex = 2;
    //                else
    //                    Item.ImageIndex = 0;
    //            }

    //            if (myListView.SelectedItems.Count == 1)
    //            {
    //                lblDisabled.Text = "This mod is disabled.";
    //                lblDisabled.ForeColor = Color.Red;
    //                cmdToggleDisabled.Text = "&Enable";
    //            }

    //            m_enabledCount -= 1;
    //            myListView.RedrawItems(Item.Index, Item.Index, true);
    //            //myListView.Refresh()
    //        }
    //    } while (!!Change);

    //    UpdateEnabledCountLabel();
    //}

    //private void RenameToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    if (myListView.SelectedItems.Count == 1)
    //    {
    //        var tMod = (Modification)myListView.SelectedItems[0].Tag;
    //        if (!m_displayNames.ContainsKey(tMod.Name))
    //            myListView.SelectedItems[0].Text = tMod.Name;
    //        else
    //            myListView.SelectedItems[0].Text = m_displayNames[tMod.Name];
    //        myListView.LabelEdit = true;
    //        myListView.SelectedItems[0].BeginEdit();
    //    }
    //}

    //private void myListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
    //{
    //    e.CancelEdit = true;
    //    myListView.LabelEdit = false;

    //    var DoSort = false;
    //    var tMod = (Modification)myListView.Items[e.Item].Tag;
    //    if (e.Label != null)
    //    {
    //        if (e.Label.Trim() == "")
    //            m_displayNames.Remove(tMod.Name);
    //        else
    //            m_displayNames[tMod.Name] = e.Label.Trim();
    //        DoSort = true;
    //    }

    //    if (ShowVersionsInMainViewToolStripMenuItem.Checked && tMod.Version != "")
    //    {
    //        if (!m_displayNames.ContainsKey(tMod.Name))
    //            myListView.Items[e.Item].Text = tMod.Name + " (v" + tMod.Version + ")";
    //        else
    //            myListView.Items[e.Item].Text = m_displayNames[tMod.Name] + " (v" + tMod.Version + ")";
    //    }
    //    else
    //    {
    //        if (!m_displayNames.ContainsKey(tMod.Name))
    //            myListView.Items[e.Item].Text = tMod.Name;
    //        else
    //            myListView.Items[e.Item].Text = m_displayNames[tMod.Name];
    //    }

    //    if (DoSort) myListView.Sort();
    //}

    //private void ResetNameToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    foreach (ListViewItem Item in myListView.SelectedItems)
    //    {
    //        var tMod = (Modification)Item.Tag;
    //        if (m_displayNames.Remove(tMod.Name))
    //        {
    //            if (ShowVersionsInMainViewToolStripMenuItem.Checked && tMod.Version != "")
    //                Item.Text = tMod.Name + " (v" + tMod.Version + ")";
    //            else
    //                Item.Text = tMod.Name;
    //            myListView.Sort();
    //        }
    //    }
    //}

    //#endregion

    //#region " Menus "

    //private void myContextMenu_Opening(object sender, CancelEventArgs e)
    //{
    //    if (myListView.SelectedItems.Count == 0)
    //    {
    //        e.Cancel = true;
    //        myEmptyContextMenu.Show(myContextMenu.Left, myContextMenu.Top);
    //    }
    //    else if (myListView.SelectedItems.Count == 1)
    //    {
    //        var tMod = (Modification)myListView.SelectedItems[0].Tag;
    //        if (tMod.Disabled)
    //            EnableDisableToolStripMenuItem.Text = "En&able";
    //        else
    //            EnableDisableToolStripMenuItem.Text = "Dis&able";
    //        EnableDisableToolStripMenuItem.Visible = true;
    //        EnableAllToolStripMenuItem.Visible = false;
    //        DisableAllToolStripMenuItem.Visible = false;
    //        RenameToolStripMenuItem.Visible = true;
    //        ResetNameToolStripMenuItem.Visible = m_displayNames.ContainsKey(tMod.Name);
    //        ResetNameToolStripMenuItem.Text = "&Reset Name";
    //        ExportAss2zToolStripMenuItem.Visible = tMod.Enabled;
    //        ToolStripMenuItem11.Visible = tMod.Enabled;
    //        if (tMod.Requirements.Count > 0)
    //        {
    //            ExportAss2zToolStripMenuItem.Enabled = false;
    //            ExportAss2zToolStripMenuItem.Text = "Cannot Export as .s2z";
    //        }
    //        else
    //        {
    //            ExportAss2zToolStripMenuItem.Enabled = true;
    //            ExportAss2zToolStripMenuItem.Text = "Export as .s2&z ...";
    //        }

    //        if ((tMod.UpdateCheck == "") | (tMod.UpdateDownload == ""))
    //        {
    //            UpdateThisModToolStripMenuItem.Enabled = false;
    //            UpdateThisModToolStripMenuItem.Text = "This mod is not updatable.";
    //            UpdateThisModToolStripMenuItem.Visible = true;
    //            CancelUpdateToolStripMenuItem.Visible = false;
    //        }
    //        else
    //        {
    //            if (tMod.IsUpdating)
    //            {
    //                UpdateThisModToolStripMenuItem.Enabled = false;
    //                UpdateThisModToolStripMenuItem.Visible = false;
    //                CancelUpdateToolStripMenuItem.Text = "Cancel &Update";
    //                CancelUpdateToolStripMenuItem.Visible = true;
    //            }
    //            else
    //            {
    //                if (tMod.Updater == null)
    //                {
    //                    UpdateThisModToolStripMenuItem.Enabled = true;
    //                    UpdateThisModToolStripMenuItem.Text = "&Update this Mod";
    //                }
    //                else
    //                {
    //                    UpdateThisModToolStripMenuItem.Enabled = false;
    //                    UpdateThisModToolStripMenuItem.Text = "Update " + tMod.Updater.StatusString;
    //                }

    //                UpdateThisModToolStripMenuItem.Visible = true;
    //                CancelUpdateToolStripMenuItem.Visible = false;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        var AllRequirementsSelected = true;
    //        var TotalCount = 0;
    //        var EnabledCount = 0;
    //        var DisabledCount = 0;
    //        var RenamedCount = 0;
    //        var UpdatableCount = 0;
    //        var UpdatingCount = 0;
    //        var UpdatedCount = 0;
    //        TotalCount = myListView.SelectedItems.Count;
    //        foreach (ListViewItem Item in myListView.SelectedItems)
    //        {
    //            var tMod = (Modification)Item.Tag;
    //            if (m_displayNames.ContainsKey(tMod.Name)) RenamedCount += 1;
    //            if (tMod.Enabled) EnabledCount += 1;
    //            if (tMod.Disabled) DisabledCount += 1;
    //            if (tMod.UpdateDownload != "" && tMod.UpdateCheck != "") UpdatableCount += 1;
    //            if (tMod.IsUpdating) UpdatingCount += 1;
    //            if (!tMod.IsUpdating && tMod.Updater != null) UpdatedCount += 1;

    //            if (AllRequirementsSelected)
    //                foreach (var tReq in tMod.Requirements)
    //                {
    //                    var Found = false;
    //                    foreach (ListViewItem Item2 in myListView.SelectedItems)
    //                    {
    //                        var tMod2 = (Modification)Item2.Tag;
    //                        if (tReq.Key == tMod2.FixedName)
    //                        {
    //                            Found = true;
    //                            break;
    //                        }
    //                    }

    //                    if (!Found)
    //                    {
    //                        AllRequirementsSelected = false;
    //                        break;
    //                    }
    //                }
    //        }

    //        EnableDisableToolStripMenuItem.Visible = false;
    //        EnableAllToolStripMenuItem.Visible = DisabledCount > 0;
    //        DisableAllToolStripMenuItem.Visible = EnabledCount > 0;
    //        RenameToolStripMenuItem.Visible = false;
    //        ResetNameToolStripMenuItem.Visible = RenamedCount > 0;
    //        ResetNameToolStripMenuItem.Text = "&Reset Names";
    //        ExportAss2zToolStripMenuItem.Visible = DisabledCount == 0;
    //        ToolStripMenuItem11.Visible = DisabledCount == 0;

    //        if (DisabledCount == 0)
    //        {
    //            if (!AllRequirementsSelected)
    //            {
    //                ExportAss2zToolStripMenuItem.Enabled = false;
    //                ExportAss2zToolStripMenuItem.Text = "Cannot Export as .s2z";
    //            }
    //            else
    //            {
    //                ExportAss2zToolStripMenuItem.Enabled = true;
    //                ExportAss2zToolStripMenuItem.Text = "Export as .s2&z ...";
    //            }
    //        }

    //        if (UpdatableCount == 0)
    //        {
    //            UpdateThisModToolStripMenuItem.Enabled = false;
    //            UpdateThisModToolStripMenuItem.Text = "These mods are not updatable.";
    //            UpdateThisModToolStripMenuItem.Visible = true;
    //            CancelUpdateToolStripMenuItem.Visible = false;
    //        }
    //        else
    //        {
    //            if (UpdatingCount > 0)
    //            {
    //                CancelUpdateToolStripMenuItem.Text = "Cancel &Updates (" + UpdatingCount + ")";
    //                CancelUpdateToolStripMenuItem.Visible = true;
    //            }
    //            else
    //            {
    //                CancelUpdateToolStripMenuItem.Visible = false;
    //            }

    //            if (UpdatableCount - UpdatingCount - UpdatedCount > 0)
    //            {
    //                UpdateThisModToolStripMenuItem.Enabled = true;
    //                UpdateThisModToolStripMenuItem.Text = "&Update these Mods";
    //                UpdateThisModToolStripMenuItem.Visible = true;
    //            }
    //            else
    //            {
    //                UpdateThisModToolStripMenuItem.Enabled = false;
    //                UpdateThisModToolStripMenuItem.Visible = false;
    //            }
    //        }
    //    }
    //}

    //private async void AboutToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    await DisplayAlert("Heroes of Newerth Modification Manager " + Version + " by Notausgang" + Environment.NewLine +
    //        Environment.NewLine + "Great game by S2 Games", "HoN_ModMan", "OK");
    //}

    //private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    Close();
    //}

    //private void ApplyModsToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    ApplyMods();
    //}

    //private async Task ApplyModsAndLaunchHoNToolStripMenuItem_ClickAsync(object sender, EventArgs e)
    //{
    //    if (ApplyMods(true))
    //        try
    //        {
    //            Environment.CurrentDirectory = GameHelper.GameDir;
    //            Process.Start(GameHelper.GameFilePath, m_runGameArguments);
    //            Close();
    //        }
    //        catch (Exception ex)
    //        {
    //            await DisplayAlert("Could not launch HoN:" + Environment.NewLine + Environment.NewLine + ex.Message, "HoN_ModMan", "OK");
    //        }
    //}

    //private void EnableDisableToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    if (cmdToggleDisabled.Enabled) cmdToggleDisabled_Click(null, null);
    //}

    //private void EnableAllToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    EnableSelected();
    //}

    //private void DisableAllToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    DisableSelected();
    //}

    //private async void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    var DeletionHappened = false;
    //    foreach (ListViewItem Item in myListView.SelectedItems)
    //    {
    //        var tMod = (Modification)Item.Tag;
    //        bool answer = await DisplayAlert("Are you sure you want to permanently delete " + Path.GetFileName(tMod.File) + "?",
    //                "Delete Mod", "Yes", "No");
    //        if (answer)
    //        {
    //            ForgetZip(tMod.File);
    //            try
    //            {
    //                File.Delete(tMod.File);
    //                DeletionHappened = true;
    //            }
    //            catch
    //            {
    //                await DisplayAlert("Could not delete " + tMod.File + "!", "Delete Mod", "OK");
    //            }
    //        }
    //    }

    //    if (DeletionHappened)
    //    {
    //        UpdateList();
    //    }
    //}

    //private void RefreshModDisplayToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    UpdateList();
    //}

    //private void OpenModFolderToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    ForgetAllZIPs();
    //    Process.Start(Path.Combine(GameHelper.ModsDir, "mods"));
    //}

    //private void ListToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    myListView.View = View.List;
    //    ListToolStripMenuItem.Checked = true;
    //    TilesToolStripMenuItem.Checked = false;
    //    SmallIconsToolStripMenuItem.Checked = false;
    //}

    //private void TilesToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    myListView.View = View.Tile;
    //    ListToolStripMenuItem.Checked = false;
    //    TilesToolStripMenuItem.Checked = true;
    //    SmallIconsToolStripMenuItem.Checked = false;
    //}

    //private void SmallIconsToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    myListView.View = View.SmallIcon;
    //    ListToolStripMenuItem.Checked = false;
    //    TilesToolStripMenuItem.Checked = false;
    //    SmallIconsToolStripMenuItem.Checked = true;
    //}

    //private void ShowVersionsInMainViewToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    ShowVersionsInMainViewToolStripMenuItem.Checked = !ShowVersionsInMainViewToolStripMenuItem.Checked;
    //    UpdateList();
    //}

    //private void CLArgumentsForLaunchingHoNToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    /*frmInputbox myDialog = new frmInputbox();
    //    myDialog.Text = "Command line arguments to use when launching HoN:";
    //    myDialog.Result = m_runGameArguments;
    //    if (myDialog.ShowDialog() == DialogResult.OK)
    //    {
    //        m_runGameArguments = myDialog.Result;
    //    }*/
    //}

    //private void ExportAss2zToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    var myDialog = new SaveFileDialog();
    //    myDialog.Title = "Export as .s2z";
    //    myDialog.Filter = "s2z archive (*.s2z)|*.s2z|All Files (*.*)|*";
    //    myDialog.FileName = "resources_mods.s2z";
    //    if (myDialog.ShowDialog() == DialogResult.OK) ApplyMods(false, myDialog.FileName);
    //}


    //private void RegisterhonmodFileExtensionToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    if (RegisterhonmodFileExtensionToolStripMenuItem.Text == "Register .honmod File Extension")
    //        RegistryHelper.RegisterFileExtension();
    //    else
    //        RegistryHelper.UnregisterFileExtension();
    //}

    //#endregion

    //#region " WebBrowser Related Stuff "

    //private void lblDescription_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    //{
    //    LaunchWebBrowser(((Modification)myListView.SelectedItems[0].Tag).WebLink);
    //}

    //private void ForumThreadToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    LaunchWebBrowser("https://github.com/Xen0byte/Heroes-Of-Newerth-Mod-Manager");
    //}

    //private static void LaunchWebBrowser(string URL)
    //{
    //    if (!URL.StartsWith("http://") && !URL.StartsWith("https://")) URL = "http://" + URL;
    //    try
    //    {
    //        Process.Start(URL);
    //    }
    //    catch
    //    {
    //    }
    //}

    //#endregion

    //#region " DragDrop "

    ////on what platforms and under which conditions exactly this works is unknown

    //private void MainForm_DragEnter(object sender, DragEventArgs e)
    //{
    //    if (!myListView.Enabled)
    //    {
    //        e.Effect = DragDropEffects.None;
    //        return;
    //    }

    //    var tPath = Path.Combine(GameHelper.ModsDir, "mods");
    //    if (Directory.Exists(tPath) &&
    //        e.Data.GetDataPresent(DataFormats.FileDrop) | e.Data.GetDataPresent("FileNameW") |
    //        e.Data.GetDataPresent("FileName"))
    //        e.Effect = DragDropEffects.Copy;
    //    else
    //        e.Effect = DragDropEffects.None;
    //}

    //private void MainForm_DragDrop(object sender, DragEventArgs e)
    //{
    //    if (!myListView.Enabled) return;


    //    var tPath = Path.Combine(GameHelper.ModsDir, "mods");
    //    if (Directory.Exists(tPath))
    //    {
    //        string[] tFiles = null;
    //        try
    //        {
    //            if (e.Data.GetDataPresent(DataFormats.FileDrop))
    //            {
    //                e.Effect = DragDropEffects.Copy;
    //                tFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
    //            }
    //            else if (e.Data.GetDataPresent("FileNameW"))
    //            {
    //                e.Effect = DragDropEffects.Copy;
    //                tFiles = (string[])e.Data.GetData("FileNameW");
    //            }
    //            else if (e.Data.GetDataPresent("FileName"))
    //            {
    //                e.Effect = DragDropEffects.Copy;
    //                tFiles = (string[])e.Data.GetData("FileName");
    //            }
    //        }
    //        catch
    //        {
    //            tFiles = null;
    //        }

    //        if (tFiles != null)
    //        {
    //            foreach (var SourceFile in tFiles) InstallMod(SourceFile);
    //            UpdateList();
    //        }
    //    }
    //}

    //private async void InstallMod(string SourceFile)
    //{
    //    if (Path.GetExtension(SourceFile) == ".honmod" &&
    //        Path.GetDirectoryName(SourceFile) != Path.Combine(GameHelper.ModsDir, "mods"))
    //        try
    //        {
    //            var DestFile = Path.Combine(Path.Combine(GameHelper.ModsDir, "mods"),
    //                Path.GetFileName(SourceFile));
    //            bool answer = await DisplayAlert(Path.GetFileName(SourceFile) + " already exists. Overwrite?", "HoN_ModMan",
    //                    "Yes", "No");
    //            if (File.Exists(DestFile) && !answer)
    //            {
    //                return;
    //            }

    //            ForgetZip(DestFile);
    //            File.Copy(SourceFile, DestFile, true);
    //        }
    //        catch
    //        {
    //            await DisplayAlert("Could not copy the file.", "An error occured", "OK");
    //        }
    //}

    //#endregion

    //#endregion

    //#region " UpdateList() "

    //private static string FixModName(string s)
    //{
    //    //returns lowercase s minus all non alphanumeric characters
    //    var myOutput = "";
    //    for (var i = 0; i <= s.Length - 1; i++)
    //        if (char.IsLetterOrDigit(s[i]))
    //            myOutput += s[i];
    //    return myOutput.ToLower();
    //}

    //private static Bitmap DisableIcon(Bitmap b)
    //{
    //    //greys out the given bitmap and paints disabled.png on top of it
    //    int i;
    //    int j;
    //    int t;
    //    Color c;
    //    for (i = 0; i <= IconWidth - 1; i++)
    //        for (j = 0; j <= IconHeight - 1; j++)
    //        {
    //            c = b.GetPixel(i, j);
    //            t = (Convert.ToInt32(c.R) + Convert.ToInt32(c.G) + Convert.ToInt32(c.B)) / 3;
    //            b.SetPixel(i, j, Color.FromArgb(c.A, t, t, t));
    //        }

    //    Graphics.FromImage(b).DrawImageUnscaled(Resources.disabled, 0, 0);
    //    return b;
    //}

    //private void UpdateList()
    //{
    //    //loads and displays all valid mods/*.honmod files. also makes sure requirement/incompatibility/applybefore/applyafter are not violated == the set of enabled mods are actually appliable
    //    //is called upon program load, "F5" and before applying mods

    //    if (GameHelper.ModsDir == null) return;

    //    lblName.Text = "";
    //    lblDescription.Text = "";
    //    lblDisabled.Visible = false;
    //    cmdToggleDisabled.Visible = false;
    //    myListView.Items.Clear();
    //    //myListView.Sorting = SortOrder.None
    //    m_mods.Clear();
    //    m_enabledCount = 0;
    //    myImageList.Images.Clear();
    //    myImageList.Images.Add(Resources.disabled);
    //    myImageList.Images.Add(Resources.updating);
    //    myImageList.Images.Add(UpdatingIcon(new Bitmap(Resources.disabled)));

    //    if (GameHelper.GameDir == "")
    //    {
    //        myListView.Enabled = false;
    //        myStatusLabel.Text = "Not attached to a valid HoN install.";
    //        RefreshModDisplayToolStripMenuItem.Enabled = false;
    //        ApplyModsToolStripMenuItem.Enabled = false;
    //        ApplyModsAndLaunchHoNToolStripMenuItem.Enabled = false;
    //        UpdateAllModsToolStripMenuItem.Enabled = false;
    //        OpenModFolderToolStripMenuItem.Enabled = false;
    //        UnapplyAllModsToolStripMenuItem.Enabled = false;
    //        return;
    //    }

    //    RefreshModDisplayToolStripMenuItem.Enabled = true;
    //    ApplyModsAndLaunchHoNToolStripMenuItem.Enabled = GameHelper.GameFilePath != "";
    //    ApplyModsToolStripMenuItem.Enabled = true;
    //    UpdateAllModsToolStripMenuItem.Enabled = true;
    //    OpenModFolderToolStripMenuItem.Enabled = true;
    //    UnapplyAllModsToolStripMenuItem.Enabled = true;

    //    var tPath = Path.Combine(GameHelper.ModsDir, "mods");
    //    if (!Directory.Exists(tPath) && Directory.Exists(GameHelper.ModsDir))
    //        Directory.CreateDirectory(tPath);

    //    if (Directory.Exists(tPath))
    //        foreach (var tFile in Directory.GetFiles(tPath, "*.honmod"))
    //            try
    //            {
    //                var tZip = GetZip(tFile);
    //                if (tZip == null) continue;

    //                var tStream = GetZippedFile(tZip, "icon.png");
    //                Bitmap tBitmap = null;
    //                if (tStream != null)
    //                    try
    //                    {
    //                        tBitmap = new Bitmap(tStream);
    //                        if ((tBitmap.Width != IconWidth) | (tBitmap.Height != IconHeight)) tBitmap = null;
    //                    }
    //                    catch
    //                    {
    //                    }

    //                tStream = GetZippedFile(tZip, "mod.xml");
    //                if (tStream == null)
    //                {
    //                    ForgetZip(tFile);
    //                    continue;
    //                }

    //                var myXmlReader = new XmlTextReader(tStream);
    //                myXmlReader.WhitespaceHandling = WhitespaceHandling.None;
    //                while (!((myXmlReader.NodeType == XmlNodeType.Element) & (myXmlReader.Name == "modification")))
    //                    if (!myXmlReader.Read())
    //                        throw new Exception("Unexpected EOF in " + Path.Combine(tFile, "mod.xml"));
    //                if (myXmlReader.IsEmptyElement) throw new Exception("Empty modification element.");

    //                if (myXmlReader.GetAttribute("application") != "Heroes of Newerth")
    //                {
    //                    ForgetZip(tFile);
    //                    continue;
    //                }

    //                var tMod = new Modification();
    //                tMod.File = tFile;
    //                tMod.Name = myXmlReader.GetAttribute("name");
    //                if (tMod.Name == "")
    //                {
    //                    ForgetZip(tFile);
    //                    continue;
    //                }

    //                tMod.FixedName = FixModName(tMod.Name);
    //                tMod.Version = myXmlReader.GetAttribute("version").Replace(" ", "");

    //                var FoundNewer = false;
    //                Modification FoundOlder = null;
    //                foreach (var tMod2 in m_mods)
    //                    if (tMod2.FixedName == tMod.FixedName)
    //                    {
    //                        if (Tools.IsNewerVersion(tMod2.Version, tMod.Version))
    //                            FoundOlder = tMod2;
    //                        else
    //                            FoundNewer = true;
    //                        break;
    //                    }

    //                if (FoundNewer)
    //                {
    //                    ForgetZip(tFile);
    //                    continue;
    //                }

    //                if (FoundOlder != null) m_mods.Remove(FoundOlder);

    //                tMod.Description = myXmlReader.GetAttribute("description");
    //                if (tMod.Description != null)
    //                    tMod.Description = tMod.Description.Replace(Convert.ToChar(13), ' ');
    //                tMod.Author = myXmlReader.GetAttribute("author");
    //                tMod.WebLink = myXmlReader.GetAttribute("weblink");
    //                tMod.UpdateCheck = myXmlReader.GetAttribute("updatecheckurl");
    //                tMod.UpdateDownload = myXmlReader.GetAttribute("updatedownloadurl");
    //                tMod.Icon = tBitmap;
    //                tMod.AppVersion = myXmlReader.GetAttribute("appversion");
    //                tMod.MMVersion = myXmlReader.GetAttribute("mmversion");
    //                if (tMod.MMVersion == "")
    //                {
    //                    ForgetZip(tFile);
    //                    continue;
    //                }

    //                tMod.MMVersion = tMod.MMVersion.Replace('*', '0');
    //                tMod.Disabled = !m_enabledMods.ContainsKey(tMod.FixedName) ||
    //                                !Tools.IsNewerVersion(tMod.MMVersion, Version.ToString()) ||
    //                                GameHelper.Version.ToString() != "" && tMod.AppVersion != "" &&
    //                                !VersionsMatch(tMod.AppVersion, GameHelper.Version.ToString());

    //                var AlreadyRead = false;
    //                while (!((myXmlReader.NodeType == XmlNodeType.EndElement) &
    //                         (myXmlReader.Name == "modification")))
    //                {
    //                    if (!AlreadyRead && !myXmlReader.Read())
    //                        throw new Exception("Unexpected EOF in " + Path.Combine(tFile, "mod.xml"));
    //                    AlreadyRead = false;
    //                    if (myXmlReader.NodeType == XmlNodeType.Element)
    //                    {
    //                        string tVersion;
    //                        switch (myXmlReader.Name)
    //                        {
    //                            case "incompatibility":
    //                                tVersion = myXmlReader.GetAttribute("version");
    //                                if (tVersion != null) tVersion = tVersion.Replace(" ", "");
    //                                tMod.Incompatibilities.Add(FixModName(myXmlReader.GetAttribute("name")),
    //                                    tVersion + " " + myXmlReader.GetAttribute("name"));
    //                                break;
    //                            case "requirement":
    //                                tVersion = myXmlReader.GetAttribute("version");
    //                                if (tVersion != null) tVersion = tVersion.Replace(" ", "");
    //                                tMod.Requirements.Add(FixModName(myXmlReader.GetAttribute("name")),
    //                                    tVersion + " " + myXmlReader.GetAttribute("name"));
    //                                break;
    //                            case "applybefore":
    //                                tMod.ApplyBefore.Add(FixModName(myXmlReader.GetAttribute("name")),
    //                                    myXmlReader.GetAttribute("version"));
    //                                break;
    //                            case "applyafter":
    //                                tMod.ApplyAfter.Add(FixModName(myXmlReader.GetAttribute("name")),
    //                                    myXmlReader.GetAttribute("version"));
    //                                break;
    //                        }

    //                        if (!myXmlReader.IsEmptyElement)
    //                        {
    //                            myXmlReader.Skip();
    //                            AlreadyRead = true;
    //                        }
    //                    }
    //                }

    //                var VString = "";
    //                if (tMod.Version != "" && ShowVersionsInMainViewToolStripMenuItem.Checked)
    //                    VString = " (v" + tMod.Version + ")";
    //                if (tMod.Icon == null)
    //                {
    //                    tMod.ImageListIdx = -1;
    //                }
    //                else
    //                {
    //                    tBitmap = new Bitmap(tMod.Icon);
    //                    if (tMod.Disabled) DisableIcon(tBitmap);
    //                    myImageList.Images.Add(tBitmap);
    //                    tMod.ImageListIdx = myImageList.Images.Count - 1;
    //                }

    //                ListViewItem v;
    //                if (tMod.ImageListIdx == -1 && tMod.Disabled)
    //                {
    //                    if (m_displayNames.ContainsKey(tMod.Name))
    //                        v = myListView.Items.Add(m_displayNames[tMod.Name] + VString, 0);
    //                    else
    //                        v = myListView.Items.Add(tMod.Name + VString, 0);
    //                }
    //                else
    //                {
    //                    if (m_displayNames.ContainsKey(tMod.Name))
    //                        v = myListView.Items.Add(m_displayNames[tMod.Name] + VString, tMod.ImageListIdx);
    //                    else
    //                        v = myListView.Items.Add(tMod.Name + VString, tMod.ImageListIdx);
    //                }

    //                v.Tag = tMod;
    //                if (tMod.Disabled)
    //                    v.ForeColor = Color.Red;
    //                else
    //                    v.ForeColor = SystemColors.WindowText;

    //                tMod.Index = m_mods.Count;
    //                m_mods.Add(tMod);
    //                if (tMod.Enabled) m_enabledCount += 1;
    //            }
    //            catch
    //            {
    //            }

    //    FillApplyFirst();
    //    //prepare graph for traversing

    //    bool Change;
    //    do
    //    {
    //        Change = false;
    //        foreach (var tMod in m_mods)
    //            if (tMod.Enabled)
    //                foreach (var tReq in tMod.Requirements)
    //                {
    //                    var Found = false;
    //                    foreach (var tMod2 in m_mods)
    //                        if (tMod2.FixedName == tReq.Key)
    //                            if (tMod2.Enabled &&
    //                                VersionsMatch(tReq.Value.Substring(0, tReq.Value.IndexOf(' ')), tMod2.Version))
    //                            {
    //                                Found = true;
    //                                break;
    //                            }

    //                    if (!Found)
    //                    {
    //                        tMod.Disabled = true;
    //                        m_enabledCount -= 1;
    //                        if (tMod.Icon != null)
    //                        {
    //                            var tBitmap = new Bitmap(tMod.Icon);
    //                            DisableIcon(tBitmap);
    //                            myImageList.Images[tMod.ImageListIdx] = tBitmap;
    //                        }
    //                        else
    //                        {
    //                            myListView.Items[tMod.Index].ImageIndex = 0;
    //                        }

    //                        myListView.Items[tMod.Index].ForeColor = Color.Red;
    //                        Change = true;
    //                        break;
    //                    }
    //                }

    //        foreach (var tMod in m_mods)
    //            if (tMod.Enabled)
    //                foreach (var tInc in tMod.Incompatibilities)
    //                {
    //                    var Found = false;
    //                    foreach (var tMod2 in m_mods)
    //                        if (tMod2.FixedName == tInc.Key)
    //                            if (tMod2.Enabled &&
    //                                VersionsMatch(tInc.Value.Substring(0, tInc.Value.IndexOf(' ')), tMod2.Version))
    //                            {
    //                                Found = true;
    //                                break;
    //                            }

    //                    if (Found)
    //                    {
    //                        tMod.Disabled = true;
    //                        m_enabledCount -= 1;
    //                        if (tMod.Icon != null)
    //                        {
    //                            var tBitmap = new Bitmap(tMod.Icon);
    //                            DisableIcon(tBitmap);
    //                            myImageList.Images[tMod.ImageListIdx] = tBitmap;
    //                        }
    //                        else
    //                        {
    //                            myListView.Items[tMod.Index].ImageIndex = 0;
    //                        }

    //                        myListView.Items[tMod.Index].ForeColor = Color.Red;
    //                        Change = true;
    //                        break;
    //                    }
    //                }

    //        //don't do the graph traversing until you've ruled the obvious out
    //        if (!Change)
    //        {
    //            var tMod = FindCycle();
    //            if (tMod != null)
    //            {
    //                tMod.Disabled = true;
    //                m_enabledCount -= 1;
    //                if (tMod.Icon != null)
    //                {
    //                    var tBitmap = new Bitmap(tMod.Icon);
    //                    DisableIcon(tBitmap);
    //                    myImageList.Images[tMod.ImageListIdx] = tBitmap;
    //                }
    //                else
    //                {
    //                    myListView.Items[tMod.Index].ImageIndex = 0;
    //                }

    //                myListView.Items[tMod.Index].ForeColor = Color.Red;
    //                Change = true;
    //            }
    //        }
    //    } while (Change);

    //    //finally screw up the .Index values and sort by alphabet
    //    //myListView.Sorting = SortOrder.Ascending
    //    myListView.Sort();
    //    //myListView.EnsureVisible(0)

    //    myStatusLabel.Text = m_mods.Count + " mods loaded.";
    //    UpdateEnabledCountLabel();

    //    //Rebuild m_enabledMods; we might have read new versions or were forced to disable some mods
    //    m_enabledMods = new Dictionary<string, string>();
    //    foreach (var tMod in m_mods)
    //        if (tMod.Enabled)
    //            m_enabledMods.Add(tMod.FixedName, tMod.Version);
    //}

    //#endregion

    //#region " Cycle Detection "

    //private void FillApplyFirst()
    //{
    //    foreach (var tMod in m_mods)
    //    {
    //        foreach (var tReq in tMod.Requirements)
    //            foreach (var tMod2 in m_mods)
    //                if (tMod2.FixedName == tReq.Key &&
    //                    VersionsMatch(tReq.Value.Substring(0, tReq.Value.IndexOf(' ')), tMod2.Version))
    //                    if (!tMod.ApplyFirst.Contains(tMod2))
    //                        tMod.ApplyFirst.Add(tMod2);
    //        foreach (var tAppBefore in tMod.ApplyBefore)
    //            foreach (var tMod2 in m_mods)
    //                if (tMod2.FixedName == tAppBefore.Key && VersionsMatch(tAppBefore.Value, tMod2.Version))
    //                    if (!tMod2.ApplyFirst.Contains(tMod))
    //                        tMod2.ApplyFirst.Add(tMod);
    //        foreach (var tAppAfter in tMod.ApplyAfter)
    //            foreach (var tMod2 in m_mods)
    //                if (tMod2.FixedName == tAppAfter.Key && VersionsMatch(tAppAfter.Value, tMod2.Version))
    //                    if (!tMod.ApplyFirst.Contains(tMod2))
    //                        tMod.ApplyFirst.Add(tMod2);
    //    }
    //}

    //private Modification FindCycle()
    //{
    //    //returns nothing if no cycle was found in the graph described by the ApplyFirst graph
    //    //if a cycle was found a mod that is part of the cycle is returned
    //    //disabled mods are ignored
    //    foreach (var tMod in m_mods)
    //        if (tMod.Enabled)
    //        {
    //            //clear marked flags
    //            foreach (var tMod2 in m_mods) tMod2.Marked = false;

    //            if (TraverseApplyFirstGraph(tMod)) return tMod;
    //        }

    //    return null;
    //}

    //private bool TraverseApplyFirstGraph(Modification StartingNode)
    //{
    //    //returns true iif one of the outgoing edges points to a modification that has already been marked (thus, a cycle was found)
    //    //disabled mods are ignored
    //    if (StartingNode.Marked) return true;
    //    StartingNode.Marked = true;
    //    foreach (var tMod in StartingNode.ApplyFirst)
    //        if (tMod.Enabled)
    //            if (TraverseApplyFirstGraph(tMod))
    //                return true;
    //    //depth first search, we're lazy
    //    StartingNode.Marked = false;
    //    //this crucial line was missing in 1.2.1
    //    return false;
    //}

    //#endregion

    //#region " Actual work [Applying mods] "

    //private bool ApplyMods()
    //{
    //    return ApplyMods(false, null);
    //}

    //private bool ApplyMods(string ExportPath)
    //{
    //    return ApplyMods(false, ExportPath);
    //}

    //private bool ApplyMods(bool SilentSucces)
    //{
    //    return ApplyMods(SilentSucces, null);
    //}

    //private async bool ApplyMods(bool SilentSuccess, string ExportPath)
    //{
    //    //if ExportPath is set, we are not "applying mods", but "exporting to s2z" (currently selected mods only)
    //    StringCollection SelectedMods = null;
    //    if (ExportPath != "")
    //    {
    //        SelectedMods = new StringCollection();
    //        foreach (ListViewItem Item in myListView.SelectedItems)
    //            if (Item.Selected)
    //            {
    //                var tMod = (Modification)Item.Tag;
    //                SelectedMods.Add(tMod.FixedName);
    //            }
    //    }

    //    myListView.Enabled = false;
    //    UpdateList();
    //    //make sure our data about mods is up to date

    //    if (GameHelper.GameDir == "") return false;

    //    //restore the selection
    //    if (SelectedMods != null)
    //        foreach (var tMod in SelectedMods)
    //            foreach (ListViewItem Item in myListView.Items)
    //            {
    //                var tMod2 = (Modification)Item.Tag;
    //                if (tMod2.FixedName == tMod)
    //                {
    //                    Item.Selected = true;
    //                    break;
    //                }
    //            }

    //    myStatusLabel.Text = "Busy ...";
    //    myStatusStrip.Refresh();

    //    //create ourselves a list of mods to apply, and order the mods according to set requirements
    //    var ModList = new List<Modification>();
    //    if (SelectedMods == null || SelectedMods.Count == 0)
    //    {
    //        foreach (var tMod in m_mods)
    //            if (tMod.Enabled)
    //                ModList.Add(tMod);
    //    }
    //    else
    //    {
    //        foreach (var SelectedMod in SelectedMods)
    //        {
    //            var Found = false;
    //            foreach (var tMod in m_mods)
    //                if (tMod.FixedName == SelectedMod)
    //                {
    //                    ModList.Add(tMod);
    //                    Found = true;
    //                }

    //            if (!Found)
    //            {
    //                //error out
    //                await DisplayAlert("A mod vanished!", "HoN_ModMan", "OK");
    //                myStatusLabel.Text = m_mods.Count + " mods loaded.";
    //                return false;
    //            }
    //        }
    //    }

    //    var i = 0;
    //    while (i < ModList.Count)
    //    {
    //        var tMod = ModList[i];
    //        foreach (var ApplyFirst in tMod.ApplyFirst)
    //            if (ModList.Contains(ApplyFirst))
    //            {
    //                var Found = false;
    //                for (var j = 0; j <= i; j++)
    //                    if (ReferenceEquals(ModList[j], ApplyFirst))
    //                    {
    //                        Found = true;
    //                        break;
    //                    }

    //                if (!Found)
    //                {
    //                    ModList.RemoveAt(i);
    //                    ModList.Add(tMod);
    //                    i -= 1;
    //                    break;
    //                }
    //            }

    //        i += 1;
    //    }

    //    //get a handle to resources0.s2z
    //    //ZipFile resources0 = GetZip(Path.Combine(Path.Combine(GameHelper.GameDir, "game"), "resources0.s2z"));
    //    var resources = new List<ZipFile>();
    //    resources.Add(GetZip(Path.Combine(Path.Combine(GameHelper.GameDir, "game"), "resources0.s2z")));
    //    resources.Add(GetZip(Path.Combine(Path.Combine(GameHelper.GameDir, "game"), "resources1.s2z")));
    //    resources.Add(GetZip(Path.Combine(Path.Combine(GameHelper.GameDir, "game"), "resources2.s2z")));
    //    resources.Add(GetZip(Path.Combine(Path.Combine(GameHelper.GameDir, "game"), "resources3.s2z")));
    //    /*
    //    if (resources0 == null)
    //    {
    //        MessageBox.Show("Could not open resources0.s2z!", "HoN_ModMan", MessageBoxButtons.OK,
    //                        MessageBoxIcon.Error);
    //        myStatusLabel.Text = m_mods.Count + " mods loaded.";
    //        return false;
    //    }
    //    */

    //    var tModName = "";
    //    try
    //    {
    //        var OutFiles = new Dictionary<string, Stream>();
    //        //this will hold all files that were already modified
    //        var FileVersions = new Dictionary<string, string>();
    //        //key: file name, value: version; for use with <copyfile version="..." overwrite="..."/>

    //        //this will be the zip comment to be added to resources999.s2z
    //        string CommentString;
    //        if (ExportPath == null)
    //            CommentString = "HoN Mod Manager v" + Version + " Output" + Environment.NewLine;
    //        else
    //            CommentString = "HoN Mod Manager v" + Version + " Export" + Environment.NewLine;
    //        if (GameHelper.Version.ToString() != "")
    //            CommentString += Environment.NewLine + "Game Version: " + GameHelper.Version + Environment.NewLine;
    //        CommentString += Environment.NewLine + "Applied Mods: ";

    //        if (ModList.Count > 0 && GameHelper.Version.ToString() != "" && ExportPath == null)
    //        {
    //            var Done = false;

    //            //add a panel to the main menu to remind people to re-apply mods after a patch was released
    //            var tStream = GetZippedFile(resources, "ui/main.interface");
    //            if (tStream != null)
    //            {
    //                Encoding Encoding = null;
    //                var s = Decode(tStream, ref Encoding);
    //                var tPos = s.IndexOf("<panel name=\"quit_confirm\"");
    //                if (tPos >= 0)
    //                {
    //                    tPos = s.IndexOf("</panel>", tPos);
    //                    if (tPos >= 0)
    //                    {
    //                        tPos += "</panel>".Length;
    //                        //Convert.ToChar(13) & Convert.ToChar(10) & Convert.ToChar(13) & Convert.ToChar(10) & Convert.ToChar(9) &
    //                        s = s.Substring(0, tPos) +
    //                            Resources.ModsOodReminder.Replace("%%%%", GameHelper.Version.ToString()) +
    //                            s.Substring(tPos);
    //                        OutFiles["ui/main.interface"] = Encode(s, Encoding);
    //                        Done = true;
    //                    }
    //                }
    //            }

    //            //same thing for FE2
    //            tStream = GetZippedFile(resources, "ui/fe2/main.interface");
    //            if (tStream != null)
    //            {
    //                Encoding Encoding = null;
    //                var s = Decode(tStream, ref Encoding);
    //                s = s.Replace("CallEvent('event_login',1);",
    //                    "CallEvent('event_login',1); Trigger('modsood_check');");

    //                tStream = GetZippedFile(resources, "ui/fe2/social_groups.package");
    //                if (tStream != null)
    //                {
    //                    Encoding Encoding2 = null;
    //                    var s2 = Decode(tStream, ref Encoding2);
    //                    var tPos = s2.IndexOf("</package>");
    //                    if (tPos >= 0)
    //                    {
    //                        s2 = s2.Substring(0, tPos) +
    //                             Resources.ModsOodReminderFE2.Replace("%%%%", GameHelper.Version.ToString()) +
    //                             s2.Substring(tPos);
    //                        OutFiles["ui/fe2/main.interface"] = Encode(s, Encoding);
    //                        OutFiles["ui/fe2/social_groups.package"] = Encode(s2, Encoding);
    //                        Done = true;
    //                    }
    //                }
    //            }

    //            if (!Done)
    //            {
    //                //FE2, in case they decide to unbox the files into /ui/ root
    //                tStream = GetZippedFile(resources, "ui/main.interface");
    //                if (tStream != null)
    //                {
    //                    Encoding Encoding = null;
    //                    var s = Decode(tStream, ref Encoding);
    //                    s = s.Replace("CallEvent('event_login',1);",
    //                        "CallEvent('event_login',1); Trigger('modsood_check');");

    //                    tStream = GetZippedFile(resources, "ui/social_groups.package");
    //                    if (tStream != null)
    //                    {
    //                        Encoding Encoding2 = null;
    //                        var s2 = Decode(tStream, ref Encoding2);
    //                        var tPos = s2.IndexOf("</package>");
    //                        if (tPos >= 0)
    //                        {
    //                            s2 = s2.Substring(0, tPos) +
    //                                 Resources.ModsOodReminderFE2.Replace("%%%%", GameHelper.Version.ToString()) +
    //                                 s2.Substring(tPos);
    //                            OutFiles["ui/main.interface"] = Encode(s, Encoding);
    //                            OutFiles["ui/social_groups.package"] = Encode(s2, Encoding);
    //                            Done = true;
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        foreach (var tMod in ModList)
    //        {
    //            CommentString += Environment.NewLine + tMod.Name + " (v" + tMod.Version + ")";

    //            tModName = tMod.Name;
    //            var ModZip = GetZip(tMod.File);
    //            if (ModZip == null) throw new Exception("Could not open " + tMod.File);

    //            var ModXMLPath = Path.Combine(tMod.File, "mod.xml");
    //            //only used for cleaner exceptions
    //            var tStream = GetZippedFile(ModZip, "mod.xml");
    //            if (tStream == null) throw new Exception("Could not open " + ModXMLPath);
    //            var myXmlReader = new XmlTextReader(tStream);
    //            myXmlReader.WhitespaceHandling = WhitespaceHandling.None;
    //            while (!(myXmlReader.NodeType == XmlNodeType.Element && myXmlReader.Name == "modification"))
    //                if (!myXmlReader.Read())
    //                    throw new Exception(ModXMLPath + " does not contain a modification.");
    //            var AlreadyRead = false;
    //            while (!(myXmlReader.NodeType == XmlNodeType.EndElement && myXmlReader.Name == "modification"))
    //            {
    //                if (!AlreadyRead && !myXmlReader.Read())
    //                    throw new Exception(
    //                        "Unexpected EOF at line " + myXmlReader.LineNumber + " of " + ModXMLPath);
    //                AlreadyRead = false;
    //                if (myXmlReader.NodeType == XmlNodeType.Element)
    //                    switch (myXmlReader.Name)
    //                    {
    //                        case "copyfile":
    //                            bool ConditionSatisfied;
    //                            try
    //                            {
    //                                ConditionSatisfied = EvalCondition(myXmlReader.GetAttribute("condition"),
    //                                    ModList);
    //                            }
    //                            catch
    //                            {
    //                                throw new Exception("Invalid condition string at line " +
    //                                                    myXmlReader.LineNumber +
    //                                                    " of " + ModXMLPath);
    //                            }


    //                            if (ConditionSatisfied)
    //                            {
    //                                if (!myXmlReader.IsEmptyElement)
    //                                    throw new Exception("Non-empty copyfile tag at line " +
    //                                                        myXmlReader.LineNumber + " of " + ModXMLPath);
    //                                var Destination = FixFilename(myXmlReader.GetAttribute("name"));
    //                                if (Destination == "")
    //                                    throw new Exception("copyfile tag without name attribute at line " +
    //                                                        myXmlReader.LineNumber + " of " + ModXMLPath);
    //                                var Source = FixFilename(myXmlReader.GetAttribute("source"));
    //                                if (Source == "") Source = Destination;

    //                                var AlreadyExists = OutFiles.ContainsKey(Destination);
    //                                var CancelWrite = false;
    //                                var tVersion = myXmlReader.GetAttribute("version");
    //                                if (AlreadyExists)
    //                                    switch (myXmlReader.GetAttribute("overwrite"))
    //                                    {
    //                                        case "newer":
    //                                            if (FileVersions[Destination] != "")
    //                                                CancelWrite =
    //                                                    !Tools.IsNewerVersion(FileVersions[Destination], tVersion);
    //                                            break;

    //                                        case "yes":
    //                                        case "no":
    //                                            CancelWrite = true;
    //                                            break;
    //                                        default:
    //                                            throw new Exception("File \"" + Destination +
    //                                                                "\" already exists! Non-overwriting write issued by line " +
    //                                                                myXmlReader.LineNumber + " of " + ModXMLPath);
    //                                    }

    //                                if (!CancelWrite)
    //                                {
    //                                    if (tVersion != "") FileVersions[Destination] = tVersion;

    //                                    tStream = GetZippedFile(ModZip, Source);
    //                                    if (tStream == null)
    //                                        throw new Exception("File \"" + Source + "\" referenced at line " +
    //                                                            myXmlReader.LineNumber + " of " + ModXMLPath +
    //                                                            " not found");

    //                                    OutFiles[Destination] = tStream;
    //                                }
    //                            }

    //                            break;

    //                        case "editfile":
    //                            if (!myXmlReader.IsEmptyElement)
    //                            {
    //                                bool _ConditionSatisfied;
    //                                try
    //                                {
    //                                    _ConditionSatisfied = EvalCondition(myXmlReader.GetAttribute("condition"),
    //                                        ModList);
    //                                }
    //                                catch
    //                                {
    //                                    throw new Exception("Invalid condition string at line " +
    //                                                        myXmlReader.LineNumber + " of " + ModXMLPath);
    //                                }

    //                                if (!_ConditionSatisfied)
    //                                {
    //                                    myXmlReader.Skip();
    //                                    AlreadyRead = true;
    //                                }
    //                                else
    //                                {
    //                                    var File = FixFilename(myXmlReader.GetAttribute("name"));
    //                                    if (File == "")
    //                                        throw new Exception("editfile tag without name attribute at line " +
    //                                                            myXmlReader.LineNumber + " of " + ModXMLPath);
    //                                    var AlreadyExists = OutFiles.ContainsKey(File);
    //                                    if (AlreadyExists)
    //                                    {
    //                                        tStream = OutFiles[File];
    //                                        tStream.Seek(0, SeekOrigin.Begin);
    //                                    }
    //                                    else
    //                                    {
    //                                        tStream = null;
    //                                    }

    //                                    string s;
    //                                    Encoding Encoding = null;
    //                                    if (tStream != null)
    //                                    {
    //                                        s = Decode(tStream, ref Encoding);
    //                                    }
    //                                    else
    //                                    {
    //                                        tStream = GetZippedFile(resources, File);
    //                                        if (tStream == null)
    //                                            throw new Exception("File \"" + File + "\" referenced at line " +
    //                                                                myXmlReader.LineNumber + " of " + ModXMLPath +
    //                                                                " not found");
    //                                        s = Decode(tStream, ref Encoding);
    //                                    }

    //                                    int Pos1;
    //                                    int Pos2;
    //                                    var FindAll = "";

    //                                    Pos1 = 0;
    //                                    Pos2 = 0;
    //                                    while (
    //                                        !(myXmlReader.NodeType == XmlNodeType.EndElement &&
    //                                          myXmlReader.Name == "editfile"))
    //                                    {
    //                                        if (!myXmlReader.Read())
    //                                            throw new Exception("Unexpected EOF at line " +
    //                                                                myXmlReader.LineNumber + " of " + ModXMLPath);
    //                                        if (myXmlReader.NodeType == XmlNodeType.Element)
    //                                        {
    //                                            var Operation = myXmlReader.Name;
    //                                            var Position = myXmlReader.GetAttribute("position");
    //                                            var Source = FixFilename(myXmlReader.GetAttribute("source"));
    //                                            if (Source != "")
    //                                            {
    //                                                tStream = GetZippedFile(ModZip, Source);
    //                                                if (tStream == null)
    //                                                    throw new Exception("File \"" + Source +
    //                                                        "\" referenced at line " +
    //                                                        myXmlReader.LineNumber + " of " +
    //                                                        ModXMLPath + " not found");
    //                                                Encoding tEnc = null;
    //                                                Source = Decode(tStream, ref tEnc);
    //                                            }
    //                                            else if (myXmlReader.IsEmptyElement)
    //                                            {
    //                                                Source = "";
    //                                            }
    //                                            else
    //                                            {
    //                                                Source = "";
    //                                                //special case: if there is exactly one CDATA child node and all other nodes are whitespace-only text nodes, then ignore the text nodes
    //                                                var SpecialCase = true;
    //                                                string CDATA = null;
    //                                                if (!Tools.IsNewerVersion("1.2", tMod.MMVersion))
    //                                                    SpecialCase = false;
    //                                                //this special case did not exist prior to 1.2
    //                                                //(normal case: join the contents of all text and cdata nodes together)
    //                                                while (
    //                                                    !(myXmlReader.NodeType == XmlNodeType.EndElement &&
    //                                                      myXmlReader.Name == Operation))
    //                                                {
    //                                                    if (!myXmlReader.Read())
    //                                                        throw new Exception("Unexpected EOF at line " +
    //                                                            myXmlReader.LineNumber + " of " +
    //                                                            ModXMLPath);
    //                                                    if (myXmlReader.NodeType == XmlNodeType.Element)
    //                                                        throw new Exception(ModXMLPath + ", line " +
    //                                                            myXmlReader.LineNumber +
    //                                                            ": Cannot have sub-elements in operation elements!");
    //                                                    if (myXmlReader.NodeType == XmlNodeType.Text)
    //                                                    {
    //                                                        var tRead =
    //                                                            myXmlReader.Value.Replace(Convert.ToChar(13), ' ');
    //                                                        if (tRead.Trim() != "") SpecialCase = false;
    //                                                        Source += tRead;
    //                                                    }

    //                                                    if (myXmlReader.NodeType == XmlNodeType.CDATA)
    //                                                    {
    //                                                        var tRead =
    //                                                            myXmlReader.Value.Replace(Convert.ToChar(13), ' ');
    //                                                        if (CDATA != null) SpecialCase = false;
    //                                                        CDATA = tRead;
    //                                                        Source += tRead;
    //                                                    }
    //                                                }

    //                                                if (SpecialCase && CDATA != null) Source = CDATA;
    //                                            }

    //                                            switch (Operation)
    //                                            {
    //                                                case "find":
    //                                                case "seek":
    //                                                case "search":
    //                                                case "findup":
    //                                                case "seekup":
    //                                                case "searchup":
    //                                                    var Up = Operation.EndsWith("up");
    //                                                    if (FindAll != "")
    //                                                        throw new Exception(
    //                                                            "findall operation not followed by insert, add, replace or delete at line " +
    //                                                            myXmlReader.LineNumber + " of " + ModXMLPath);

    //                                                    if (Source != "")
    //                                                    {
    //                                                        if (Up)
    //                                                        {
    //                                                            Pos1 -= 1;
    //                                                            if (Pos1 >= 0)
    //                                                                Pos1 = s.LastIndexOf(Source, Pos1 - 1);
    //                                                        }
    //                                                        else
    //                                                        {
    //                                                            Pos1 = s.IndexOf(Source, Pos2);
    //                                                        }

    //                                                        if (Pos1 < 0)
    //                                                        {
    //                                                            var SoughtDisplay = "";
    //                                                            foreach (
    //                                                                var Line in Source.Split(Convert.ToChar(10)))
    //                                                            {
    //                                                                var Line2 = Line.Trim();
    //                                                                if (Line2 != "")
    //                                                                {
    //                                                                    SoughtDisplay = Line2;
    //                                                                    break;
    //                                                                }
    //                                                            }

    //                                                            throw new Exception(
    //                                                                "Could not find string starting with \"" +
    //                                                                SoughtDisplay + "\" as sought by line " +
    //                                                                myXmlReader.LineNumber + " of " + ModXMLPath +
    //                                                                Environment.NewLine + Environment.NewLine +
    //                                                                "This may be caused by the mod being outdated or by an incompatibility with another enabled mod.");
    //                                                        }

    //                                                        Pos2 = Pos1 + Source.Length;
    //                                                    }
    //                                                    else
    //                                                    {
    //                                                        switch (Position)
    //                                                        {
    //                                                            case "before":
    //                                                            case "begin":
    //                                                            case "start":
    //                                                            case "head":
    //                                                                Pos1 = 0;
    //                                                                Pos2 = 0;
    //                                                                break;
    //                                                            case "after":
    //                                                            case "end":
    //                                                            case "tail":
    //                                                            case "eof":
    //                                                                Pos1 = s.Length;
    //                                                                Pos2 = Pos1;
    //                                                                break;
    //                                                            case "":
    //                                                                throw new Exception(
    //                                                                    "find operation without parameters at line " +
    //                                                                    myXmlReader.LineNumber + " of " +
    //                                                                    ModXMLPath);
    //                                                            default:
    //                                                                int tInt;
    //                                                                if (int.TryParse(Position, out tInt))
    //                                                                {
    //                                                                    if (Up)
    //                                                                        Pos1 -= tInt;
    //                                                                    else
    //                                                                        Pos1 += tInt;
    //                                                                    if (Pos1 < 0) Pos1 = 0;
    //                                                                    if (Pos1 > s.Length) Pos1 = s.Length;
    //                                                                    if (Up)
    //                                                                        Pos2 -= tInt;
    //                                                                    else
    //                                                                        Pos2 += tInt;
    //                                                                    if (Pos2 < 0) Pos2 = 0;
    //                                                                    if (Pos2 > s.Length) Pos2 = s.Length;
    //                                                                }
    //                                                                else
    //                                                                {
    //                                                                    throw new Exception(
    //                                                                        "find operation with invalid position parameter at line " +
    //                                                                        myXmlReader.LineNumber + " of " +
    //                                                                        ModXMLPath);
    //                                                                }

    //                                                                break;
    //                                                        }
    //                                                    }

    //                                                    break;

    //                                                case "insert":
    //                                                case "add":
    //                                                    if (FindAll == "")
    //                                                    {
    //                                                        if (Position == "before")
    //                                                        {
    //                                                            s = s.Substring(0, Pos1) + Source +
    //                                                                s.Substring(Pos1);
    //                                                            Pos2 = Pos1 + Source.Length;
    //                                                        }
    //                                                        else if (Position == "after")
    //                                                        {
    //                                                            s = s.Substring(0, Pos2) + Source +
    //                                                                s.Substring(Pos2);
    //                                                            Pos1 = Pos2;
    //                                                            Pos2 = Pos1 + Source.Length;
    //                                                        }
    //                                                        else
    //                                                        {
    //                                                            throw new Exception(
    //                                                                "insert operation without position attribute at line " +
    //                                                                myXmlReader.LineNumber + " of " + ModXMLPath);
    //                                                        }
    //                                                    }
    //                                                    else
    //                                                    {
    //                                                        if (Position == "before")
    //                                                            s = s.Replace(FindAll, Source + FindAll);
    //                                                        else if (Position == "after")
    //                                                            s = s.Replace(FindAll, FindAll + Source);
    //                                                        else
    //                                                            throw new Exception(
    //                                                                "insert operation without position attribute at line " +
    //                                                                myXmlReader.LineNumber + " of " + ModXMLPath);
    //                                                        FindAll = "";
    //                                                        Pos1 = 0;
    //                                                        Pos2 = 0;
    //                                                    }

    //                                                    break;

    //                                                case "replace":
    //                                                    if (FindAll == "")
    //                                                    {
    //                                                        s = s.Substring(0, Pos1) + Source + s.Substring(Pos2);
    //                                                        Pos2 = Pos1 + Source.Length;
    //                                                    }
    //                                                    else
    //                                                    {
    //                                                        s = s.Replace(FindAll, Source);
    //                                                        FindAll = "";
    //                                                        Pos1 = 0;
    //                                                        Pos2 = 0;
    //                                                    }

    //                                                    break;

    //                                                case "delete":
    //                                                    if (FindAll == "")
    //                                                    {
    //                                                        s = s.Substring(0, Pos1) + s.Substring(Pos2);
    //                                                        Pos2 = Pos1;
    //                                                    }
    //                                                    else
    //                                                    {
    //                                                        s = s.Replace(FindAll, "");
    //                                                        FindAll = "";
    //                                                        Pos1 = 0;
    //                                                        Pos2 = 0;
    //                                                    }

    //                                                    break;

    //                                                case "findall":
    //                                                    if (FindAll != "")
    //                                                        throw new Exception(
    //                                                            "findall operation not followed by insert, add, replace or delete at line " +
    //                                                            myXmlReader.LineNumber + " of " + ModXMLPath);

    //                                                    if (Source == "")
    //                                                        throw new Exception(
    //                                                            "findall operation without search term at line " +
    //                                                            myXmlReader.LineNumber + " of " + ModXMLPath);

    //                                                    FindAll = Source;
    //                                                    break;
    //                                                default:
    //                                                    throw new Exception("Unknown operation \"" + Operation +
    //                                                        "\" at line " + myXmlReader.LineNumber +
    //                                                        " of " + ModXMLPath);
    //                                            }
    //                                        }
    //                                    }

    //                                    OutFiles[File] = Encode(s, Encoding);
    //                                }
    //                            }

    //                            break;

    //                        case "editxmlfile":
    //                            throw new Exception("editxmlfile as issued by line " + myXmlReader.LineNumber +
    //                                                " of " + ModXMLPath + " is not yet implemented!");
    //                        case "requirement":
    //                        case "incompatibility":
    //                        case "applybefore":
    //                        case "applyafter":
    //                            break;
    //                        default:
    //                            throw new Exception("Unknown element \"" + myXmlReader.Name + "\" at line " +
    //                                                myXmlReader.LineNumber + " of " + ModXMLPath);
    //                    }
    //            }
    //        }

    //        tModName = "";

    //        var OutFile = new ZipFile();
    //        OutFile.CompressionLevel = CompressionLevel.BestCompression;

    //        foreach (var File in OutFiles)
    //        {
    //            File.Value.Seek(0, SeekOrigin.Begin);
    //            OutFile.AddEntry(File.Key, null, File.Value);
    //        }

    //        OutFile.Comment = CommentString;
    //        if (ExportPath == null)
    //        {
    //            OutFile.Save(Path.Combine(GameHelper.ModsDir, "resources999.s2z"));

    //            m_appliedMods = m_enabledMods.DeepCopyDictionary();
    //            m_appliedGameVersion = GameHelper.Version.ToString();
    //        }
    //        else
    //        {
    //            OutFile.Save(ExportPath);
    //        }

    //        myStatusLabel.Text = "Great Success!";
    //        if (!SilentSuccess)
    //            await DisplayAlert("Great Success!", "Success", "OK");
    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        if (tModName == "")
    //        {
    //            await DisplayAlert("A problem occurred: " + Environment.NewLine + Environment.NewLine + ex.Message,
    //                "HoN_ModMan", "OK");
    //        }
    //        else
    //        {
    //            await DisplayAlert("\"" + tModName + "\" caused a problem: " + Environment.NewLine + Environment.NewLine +
    //                ex.Message, "HoN_ModMan", "OK");
    //            return false;
    //        }
    //    }
    //    finally
    //    {
    //        myStatusLabel.Text = m_mods.Count + " mods loaded.";
    //        myListView.Enabled = true;
    //    }
    //}

    //private static bool EvalCondition(string Condition, List<Modification> Mods)
    //{
    //    if (Condition == null || Condition == "") return true;

    //    Condition = Condition.TrimStart();

    //    //check for prepended not
    //    var Negate = false;
    //    if (Condition.StartsWith("not", true, null))
    //    {
    //        Negate = true;
    //        Condition = Condition.Substring("not".Length).TrimStart();
    //    }

    //    //process a token or a block enclosed in parentheses
    //    bool TokenValue;
    //    if (Condition.StartsWith("\'"))
    //    {
    //        var TokenEnd = Condition.IndexOf('\'', 1);
    //        //note that there may not be any single quotes in the string! (otherwise: exception)

    //        //read token
    //        var ModName = Condition.Substring(1, TokenEnd - 1);
    //        var Version = "*";
    //        //there might be a version string specified!
    //        if (ModName.EndsWith("]"))
    //        {
    //            var i = ModName.LastIndexOf("[v");
    //            if (i >= 0)
    //            {
    //                Version = ModName.Substring(i + 2, ModName.Length - (i + 2) - 1);
    //                ModName = ModName.Substring(0, i);
    //            }
    //        }

    //        //evaluate its value
    //        TokenValue = false;
    //        ModName = FixModName(ModName);
    //        foreach (var tMod in Mods)
    //            if (tMod.FixedName == ModName && VersionsMatch(Version, tMod.Version))
    //            {
    //                TokenValue = true;
    //                break;
    //            }

    //        Condition = Condition.Substring(TokenEnd + 1);
    //    }
    //    else if (Condition.StartsWith("("))
    //    {
    //        //search for the matching closing counterpart
    //        var Parentheses = 1;
    //        var Quotes = false;
    //        int i;
    //        //note that this will throw an exception if we unexpectedly reach the end of the string
    //        for (i = 1; i <= Condition.Length; i++)
    //            switch (Condition[i])
    //            {
    //                case '(':
    //                    if (!Quotes) Parentheses += 1;
    //                    break;
    //                case ')':
    //                    if (!Quotes) Parentheses -= 1;
    //                    if (Parentheses == 0) break;
    //                    break;

    //                case '\'':
    //                    Quotes = !Quotes;
    //                    break;
    //            }

    //        TokenValue = EvalCondition(Condition.Substring(1, i - 1), Mods);
    //        Condition = Condition.Substring(i + 1);
    //    }
    //    else
    //    {
    //        throw new Exception();
    //    }

    //    //apply possibly prepended not
    //    TokenValue = TokenValue ^ Negate;

    //    //proceed with the remainder
    //    Condition = Condition.TrimStart();
    //    if (Condition == "") return TokenValue;

    //    if (Condition.StartsWith("and", true, null))
    //    {
    //        if (!TokenValue)
    //            return false;
    //        return EvalCondition(Condition.Substring("and".Length), Mods);
    //    }

    //    if (Condition.StartsWith("or", true, null))
    //    {
    //        if (TokenValue)
    //            return true;
    //        return EvalCondition(Condition.Substring("or".Length), Mods);
    //    }

    //    return true;
    //}

    //private static string FixFilename(string s)
    //{
    //    //changes filenames to style found in ZIP files; these are used as index of the dictionary of edited files held while applying mods
    //    if (s == null) return "";
    //    var x = s.Trim().Replace('\\', '/');
    //    if (x.StartsWith("/")) x = x.Substring(1);
    //    return x;
    //}

    //private static string Decode(Stream Data, ref Encoding Encoding)
    //{
    //    var myTextReader = new StreamReader(Data);
    //    var myOutput = myTextReader.ReadToEnd();
    //    Encoding = myTextReader.CurrentEncoding;
    //    // Remove the UTF-8 BOM marker by creating a custom encoding. This is necessary
    //    // to support writing Lua files that HoN can parse.
    //    if (Encoding.GetType() == typeof(UTF8Encoding)) Encoding = new UTF8Encoding(false);
    //    return myOutput.Replace(Convert.ToChar(13), ' ');
    //}

    //private static Stream Encode(string Data, Encoding Encoding)
    //{
    //    var myOutput = new MemoryStream();
    //    var myTextWriter = new StreamWriter(myOutput, Encoding);
    //    myTextWriter.Write(Data);
    //    myTextWriter.Flush();
    //    return myOutput;
    //}

    //#endregion

    //#region " HoN dir business "

    //private async void ChangeHoNPathToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    var o = new FolderBrowserDialog();
    //    o.Description = "Choose a HoN install - point to the folder containing the binary!";
    //    o.SelectedPath = GameHelper.GameDir;
    //    o.ShowNewFolderButton = false;
    //    if (o.ShowDialog() == DialogResult.OK)
    //    {
    //        var s = o.SelectedPath;
    //        if (!GameHelper.IsValidGameDir(s)) s = GameHelper.TryFixUserPath(s);

    //        if (s == "")
    //            await DisplayAlert("Invalid path. The path you specified either does not point to a Heroes of Newerth install or is inaccessible to this application.",
    //                "Error verifying HoN install", "OK");
    //        else
    //            SetGameDir(s);
    //    }
    //}

    //private void EnterHoNPathmanuallyToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //    /*
    //    frmInputbox myDialog = new frmInputbox();
    //    myDialog.Text = "Enter HoN path manually:";
    //    myDialog.Result = m_gameDir;
    //    if (myDialog.ShowDialog() == DialogResult.OK)
    //    {
    //        string s = myDialog.Result;
    //        if (!IsValidGameDir(s)) s = TryFixUserPath(s);

    //        if (s == "")
    //        {
    //            MessageBox.Show(
    //                "Invalid path. The path you specified either does not point to a Heroes of Newerth install or is inaccessible to this application.",
    //                "Error verifying HoN install", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //        }
    //        else
    //        {
    //            SetGameDir(s);
    //        }
    //    }
    //    */
    //}

    //#endregion

}

