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
        private void StartGomb_Click(object sender, RoutedEventArgs e)
        {
            vm.JatekInditasa();

            if (vm.AduValasztasFolyamatban)
            {
                AduValasztoWindow aduAblak = new AduValasztoWindow();
                aduAblak.Owner = this;

                if (aduAblak.ShowDialog() == true)
                {
                    vm.JatekosAdutValaszt(aduAblak.ValasztottAdu);
                }
            }
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