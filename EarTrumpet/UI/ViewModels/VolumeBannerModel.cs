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
        private string _mutedText;
        public string MutedText
        {
            get => _mutedText;
            set
            {
                if (_mutedText != value)
                {
                    _mutedText = value;
                    RaisePropertyChanged(nameof(MutedText));
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
