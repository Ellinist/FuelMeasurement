using FuelMeasurement.Common.Constants;
using Prism.Commands;
using Prism.Mvvm;

namespace FuelMeasurement.Common.SettingsModule.ViewModels.DetailedViewModels
{
    public class AppConfigurationViewModel : BindableBase
    {
        private bool _isFieldsEnabled = false;
        public double MinPitch { get; set; }
        public double MaxPitch { get; set; }
        public double MinRoll { get; set; }
        public double MaxRoll { get; set; }
        public double PitchStep { get; set; }
        public double RollStep { get; set; }
        public double ReferencedPitch { get; set; }
        public double ReferencedRoll { get; set; }
        public int NodesQuantity { get; set; }
        public bool IsFieldsEnabled
        {
            get => _isFieldsEnabled;
            set
            {
                _isFieldsEnabled = value;
                RaisePropertyChanged(nameof(IsFieldsEnabled));
            }
        }
        public DelegateCommand EditCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        public AppConfigurationViewModel()
        {
            SetStartParameters();
            EditCommand = new DelegateCommand(EditConfigurationParameters);
            SaveCommand = new DelegateCommand(SaveConfigurationParameters);
        }

        private void EditConfigurationParameters()
        {
            IsFieldsEnabled = true;
        }

        private void SaveConfigurationParameters()
        {
            IsFieldsEnabled = false;
        }

        private void SetStartParameters()
        {
            MinPitch = BranchConfigurationModelDefault.MinPitch;
            MaxPitch = BranchConfigurationModelDefault.MaxPitch;
            MinRoll = BranchConfigurationModelDefault.MinRoll;
            MaxRoll = BranchConfigurationModelDefault.MaxRoll;
            PitchStep = BranchConfigurationModelDefault.PitchStep;
            RollStep = BranchConfigurationModelDefault.RollStep;
            ReferencedPitch = BranchConfigurationModelDefault.ReferencedPitch;
            ReferencedRoll = BranchConfigurationModelDefault.ReferencedRoll;
        }
    }
}
