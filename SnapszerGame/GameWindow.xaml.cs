using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SnapszerGame.game;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq; // LINQ for Sum

namespace SnapszerGame
{
    public partial class GameWindow : Window
    {
        private GameViewModel _vm;
        private Enemy _gep;
        private bool _jatekFolyamatban = false;

        public GameWindow(GameViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            _vm = vm;
            _gep = new Enemy(new SnapszerLogic());
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
                    if (pcard != null) { _vm.JatekosLapok.Add(pcard); _vm.NotifyCardDrawn(pcard); }
                    await Task.Delay(250);

                    var ecard = _vm.Pakli.Huzas();
                    if (ecard != null) { _vm.EllensegLapok.Add(ecard); _vm.NotifyCardDrawn(ecard); }
                    await Task.Delay(250);
                }
                else
                {
                    var ecard = _vm.Pakli.Huzas();
                    if (ecard != null) { _vm.EllensegLapok.Add(ecard); _vm.NotifyCardDrawn(ecard); }
                    await Task.Delay(250);

                    var pcard = _vm.Pakli.Huzas();
                    if (pcard != null) { _vm.JatekosLapok.Add(pcard); _vm.NotifyCardDrawn(pcard); }
                    await Task.Delay(250);
                }
            }

            // Dealing finished - handle adu choice
            // Update talon (peek last card) so UI can show it
            _vm.FelforditottKartya = _vm.Pakli.PeekLast();
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
                    _jatekFolyamatban = true;
                    // Jatekos jon
                    _vm.EnKovetkezem = true;
                    _vm.FrissitBemondasLehetoseg();
                    _vm.StatuszUzenet = "Te jossz hivassal!";
                }
                else
                {
                    // If dialog canceled
                    _vm.AduValasztasFolyamatban = false;
                    _vm.StatuszUzenet = string.Empty;
                }
            }
            else
            {
                _vm.AduValasztasFolyamatban = false;
                _vm.AduSzin = _vm.EllensegLapok.GroupBy(l => l.szin).OrderByDescending(g => g.Count()).First().Key;
                _vm.StatuszUzenet = $"A gep valasztott adut: {_vm.AduSzin}. Hivashoz keszul...";
                _vm.EnKovetkezem = false;
                _jatekFolyamatban = true;
                _vm.FrissitBemondasLehetoseg();

                // Enemy may announce immediately after dealing (20/40)
                var bem = _gep.TryBemond(_vm.EllensegLapok, _vm.AduSzin, _vm.BemondottSzinek);
                if (bem.did)
                {
                    _vm.EnemyBemond(bem.szin, bem.is40);
                    await MutassBemondasAnimaciot(bem.is40, false);
                }

                // Enemy may also declare snapszer immediately after dealing
                if (_gep.TrySnapszer(_vm.EllensegLapok))
                {
                    _vm.DeclareSnapszer(false);
                    _vm.StatuszUzenet = "A gep snapszert jelentett be!";
                }

                await Task.Delay(1500);
                await GepHiv();
            }
        }

        // Bemondasok
        private void Bemond20_Click(object sender, RoutedEventArgs e)
        {
            _vm.Bemond(false);
            GyozveEllenorzes(); // Ellenorizni kell, h a bemondassal nyert-e
        }

        private void Bemond40_Click(object sender, RoutedEventArgs e)
        {
            _vm.Bemond(true);
            GyozveEllenorzes(); // Ellenorizni kell, h a bemondassal nyert-e
        }

        // Snapszer (declare) button handler
        private void Snapszer_Click(object sender, RoutedEventArgs e)
        {
            if (!_jatekFolyamatban || !_vm.LehetSnapszer) return;

            // Mark that player declared snapszer. Actual resolution happens as tricks are evaluated / at game end.
            _vm.DeclareSnapszer(true);
            _vm.StatuszUzenet = "SNAPSZER bejelentve! Ezt el kell vinni, nem veszthetsz egy ütést sem.";
        }

        // Helper: sum remaining points (played cards, hands and drain deck)
        private int SumAndDrainRemainingPoints()
        {
            int remainingPoints = 0;
            if (_vm.JatekosHivottLap != null) remainingPoints += _vm.JatekosHivottLap.pont;
            if (_vm.EllensegHivottLap != null) remainingPoints += _vm.EllensegHivottLap.pont;

            remainingPoints += _vm.JatekosLapok.Sum(c => c.pont);
            remainingPoints += _vm.EllensegLapok.Sum(c => c.pont);

            // Drain deck
            while (true)
            {
                var c = _vm.Pakli.Huzas();
                if (c == null) break;
                _vm.NotifyCardDrawn(c);
                remainingPoints += c.pont;
            }

            return remainingPoints;
        }

        // Kartyara kattintas (Jatekos lejatssza a lapjat)
        private async void ItemsControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_jatekFolyamatban || !_vm.EnKovetkezem) return;

            var felement = e.OriginalSource as FrameworkElement;
            if (felement == null) return;
            
            var card = felement.DataContext as Card;
            if (card != null)
            {
                // If player still can declare snapszer at moment of first card play, they can also declare via button pre-play. We respect explicit declare via Snapszer_Click.
                await JatekosHiv(card);
            }
        }

        private async Task JatekosHiv(Card lerakandoKartya)
        {
            SnapszerLogic valLogic = new SnapszerLogic();

            // Ha a jatekos valaszol, ellenorizni kell, hogy szabalyos-e
            if (_vm.EllensegHivottLap != null)
            {
                if (!valLogic.SzabalyosKartyaRakas(lerakandoKartya, _vm.EllensegHivottLap, _vm.AduSzin, _vm.PakliLezarva, _vm.JatekosLapok))
                {
                    _vm.StatuszUzenet = "Ezt a lapot szabalyellenes lerakni!";
                    return;
                }
            }

            // Jatekos lerakja
            _vm.JatekosLapok.Remove(lerakandoKartya);
            _vm.JatekosHivottLap = lerakandoKartya;

            _vm.EnKovetkezem = false; // Kikapcsoljuk az interakciot amig a kor lejatszodik

            if (_vm.EllensegHivottLap == null)
            {
                // Jatekos hivott
                _vm.StatuszUzenet = "Te hivtal. Gep valaszol...";
                await Task.Delay(1000);
                
                var gepValasz = _gep.GepValaszol(lerakandoKartya, _vm.AduSzin, _vm.PakliLezarva, _vm.EllensegLapok);
                _vm.EllensegLapok.Remove(gepValasz);
                _vm.EllensegHivottLap = gepValasz;
            }

            // Ki ertekeli a kort!
            await UtesKiErtekelese();
        }

        private async Task GepHiv()
        {
            // Enemy may decide to bemond before leading
            var tryBem = _gep.TryBemond(_vm.EllensegLapok, _vm.AduSzin, _vm.BemondottSzinek);
            if (tryBem.did)
            {
                _vm.EnemyBemond(tryBem.szin, tryBem.is40);
                await MutassBemondasAnimaciot(tryBem.is40, false);
            }

            var hivottLap = _gep.GepHiv(_vm.EllensegLapok, _vm.AduSzin);
            _vm.EllensegLapok.Remove(hivottLap);
            _vm.EllensegHivottLap = hivottLap;
            
            _vm.StatuszUzenet = "A gep hivott, te jossz!";
            _vm.EnKovetkezem = true;
            _vm.FrissitBemondasLehetoseg();
        }

        private void TalonCsere_Click(object sender, RoutedEventArgs e)
        {
            // Can only swap if allowed by VM
            if (!_vm.LehetTalonCsere) return;

            // Player must have adu alsokiraly in hand
            var playerAlsokiraly = _vm.JatekosLapok.FirstOrDefault(c => c.szin == _vm.AduSzin && c.ertek == Ertek.Alsokiraly);
            var talon = _vm.Pakli.PeekLast();
            if (playerAlsokiraly == null || talon == null) return;

            // Swap: replace last with player's card, give last card to player
            var oldLast = _vm.Pakli.ReplaceLast(playerAlsokiraly);

            // Remove player's card from hand and give the old last card
            _vm.JatekosLapok.Remove(playerAlsokiraly);
            _vm.JatekosLapok.Add(oldLast);

            // Update the viewmodel's felforditott card so UI shows rotated card under deck
            _vm.FelforditottKartya = oldLast;

            // After swapping, talon swap is no longer allowed
            _vm.LehetTalonCsere = false;
            _vm.StatuszUzenet = "Talon csere megtortent.";
        }

        private void TalonArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // If swap is allowed, perform it; otherwise show message
            if (_vm.LehetTalonCsere)
            {
                TalonCsere_Click(sender, null);
            }
            else
            {
                _vm.StatuszUzenet = "Talon csere nem megengedett. Csak az els? körben és ha van adu alsókirályod.";
            }
        }

        private async Task UtesKiErtekelese()
        {
            await Task.Delay(2000); // 2 masodperc nezelodes
            
            SnapszerLogic logic = new SnapszerLogic();
            var dummyHivo = new Player("1", 0, 0);
            var dummyValaszolo = new Player("2", 0, 0);

            Player nyertes;

            // Aki eloszor rakott, az e az elony a fuggvenyben bemenetken.
            // P1 = Hivott, P2 = Valaszolt. Szamoltassuk ki.
            if (_vm.EllensegHivottLap != null && _vm.JatekosHivottLap != null)
            {
                bool enHivtam = _vm.EllensegHivottLap != null && _vm.JatekosHivottLap != null && _vm.StatuszUzenet.Contains("valaszol");

                bool enVittem = false;

                if (_vm.StatuszUzenet.Contains("valaszol") || _vm.StatuszUzenet.Contains("valaszolt"))
                {
                   // En hivtam
                   nyertes = logic.GetWinnerOfTrick(_vm.JatekosHivottLap, _vm.EllensegHivottLap, dummyHivo, dummyValaszolo, _vm.AduSzin);
                   enVittem = (nyertes == dummyHivo);
                }
                else
                {
                   // Gep hivott
                   nyertes = logic.GetWinnerOfTrick(_vm.EllensegHivottLap, _vm.JatekosHivottLap, dummyHivo, dummyValaszolo, _vm.AduSzin);
                   enVittem = (nyertes == dummyValaszolo);
                }

                if (enVittem)
                {
                    _vm.StatuszUzenet = "Ezt te vitted! Hivhatsz.";
                    _vm.JatekosPont += _vm.EllensegHivottLap.pont + _vm.JatekosHivottLap.pont;
                    // mark trick won
                    _vm.JatekosNyertUtesek++;
                }
                else
                {
                    _vm.StatuszUzenet = "A Gep vitte az utest! O hiv...";
                    _vm.EllensegPont += _vm.EllensegHivottLap.pont + _vm.JatekosHivottLap.pont;
                    _vm.EllensegNyertUtesek++;
                }

                // One more trick evaluated
                _vm.KiertekeltUtesek++;
                // After first trick, snapszer is no longer allowed
                if (_vm.KiertekeltUtesek > 0)
                {
                    _vm.LehetSnapszer = false;
                }

                // If either side declared snapszer, check success/failure immediately
                if (_vm.PlayerDeclaredSnapszer || _vm.EnemyDeclaredSnapszer)
                {
                    // If player declared and failed
                    if (_vm.PlayerDeclaredSnapszer && !enVittem)
                    {
                        // Award all remaining unassigned points to opponent. Avoid double-counting points already added during tricks.
                        const int TotalPointsInGame = 120;
                        int remaining = TotalPointsInGame - (_vm.JatekosPont + _vm.EllensegPont);
                        if (remaining < 0) remaining = 0;
                        _vm.EllensegPont += remaining;
                        // Clear remaining deck so no further draws occur
                        _vm.Pakli.SumRemainingPointsAndClear();
                         // opponent wins the round
                         _vm.MerkozesNyertEllenseg++;
                         _vm.StatuszUzenet = "Snapszer bukta: az ellenfel vette az utast. Merlegezunk...";
                         // clear table and hands
                         _vm.EllensegHivottLap = null;
                         _vm.JatekosHivottLap = null;
                         _vm.JatekosLapok.Clear();
                         _vm.EllensegLapok.Clear();
                         _vm.PakliLezarasa();
                         EndRound(false);
                         return;
                    }

                    // If enemy declared and failed
                    if (_vm.EnemyDeclaredSnapszer && enVittem)
                    {
                        const int TotalPointsInGame = 120;
                        int remaining = TotalPointsInGame - (_vm.JatekosPont + _vm.EllensegPont);
                        if (remaining < 0) remaining = 0;
                        _vm.JatekosPont += remaining;
                        _vm.Pakli.SumRemainingPointsAndClear();
                         // player wins the round
                         _vm.MerkozesNyertJatekos++;
                         _vm.StatuszUzenet = "A gep bukta a snapszert, te viszed a maradekot.";
                         _vm.EllensegHivottLap = null;
                         _vm.JatekosHivottLap = null;
                         _vm.JatekosLapok.Clear();
                         _vm.EllensegLapok.Clear();
                         _vm.PakliLezarasa();
                         EndRound(true);
                         return;
                    }

                    // If player declared and succeeded on this trick and there are no more cards/hands, award remaining
                    if (_vm.PlayerDeclaredSnapszer && enVittem && _vm.JatekosLapok.Count == 0 && _vm.EllensegLapok.Count == 0)
                    {
                        const int TotalPointsInGame = 120;
                        int remaining = TotalPointsInGame - (_vm.JatekosPont + _vm.EllensegPont);
                        if (remaining < 0) remaining = 0;
                        _vm.JatekosPont += remaining;
                        _vm.MerkozesNyertJatekos++;
                         _vm.StatuszUzenet = "Snapszer siker! Megkaptad a maradek pontokat.";
                         _vm.PakliLezarasa();
                         EndRound(true);
                         return;
                    }

                    // If enemy declared and succeeded similarly
                    if (_vm.EnemyDeclaredSnapszer && !enVittem && _vm.JatekosLapok.Count == 0 && _vm.EllensegLapok.Count == 0)
                    {
                        const int TotalPointsInGame = 120;
                        int remaining = TotalPointsInGame - (_vm.JatekosPont + _vm.EllensegPont);
                        if (remaining < 0) remaining = 0;
                        _vm.EllensegPont += remaining;
                        _vm.MerkozesNyertEllenseg++;
                         _vm.StatuszUzenet = "A gep sikeresen snapszerelt, megkapta a maradek pontokat.";
                         _vm.Pakli.SumRemainingPointsAndClear();
                         _vm.PakliLezarasa();
                         EndRound(false);
                         return;
                    }
                }

                // Takaritas az asztalrol
                _vm.EllensegHivottLap = null;
                _vm.JatekosHivottLap = null;

                // Huzas
                var pT = _vm.Pakli.Huzas();
                if (pT != null)
                {
                    _vm.NotifyCardDrawn(pT);
                    if (enVittem)
                    {
                        _vm.JatekosLapok.Add(pT);
                        var eT = _vm.Pakli.Huzas();
                        if (eT != null) { _vm.EllensegLapok.Add(eT); _vm.NotifyCardDrawn(eT); }
                    }
                    else
                    {
                        _vm.EllensegLapok.Add(pT);
                        var eT = _vm.Pakli.Huzas();
                        if (eT != null) { _vm.JatekosLapok.Add(eT); _vm.NotifyCardDrawn(eT); }
                    }
                }
                else
                {
                    _vm.PakliLezarasa(); // Nincs tobb húzni valo
                }
                
                GyozveEllenorzes(enVittem);

                if (_jatekFolyamatban)
                {
                    if (enVittem)
                    {
                       _vm.EnKovetkezem = true; 
                       _vm.FrissitBemondasLehetoseg();
                    }
                    else
                    {
                       await GepHiv();
                    }
                }
            }
         }

         private void GyozveEllenorzes(bool isUtolsoBekerult = false)
         {
             if (!_jatekFolyamatban) return; // mar vege

             // Jatek Vege Ellenorzes
             if (_vm.JatekosPont >= 66 || _vm.EllensegPont >= 66 || (_vm.JatekosLapok.Count == 0 && _vm.EllensegLapok.Count == 0))
             {
                 _jatekFolyamatban = false;
                 _vm.EnKovetkezem = false;

                 // Utolso utes szabaly extra pont - ha a pakli ures volt és most fogyott el, az kap +10-et
                 if (_vm.JatekosLapok.Count == 0 && _vm.EllensegLapok.Count == 0 && _vm.JatekosPont < 66 && _vm.EllensegPont < 66)
                 {
                     if(isUtolsoBekerult) _vm.JatekosPont += 10;
                     else _vm.EllensegPont += 10;
                 }

                 // Silent snapszer (csendes snapszer): if nobody declared snapszer but one player won all tricks (opponent won 0), award extra bonus
                 const int SilentSnapszerBonus = 30; // configurable bonus for silent snapszer
                 if (!_vm.PlayerDeclaredSnapszer && !_vm.EnemyDeclaredSnapszer)
                 {
                     if (_vm.EllensegNyertUtesek == 0 && _vm.KiertekeltUtesek > 0 && _vm.JatekosNyertUtesek == _vm.KiertekeltUtesek)
                     {
                         _vm.JatekosPont += SilentSnapszerBonus;
                         _vm.StatuszUzenet = "Csendes snapszer: plusz pont a vegelsozamolasnal.";
                     }
                     else if (_vm.JatekosNyertUtesek == 0 && _vm.KiertekeltUtesek > 0 && _vm.EllensegNyertUtesek == _vm.KiertekeltUtesek)
                     {
                         _vm.EllensegPont += SilentSnapszerBonus;
                         _vm.StatuszUzenet = "Gep csendes snapszert ert el; extra pontot kapott.";
                     }
                 }

                 if (_vm.JatekosPont >= 66 || _vm.JatekosPont > _vm.EllensegPont)
                 {
                     _vm.StatuszUzenet = $"NYERTEL! Eredmeny: Te {_vm.JatekosPont} - {_vm.EllensegPont} Gep.";
                     MessageBox.Show($"Gy?ztél!\n\nPontjaid: {_vm.JatekosPont}\nGép pontjai: {_vm.EllensegPont}", "Játék Vége", MessageBoxButton.OK, MessageBoxImage.Information);
                 }
                 else
                 {
                     _vm.StatuszUzenet = $"VESZTETTEL! Eredmeny: Te {_vm.JatekosPont} - {_vm.EllensegPont} Gep.";
                     MessageBox.Show($"Vesztettél!\n\nPontjaid: {_vm.JatekosPont}\nGép pontjai: {_vm.EllensegPont}", "Játék Vége", MessageBoxButton.OK, MessageBoxImage.Error);
                 }

                 // Vissza a fomenube
                 this.Close();
             }
         }

         private void EndRound(bool playerWonRound)
         {
            _jatekFolyamatban = false;
            _vm.EnKovetkezem = false;

            const int MatchWinThreshold = 7;
            if (_vm.MerkozesNyertJatekos >= MatchWinThreshold || _vm.MerkozesNyertEllenseg >= MatchWinThreshold)
            {
                string winnerMsg = _vm.MerkozesNyertJatekos >= MatchWinThreshold ? "Te nyerted a meccset!" : "A gép nyerte a meccset.";
                MessageBox.Show($"{winnerMsg}\n\nEredmény: {_vm.MerkozesNyertJatekos} - {_vm.MerkozesNyertEllenseg}", "Meccs vége", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                return;
            }

            // Prepare next round (keep match counters)
            _vm.ResetRoundState();
            _vm.Pakli = new KartyaCsomag();
            _vm.Pakli.PakliKeveres();

            // Deal 5 cards each
            for (int i = 0; i < 5; i++)
            {
                var p = _vm.Pakli.Huzas(); if (p != null) { _vm.JatekosLapok.Add(p); _vm.NotifyCardDrawn(p); }
                var e = _vm.Pakli.Huzas(); if (e != null) { _vm.EllensegLapok.Add(e); _vm.NotifyCardDrawn(e); }
            }

            // Update talon
            _vm.FelforditottKartya = _vm.Pakli.PeekLast();

            // Randomly choose who starts next round
            bool playerStarts = new Random().Next(2) == 0;
            if (playerStarts)
            {
                _vm.AduValasztasFolyamatban = true;
                _vm.StatuszUzenet = "Válassz adut";
            }
            else
            {
                _vm.AduValasztasFolyamatban = false;
                _vm.AduSzin = _vm.EllensegLapok.GroupBy(l => l.szin).OrderByDescending(g => g.Count()).First().Key;
                _vm.StatuszUzenet = "A gép választott adut";
                _vm.EnKovetkezem = true;
            }

            _jatekFolyamatban = true;
         }

         private async Task MutassBemondasAnimaciot(bool is40, bool toPlayer = true)
         {
            // Set message and suspend interaction for a short time to show animation
            if (is40)
            {
                BemondasText.Text = "BEMONDAS: 40";
            }
            else
            {
                BemondasText.Text = "BEMONDAS: 20";
            }

            BemondasSzinText.Text = _vm.LastBemondottSzin?.ToString() ?? string.Empty;

            var sbShow = (Storyboard)FindResource("BemondasAppear");
            var sbHide = (Storyboard)FindResource("BemondasDisappear");

            BemondasPanel.BeginStoryboard(sbShow);
            BemondasPanel.BeginStoryboard(sbHide);

            // Also animate a card moving from PakliImage to center table by temporarily creating a flying Image
            if (_vm.PakliVisibility == Visibility.Visible)
            {
                var img = new Image()
                {
                    Source = PakliImage.Source,
                    Width = PakliImage.ActualWidth,
                    Height = PakliImage.ActualHeight,
                    RenderTransform = new TranslateTransform()
                };

                var root = (Grid)this.Content;

                // Ensure we have an overlay canvas
                Canvas overlay = null;
                foreach (var child in root.Children)
                {
                    if (child is Canvas c && c.Name == "_overlayCanvas") { overlay = c; break; }
                }
                if (overlay == null)
                {
                    overlay = new Canvas() { IsHitTestVisible = false, Name = "_overlayCanvas" };
                    root.Children.Add(overlay);
                }

                overlay.Children.Add(img);

                // Position image at PakliImage location (relative to window)
                var pakliPos = PakliImage.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
                Canvas.SetLeft(img, pakliPos.X);
                Canvas.SetTop(img, pakliPos.Y);

                // Animate to center (table area center)
                var center = new System.Windows.Point((this.ActualWidth / 2) - img.Width / 2, (this.ActualHeight / 2) - img.Height / 2);

                var leftAnim = new DoubleAnimation(pakliPos.X, center.X, new Duration(System.TimeSpan.FromMilliseconds(900)));
                var topAnim = new DoubleAnimation(pakliPos.Y, center.Y, new Duration(System.TimeSpan.FromMilliseconds(900)));

                leftAnim.FillBehavior = FillBehavior.Stop;
                topAnim.FillBehavior = FillBehavior.Stop;

                leftAnim.Completed += (s, e) =>
                {
                    overlay.Children.Remove(img);

                    // Give extra card to the announcer
                    var extra = _vm.Pakli.Huzas();
                    if (extra != null)
                    {
                        _vm.NotifyCardDrawn(extra);
                        if (toPlayer)
                            _vm.JatekosLapok.Add(extra);
                        else
                            _vm.EllensegLapok.Add(extra);
                    }
                    else
                    {
                        _vm.PakliLezarasa();
                    }
                };

                // Apply animations via Storyboard targeting Canvas.Left/Top
                var sb = new Storyboard();
                sb.Children.Add(leftAnim);
                sb.Children.Add(topAnim);
                Storyboard.SetTarget(leftAnim, img);
                Storyboard.SetTargetProperty(leftAnim, new PropertyPath("(Canvas.Left)"));
                Storyboard.SetTarget(topAnim, img);
                Storyboard.SetTargetProperty(topAnim, new PropertyPath("(Canvas.Top)"));

                sb.Begin();

                await Task.Delay(1200);
             }
         }
    }
}