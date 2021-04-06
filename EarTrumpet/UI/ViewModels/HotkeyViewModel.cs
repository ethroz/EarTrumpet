using EarTrumpet.Interop.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class HotkeyViewModel : BindableBase
    {
        private string _hotkeyText;
        public string HotkeyText
        {
            get => _hotkeyText;
            set
            {
                if (_hotkeyText != value)
                {
                    _hotkeyText = value;
                    RaisePropertyChanged(nameof(HotkeyText));

                    if (string.IsNullOrWhiteSpace(_hotkeyText))
                    {
                        _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                        _hotkey.Key = System.Windows.Forms.Keys.None;
                    }
                }
            }
        }

        private readonly Action<HotkeyData> _save;
        private HotkeyData _hotkey;
        private HotkeyData _savedHotkey;
        private readonly bool legal;

        public HotkeyViewModel(HotkeyData hotkey, Action<HotkeyData> save, bool legal)
        {
            _hotkey = hotkey;
            _savedHotkey = new HotkeyData { Key = hotkey.Key, Modifiers = hotkey.Modifiers };
            _save = save;
            this.legal = legal;

            SetHotkeyText();
        }

        public void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If Alt is down, a system key is being prepared
            var key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            // Impossible hotkeys (even with modifiers):
            // Tab, Backspace, Escape
            if (key == Key.Tab)
            {
                return;
            }

            e.Handled = true;

            if (key == Key.Escape || key == Key.Back)
            {
                // Clear selection
                _hotkey.Key = System.Windows.Forms.Keys.None;
                _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                _hotkey.Combination = null;
            }
            else
            {
                if (legal)
                {
                    _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                    _hotkey.Key = System.Windows.Forms.Keys.None;

                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        _hotkey.Modifiers = System.Windows.Forms.Keys.Control;
                    }

                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        _hotkey.Modifiers |= System.Windows.Forms.Keys.Shift;
                    }

                    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    {
                        _hotkey.Modifiers |= System.Windows.Forms.Keys.Alt;
                    }

                    if (key != Key.LeftShift && key != Key.RightShift &&
                        key != Key.LeftAlt && key != Key.RightAlt &&
                        key != Key.LeftCtrl && key != Key.RightCtrl &&
                        key != Key.CapsLock && key != Key.LWin && key != Key.RWin)
                    {
                        // Ignore all types of modifiers
                        _hotkey.Key = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(key);
                    }
                }
                else
                {
                    _hotkey.Combination = null;
                    List<Key> mods = new List<Key>();
                    _hotkey.Indices = new bool[8];

                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        mods.Add(Key.LeftCtrl);
                        _hotkey.Indices[0] = true;
                    }
                    if (Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        mods.Add(Key.RightCtrl);
                        _hotkey.Indices[1] = true;
                    }
                    if (Keyboard.IsKeyDown(Key.LeftAlt))
                    {
                        mods.Add(Key.LeftAlt);
                        _hotkey.Indices[2] = true;
                    }
                    if (Keyboard.IsKeyDown(Key.RightAlt))
                    {
                        mods.Add(Key.RightAlt);
                        _hotkey.Indices[3] = true;
                    }
                    if (Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        mods.Add(Key.LeftShift);
                        _hotkey.Indices[4] = true;
                    }
                    if (Keyboard.IsKeyDown(Key.RightShift))
                    {
                        mods.Add(Key.RightShift);
                        _hotkey.Indices[5] = true;
                    }
                    if (Keyboard.IsKeyDown(Key.LWin))
                    {
                        mods.Add(Key.LWin);
                        _hotkey.Indices[6] = true;
                    }
                    if (Keyboard.IsKeyDown(Key.RWin))
                    {
                        mods.Add(Key.RWin);
                        _hotkey.Indices[7] = true;
                    }

                    _hotkey.Combination = mods.ToArray();
                }
            }
            SetHotkeyText();
        }

        public void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (legal)
            {
                // Disallow e.g. Alt+None modifier-only hotkeys.
                if (_hotkey.Key == System.Windows.Forms.Keys.None &&
                    _hotkey.Modifiers != System.Windows.Forms.Keys.None)
                {
                    _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                    SetHotkeyText();
                }
            }
            else
            {
                if (_hotkey.Combination == null)
                {
                    HotkeyText = "";
                }
            }

            if (_hotkey != _savedHotkey)
            {
                _save(_hotkey);
                _savedHotkey = new HotkeyData { Key = _hotkey.Key, Modifiers = _hotkey.Modifiers, Indices = _hotkey.Indices, Combination = _hotkey.Combination };
            }
            HotkeyManager.Current.Resume();
        }

        public void OnGotFocus(object sender, RoutedEventArgs e)
        {
            HotkeyManager.Current.Pause();
        }

        private void SetHotkeyText()
        {
            if (legal)
            {
                HotkeyText = _hotkey.ToString().Replace(System.Windows.Forms.Keys.None.ToString(), "");
            }
            else
            {
                string output = "";
                if (_hotkey.Combination != null)
                {
                    for (int i = 0; i < _hotkey.Combination.Length; i++)
                    {
                        if (i == _hotkey.Combination.Length - 1)
                            output += _hotkey.Combination[_hotkey.Combination.Length - 1].ToString();
                        else
                            output += _hotkey.Combination[i].ToString() + "+";
                    }
                }
                HotkeyText = output;
            }
        }
    }
}
