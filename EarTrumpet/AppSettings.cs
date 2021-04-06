using EarTrumpet.DataModel.Storage;
using EarTrumpet.Interop.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace EarTrumpet
{
    public class AppSettings
    {
        public event EventHandler<bool> UseLegacyIconChanged;
        public event Action FlyoutHotkeyTyped;
        public event Action MixerHotkeyTyped;
        public event Action SettingsHotkeyTyped;
        public event Action MuteVolumeHotkeyTyped;
        public event Action MuteAppVolumeHotkeyTyped;
        public event Action SaveVolumesHotkeyTyped;
        public event Action OpenVolumesHotkeyTyped;
        public event Action EnableUnlimitedAppControlHotKeyTyped;

        private ISettingsBag _settings = StorageFactory.GetSettings();

        public void RegisterHotkeys()
        {
            HotkeyManager.Current.Register(FlyoutHotkey);
            HotkeyManager.Current.Register(MixerHotkey);
            HotkeyManager.Current.Register(SettingsHotkey);
            HotkeyManager.Current.Register(MuteVolumeHotkey);
            HotkeyManager.Current.Register(MuteAppVolumeHotkey);
            HotkeyManager.Current.Register(SaveVolumesHotkey);
            HotkeyManager.Current.Register(OpenVolumesHotkey);

            VolumeShiftHotkey.Indices = new bool[8];
            if (VolumeShiftHotkey.Combination != null)
            {
                foreach (Key k in VolumeShiftHotkey.Combination)
                {
                    switch (k)
                    {
                        case Key.LeftCtrl:
                            VolumeShiftHotkey.Indices[0] = true;
                            break;
                        case Key.RightCtrl:
                            VolumeShiftHotkey.Indices[1] = true;
                            break;
                        case Key.LeftAlt:
                            VolumeShiftHotkey.Indices[2] = true;
                            break;
                        case Key.RightAlt:
                            VolumeShiftHotkey.Indices[3] = true;
                            break;
                        case Key.LeftShift:
                            VolumeShiftHotkey.Indices[4] = true;
                            break;
                        case Key.RightShift:
                            VolumeShiftHotkey.Indices[5] = true;
                            break;
                        case Key.LWin:
                            VolumeShiftHotkey.Indices[6] = true;
                            break;
                        case Key.RWin:
                            VolumeShiftHotkey.Indices[7] = true;
                            break;
                    }
                }
            }

            AppVolumeShiftHotkey.Indices = new bool[8];
            if (AppVolumeShiftHotkey.Combination != null)
            {
                foreach (Key k in AppVolumeShiftHotkey.Combination)
                {
                    switch (k)
                    {
                        case Key.LeftCtrl:
                            AppVolumeShiftHotkey.Indices[0] = true;
                            break;
                        case Key.RightCtrl:
                            AppVolumeShiftHotkey.Indices[1] = true;
                            break;
                        case Key.LeftAlt:
                            AppVolumeShiftHotkey.Indices[2] = true;
                            break;
                        case Key.RightAlt:
                            AppVolumeShiftHotkey.Indices[3] = true;
                            break;
                        case Key.LeftShift:
                            AppVolumeShiftHotkey.Indices[4] = true;
                            break;
                        case Key.RightShift:
                            AppVolumeShiftHotkey.Indices[5] = true;
                            break;
                        case Key.LWin:
                            AppVolumeShiftHotkey.Indices[6] = true;
                            break;
                        case Key.RWin:
                            AppVolumeShiftHotkey.Indices[7] = true;
                            break;
                    }
                }
            }

            HotkeyManager.Current.KeyPressed += (hotkey) =>
            {
                if (hotkey.Equals(FlyoutHotkey))
                {
                    Trace.WriteLine("AppSettings FlyoutHotkeyTyped");
                    FlyoutHotkeyTyped?.Invoke();
                }
                else if (hotkey.Equals(SettingsHotkey))
                {
                    Trace.WriteLine("AppSettings SettingsHotkeyTyped");
                    SettingsHotkeyTyped?.Invoke();
                }
                else if (hotkey.Equals(MixerHotkey))
                {
                    Trace.WriteLine("AppSettings MixerHotkeyTyped");
                    MixerHotkeyTyped?.Invoke();
                }
                else if (hotkey.Equals(MuteVolumeHotkey))
                {
                    Trace.WriteLine("AppSettings MuteVolumeHotkeyTyped");
                    MuteVolumeHotkeyTyped?.Invoke();
                }
                else if (hotkey.Equals(MuteAppVolumeHotkey))
                {
                    Trace.WriteLine("AppSettings MuteAppVolumeHotkeyTyped");
                    MuteAppVolumeHotkeyTyped?.Invoke();
                }
                else if (hotkey.Equals(SaveVolumesHotkey))
                {
                    Trace.WriteLine("AppSettings SaveVolumesHotkeyTyped");
                    SaveVolumesHotkeyTyped?.Invoke();
                }
                else if (hotkey.Equals(OpenVolumesHotkey))
                {
                    Trace.WriteLine("AppSettings OpenVolumesHotkeyTyped");
                    OpenVolumesHotkeyTyped?.Invoke();
                }
                else if (hotkey.Equals(EnableUnlimitedAppControlHotKey))
                {
                    Trace.WriteLine("AppSettings EnableUnlimitedAppControlHotKeyTyped");
                    EnableUnlimitedAppControlHotKeyTyped?.Invoke();
                }
            };
        }

        public HotkeyData FlyoutHotkey
        {
            get => _settings.Get("Hotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(FlyoutHotkey);
                _settings.Set("Hotkey", value);
                HotkeyManager.Current.Register(FlyoutHotkey);
            }
        }

        public HotkeyData MixerHotkey
        {
            get => _settings.Get("MixerHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(MixerHotkey);
                _settings.Set("MixerHotkey", value);
                HotkeyManager.Current.Register(MixerHotkey);
            }
        }

        public HotkeyData SettingsHotkey
        {
            get => _settings.Get("SettingsHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(SettingsHotkey);
                _settings.Set("SettingsHotkey", value);
                HotkeyManager.Current.Register(SettingsHotkey);
            }
        }

        public HotkeyData VolumeShiftHotkey
        {
            get => _settings.Get("VolumeShiftHotkey", new HotkeyData { });
            set
            {
                _settings.Set("VolumeShiftHotkey", value);
            }
        }

        public HotkeyData MuteVolumeHotkey
        {
            get => _settings.Get("MuteVolumeHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(MuteVolumeHotkey);
                _settings.Set("MuteVolumeHotkey", value);
                HotkeyManager.Current.Register(MuteVolumeHotkey);
            }
        }

        public HotkeyData AppVolumeShiftHotkey
        {
            get
            {
                HotkeyData hk = _settings.Get("AppVolumeShiftHotkey", new HotkeyData { });
                return hk;
            }
            set
            {
                _settings.Set("AppVolumeShiftHotkey", value);
            }
        }

        public HotkeyData MuteAppVolumeHotkey
        {
            get => _settings.Get("MuteAppVolumeHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(MuteAppVolumeHotkey);
                _settings.Set("MuteAppVolumeHotkey", value);
                HotkeyManager.Current.Register(MuteAppVolumeHotkey);
            }
        }

        public HotkeyData SaveVolumesHotkey
        {
            get => _settings.Get("SaveVolumesHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(SaveVolumesHotkey);
                _settings.Set("SaveVolumesHotkey", value);
                HotkeyManager.Current.Register(SaveVolumesHotkey);
            }
        }

        public HotkeyData OpenVolumesHotkey
        {
            get => _settings.Get("OpenVolumesHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(OpenVolumesHotkey);
                _settings.Set("OpenVolumesHotkey", value);
                HotkeyManager.Current.Register(OpenVolumesHotkey);
            }
        }

        public HotkeyData EnableUnlimitedAppControlHotKey
        {
            get => _settings.Get("EnableUnlimitedAppControlHotKey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(EnableUnlimitedAppControlHotKey);
                _settings.Set("EnableUnlimitedAppControlHotKey", value);
                HotkeyManager.Current.Register(EnableUnlimitedAppControlHotKey);
            }
        }

        public void SaveVolumes(string[][][] volumes)
        {
            _settings.Set("SavedVolumes", volumes);
        }

        public string[][][] OpenVolumes()
        {
            string[][][] defaut = new string[0][][];
            string[][][] output = _settings.Get("SavedVolumes", defaut);
            if (output.Length == 0)
                return null;
            return output;
        }

        public void SaveBool(string name, bool value)
        {
            _settings.Set(name, value);
        }

        public bool OpenBool(string name, bool defaut)
        {
            return _settings.Get(name, defaut);
        }

        public bool UseLegacyIcon
        {
            get
            {
                // Note: Legacy compat, we used to write string bools.
                var ret = _settings.Get("UseLegacyIcon", "False");
                bool.TryParse(ret, out bool isUseLegacyIcon);
                return isUseLegacyIcon;
            }
            set
            {
                _settings.Set("UseLegacyIcon", value.ToString());
                UseLegacyIconChanged?.Invoke(null, UseLegacyIcon);
            }
        }

        public bool HasShownFirstRun
        {
            get => _settings.HasKey("hasShownFirstRun");
            set => _settings.Set("hasShownFirstRun", value);
        }

        public bool IsTelemetryEnabled
        {
            get
            {
                return _settings.Get("IsTelemetryEnabled", IsTelemetryEnabledByDefault());
            }
            set => _settings.Set("IsTelemetryEnabled", value);
        }

        private bool IsTelemetryEnabledByDefault()
        {
            // Discussion on what to include:
            // https://gist.github.com/henrik/1688572
            var europeanUnionRegions = new string[]
            {
                // EU 28
                "AT", // Austria
                "BE", // Belgium
                "BG", // Bulgaria
                "HR", // Croatia
                "CY", // Cyprus
                "CZ", // Czech Republic
                "DK", // Denmark
                "EE", // Estonia
                "FI", // Finland
                "FR", // France
                "DE", // Germany
                "GR", // Greece
                "HU", // Hungary
                "IE", // Ireland, Republic of (EIRE)
                "IT", // Italy
                "LV", // Latvia
                "LT", // Lithuania
                "LU", // Luxembourg
                "MT", // Malta
                "NL", // Netherlands
                "PL", // Poland
                "PT", // Portugal
                "RO", // Romania
                "SK", // Slovakia
                "SI", // Slovenia
                "ES", // Spain
                "SE", // Sweden
                "GB", // United Kingdom (Great Britain)

                // Outermost Regions (OMR)
                "GF", // French Guiana
                "GP", // Guadeloupe
                "MQ", // Martinique
                "ME", // Montenegro
                "YT", // Mayotte
                "RE", // Réunion
                "MF", // Saint Martin

                // Special Cases: Part of EU
                "GI", // Gibraltar
                "AX", // Åland Islands

                // Overseas Countries and Territories (OCT)
                "PM", // Saint Pierre and Miquelon
                "GL", // Greenland
                "BL", // Saint Bartelemey
                "SX", // Sint Maarten
                "AW", // Aruba
                "CW", // Curacao
                "WF", // Wallis and Futuna
                "PF", // French Polynesia
                "NC", // New Caledonia
                "TF", // French Southern Territories
                "AI", // Anguilla
                "BM", // Bermuda
                "IO", // British Indian Ocean Territory
                "VG", // Virgin Islands, British
                "KY", // Cayman Islands
                "FK", // Falkland Islands (Malvinas)
                "MS", // Montserrat
                "PN", // Pitcairn
                "SH", // Saint Helena
                "GS", // South Georgia and the South Sandwich Islands
                "TC", // Turks and Caicos Islands

                // Microstates
                "AD", // Andorra
                "LI", // Liechtenstein
                "MC", // Monaco
                "SM", // San Marino
                "VA", // Vatican City

                // Other
                "JE", // Jersey
                "GG", // Guernsey
            };
            var region = new Windows.Globalization.GeographicRegion();
            return !europeanUnionRegions.Contains(region.CodeTwoLetter);
        }
    }
}