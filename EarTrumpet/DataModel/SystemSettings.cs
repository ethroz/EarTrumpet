using Microsoft.Win32;
using System.Globalization;
using EarTrumpet.Extensions;
using System;
using System.Windows.Media;

namespace EarTrumpet.DataModel
{
    public static class SystemSettings
    {
        static readonly string s_PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        public static bool IsTransparencyEnabled => ReadDword(s_PersonalizeKey, "EnableTransparency");
        public static bool UseAccentColor => ReadDword(s_PersonalizeKey, "ColorPrevalence");
        public static bool IsLightTheme => ReadDword(s_PersonalizeKey, "AppsUseLightTheme", 1);
        public static bool IsSystemLightTheme => LightThemeShim(ReadDword(s_PersonalizeKey, "SystemUsesLightTheme"));
        public static bool UseDynamicScrollbars => ReadDword(@"Control Panel\Accessibility", "DynamicScrollbars", 1);
        public static bool UseAccentColorOnWindowBorders => ReadDword(@"Software\Microsoft\Windows\DWM", "ColorPrevalence");
        public static bool IsRTL => CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;

        public static string BuildLabel
        {
            get
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var subKey = baseKey.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion"))
                {
                    return (string)subKey.GetValue("BuildLabEx", "No BuildLabEx set");
                }
            }
        }

        public static Color AccentColor => GetAccentColor();

        private static bool ReadDword(string key, string valueName, int defaultValue = 0)
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            using (var subKey = baseKey.OpenSubKey(key))
            {
                return subKey.GetValue<int>(valueName, defaultValue) > 0;
            }
        }


        private static Color GetAccentColor()
        {
            const string DWM_KEY = @"Software\Microsoft\Windows\DWM";
            using (RegistryKey dwmKey = Registry.CurrentUser.OpenSubKey(DWM_KEY, RegistryKeyPermissionCheck.ReadSubTree))
            {
                const string KEY_EX_MSG = "The \"HKCU\\" + DWM_KEY + "\" registry key does not exist.";
                if (dwmKey is null) throw new InvalidOperationException(KEY_EX_MSG);

                object accentColorObj = dwmKey.GetValue("AccentColor");
                if (accentColorObj is int accentColorDword)
                {
                    return ParseDWordColor(accentColorDword);
                }
                else
                {
                    const string VALUE_EX_MSG = "The \"HKCU\\" + DWM_KEY + "\\AccentColor\" registry key value could not be parsed as an ABGR color.";
                    throw new InvalidOperationException(VALUE_EX_MSG);
                }
            }

        }

        private static Color ParseDWordColor(int color)
        {
            byte a = (byte)((color >> 24) & 0xFF),
                 b = (byte)((color >> 16) & 0xFF),
                 g = (byte)((color >> 8) & 0xFF),
                 r = (byte)((color >> 0) & 0xFF);
            return Color.FromArgb(a, r, g, b);
        }

        private static bool LightThemeShim(bool registryValue)
        {
            if (Environment.OSVersion.IsGreaterThan(OSVersions.RS5_1809))
            {
                return registryValue;
            }
            return false; // No system theme prior to 19H1/RS6.
        }
    }
}
