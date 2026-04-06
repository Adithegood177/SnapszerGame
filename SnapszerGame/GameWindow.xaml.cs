using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SnapszerGame.game;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Media;

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

        // Kartyara kattintas (Jatekos lejatssza a lapjat)
        private async void ItemsControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_jatekFolyamatban || !_vm.EnKovetkezem) return;

            var felement = e.OriginalSource as FrameworkElement;
            if (felement == null) return;
            
            var card = felement.DataContext as Card;
            if (card != null)
            {
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
            var hivottLap = _gep.GepHiv(_vm.EllensegLapok, _vm.AduSzin);
            _vm.EllensegLapok.Remove(hivottLap);
            _vm.EllensegHivottLap = hivottLap;
            
            _vm.StatuszUzenet = "A gep hivott, te jossz!";
            _vm.EnKovetkezem = true;
            _vm.FrissitBemondasLehetoseg();
        }

        private async Task UtesKiErtekelese()
        {
            await Task.Delay(2000); // 2 masodperc nezelodes
            
            SnapszerLogic logic = new SnapszerLogic();
            var dummyHivo = new Player("1", 0, 0);
            var dummyValaszolo = new Player("2", 0, 0);

            Player nyertes;

            // Aki eloszor rakott, az e az elony a fuggvenyben bemenetkent.
            // P1 = Hivott, P2 = Valaszolt. Szamoltassuk ki.
            if (_vm.EllensegHivottLap != null && _vm.JatekosHivottLap != null)
            {
                // Ha a gep hivott (mert az oje mar lent volt mikor mi raktuk, vagy mert o hívott miutan lejatszottuk) 
                // Varj, az UtesKiertekelesben tenyleges gombkattintas alapjan le kell fogadni oket.
                // Hasznaljunk sajat egyszeru format a pontossag miatt
                Card elsoLap = _vm.EnKovetkezem ? _vm.JatekosHivottLap : _vm.EllensegHivottLap; // Hibás dedukcio. A gepHiv utan false, mi lerakjuk es false marad.

                // Hogy megtudjuk ki hivott: Ha en hivtam, akkor valaszolt a Gep miutan _EllensegHivottLap erteket kapott a 'KartyaraKattint'-ban
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
                }
                else
                {
                    _vm.StatuszUzenet = "A Gep vitte az utest! O hiv...";
                    _vm.EllensegPont += _vm.EllensegHivottLap.pont + _vm.JatekosHivottLap.pont;
                }

                await Task.Delay(1500); // Üzenet mutatas

                // Takaritas az asztalrol
                _vm.EllensegHivottLap = null;
                _vm.JatekosHivottLap = null;

                // Huzas
                var pT = _vm.Pakli.Huzas();
                if (pT != null)
                {
                    if (enVittem)
                    {
                        _vm.JatekosLapok.Add(pT);
                        var eT = _vm.Pakli.Huzas();
                        if (eT != null) _vm.EllensegLapok.Add(eT);
                    }
                    else
                    {
                        _vm.EllensegLapok.Add(pT);
                        var eT = _vm.Pakli.Huzas();
                        if (eT != null) _vm.JatekosLapok.Add(eT);
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

        private async void MutassBemondasAnimaciot(bool is40)
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

                var animX = new DoubleAnimation(pakliPos.X, center.X, new Duration(System.TimeSpan.FromMilliseconds(500)));
                var animY = new DoubleAnimation(pakliPos.Y, center.Y, new Duration(System.TimeSpan.FromMilliseconds(500)));

                // Use attached properties animation
                var daX = new DoubleAnimationUsingKeyFrames();
                daX.KeyFrames.Add(new LinearDoubleKeyFrame(pakliPos.X, KeyTime.FromTimeSpan(System.TimeSpan.Zero)));
                daX.KeyFrames.Add(new LinearDoubleKeyFrame(center.X, KeyTime.FromTimeSpan(System.TimeSpan.FromMilliseconds(500))));

                var daY = new DoubleAnimationUsingKeyFrames();
                daY.KeyFrames.Add(new LinearDoubleKeyFrame(pakliPos.Y, KeyTime.FromTimeSpan(System.TimeSpan.Zero)));
                daY.KeyFrames.Add(new LinearDoubleKeyFrame(center.Y, KeyTime.FromTimeSpan(System.TimeSpan.FromMilliseconds(500))));

                var leftAnim = new DoubleAnimation(pakliPos.X, center.X, new Duration(System.TimeSpan.FromMilliseconds(500)));
                var topAnim = new DoubleAnimation(pakliPos.Y, center.Y, new Duration(System.TimeSpan.FromMilliseconds(500)));

                leftAnim.FillBehavior = FillBehavior.Stop;
                topAnim.FillBehavior = FillBehavior.Stop;

                leftAnim.Completed += (s, e) =>
                {
                    overlay.Children.Remove(img);

                    // Give extra card to player
                    var extra = _vm.Pakli.Huzas();
                    if (extra != null)
                    {
                        _vm.JatekosLapok.Add(extra);
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

                await Task.Delay(800);
            }
        }
    }
}