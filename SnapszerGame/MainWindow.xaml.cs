using System.Windows;
using SnapszerGame.game;

namespace SnapszerGame
{
    public partial class MainWindow : Window
    {
        // Létrehozzuk a ViewModel-t
        GameViewModel vm = new GameViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = vm; // Összekötjük a grafikát a logikával
        }

        // Játék indítása gomb
        private void StartGomb_Click(object sender, RoutedEventArgs e)
        {
            vm.JatekInditasa();
        }

        // A 4 adu választó gomb eseménye
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