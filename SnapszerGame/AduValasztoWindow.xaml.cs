using System.Windows;
using SnapszerGame.game;

namespace SnapszerGame
{
    public partial class AduValasztoWindow : Window
    {
        public Szin ValasztottAdu { get; private set; }

        public AduValasztoWindow()
        {
            InitializeComponent();
        }

        private void PirosGomb_Click(object sender, RoutedEventArgs e)
        {
            ValasztottAdu = Szin.Piros;
            DialogResult = true;
            Close();
        }

        private void ZoldGomb_Click(object sender, RoutedEventArgs e)
        {
            ValasztottAdu = Szin.Zold;
            DialogResult = true;
            Close();
        }

        private void MakkGomb_Click(object sender, RoutedEventArgs e)
        {
            ValasztottAdu = Szin.Makk;
            DialogResult = true;
            Close();
        }

        private void TokGomb_Click(object sender, RoutedEventArgs e)
        {
            ValasztottAdu = Szin.Tok;
            DialogResult = true;
            Close();
        }
    }
}
