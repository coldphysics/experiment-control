using System.Windows;

namespace Controller.Data.Channels
{
    //RECO implement using MVVM
    /// <summary>
    /// Interaktionslogik für SimpleStringOkWindow.xaml
    /// </summary>
    public partial class AnalogSettingsWindow : Window
    {
        public AnalogSettingsWindow(ChannelSettingsController controller)
        {
            this.DataContext = controller;
            InitializeComponent();
        }




    }
}
