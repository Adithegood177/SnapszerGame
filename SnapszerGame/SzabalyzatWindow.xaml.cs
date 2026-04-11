using System.Windows;

namespace SnapszerGame
{
    public partial class SzabalyzatWindow : Window
    {
        public SzabalyzatWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}