﻿using EarTrumpet.Interop.Helpers;

namespace EarTrumpet.UI.ViewModels
{
    internal class EarTrumpetShortcutsPageViewModel : SettingsPageViewModel
    {
        private static readonly string s_hotkeyNoneText = new HotkeyData().ToString();

        public HotkeyViewModel OpenFlyoutHotkey { get; }
        public string DefaultHotKey => s_hotkeyNoneText;

        public HotkeyViewModel OpenMixerHotkey { get; }
        public string DefaultMixerHotKey => s_hotkeyNoneText;

        public HotkeyViewModel OpenSettingsHotkey { get; }
        public string DefaultSettingsHotKey => s_hotkeyNoneText;

        public HotkeyViewModel VolumeShiftHotkey { get; }
        public string DefaultVolumeShiftHotKey => s_hotkeyNoneText;

        public HotkeyViewModel MuteVolumeHotkey { get; }
        public string DefaultMuteVolumeHotKey => s_hotkeyNoneText;

        public HotkeyViewModel AppVolumeShiftHotkey { get; }
        public string DefaultAppVolumeShiftHotKey => s_hotkeyNoneText;

        public HotkeyViewModel MuteAppVolumeHotkey { get; }
        public string DefaultMuteAppVolumeHotKey => s_hotkeyNoneText;

        public HotkeyViewModel SaveVolumesHotkey { get; }
        public string DefaultSaveVolumesHotKey => s_hotkeyNoneText;

        public HotkeyViewModel OpenVolumesHotkey { get; }
        public string DefaultOpenVolumesHotKey => s_hotkeyNoneText;

        public HotkeyViewModel EnableUnlimitedAppControlHotKey { get; }
        public string DefaultEnableUnlimitedAppControlHotKey => s_hotkeyNoneText;

        public EarTrumpetShortcutsPageViewModel(AppSettings settings) : base(null)
        {
            Title = Properties.Resources.ShortcutsPageText;
            Glyph = "\xE765";

            OpenFlyoutHotkey = new HotkeyViewModel(settings.FlyoutHotkey, (newHotkey) => settings.FlyoutHotkey = newHotkey, true);
            OpenMixerHotkey = new HotkeyViewModel(settings.MixerHotkey, (newHotkey) => settings.MixerHotkey = newHotkey, true);
            OpenSettingsHotkey = new HotkeyViewModel(settings.SettingsHotkey, (newHotkey) => settings.SettingsHotkey = newHotkey, true);
            VolumeShiftHotkey = new HotkeyViewModel(settings.VolumeShiftHotkey, (newHotkey) => settings.VolumeShiftHotkey = newHotkey, false);
            MuteVolumeHotkey = new HotkeyViewModel(settings.MuteVolumeHotkey, (newHotkey) => settings.MuteVolumeHotkey = newHotkey, true);
            AppVolumeShiftHotkey = new HotkeyViewModel(settings.AppVolumeShiftHotkey, (newHotkey) => settings.AppVolumeShiftHotkey = newHotkey, false);
            MuteAppVolumeHotkey = new HotkeyViewModel(settings.MuteAppVolumeHotkey, (newHotkey) => settings.MuteAppVolumeHotkey = newHotkey, true);
            SaveVolumesHotkey = new HotkeyViewModel(settings.SaveVolumesHotkey, (newHotkey) => settings.SaveVolumesHotkey = newHotkey, true);
            OpenVolumesHotkey = new HotkeyViewModel(settings.OpenVolumesHotkey, (newHotkey) => settings.OpenVolumesHotkey = newHotkey, true);
            EnableUnlimitedAppControlHotKey = new HotkeyViewModel(settings.EnableUnlimitedAppControlHotKey, (newHotkey) => settings.EnableUnlimitedAppControlHotKey = newHotkey, true);
        }
    }
}