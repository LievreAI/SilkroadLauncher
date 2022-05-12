﻿using Pk2ReaderAPI;
using SilkroadLauncher.Network;
using SilkroadLauncher.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SilkroadLauncher
{
    /// <summary>
    /// Handles all Silkroad launcher processes
    /// </summary>
    public class LauncherViewModel : BaseViewModel
    {
        #region Singleton
        /// <summary>
        /// Unique instance of this class
        /// </summary>
        public static LauncherViewModel Instance { get; } = new LauncherViewModel();
        #endregion

        #region Private Properties
        /// <summary>
        /// Application title
        /// </summary>
        private string m_Title = LauncherSettings.APP_TITLE;
        /// <summary>
        /// The window this view model controls
        /// </summary>
        private IWindow m_Window;
        /// <summary>
        /// Division info
        /// </summary>
        private Dictionary<string,List<string>> m_DivisionInfo;
        /// <summary>
        /// Gateway port
        /// </summary>
        private ushort m_Gateport;
        /// <summary>
        /// Client version handled internally by game
        /// </summary>
        private uint m_Version;
        /// <summary>
        /// Client version shown to the user
        /// </summary>
        private string m_ClientVersion = string.Empty;
        /// <summary>
        /// Locale type
        /// </summary>
        private byte m_Locale;
        /// <summary>
        /// Arguments used to start the game client
        /// </summary>
        private string m_SRClientArguments;
        /// <summary>
        /// Indicates if the Pk2 has been loaded correctly
        /// </summary>
        private bool m_IsLoaded;
        /// <summary>
        /// Indicates if the user is viewing game config
        /// </summary>
        private bool m_IsViewingConfig;
        /// <summary>
        /// Game bsic config
        /// </summary>
        private ConfigViewModel m_Config = new ConfigViewModel();
        /// <summary>
        /// Indicates if the user is viewing language options
        /// </summary>
        private bool m_IsViewingLangConfig;
        /// <summary>
        /// Language temporally being selected as language option
        /// </summary>
        private int m_LangConfigIndex;
        /// <summary>
        /// The initial connection to server
        /// </summary>
        private Session m_GatewaySession;
        /// <summary>
        /// Indicates if the launcher is checking for updates
        /// </summary>
        private bool m_IsCheckingUpdates;
        /// <summary>
        /// Indicates if cannot connect to server
        /// </summary>
        private bool m_IsUnderInspection = true;
        /// <summary>
        /// Recent web notices
        /// </summary>
        private List<WebNoticeViewModel> m_WebNotices = new List<WebNoticeViewModel>();
        /// <summary>
        /// The notice being shown
        /// </summary>
        private WebNoticeViewModel m_SelectedWebNotice;
        /// <summary>
        /// Indicates if the launcher is on updating process
        /// </summary>
        private bool m_IsUpdating;
        /// <summary>
        /// The total bytes required download to apply patch
        /// </summary>
        private ulong m_UpdatingBytesMaxDownloading;
        /// <summary>
        /// The current bytes downloaded to apply patch
        /// </summary>
        private ulong m_UpdatingBytesDownloading;
        /// <summary>
        /// The current patch percentage
        /// </summary>
        private int m_UpdatingPercentage;
        /// <summary>
        /// The current file path being updated
        /// </summary>
        private string m_UpdatingFilePath;
        /// <summary>
        /// The current file progress updating percentage
        /// </summary>
        private int m_UpdatingFilePercentage;
        /// <summary>
        /// Indicates if the game can be started
        /// </summary>
        private bool m_CanStartGame;
        #endregion

        #region Public Properties
        /// <summary>
        /// Application title shown on windows title bar
        /// </summary>
        public string Title {
            get { return m_Title; }
            set
            {
                m_Title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        /// <summary>
        /// Client locale
        /// </summary>
        public byte Locale
        {
            get { return m_Locale; }
            set
            {
                // set new value
                m_Locale = value;
                // notify event
                OnPropertyChanged(nameof(Locale));
            }
        }
        /// <summary>
        /// Client version handled internally
        /// </summary>
        public uint Version
        {
            get { return m_Version; }
            set
            {
                // set new value
                m_Version = value;
                // notify event
                OnPropertyChanged(nameof(Version));
                // Set as string
                var strVer = (1000 + m_Version).ToString();
                ClientVersion = strVer.Substring(0, 1) + "." + strVer.Substring(1);
            }
        }
        /// <summary>
        /// Client version shown to the user
        /// </summary>
        public string ClientVersion 
        {
            get { return m_ClientVersion; }
            private set
            {
                // set new value
                m_ClientVersion = value;
                // notify event
                OnPropertyChanged(nameof(ClientVersion));
            }
        }
        /// <summary>
        /// Check if the Pk2 has been correctly loaded
        /// </summary>
        public bool IsLoaded
        {
            get { return m_IsLoaded; }
            private set
            {
                // set new value
                m_IsLoaded = value;
                // notify event
                OnPropertyChanged(nameof(IsLoaded));
            }
        }
        /// <summary>
        /// Check if the launcher is on game config screen
        /// </summary>
        public bool IsViewingConfig
        {
            get { return m_IsViewingConfig; }
            set
            {
                // set new value
                m_IsViewingConfig = value;
                // notify event
                OnPropertyChanged(nameof(IsViewingConfig));
            }
        }
        /// <summary>
        /// The basic view config used by the game client
        /// </summary>
        public ConfigViewModel Config
        {
            get { return m_Config; }
            set
            {
                // set new value
                m_Config = value;
                // notify event
                OnPropertyChanged(nameof(Config));
            }
        }
        /// <summary>
        /// Check if the launcher is on game language config window
        /// </summary>
        public bool IsViewingLangConfig
        {
            get { return m_IsViewingLangConfig; }
            set
            {
                // set new value
                m_IsViewingLangConfig = value;
                // notify event
                OnPropertyChanged(nameof(IsViewingLangConfig));
            }
        }
        /// <summary>
        /// Language temporally selected as new option
        /// </summary>
        public int LangConfigIndex
        {
            get { return m_LangConfigIndex; }
            set
            {
                // set new value
                m_LangConfigIndex = value;
                // notify event
                OnPropertyChanged(nameof(LangConfigIndex));
            }
        }
        /// <summary>
        /// Check if the launcher is looking for update the client
        /// </summary>
        public bool IsCheckingUpdates
        {
            get { return m_IsCheckingUpdates; }
            set
            {
                // set new value
                m_IsCheckingUpdates = value;
                // notify event
                OnPropertyChanged(nameof(IsCheckingUpdates));
            }
        }
        /// <summary>
        /// Check if cannot connect to server
        /// </summary>
        public bool IsUnderInspection
        {
            get { return m_IsUnderInspection; }
            set
            {
                // set new value
                m_IsUnderInspection = value;
                // notify event
                OnPropertyChanged(nameof(IsUnderInspection));
            }
        }
        /// <summary>
        /// All notices loaded after checking updates
        /// </summary>
        public List<WebNoticeViewModel> WebNotices
        {
            get { return m_WebNotices; }
            set
            {
                m_WebNotices = value;
                // notify event
                OnPropertyChanged(nameof(WebNotices));
            }
        }
        /// <summary>
        /// The notice selected, the first one as default.
        /// </summary>
        public WebNoticeViewModel SelectedWebNotice
        {
            get { return m_SelectedWebNotice; }
            set
            {
                // set new value
                m_SelectedWebNotice = value;
                // notify event
                OnPropertyChanged(nameof(SelectedWebNotice));
            }
        }
        /// <summary>
        /// Check if the launcher is updating the client
        /// </summary>
        public bool IsUpdating
        {
            get { return m_IsUpdating; }
            set
            {
                // set new value
                m_IsUpdating = value;
                // notify event
                OnPropertyChanged(nameof(IsUpdating));
            }
        }
        /// <summary>
        /// Get or sets the max. bytes quantity to be downloaded to apply patch
        /// </summary>
        public ulong UpdatingBytesMaxDownloading
        {
            get { return m_UpdatingBytesMaxDownloading; }
            set
            {
                // set new value
                m_UpdatingBytesMaxDownloading = value;
                // notify event
                OnPropertyChanged(nameof(UpdatingBytesMaxDownloading));
            }
        }
        /// <summary>
        /// Get or sets the bytes quantity downloaded to apply patch
        /// </summary>
        public ulong UpdatingBytesDownloading
        {
            get { return m_UpdatingBytesDownloading; }
            set
            {
                // set new value
                m_UpdatingBytesDownloading = value;
                // notify event
                OnPropertyChanged(nameof(UpdatingBytesDownloading));

                // Set percentage
                if(m_UpdatingBytesMaxDownloading != 0)
                    UpdatingPercentage = (int)(m_UpdatingBytesDownloading * 100ul / m_UpdatingBytesMaxDownloading);
            }
        }
        /// <summary>
        /// Get the current percentage from patch download
        /// </summary>
        public int UpdatingPercentage
        {
            get { return m_UpdatingPercentage; }
            set
            {
                if (m_UpdatingPercentage == value)
                    return;

                m_UpdatingPercentage = value;
                OnPropertyChanged(nameof(UpdatingPercentage));
            }
        }
        /// <summary>
        /// The current file being downloaded and imported
        /// </summary>
        public string UpdatingFilePath
        {
            get { return m_UpdatingFilePath; }
            set
            {
                m_UpdatingFilePath = value;
                OnPropertyChanged(nameof(UpdatingFilePath));
            }
        }
        /// <summary>
        /// Get the current file percentage being updated
        /// </summary>
        public int UpdatingFilePercentage
        {
            get { return m_UpdatingFilePercentage; }
            set
            {
                m_UpdatingFilePercentage = value;
                OnPropertyChanged(nameof(UpdatingFilePercentage));
            }
        }
        /// <summary>
        /// Check if the game can be started
        /// </summary>
        public bool CanStartGame
        {
            get { return m_CanStartGame; }
            set
            {
                // set new value
                m_CanStartGame = value;
                // notify event
                OnPropertyChanged(nameof(CanStartGame));
            }
        }
        /// <summary>
        /// Contains all assets to be displayed
        /// </summary>
        public LauncherAssetsViewModel Assets { get; set; } = new LauncherAssetsViewModel();
        #endregion

        #region Commands
        /// <summary>
        /// Minimize the window
        /// </summary>
        public ICommand CommandMinimize { get; set; }
        /// <summary>
        /// Switch the window between restore and maximize
        /// </summary>
        public ICommand CommandRestore { get; set; }
        /// <summary>
        /// Close the window
        /// </summary>
        public ICommand CommandClose { get; set; }
        /// <summary>
        /// Starts the client
        /// </summary>
        public ICommand CommandStartGame { get; set; }
        /// <summary>
        /// Open an hyperlink on default browser
        /// </summary>
        public ICommand CommandOpenLink { get; set; }
        /// <summary>
        /// Set the config viewing state
        /// </summary>
        public ICommand CommandToggleConfig { get; set; }
        /// <summary>
        /// Set the language window viewing state
        /// </summary>
        public ICommand CommandToggleLangConfig { get; set; }
        /// <summary>
        /// Save the config file
        /// </summary>
        public ICommand CommandSaveConfig { get; set; }
        /// <summary>
        /// Save the config file
        /// </summary>
        public ICommand CommandSaveLangConfig { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        private LauncherViewModel()
        {
            #region Commands Setup
            // Windows commands
            CommandMinimize = new RelayCommand(() => {
                if (m_Window != null)
                    m_Window.WindowState = WindowState.Minimized;
            });
            CommandRestore = new RelayCommand(() => {
                if (m_Window != null)
                {
                    // Check the WindowState and change it
                    m_Window.WindowState = m_Window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
            });
            CommandClose = new RelayCommand(Exit);
            CommandStartGame = new RelayCommand(()=> {
                // Starts the game but only if is ready and exists
                if (CanStartGame && File.Exists(LauncherSettings.CLIENT_EXECUTABLE)) { 
                    System.Diagnostics.Process.Start(LauncherSettings.CLIENT_EXECUTABLE, m_SRClientArguments);
                    // Closing launcher
                    CommandClose.Execute(null);
                }
            });
            CommandOpenLink = new RelayParameterizedCommand((url) => {
                // Run link with default browser
                if (url is string link)
                    System.Diagnostics.Process.Start(link);
            });
            CommandToggleConfig = new RelayCommand(() => {
                IsViewingConfig = !IsViewingConfig;
            });
            CommandSaveConfig = new RelayCommand(() => {
                // Make sure pk2 it's not being used
                if (!IsUpdating)
                {
                    Config.Save();
                    IsViewingConfig = false;
                }
            });
            CommandToggleLangConfig = new RelayCommand(() => {
                // Update language selected currently
                if (!IsViewingLangConfig)
                    LangConfigIndex = Config.SupportedLanguageIndex;
                IsViewingLangConfig = !IsViewingLangConfig;
            });
            CommandSaveLangConfig = new RelayCommand(() => {
                // Make sure pk2 it's not being used
                if (!IsUpdating)
                {
                    // Avoid unchecked selection
                    if (LangConfigIndex != -1)
                    {
                        // Set new language selected
                        Config.SupportedLanguageIndex = LangConfigIndex;
                        Config.Save();
                    }
                    IsViewingLangConfig = false;
                }
            });
            #endregion

            // Init stuffs
            Initialize();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set the window this view model controls
        /// </summary>
        public void SetWindow(IWindow Window)
        {
            // Just save a reference
            m_Window = Window;
        }
        /// <summary>
        /// Show message to the user
        /// </summary>
        public void ShowMessage(string Text)
        {
            m_Window?.ShowMessage(Text,Title);
        }
        /// <summary>
        /// Check and loads the patch updates
        /// </summary>
        public async Task CheckUpdatesAsync()
        {
            // Avoid connection
            if (!IsLoaded)
                return;

            IsCheckingUpdates = true;
            // Check all IP's and try to connect one at least
            m_GatewaySession = new Session();

            // Add handlers for updating
            m_GatewaySession.AddHandler(GatewayModule.Opcode.GLOBAL_IDENTIFICATION, new PacketHandler(GatewayModule.Server_GlobalIdentification));
            m_GatewaySession.AddHandler(GatewayModule.Opcode.SERVER_PATCH_RESPONSE, new PacketHandler(GatewayModule.Server_PatchResponse));
            m_GatewaySession.AddHandler(GatewayModule.Opcode.SERVER_SHARD_LIST_RESPONSE, new PacketHandler(GatewayModule.Server_ShardListResponse));
            m_GatewaySession.AddHandler(GatewayModule.Opcode.SERVER_WEB_NOTICE_RESPONSE, new PacketHandler(GatewayModule.Server_WebNoticeResponse));

            m_GatewaySession.Disconnect += new EventHandler((_Session, _Event) => {
                System.Diagnostics.Debug.WriteLine("Gateway: Session disconnected");
            });
            bool connectionSolved = false;
            // Save at the same time the connection arguments
            int divIndex = 0, hostIndex = 0;
            foreach (var division in m_DivisionInfo)
            {
                for (int i = 0; i < division.Value.Count; i++)
                {
                    // Try to connect to the address
                    System.Diagnostics.Debug.WriteLine("Starting Session..");
                    connectionSolved = await Task.Run(() => m_GatewaySession.Start(division.Value[i], m_Gateport, 5000));
                    if (connectionSolved)
                    {
                        hostIndex = i;
                        break;
                    }
                }
                if (connectionSolved)
                {
                    // Set client args
                    m_SRClientArguments = "0 /" + m_Locale + " " + divIndex + " " + hostIndex;
                    break;
                }
                divIndex++;
            }
            // Not able to connect to server
            if (!connectionSolved)
            {
                IsCheckingUpdates = false;
                IsUnderInspection = true;
                ShowMessage(LauncherSettings.MSG_INSPECTION);
            }
        }
        /// <summary>
        /// Exit from Launcher
        /// </summary>
        public void Exit()
        {
            App.Current.Dispatcher.Invoke(() => {
                m_GatewaySession?.Stop();
                m_Window?.Close();
            });
        }
        #endregion

        #region Private Helpers
        /// <summary>
        /// Initialize all stuffs required for the connection and settings
        /// </summary>
        private void Initialize()
        {
            Pk2Reader pk2Reader = null;
            try
            {
                // Load pk2 reader
                pk2Reader = new Pk2Reader(LauncherSettings.PATH_PK2_MEDIA, LauncherSettings.CLIENT_BLOWFISH_KEY);

                // Load assets from client
                LoadAssets(pk2Reader);

                // Extract essential stuffs for the process
                if (pk2Reader.TryGetDivisionInfo(out m_DivisionInfo) && pk2Reader.TryGetGateport(out m_Gateport))
                {
                    // Abort operations if host or port is not verified
                    if (!VerifyHosts(m_DivisionInfo) || !VerifyPort(m_Gateport))
                        return;

                    // Load settings
                    m_Config.Load(pk2Reader);
                    // continue extracting
                    if (pk2Reader.TryGetVersion(out m_Version)
                    && pk2Reader.TryGetLocale(out m_Locale))
                    {
                        IsLoaded = true;
                        // Force string to be updated
                        Version = m_Version;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                // Forced shutdown
                Application.Current.Shutdown();
            }
            finally
            {
                pk2Reader?.Close();
            }
        }
        /// <summary>
        /// Loads all images assets from client
        /// </summary>
        private void LoadAssets(Pk2Reader reader)
        {
            Assets.Background = new ImageBrush(reader.GetImage("launcher_wpf/background.png"));
            Assets.HomeIcon = new ImageBrush(reader.GetImage("launcher_wpf/home_icon.png"));
            Assets.HomeIconOnHover = new ImageBrush(reader.GetImage("launcher_wpf/home_icon_onhover.png"));

            Assets.OptionButton = new ImageBrush(reader.GetImage("launcher_wpf/option_button.png"));
            Assets.OptionButtonOnHover = new ImageBrush(reader.GetImage("launcher_wpf/option_button_onhover.png"));
            Assets.OptionButtonOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/option_button_onpressed.png"));
            Assets.GuideButton = new ImageBrush(reader.GetImage("launcher_wpf/guide_button.png"));
            Assets.GuideButtonOnHover = new ImageBrush(reader.GetImage("launcher_wpf/guide_button_onhover.png"));
            Assets.GuideButtonOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/guide_button_onpressed.png"));
            Assets.MovieButton = new ImageBrush(reader.GetImage("launcher_wpf/movie_button.png"));
            Assets.MovieButtonOnHover = new ImageBrush(reader.GetImage("launcher_wpf/movie_button_onhover.png"));
            Assets.MovieButtonOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/movie_button_onpressed.png"));
            Assets.ExitButton = new ImageBrush(reader.GetImage("launcher_wpf/exit_button.png"));
            Assets.ExitButtonOnHover = new ImageBrush(reader.GetImage("launcher_wpf/exit_button_onhover.png"));
            Assets.ExitButtonOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/exit_button_onpressed.png"));
            Assets.StartButton = new ImageBrush(reader.GetImage("launcher_wpf/start_button.png"));
            Assets.StartButtonOnHover = new ImageBrush(reader.GetImage("launcher_wpf/start_button_onhover.png"));
            Assets.StartButtonOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/start_button_onpressed.png"));
            Assets.StartButtonUpdating = new ImageBrush(reader.GetImage("launcher_wpf/start_button_updating.png"));

            Assets.UpdatingBackground = new ImageBrush(reader.GetImage("launcher_wpf/updating_background.png"));
            Assets.UpdatingBar = new ImageBrush(reader.GetImage("launcher_wpf/updating_bar.png"));

            Assets.NoticeSelectedIcon = new ImageBrush(reader.GetImage("launcher_wpf/notice/selected_icon.png"));
            Assets.NoticeScrollBackground = new ImageBrush(reader.GetImage("launcher_wpf/notice/scroll_background.png"));
            Assets.NoticeScrollThumb = new ImageBrush(reader.GetImage("launcher_wpf/notice/scroll_thumb.png"));
            Assets.NoticeScrollArrowUp = new ImageBrush(reader.GetImage("launcher_wpf/notice/scroll_arrow_up.png"));
            Assets.NoticeScrollArrowUpOnHover = new ImageBrush(reader.GetImage("launcher_wpf/notice/scroll_arrow_up_onhover.png"));
            Assets.NoticeScrollArrowUpOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/notice/scroll_arrow_up_onpressed.png"));
            Assets.NoticeScrollArrowDown = new ImageBrush(reader.GetImage("launcher_wpf/notice/scroll_arrow_down.png"));
            Assets.NoticeScrollArrowDownOnHover = new ImageBrush(reader.GetImage("launcher_wpf/notice/scroll_arrow_down_onhover.png"));
            Assets.NoticeScrollArrowDownOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/notice/scroll_arrow_down_onpressed.png"));

            Assets.LanguageDisplayBackground = new ImageBrush(reader.GetImage("launcher_wpf/language_display_background.png"));
            Assets.LanguageButton = new ImageBrush(reader.GetImage("launcher_wpf/language_button.png"));
            Assets.LanguageButtonOnHover = new ImageBrush(reader.GetImage("launcher_wpf/language_button_onhover.png"));
            Assets.LanguageButtonOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/language_button_onpressed.png"));

            Assets.LanguagePopupFrameTop = new ImageBrush(reader.GetImage("launcher_wpf/language_popup/frame_top.png"));
            Assets.LanguagePopupFrameMid = new ImageBrush(reader.GetImage("launcher_wpf/language_popup/frame_mid.png"));
            Assets.LanguagePopupFrameBottom = new ImageBrush(reader.GetImage("launcher_wpf/language_popup/frame_bottom.png"));
            Assets.LanguagePopupCheckbox = new ImageBrush(reader.GetImage("launcher_wpf/language_popup/checkbox.png"));
            Assets.LanguagePopupCheckboxChecked = new ImageBrush(reader.GetImage("launcher_wpf/language_popup/checkbox_checked.png"));
            Assets.LanguagePopupAcceptButton = new ImageBrush(reader.GetImage("launcher_wpf/language_popup/accept_button.png"));
            Assets.LanguagePopupAcceptButtonOnHover = new ImageBrush(reader.GetImage("launcher_wpf/language_popup/accept_button_onhover.png"));
            Assets.LanguagePopupAcceptButtonOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/language_popup/accept_button_onpressed.png"));
            Assets.LanguagePopupCancelButton = new ImageBrush(reader.GetImage("launcher_wpf/language_popup/cancel_button.png"));
            Assets.LanguagePopupCancelButtonOnHover = new ImageBrush(reader.GetImage("launcher_wpf/language_popup/cancel_button_onhover.png"));
            Assets.LanguagePopupCancelButtonOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/language_popup/cancel_button_onpressed.png"));

            Assets.SettingsFrameTop = new ImageBrush(reader.GetImage("launcher_wpf/settings/frame_top.png"));
            Assets.SettingsComboboxBackground = new ImageBrush(reader.GetImage("launcher_wpf/settings/combobox_background.png"));
            Assets.SettingsComboboxArrow = new ImageBrush(reader.GetImage("launcher_wpf/settings/combobox_arrow.png"));
            Assets.SettingsComboboxArrowOnHover = new ImageBrush(reader.GetImage("launcher_wpf/settings/combobox_arrow_onhover.png"));
            Assets.SettingsComboboxArrowOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/settings/combobox_arrow_onpressed.png"));
            Assets.SettingsSaveButton = new ImageBrush(reader.GetImage("launcher_wpf/settings/save_button.png"));
            Assets.SettingsSaveButtonOnHover = new ImageBrush(reader.GetImage("launcher_wpf/settings/save_button_onhover.png"));
            Assets.SettingsSaveButtonOnPressed = new ImageBrush(reader.GetImage("launcher_wpf/settings/save_button_onpressed.png"));
            Assets.SettingsCheckbox = new ImageBrush(reader.GetImage("launcher_wpf/settings/checkbox.png"));
            Assets.SettingsCheckboxChecked = new ImageBrush(reader.GetImage("launcher_wpf/settings/checkbox_checked.png"));
        }
        /// <summary>
        /// Verify host used to start the game it's linked to launcher config
        /// </summary>
        private bool VerifyHosts(Dictionary<string, List<string>> divInfo)
        {
            // Verification not required
            if (LauncherSettings.CLIENT_VERIFY_HOST.Length == 0)
                return true;

            // Check every host from divisions
            foreach (var div in divInfo.Values)
            {
                foreach (var host in div)
                {
                    // Check if host is verified
                    foreach (var h in LauncherSettings.CLIENT_VERIFY_HOST)
                        if (h != host)
                            return false;
                }
            }

            // All host has been succeed
            return true;
        }
        /// <summary>
        /// Verify port used to start the game it's linked to launcher config
        /// </summary>
        private bool VerifyPort(int port)
        {
            return LauncherSettings.CLIENT_VERIFY_PORT == 0 || LauncherSettings.CLIENT_VERIFY_PORT == port;
        }
        #endregion
    }
}