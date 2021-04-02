using EarTrumpet.Interop.Helpers;
using System;
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
                    _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                    _hotkey.Key = System.Windows.Forms.Keys.None;
                    _hotkey.indices = new bool[8];

                    if (Keyboard.IsKeyDown(Key.LeftCtrl) && key != Key.LeftCtrl)
                    {
                        _hotkey.Modifiers |= System.Windows.Forms.Keys.Control;
                        _hotkey.indices[0] = true;
                    }
                    else if (Keyboard.IsKeyDown(Key.RightCtrl) && key != Key.RightCtrl)
                    {
                        _hotkey.Modifiers |= System.Windows.Forms.Keys.Control;
                        _hotkey.indices[1] = true;
                    }

                    if (Keyboard.IsKeyDown(Key.LeftShift) && key != Key.LeftShift)
                    {
                        _hotkey.Modifiers |= System.Windows.Forms.Keys.Shift;
                        _hotkey.indices[2] = true;
                    }
                    else if (Keyboard.IsKeyDown(Key.RightShift) && key != Key.RightShift)
                    {
                        _hotkey.Modifiers |= System.Windows.Forms.Keys.Shift;
                        _hotkey.indices[3] = true;
                    }

                    if (Keyboard.IsKeyDown(Key.LeftAlt) && key != Key.LeftAlt)
                    {
                        _hotkey.Modifiers |= System.Windows.Forms.Keys.Alt;
                        _hotkey.indices[4] = true;
                    }
                    else if (Keyboard.IsKeyDown(Key.RightAlt) && key != Key.RightAlt)
                    {
                        _hotkey.Modifiers |= System.Windows.Forms.Keys.Alt;
                        _hotkey.indices[5] = true;
                    }

                    if (key == Key.LeftShift || key == Key.RightShift ||
                        key == Key.LeftAlt || key == Key.RightAlt ||
                        key == Key.LeftCtrl || key == Key.RightCtrl ||
                        key == Key.LWin || key == Key.RWin)
                    {
                        // only allow modifiers
                        _hotkey.Key = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(key);
                        switch (key)
                        {
                            case Key.LeftCtrl:
                                _hotkey.indices[0] = true;
                                break;
                            case Key.RightCtrl:
                                _hotkey.indices[1] = true;
                                break;
                            case Key.LeftShift:
                                _hotkey.indices[2] = true;
                                break;
                            case Key.RightShift:
                                _hotkey.indices[3] = true;
                                break;
                            case Key.LeftAlt:
                                _hotkey.indices[4] = true;
                                break;
                            case Key.RightAlt:
                                _hotkey.indices[5] = true;
                                break;
                            case Key.LWin:
                                _hotkey.indices[6] = true;
                                break;
                            case Key.RWin:
                                _hotkey.indices[7] = true;
                                break;
                        }
                    }
                }
            }
            SetHotkeyText();
        }

        public void OnLostFocus(object sender, RoutedEventArgs e)
        {
            // Disallow e.g. Alt+None modifier-only hotkeys.
            if (_hotkey.Key == System.Windows.Forms.Keys.None &&
                _hotkey.Modifiers != System.Windows.Forms.Keys.None)
            {
                _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                SetHotkeyText();
            }

            if (_hotkey != _savedHotkey)
            {
                _save(_hotkey);
                _savedHotkey = new HotkeyData { Key = _hotkey.Key, Modifiers = _hotkey.Modifiers };
            }
            HotkeyManager.Current.Resume();
        }

        public void OnGotFocus(object sender, RoutedEventArgs e)
        {
            HotkeyManager.Current.Pause();
        }

        private void SetHotkeyText()
        {
            HotkeyText = _hotkey.ToString().Replace(System.Windows.Forms.Keys.None.ToString(), "");
        }
    }
}
