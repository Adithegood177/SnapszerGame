using System.Windows;
using SnapszerGame.game;

namespace SnapszerGame
{
    public partial class MainWindow : Window
    {
        GameViewModel vm = new GameViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = vm; // DataContext összekötése
        }

        // Start gomb handler
        private async void StartGomb_Click(object sender, RoutedEventArgs e)
        {
            // Prepare deck and get who starts
            bool playerStarts = vm.PrepareGameStart();

            // Hide main menu
            vm.FomenLathato = Visibility.Collapsed;

            // Open dedicated game window
            GameWindow gw = new GameWindow(vm);
            gw.Owner = this;
            gw.Show();

            // Start dealing animation in the GameWindow
            await gw.StartDealing(playerStarts);
        }

        private void SzabalyzatGomb_Click(object sender, RoutedEventArgs e)
        {
            var win = new SzabalyzatWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        // Kilépés menü
        private void KilepesMenu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Kilépés gomb
        private void KilepesGomb_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}