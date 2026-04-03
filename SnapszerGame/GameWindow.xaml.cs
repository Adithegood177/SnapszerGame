using System.Threading.Tasks;
using System.Windows;
using SnapszerGame.game;

namespace SnapszerGame
{
    public partial class GameWindow : Window
    {
        private GameViewModel _vm;

        public GameWindow(GameViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            _vm = vm;
        }

        // Animate dealing: alternate cards, then prompt for adu if player starts
        public async Task StartDealing(bool playerStarts)
        {
            // Clear any existing hands (safety)
            _vm.JatekosLapok.Clear();
            _vm.EllensegLapok.Clear();

            // Simple alternate dealing animation
            for (int i = 0; i < 5; i++)
            {
                if (playerStarts)
                {
                    var pcard = _vm.Pakli.Huzas();
                    if (pcard != null) _vm.JatekosLapok.Add(pcard);
                    await Task.Delay(250);

                    var ecard = _vm.Pakli.Huzas();
                    if (ecard != null) _vm.EllensegLapok.Add(ecard);
                    await Task.Delay(250);
                }
                else
                {
                    var ecard = _vm.Pakli.Huzas();
                    if (ecard != null) _vm.EllensegLapok.Add(ecard);
                    await Task.Delay(250);

                    var pcard = _vm.Pakli.Huzas();
                    if (pcard != null) _vm.JatekosLapok.Add(pcard);
                    await Task.Delay(250);
                }
            }

            // Dealing finished - handle adu choice
            if (playerStarts)
            {
                _vm.AduValasztasFolyamatban = true;
                _vm.StatuszUzenet = "Válassz adut";

                // show adu selector modal on top of game window
                var aduWindow = new AduValasztoWindow();
                aduWindow.Owner = this;
                var result = aduWindow.ShowDialog();
                if (result == true)
                {
                    _vm.JatekosAdutValaszt(aduWindow.ValasztottAdu);
                }
                else
                {
                    // If dialog canceled, clear selection state
                    _vm.AduValasztasFolyamatban = false;
                    _vm.StatuszUzenet = string.Empty;
                }
            }
            else
            {
                _vm.AduValasztasFolyamatban = false;
                _vm.AduSzin = _vm.EllensegLapok.GroupBy(l => l.szin).OrderByDescending(g => g.Count()).First().Key;
                _vm.EnKovetkezem = true;
                _vm.StatuszUzenet = "A gép választott adut";
            }
        }
    }
}