using System.Windows;

namespace Controller.Control
{
    //RECO implement using MVVM
    /// <summary>
    /// Interaktionslogik für SimpleStringOkWindow.xaml
    /// </summary>
    public partial class SimpleStringOkWindow : Window
    {
        public SimpleStringOkWindow()
        {
            InitializeComponent();
        }


        private static SimpleStringOkWindow window = null;
        public static void ShowNewSimpleStringOkWindow(string title, string text)
        {
            if (window != null)
            {
                window.Close();
            }
            window = new SimpleStringOkWindow();
            window.TextBlock1.Text = text;
            window.Title = title;
            window.Show();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
