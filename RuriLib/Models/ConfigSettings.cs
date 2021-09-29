using RuriLib.Models;
using RuriLib.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RuriLib
{
    /// <summary>
    /// Contains all the settings of a Config.
    /// </summary>
    public class ConfigSettings : ViewModelBase
    {
        #region General

        private string name = "";

        /// <summary>The name of the Config.</summary>
        public string Name { get { return name; } set { name = value; OnPropertyChanged(); } }

        private int suggestedBots = 1;

        /// <summary>The suggested amount of Bots that should be used with the config.</summary>
        public int SuggestedBots { get { return suggestedBots; } set { suggestedBots = value; OnPropertyChanged(); } }

        private int maxCPM = 0;

        /// <summary>The maximum CPM around which the Master Worker will stop starting bots and wait for the CPM to decrease below the threshold.</summary>
        public int MaxCPM { get { return maxCPM; } set { maxCPM = value; OnPropertyChanged(); } }

        private DateTime lastModified = DateTime.Now;

        /// <summary>When the Config was last modified.</summary>
        public DateTime LastModified { get { return lastModified; } set { lastModified = value; OnPropertyChanged(); } }

        private string additionalInfo = "";

        /// <summary>Additional information on the Config usage.</summary>
        public string AdditionalInfo { get { return additionalInfo; } set { additionalInfo = value; OnPropertyChanged(); } }

        private string author = "";

        /// <summary>The name of the Author of the Config.</summary>
        public string Author { get { return author; } set { author = value; OnPropertyChanged(); } }

        private string version = "1.1.5";

        /// <summary>The version of RuriLib the Config was made with.</summary>
        public string Version { get { return version; } set { version = value; OnPropertyChanged(); } }

        #endregion General

        #region Requests

        private bool ignoreResponseErrors = false;

        /// <summary>Whether to proceed if an HTTP request fails instead of giving the ERROR status.</summary>
        public bool IgnoreResponseErrors { get { return ignoreResponseErrors; } set { ignoreResponseErrors = value; OnPropertyChanged(); } }

        private int _maxRedirects = 8;

        /// <summary>The maximum amount of times we can be redirected to different URLs for a single request.</summary>
        public int MaxRedirects { get { return _maxRedirects; } set { _maxRedirects = value; OnPropertyChanged(); } }

        #endregion Requests

        #region Proxy

        private bool needsProxies = false;

        /// <summary>Whether the Config needs proxies to work (the Runner can override this).</summary>
        public bool NeedsProxies { get { return needsProxies; } set { needsProxies = value; OnPropertyChanged(); } }

        private bool onlySocks = false;

        /// <summary>Whether only SOCKS proxies should be used.</summary>
        public bool OnlySocks { get { return onlySocks; } set { onlySocks = value; OnPropertyChanged(); } }

        private bool onlySsl = false;

        /// <summary>Whether only SSL proxies should be used.</summary>
        public bool OnlySsl { get { return onlySsl; } set { onlySsl = value; OnPropertyChanged(); } }

        private int maxProxyUses = 0;

        /// <summary>The maximum amount of times a proxy can be used before being banned by the Runner (0 for infinite).</summary>
        public int MaxProxyUses { get { return maxProxyUses; } set { maxProxyUses = value; OnPropertyChanged(); } }

        private bool banProxyAfterGoodStatus = false;

        /// <summary>Whether to ban the proxy after a SUCCESS, CUSTOM or NONE status (not FAIL or RETRY).</summary>
        public bool BanProxyAfterGoodStatus { get { return banProxyAfterGoodStatus; } set { banProxyAfterGoodStatus = value; OnPropertyChanged(); } }

        #endregion Proxy

        #region Data

        private bool encodeData = false;

        /// <summary>Whether the data should be URLencoded after being sliced.</summary>
        public bool EncodeData { get { return encodeData; } set { encodeData = value; OnPropertyChanged(); } }

        private string allowedWordlist1 = "";

        /// <summary>The name of the first allowed WordlistType.</summary>
        public string AllowedWordlist1 { get { return allowedWordlist1; } set { allowedWordlist1 = value; OnPropertyChanged(); } }

        private string allowedWordlist2 = "";

        /// <summary>The name of the second allowed WordlistType.</summary>
        public string AllowedWordlist2 { get { return allowedWordlist2; } set { allowedWordlist2 = value; OnPropertyChanged(); } }

        /// <summary>The collection of data rules that check if a data line is valid.</summary>
        public ObservableCollection<DataRule> DataRules { get; set; } = new ObservableCollection<DataRule>();

        #endregion Data

        #region Inputs

        /// <summary>The collection of custom inputs that the Runner asks to the user upon start.</summary>
        public ObservableCollection<CustomInput> CustomInputs { get; set; } = new ObservableCollection<CustomInput>();

        #endregion Inputs

        #region OCR

        private string captchaUrl = "";

        /// <summary>The URL used for OCR image identification.</summary>
        public string CaptchaUrl { get { return captchaUrl; } set { captchaUrl = value; OnPropertyChanged(); } }

        private string base64 = "";

        /// <summary>The URL used for OCR image identification.</summary>
        public string Base64 { get { return base64; } set { base64 = value; OnPropertyChanged(); } }

        private bool grayscale = false;

        /// <summary>The Grayscale of Image.</summary>
        public bool Grayscale { get { return grayscale; } set { grayscale = value; OnPropertyChanged(); } }

        private bool removeLines = false;

        /// <summary>Removing random line in Image.</summary>
        public bool RemoveLines { get { return removeLines; } set { removeLines = value; OnPropertyChanged(); } }

        private bool removeNoise = false;

        /// <summary>Removing background noise of an Image.</summary>
        public bool RemoveNoise { get { return removeNoise; } set { removeNoise = value; OnPropertyChanged(); } }

        private bool dilate = false;

        /// <summary>Whether to add Dilation or not to an Image.</summary>
        public bool Dilate { get { return dilate; } set { dilate = value; OnPropertyChanged(); } }

        private float threshold = 1;

        /// <summary>The amount of threshold.</summary>
        public float Threshold { get { return threshold; } set { threshold = value; OnPropertyChanged(); } }

        private float diffKeep = 0;

        /// <summary>The amount of difference a pixel can have.</summary>
        public float DiffKeep { get { return diffKeep; } set { diffKeep = value; OnPropertyChanged(); } }

        private float diffHide = 0;

        /// <summary>The amount of difference a pixel can have.</summary>
        public float DiffHide { get { return diffHide; } set { diffHide = value; OnPropertyChanged(); } }

        private bool saturate = false;

        /// <summary>Whether to add Dilation or not to an Image.</summary>
        public bool Saturate { get { return saturate; } set { saturate = value; OnPropertyChanged(); } }

        private float saturation = 0;

        /// <summary>The amount of difference a pixel can have.</summary>
        public float Saturation { get { return saturation; } set { saturation = value; OnPropertyChanged(); } }

        private bool transparent = false;

        /// <summary>Setting transparnet colors in Image.</summary>
        public bool Transparent { get { return transparent; } set { transparent = value; OnPropertyChanged(); } }

        private bool contour = false;

        /// <summary>Setting transparnet colors in Image.</summary>
        public bool Contour { get { return contour; } set { contour = value; OnPropertyChanged(); } }

        private bool onlyShow = false;

        /// <summary>Setting to show only specified colors in Image.</summary>
        public bool OnlyShow { get { return onlyShow; } set { onlyShow = value; OnPropertyChanged(); } }

        private bool contrastGamma = false;

        /// <summary>Setting the contrast and gamma of an Image.</summary>
        public bool ContrastGamma { get { return contrastGamma; } set { contrastGamma = value; OnPropertyChanged(); } }

        private float contrast = 1;

        /// <summary>The amount of contrast to apply.</summary>
        public float Contrast { get { return contrast; } set { contrast = value; OnPropertyChanged(); } }

        private float gamma = 1;

        /// <summary>The amount of gamma to apply.</summary>
        public float Gamma { get { return gamma; } set { gamma = value; OnPropertyChanged(); } }

        private float brightness = 1;

        /// <summary>The amount of brightness to apply.</summary>
        public float Brightness { get { return brightness; } set { brightness = value; OnPropertyChanged(); } }

        private int removeLinesMin = 0;

        /// <summary>The amount of lines to remove by.</summary>
        public int RemoveLinesMin { get { return removeLinesMin; } set { removeLinesMin = value; OnPropertyChanged(); } }

        private int removeLinesMax = 0;

        /// <summary>The amount of lines to remove by.</summary>
        public int RemoveLinesMax { get { return removeLinesMax; } set { removeLinesMax = value; OnPropertyChanged(); } }

        #endregion OCR

        #region Selenium

        private bool forceHeadless = false;

        /// <summary>Whether to override the default choice of the user and force headless mode.</summary>
        public bool ForceHeadless { get { return forceHeadless; } set { forceHeadless = value; OnPropertyChanged(); } }

        private bool alwaysOpen = false;

        /// <summary>Whether to always open a browser at the start of the checking process (if not already open).</summary>
        public bool AlwaysOpen { get { return alwaysOpen; } set { alwaysOpen = value; OnPropertyChanged(); } }

        private bool alwaysQuit = false;

        /// <summary>Whether to always quit the browser and dispose of the WebDriver at the end of the checking process (if any browser is open).</summary>
        public bool AlwaysQuit { get { return alwaysQuit; } set { alwaysQuit = value; OnPropertyChanged(); } }

        private bool disableNotifications = false;

        /// <summary>Whether to disable notifications the lock the page and make it impossible to proceed.</summary>
        public bool DisableNotifications { get { return disableNotifications; } set { disableNotifications = value; OnPropertyChanged(); } }

        private string customUserAgent = "";

        /// <summary>The custom User-Agent header that should be used for every request of the browser.</summary>
        public string CustomUserAgent { get { return customUserAgent; } set { customUserAgent = value; OnPropertyChanged(); } }

        private bool randomUA = false;

        /// <summary>Whether to choose a random User-Agent header when opening a browser instance.</summary>
        public bool RandomUA { get { return randomUA; } set { randomUA = value; OnPropertyChanged(); } }

        private string customCMDArgs = "";

        /// <summary>The custom command line arguments that are sent when running the executable file.</summary>
        public string CustomCMDArgs { get { return customCMDArgs; } set { customCMDArgs = value; OnPropertyChanged(); } }

        #endregion Selenium

        #region Methods

        /// <summary>
        /// Removes a DataRule given its id.
        /// </summary>
        /// <param name="id">The unique id of the DataRule</param>
        public void RemoveDataRuleById(int id)
        {
            DataRules.Remove(GetDataRuleById(id));
        }

        /// <summary>
        /// Gets a DataRule given its id.
        /// </summary>
        /// <param name="id">The unique id of the DataRule</param>
        /// <returns>The DataRule</returns>
        public DataRule GetDataRuleById(int id)
        {
            return DataRules.FirstOrDefault(r => r.Id == id);
        }

        /// <summary>
        /// Removes a CustomInput given its id.
        /// </summary>
        /// <param name="id">The unique id of the CustomInput</param>
        public void RemoveCustomInputById(int id)
        {
            CustomInputs.Remove(GetCustomInputById(id));
        }

        /// <summary>
        /// Gets a CustomInput given its id.
        /// </summary>
        /// <param name="id">The unique id of the CustomInput</param>
        /// <returns>The CustomInput</returns>
        public CustomInput GetCustomInputById(int id)
        {
            return CustomInputs.FirstOrDefault(c => c.Id == id);
        }

        #endregion Methods
    }
}