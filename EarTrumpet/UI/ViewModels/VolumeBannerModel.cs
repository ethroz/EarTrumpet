using System.Windows;

namespace EarTrumpet.UI.ViewModels
{
    class VolumeBannerModel : BindableBase
    {
        private string _volumeText;
        public string VolumeText
        {
            get => _volumeText;
            set
            {
                if (_volumeText != value)
                {
                    _volumeText = value;
                    RaisePropertyChanged(nameof(VolumeText));
                }
            }
        }
        private bool _isMuted;
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (_isMuted != value)
                {
                    _isMuted = value;
                    RaisePropertyChanged(nameof(IsMuted));
                }
            }
        }
        private Thickness _boxMargin;
        public Thickness BoxMargin
        {
            get => _boxMargin;
            set
            {
                if (_boxMargin != value)
                {
                    _boxMargin = value;
                    RaisePropertyChanged(nameof(BoxMargin));
                }
            }
        }
    }
}
