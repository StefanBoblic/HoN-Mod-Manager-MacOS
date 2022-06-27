using HonModManagerForMac.Utils;

namespace HonModManagerForMac.Model
{
    public class Modification
    {
        //for use with the GUI
        //<-- only accurate during UpdateList()
        //was this mod found to be applied when reading resources999.s2z?
        public bool Applied { get; set; }
        public Dictionary<string, string> ApplyAfter { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ApplyBefore { get; set; } = new Dictionary<string, string>();
        public List<Modification> ApplyFirst { get; set; } = new List<Modification>();

        public string AppVersion { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }

        //full .honmod file path
        public string File { get; set; }

        //e.g. "WC3 Reserve Voices: Full"
        //e.g. "wc3reservevoicesfull" (same as name, but only lowercase letters and digits; used to check for mod identity)
        public string FixedName { get; set; }

        //description string to be displayed by GUI
        public Image Icon { get; set; }

        public int ImageListIdx { get; set; }
        //required HoN version matching string ("1.5-2.*" allowed)

        //raw incompatibility, requirement and priority data
        //Key: FixedName of the incompatible mod, Value: version matching string, space character, non-fixed mod name
        public Dictionary<string, string> Incompatibilities { get; set; } = new Dictionary<string, string>();

        public int Index { get; set; }

        //Same as Incompatibilities
        //used for cycle detection
        public bool Marked { get; set; }
        public string MMVersion { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Requirements { get; set; } = new Dictionary<string, string>();


        //URL for checking for updates
        public string UpdateCheck { get; set; }

        //URL for downloading newest version of the mod
        public string UpdateDownload { get; set; }

        //Reference to a ModUpdater object if this mod is currently being updated
        public ModUpdater Updater { get; set; }

        public string Version { get; set; }

        //website link to be displayed by GUI
        public string WebLink { get; set; }

        public bool Enabled;

        public bool IsUpdating => !(Updater == null || Updater.Status >= ModUpdaterStatus.NoUpdateInformation);
    }
}