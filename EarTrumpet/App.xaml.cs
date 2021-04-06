using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.Diagnosis;
using EarTrumpet.Extensibility.Hosting;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.UI.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet
{
    public partial class App
    {
        public static bool IsShuttingDown { get; private set; }
        public static bool HasIdentity { get; private set; }
        public static Version PackageVersion { get; private set; }
        public static TimeSpan Duration => s_appTimer.Elapsed;

        private static readonly Stopwatch s_appTimer = Stopwatch.StartNew();
        private FlyoutViewModel _flyoutViewModel;
        private FlyoutWindow _flyoutWindow;
        private DeviceCollectionViewModel _collectionViewModel;
        private ShellNotifyIcon _trayIcon;
        private WindowHolder _mixerWindow;
        private WindowHolder _settingsWindow;
        private ErrorReporter _errorReporter;
        public static AppSettings Settings;
        private static bool _AllowAppChange;
        public static bool AllowAppChange
        {
            get => _AllowAppChange;
            set
            {
                _AllowAppChange = value;
                Settings.SaveBool("AllowAppChange", value);
            }
        }
        public static bool MasterModifier, AppModifier, DisableMaster, master, app, above;

        private static SynchronizationContext sc;
        private static long bannerTimeStamp;
        private static VolumeBanner volumeBanner;
        public static IAudioDevice device;
        private static string targetWindowName;
        private static string[][][] savedVolumes;
        private static bool masterMute;

        private void OnAppStartup(object sender, StartupEventArgs e)
        {
            Exit += (_, __) => IsShuttingDown = true;
            HasIdentity = PackageHelper.CheckHasIdentity();
            PackageVersion = PackageHelper.GetVersion(HasIdentity);
            Settings = new AppSettings();
            _errorReporter = new ErrorReporter(Settings);

            if (SingleInstanceAppMutex.TakeExclusivity())
            {
                Exit += (_, __) => SingleInstanceAppMutex.ReleaseExclusivity();

                try
                {
                    ContinueStartup();
                }
                catch (Exception ex) when (IsCriticalFontLoadFailure(ex))
                {
                    ErrorReporter.LogWarning(ex);
                    OnCriticalFontLoadFailure();
                }
            }
            else
            {
                Shutdown();
            }
        }

        private void ContinueStartup()
        {
            ((UI.Themes.Manager)Resources["ThemeManager"]).Load();

            var deviceManager = WindowsAudioFactory.Create(AudioDeviceKind.Playback);
            deviceManager.Loaded += (_, __) => CompleteStartup();
            _collectionViewModel = new DeviceCollectionViewModel(deviceManager, Settings);

            _trayIcon = new ShellNotifyIcon(new TaskbarIconSource(_collectionViewModel, Settings));
            Exit += (_, __) => _trayIcon.IsVisible = false;
            _collectionViewModel.TrayPropertyChanged += () => _trayIcon.SetTooltip(_collectionViewModel.GetTrayToolTip());

            _flyoutViewModel = new FlyoutViewModel(_collectionViewModel, () => _trayIcon.SetFocus());
            _flyoutWindow = new FlyoutWindow(_flyoutViewModel);
            // Initialize the FlyoutWindow last because its Show/Hide cycle will pump messages, causing UI frames
            // to be executed, breaking the assumption that startup is complete.
            _flyoutWindow.Initialize();
        }

        private void CompleteStartup()
        {
            AddonManager.Load();
            Exit += (_, __) => AddonManager.Shutdown();

#if DEBUG
            DebugHelpers.Add();
#endif
            _mixerWindow = new WindowHolder(CreateMixerExperience);
            _settingsWindow = new WindowHolder(CreateSettingsExperience);
            volumeBanner = new VolumeBanner();
            sc = SynchronizationContext.Current;

            Settings.FlyoutHotkeyTyped += () => _flyoutViewModel.OpenFlyout(InputType.Keyboard);
            Settings.MixerHotkeyTyped += () => _mixerWindow.OpenOrClose();
            Settings.SettingsHotkeyTyped += () => _settingsWindow.OpenOrBringToFront();
            Settings.MuteVolumeHotkeyTyped += () => MuteVolume();
            Settings.MuteAppVolumeHotkeyTyped += () => MuteAppVolume();
            Settings.SaveVolumesHotkeyTyped += () => SaveVolumes();
            Settings.OpenVolumesHotkeyTyped += () => OpenVolumes();
            Settings.EnableUnlimitedAppControlHotKeyTyped += () => AllowAppChange = !AllowAppChange;
            Settings.RegisterHotkeys();

            _trayIcon.PrimaryInvoke += (_, type) => _flyoutViewModel.OpenFlyout(type);
            _trayIcon.SecondaryInvoke += (_, __) => _trayIcon.ShowContextMenu(GetTrayContextMenuItems());
            _trayIcon.TertiaryInvoke += (_, __) => _collectionViewModel.Default?.ToggleMute.Execute(null);
            _trayIcon.Scrolled += (_, wheelDelta) => _collectionViewModel.Default?.IncrementVolume(Math.Sign(wheelDelta) * 2);
            _trayIcon.SetTooltip(_collectionViewModel.GetTrayToolTip());
            _trayIcon.IsVisible = true;

            // initialize hook events, keyboard and mouse events, and exit events
            InterceptInput.MouseWheelEvent += App_MouseWheelEvent;
            InterceptInput.IsMouseInsideIcon += (x, y) => _trayIcon.IsCursorOnIcon(x, y);
            InterceptInput.SetKeyboardHook();
            InterceptInput.SetMouseHook();
            Exit += (_, __) => InterceptInput.UnHookKeyboard();
            Exit += (_, __) => InterceptInput.UnHookMouse();

            // initialize the listener variables
            MasterModifier = AppModifier = DisableMaster = false;

            // initialize other variables
            device = _collectionViewModel.GetDeviceManager().Default;
            savedVolumes = Settings.OpenVolumes();
            _AllowAppChange = Settings.OpenBool("AllowAppChange", false);
            masterMute = device.IsMuted;

            DisplayFirstRunExperience();
        }

        private void App_MouseWheelEvent(int wheelDelta)
        {
            master = MasterModifier && !DisableMaster;
            app = AppModifier && !DisableMaster;
            if (app)
            {
                ChangeAppVolume(wheelDelta);
            }
            // if the knob is at the end of the slider or master is changed
            if (above || master)
            {
                ChangeVolume(wheelDelta);
            }
        }

        private static void GetTargetWindow()
        {
            GetWindowThreadProcessId(WindowFromPoint(System.Windows.Forms.Cursor.Position), out uint processId);
            try
            {
                var appResolver = (IApplicationResolver)new ApplicationResolver();
                appResolver.GetAppIDForProcess(processId, out string appId, out _, out _, out _);
                Marshal.ReleaseComObject(appResolver);

                var shellItem = Shell32.SHCreateItemInKnownFolder(FolderIds.AppsFolder, Shell32.KF_FLAG_DONT_VERIFY, appId, typeof(IShellItem2).GUID);
                targetWindowName = shellItem.GetString(ref PropertyKeys.PKEY_ItemNameDisplay);
            }
            catch { }

            if (string.IsNullOrWhiteSpace(targetWindowName))
            {
                try
                {
                    using (var proc = Process.GetProcessById((int)processId))
                    {
                        targetWindowName = proc.MainWindowTitle;
                    }
                }
                catch { }
            }
#if DEBUG
            Trace.WriteLine("Target Window: " + targetWindowName);
#endif
        }

        public static void ChangeAppVolume(int direction)
        {
            GetTargetWindow();
            above = false;
            int index = -1;
            for (int i = 0; i < device.Groups.Count; i++)
            {
                // only change volume if it is the window that is under the mouse
                if (device.Groups[i].DisplayName == targetWindowName)
                {
                    if (device.Groups[i].Volume == 1.0f && direction > 0 && AllowAppChange)
                    {
                        above = true;
                    }
                    else
                    {
                        device.Groups[i].Volume = Math.Max(Math.Min((float)Math.Round(device.Groups[i].Volume * 50 + direction) / 50.0f, 1.0f), 0.0f);
                    }
                    device.Groups[i].IsMuted = false;
                    index = i;
                }
            }

            if (above)
            {
                for (int i = 0; i < device.Groups.Count; i++)
                {
                    if (i != index)
                    {
                        device.Groups[i].Volume = Math.Max((float)Math.Round(device.Groups[i].Volume * 50 - direction) / 50.0f, 0.0f);
                    }
                }
            }
            else if (index != -1)
            {
                ShowAppVolumeBanner(device.Groups[index]);
            }
        }

        public static void ChangeVolume(int direction)
        {
            device.Volume = Math.Max(Math.Min((float)Math.Round(device.Volume * 50 + direction) / 50.0f, 1.0f), 0.0f);
            ShowVolumeBanner();
        }

        public static void MuteVolume()
        {
            masterMute = device.IsMuted;
            device.IsMuted = !masterMute;
            masterMute = !masterMute;
            Trace.WriteLine("master muted");
            ShowVolumeBanner();
        }

        public static void MuteAppVolume()
        {
            GetTargetWindow();
            for (int i = 0; i < device.Groups.Count; i++)
            {
                // only mute the app if it is the window that is under the mouse
                if (device.Groups[i].DisplayName == targetWindowName)
                {
                    device.Groups[i].IsMuted = !device.Groups[i].IsMuted;
                    ShowAppVolumeBanner(device.Groups[i]);
                    break;
                }
            }
        }

        private static void SaveVolumes()
        {
            if (savedVolumes == null)
            {
                savedVolumes = new string[1][][];
                goto NotExist;
            }

            // check if the device exists
            int index;
            for (int i = 0; i < savedVolumes.Length; i++)
            {
                if (device.DisplayName == savedVolumes[i][0][0])
                {
                    index = i;
                    goto Exists;
                }
            }
            goto NotExist;

        // if the device already exists
        Exists:
            List<string[]> newList = new List<string[]>();
            newList.AddRange(savedVolumes[index]);
            newList[0][1] = device.Volume.ToString();
            for (int i = 0; i < device.Groups.Count; i++)
            {
                bool match = false;
                for (int j = 1; j < savedVolumes[index].Length; j++)
                {
                    if (device.Groups[i].ExeName == newList[j][0])
                    {
                        newList[j][1] = device.Groups[i].Volume.ToString();
                        match = true;
                        break;
                    }
                }
                if (!match)
                    newList.Add(new string[2] { device.Groups[i].ExeName, device.Groups[i].Volume.ToString() });
            }
            savedVolumes[index] = newList.ToArray();
            Settings.SaveVolumes(savedVolumes);
            return;

        // if the device doesnt already exist
        NotExist:
            string[][] newArray = new string[device.Groups.Count + 1][];
            newArray[0] = new string[2] { device.DisplayName, device.Volume.ToString() };
            for (int i = 0; i < device.Groups.Count; i++)
            {
                newArray[i + 1] = new string[2] { device.Groups[i].ExeName, device.Groups[i].Volume.ToString() };
            }
            savedVolumes[savedVolumes.Length - 1] = newArray;
            Settings.SaveVolumes(savedVolumes);
            Trace.WriteLine("Volumes Saved");
        }

        private static void OpenVolumes()
        {
            int index;
            for (int i = 0; i < savedVolumes.Length; i++)
            {
                if (device.DisplayName == savedVolumes[i][0][0])
                {
                    index = i;
                    goto Exists;
                }
            }
            return;
        Exists:
            device.Volume = float.Parse(savedVolumes[index][0][1]);
            for (int i = 0; i < device.Groups.Count; i++)
            {
                for (int j = 1; j < savedVolumes[index].Length; j++)
                {
                    if (device.Groups[i].ExeName == savedVolumes[index][j][0])
                    {
                        device.Groups[i].Volume = float.Parse(savedVolumes[index][j][1]);
                        break;
                    }
                }
            }
            Trace.WriteLine("Volumes Opened");
        }

        public static void UpdateBannerTimeStamp()
        {
            bannerTimeStamp = s_appTimer.ElapsedMilliseconds;
        }

        private static void ShowAppVolumeBanner(IAudioDeviceSession session)
        {
            volumeBanner.ChangeAppVolume(session);
        }

        private static void ShowVolumeBanner()
        {
            volumeBanner.ChangeMasterVolume(device.Volume, masterMute);
        }

        public static void InvokeHideBanner()
        {
            if (s_appTimer.ElapsedMilliseconds - bannerTimeStamp >= VolumeBanner.DisplayTime)
            sc.Post(HideVolumeBanner, new object());
        }

        private static void HideVolumeBanner(object state)
        {
            volumeBanner.PopDown();
        }

        private void DisplayFirstRunExperience()
        {
            if (!Settings.HasShownFirstRun
#if DEBUG
                || Keyboard.IsKeyDown(Key.LeftCtrl)
#endif
                )
            {
                Trace.WriteLine($"App DisplayFirstRunExperience Showing welcome dialog");
                Settings.HasShownFirstRun = true;

                var dialog = new DialogWindow { DataContext = new WelcomeViewModel(Settings) };
                dialog.Show();
                dialog.RaiseWindow();
            }
        }

        private bool IsCriticalFontLoadFailure(Exception ex)
        {
            return ex.StackTrace.Contains("MS.Internal.Text.TextInterface.FontFamily.GetFirstMatchingFont") ||
                   ex.StackTrace.Contains("MS.Internal.Text.Line.Format");
        }

        private void OnCriticalFontLoadFailure()
        {
            Trace.WriteLine($"App OnCriticalFontLoadFailure");

            new Thread(() =>
            {
                if (MessageBox.Show(
                    EarTrumpet.Properties.Resources.CriticalFailureFontLookupHelpText,
                    EarTrumpet.Properties.Resources.CriticalFailureDialogHeaderText,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK) == MessageBoxResult.OK)
                {
                    Trace.WriteLine($"App OnCriticalFontLoadFailure OK");
                    ProcessHelper.StartNoThrow("https://eartrumpet.app/jmp/fixfonts");
                }
                Environment.Exit(0);
            }).Start();

            // Stop execution because callbacks to the UI thread will likely cause another cascading font error.
            new AutoResetEvent(false).WaitOne();
        }

        private IEnumerable<ContextMenuItem> GetTrayContextMenuItems()
        {
            var ret = new List<ContextMenuItem>(_collectionViewModel.AllDevices.OrderBy(x => x.DisplayName).Select(dev => new ContextMenuItem
            {
                DisplayName = dev.DisplayName,
                IsChecked = dev.Id == _collectionViewModel.Default?.Id,
                Command = new RelayCommand(() => dev.MakeDefaultDevice()),
            }));

            if (!ret.Any())
            {
                ret.Add(new ContextMenuItem
                {
                    DisplayName = EarTrumpet.Properties.Resources.ContextMenuNoDevices,
                    IsEnabled = false,
                });
            }

            ret.AddRange(new List<ContextMenuItem>
                {
                    new ContextMenuSeparator(),
                    new ContextMenuItem
                    {
                        DisplayName = EarTrumpet.Properties.Resources.WindowsLegacyMenuText,
                        Children = new List<ContextMenuItem>
                        {
                            new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.LegacyVolumeMixerText, Command =  new RelayCommand(LegacyControlPanelHelper.StartLegacyAudioMixer) },
                            new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.PlaybackDevicesText, Command = new RelayCommand(() => LegacyControlPanelHelper.Open("playback")) },
                            new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.RecordingDevicesText, Command = new RelayCommand(() => LegacyControlPanelHelper.Open("recording")) },
                            new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.SoundsControlPanelText, Command = new RelayCommand(() => LegacyControlPanelHelper.Open("sounds")) },
                            new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.OpenSoundSettingsText, Command = new RelayCommand(() => SettingsPageHelper.Open("sound")) },
                        },
                    },
                    new ContextMenuSeparator(),
                });

            var addonItems = AddonManager.Host.TrayContextMenuItems?.OrderBy(x => x.Items.FirstOrDefault()?.DisplayName).SelectMany(ext => ext.Items);
            if (addonItems != null && addonItems.Any())
            {
                ret.AddRange(addonItems);
                ret.Add(new ContextMenuSeparator());
            }

            ret.AddRange(new List<ContextMenuItem>
                {
                    new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.FullWindowTitleText, Command = new RelayCommand(_mixerWindow.OpenOrBringToFront) },
                    new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.SettingsWindowText, Command = new RelayCommand(_settingsWindow.OpenOrBringToFront) },
                    new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.ContextMenuExitTitle, Command = new RelayCommand(Shutdown) },
                });
            return ret;
        }

        private Window CreateSettingsExperience()
        {
            var defaultCategory = new SettingsCategoryViewModel(
                EarTrumpet.Properties.Resources.SettingsCategoryTitle,
                "\xE71D",
                EarTrumpet.Properties.Resources.SettingsDescriptionText,
                null,
                new SettingsPageViewModel[]
                    {
                        new EarTrumpetShortcutsPageViewModel(Settings),
                        new EarTrumpetLegacySettingsPageViewModel(Settings),
                        new EarTrumpetAboutPageViewModel(() => _errorReporter.DisplayDiagnosticData(), Settings)
                    });

            var allCategories = new List<SettingsCategoryViewModel>();
            allCategories.Add(defaultCategory);

            if (AddonManager.Host.SettingsItems != null)
            {
                allCategories.AddRange(AddonManager.Host.SettingsItems.Select(a => a.Get(AddonManager.FindAddonInfoForObject(a))));
            }

            var viewModel = new SettingsViewModel(EarTrumpet.Properties.Resources.SettingsWindowText, allCategories);
            return new SettingsWindow { DataContext = viewModel };
        }

        private Window CreateMixerExperience() => new FullWindow { DataContext = new FullWindowViewModel(_collectionViewModel) };

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint procId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr WindowFromPoint(System.Drawing.Point Point);
    }
}