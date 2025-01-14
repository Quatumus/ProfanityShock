using ProfanityShock.Config;
using System.ComponentModel;

public enum ControlType
{
    Stop = 0,
    Shock = 1,
    Vibrate = 2,
    Sound = 3
}

namespace ProfanityShock.Config
{
    public class Shocker : INotifyPropertyChanged
    {
        public string? Name { get; set; }
        public required string ID { get; set; }
        public string? Device { get; set; }
        public required ControlType Controltype { get; set; } = ControlType.Shock;
        public required int Warning { get; set; }
        public required float Delay { get; set; }
        public bool Paused { get; set; }

        private int intensity;
        private int duration;

        public int Intensity
        {
            get { return intensity; }
            set
            {
                intensity = value;
                OnPropertyChanged(nameof(Intensity));
            }
        }

        public int Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }

    
    

}