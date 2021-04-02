using EarTrumpet.DataModel;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.UI.Controls;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace EarTrumpet.UI.Views
{
    /// <summary>
    /// Interaction logic for AppVolumeBanner.xaml
    /// </summary>
    public partial class VolumeBanner
    {
        private VolumeBannerModel Model;
        private IAudioDeviceSession session;
        private float volume;
        //private Task close;
        public const int DisplayTime = 2000;
        public const int FadeTime = 1000;
        private const int masterHeight = 141;
        private const int appHeight = 179;

        public VolumeBanner()
        {
            InitializeComponent();
            Model = new VolumeBannerModel();
            DataContext = Model;
            AccentRect.Fill = new SolidColorBrush(SystemSettings.AccentColor);
            Show();
            Hide();
            this.ApplyExtendedWindowStyle(User32.WS_EX_TOOLWINDOW);
            SourceInitialized += WindowSourceInitialized;
            Themes.Manager.Current.ThemeChanged += Current_ThemeChanged;
        }

        private void Current_ThemeChanged()
        {
            AccentRect.Fill = new SolidColorBrush(SystemSettings.AccentColor);
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MOVE = 0xF010;

            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        public void ChangeAppVolume(IAudioDeviceSession sess)
        {
            if (session == null || sess.ProcessId != session.ProcessId)
            {
                session = sess;
                AppIcon.Source = ImageEx.LoadShellIcon(session.IconPath, session.IsDesktopApp, 24, 24);
            }
            AppIcon.Opacity = 1.0;
            Height = appHeight;
            Top = 22;
            if (sess.IsMuted)
            {
                Model.MutedText = "+";
                Model.VolumeText = "";
            }
            else
            {
                Model.MutedText = "";
                Model.VolumeText = ((int)Math.Round(session.Volume * 100.0f)).ToString();
            }
            AccentRect.Height = (int)(session.Volume * (80 - 12));
            Model.BoxMargin = new Thickness(0, 0, 0, session.Volume * (80 - 12) + 42);
            PopUp();
        }

        public void ChangeMasterVolume(float vol, bool muted)
        {
            volume = vol;
            AppIcon.Opacity = 0.0;
            Height = masterHeight;
            Top = 60;
            if (muted)
            {
                Model.MutedText = "+";
                Model.VolumeText = "";
            }
            else
            {
                Model.MutedText = "";
                Model.VolumeText = ((int)Math.Round(volume * 100.0f)).ToString();
            }
            AccentRect.Height = (int)(volume * (80 - 12));
            Model.BoxMargin = new Thickness(0, 0, 0, volume * (80 - 12) + 42);
            PopUp();
        }

        public void PopUp()
        {
            WindowAnimationLibrary.StopAnimation(this);
            if (Visibility != Visibility.Visible)
            {
                Opacity = 1.0f;
                Show();
            }
            App.UpdateBannerTimeStamp();
            Task.Delay(new TimeSpan(0, 0, 0, 0, DisplayTime)).ContinueWith(o => App.InvokeHideBanner());
        }

        public void PopDown()
        {
            if (Visibility == Visibility.Visible)
            {
                WindowAnimationLibrary.BeginBannerExitAnimation(this);
            }
        }

        public void ForceHide()
        {
            if (Visibility == Visibility.Visible)
            {
                Hide();
            }
        }
    }
}
