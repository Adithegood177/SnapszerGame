using System.Windows;
using SnapszerGame.game;

namespace SnapszerGame
{
    public partial class AduValasztoWindow : Window
    {
        // Ide mentjük mit választott a user
        public Szin ValasztottAdu { get; private set; }

        // Konstruktor → betölti az ablakot
        public AduValasztoWindow()
        {
            InitializeComponent();
        }
        // Piros gomb → user ezt választotta
        private void PirosGomb_Click(object sender, RoutedEventArgs e)
        {
            ValasztottAdu = Szin.Piros;
            DialogResult = true; // jelzi hogy OK-val zártuk
            Close(); // ablak bezár, többi same
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