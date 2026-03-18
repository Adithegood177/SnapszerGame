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
        }

        // Adu választó gombok
        private void PirosGomb_Click(object sender, RoutedEventArgs e)
        {
            vm.JatekosAdutValaszt(Szin.Piros);
        }

        private void ZoldGomb_Click(object sender, RoutedEventArgs e)
        {
            vm.JatekosAdutValaszt(Szin.Zold);
        }

        private void MakkGomb_Click(object sender, RoutedEventArgs e)
        {
            vm.JatekosAdutValaszt(Szin.Makk);
        }

        private void TokGomb_Click(object sender, RoutedEventArgs e)
        {
            vm.JatekosAdutValaszt(Szin.Tok);
        }
    }
}