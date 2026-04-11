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

        // Osztás animálása: felváltva kapjuk a lapokat, majd ha a játékos kezd, kérjük be az adut
        public async Task StartDealing(bool playerStarts)
        {
            // Biztonsági törlés, nehogy maradjon valami a kézben korábbról
            _vm.JatekosLapok.Clear();
            _vm.EllensegLapok.Clear();

            // Szépen felváltva adogatjuk a lapokat
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

            // Kiosztottunk mindent, jöhet a talon és az adu beállítása
            // Lekérjük az utolsó lapot (a talont), hogy kint legyen a képerny?n
            _vm.FelforditottKartya = _vm.Pakli.PeekLast();
            if (playerStarts)
            {
                _vm.AduValasztasFolyamatban = true;
                _vm.StatuszUzenet = "Válassz adut";

                // Feldobjuk az aduválasztó ablakot a játékablak f?lé
                var aduWindow = new AduValasztoWindow();
                aduWindow.Owner = this;
                var result = aduWindow.ShowDialog();
                if (result == true)
                {
                    _vm.JatekosAdutValaszt(aduWindow.ValasztottAdu);
                    _jatekFolyamatban = true;
                    // Most a játékos jön
                    _vm.EnKovetkezem = true;
                    _vm.FrissitBemondasLehetoseg();
                    _vm.StatuszUzenet = "Te jössz hívással!";
                }
                else
                {
                    // Ha véletlenül kinyomná (bár elvileg nem nagyon lehet)
                    _vm.AduValasztasFolyamatban = false;
                    _vm.StatuszUzenet = string.Empty;
                }
            }
            else
            {
                _vm.AduValasztasFolyamatban = false;
                _vm.AduSzin = _vm.EllensegLapok.GroupBy(l => l.szin).OrderByDescending(g => g.Count()).First().Key;
                _vm.StatuszUzenet = $"A gép választott adut: {_vm.AduSzin}. Híváshoz készül...";
                _vm.EnKovetkezem = false;
                _jatekFolyamatban = true;
                _vm.FrissitBemondasLehetoseg();

                // A gép esetleg mondhat 20-at vagy 40-et rögtön az osztás után
                var bem = _gep.TryBemond(_vm.EllensegLapok, _vm.AduSzin, _vm.BemondottSzinek);
                if (bem.did)
                {
                    _vm.EnemyBemond(bem.szin, bem.is40);
                    await MutassBemondasAnimaciot(bem.is40, false);
                }

                // Vagy akár egyb?l bemondhatja a snapszert is
                if (_gep.TrySnapszer(_vm.EllensegLapok))
                {
                    _vm.DeclareSnapszer(false);
                    _vm.StatuszUzenet = "A gép snapszert jelentett be!";
                }

                await Task.Delay(1500);
                await GepHiv();
            }
        }

        // --- ÚJ GOMB ---
        private void SzabalyzatGomb_Click(object sender, RoutedEventArgs e)
        {
            var win = new SzabalyzatWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        // 20 és 40 gombok eventjei
        private void Bemond20_Click(object sender, RoutedEventArgs e)
        {
            _vm.Bemond(false);
            GyozveEllenorzes(); // Lecsekkoljuk egyb?l, mert a pontokkal nyerhet is
        }

        private void Bemond40_Click(object sender, RoutedEventArgs e)
        {
            _vm.Bemond(true);
            GyozveEllenorzes(); // Ugyanúgy, le kell csekkolni, hátha megvan a 66 pont
        }

        // Snapszer gomb, mikor a játékos vállalja, hogy mindent visz
        private void Snapszer_Click(object sender, RoutedEventArgs e)
        {
            if (!_jatekFolyamatban || !_vm.LehetSnapszer) return;

            // Bejegyezzük, hogy a játékos snapszert mondott. Az ellen?rzés kés?bb folyamatos.
            _vm.DeclareSnapszer(true);
            _vm.StatuszUzenet = "SNAPSZER bejelentve! Ezt el kell vinni, nem veszthetsz egy ütést sem.";
        }

        private void Nyertem_Click(object sender, RoutedEventArgs e)
        {
            if (!_jatekFolyamatban || !_vm.LehetNyertem) return;

            _vm.StatuszUzenet = "NYERTEM bemondva: 5 lapból megvan a 66 pont!";
            _vm.MerkozesNyertJatekos += 3; // Ezzel kemény 3 pontot kapunk a meccsen
            
            // Ha közben a gép már összeszedett 33 pontot valahogy (mondjuk egy 40-es bemondással id?közben?), akkor ? is kap 1 pontot
            if (_vm.EllensegPont >= 33)
            {
                _vm.MerkozesNyertEllenseg += 1;
            }

            ShowRoundResultWithPrompt("Gratulálok, megvan a 66 pont!", $"Kaptál 3 pontot. (Gép: {(_vm.EllensegPont >= 33 ? "+1" : "0")} pont)", true);
        }

        // Segédfüggvény: összegzi a kézben maradt lapok és a talon pontjait a kövégi elszámoláshoz
        private int SumAndDrainRemainingPoints()
        {
            int remainingPoints = 0;
            if (_vm.JatekosHivottLap != null) remainingPoints += _vm.JatekosHivottLap.pont;
            if (_vm.EllensegHivottLap != null) remainingPoints += _vm.EllensegHivottLap.pont;

            remainingPoints += _vm.JatekosLapok.Sum(c => c.pont);
            remainingPoints += _vm.EllensegLapok.Sum(c => c.pont);

            // Kifogyasztjuk a paklit, beleszámolva a maradék pontokat
            while (true)
            {
                var c = _vm.Pakli.Huzas();
                if (c == null) break;
                _vm.NotifyCardDrawn(c);
                remainingPoints += c.pont;
            }

            return remainingPoints;
        }

        // Ide jön be, mikor a játékos rákattint egy lapjára, hogy leteszi
        private async void ItemsControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_jatekFolyamatban || !_vm.EnKovetkezem) return;

            var felement = e.OriginalSource as FrameworkElement;
            if (felement == null) return;
            
            var card = felement.DataContext as Card;
            if (card != null)
            {
                // Még a lap lerakása el?tt is mondhattunk volna snapszert a gombbal, de ha ideértünk, lejátsszuk a lapot
                await JatekosHiv(card);
            }
        }

        private async Task JatekosHiv(Card lerakandoKartya)
        {
            SnapszerLogic valLogic = new SnapszerLogic();

            // Ha a gép már hívott és mi válaszolunk, ellen?rizni kell a szabályokat (zárt paklinál színre szín)
            if (_vm.EllensegHivottLap != null)
            {
                if (!valLogic.SzabalyosKartyaRakas(lerakandoKartya, _vm.EllensegHivottLap, _vm.AduSzin, _vm.PakliLezarva, _vm.JatekosLapok))
                {
                    _vm.StatuszUzenet = "Ezt a lapot szabályellenes lerakni!";
                    return;
                }
            }

            // Ha átment az ellen?rzésen, kikerül a kézb?l és lekerül az asztalra
            _vm.JatekosLapok.Remove(lerakandoKartya);
            _vm.JatekosHivottLap = lerakandoKartya;

            _vm.EnKovetkezem = false; // Átmenetileg letiltjuk a kattintgatást nullára

            if (_vm.EllensegHivottLap == null)
            {
                // Els?nek tettük le, most a gépnek kell reagálnia
                _vm.StatuszUzenet = "Te hívtál. Gép válaszol...";
                await Task.Delay(1000);
                
                var gepValasz = _gep.GepValaszol(lerakandoKartya, _vm.AduSzin, _vm.PakliLezarva, _vm.EllensegLapok);
                _vm.EllensegLapok.Remove(gepValasz);
                _vm.EllensegHivottLap = gepValasz;
            }

            // Most már mindketten leraktak valamit, nézzük meg ki viszi
            await UtesKiErtekelese();
        }

        private async Task GepHiv()
        {
            // Miel?tt lépne, megnézi tud-e 20/40-et mondani
            var tryBem = _gep.TryBemond(_vm.EllensegLapok, _vm.AduSzin, _vm.BemondottSzinek);
            if (tryBem.did)
            {
                _vm.EnemyBemond(tryBem.szin, tryBem.is40);
                await MutassBemondasAnimaciot(tryBem.is40, false);
            }

            var hivottLap = _gep.GepHiv(_vm.EllensegLapok, _vm.AduSzin);
            _vm.EllensegLapok.Remove(hivottLap);
            _vm.EllensegHivottLap = hivottLap;
            
            _vm.StatuszUzenet = "A gép hívott, te jössz!";
            _vm.EnKovetkezem = true;
            _vm.FrissitBemondasLehetoseg();
        }

        private void TalonCsere_Click(object sender, RoutedEventArgs e)
        {
            // A ViewModel dönti el, engedi-e a cserét
            if (!_vm.LehetTalonCsere) return;

            // Kell a kezünkbe az adu alsó, anélkül nem megy
            var playerAlsokiraly = _vm.JatekosLapok.FirstOrDefault(c => c.szin == _vm.AduSzin && c.ertek == Ertek.Alsokiraly);
            var talon = _vm.Pakli.PeekLast();
            if (playerAlsokiraly == null || talon == null) return;

            // Kicseréljük az utolsó lapra
            var oldLast = _vm.Pakli.ReplaceLast(playerAlsokiraly);

            // Kivesszük az alsót a kezünkb?l, s a helyére bemegy a felhúzott lap
            _vm.JatekosLapok.Remove(playerAlsokiraly);
            _vm.JatekosLapok.Add(oldLast);

            // Szólunk a UI-nak, hogy alul a talon már másik lap lett
            _vm.FelforditottKartya = oldLast;

            // Megcsináltuk a cserét, többet nem lehet ezel
            _vm.LehetTalonCsere = false;
            _vm.StatuszUzenet = "Talon csere megtörtént.";
        }

        private void TalonArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Ha ráklikkelt a talonra és lehet cserélni, meghívjuk
            if (_vm.LehetTalonCsere)
            {
                TalonCsere_Click(sender, null);
            }
            else
            {
                _vm.StatuszUzenet = "Talon csere nem engedélyezett.";
            }
        }

        private async Task UtesKiErtekelese()
        {
            await Task.Delay(2000); // 2 másodpercet várunk, hogy látszódjanak a lapok miel?tt elt?nnek
            
            SnapszerLogic logic = new SnapszerLogic();
            var dummyHivo = new Player("1", 0, 0);
            var dummyValaszolo = new Player("2", 0, 0);

            Player nyertes;

            // Akkor értékelünk, ha már tényleg mind a két lap lenn van
            if (_vm.EllensegHivottLap != null && _vm.JatekosHivottLap != null)
            {
                bool enHivtam = _vm.EllensegHivottLap != null && _vm.JatekosHivottLap != null && _vm.StatuszUzenet.Contains("valaszol");

                bool enVittem = false;

                if (_vm.StatuszUzenet.Contains("valaszol") || _vm.StatuszUzenet.Contains("valaszolt"))
                {
                   // Mi hívtunk
                   nyertes = logic.GetWinnerOfTrick(_vm.JatekosHivottLap, _vm.EllensegHivottLap, dummyHivo, dummyValaszolo, _vm.AduSzin);
                   enVittem = (nyertes == dummyHivo);
                }
                else
                {
                   // Gép hívott
                   nyertes = logic.GetWinnerOfTrick(_vm.EllensegHivottLap, _vm.JatekosHivottLap, dummyHivo, dummyValaszolo, _vm.AduSzin);
                   enVittem = (nyertes == dummyValaszolo);
                }

                if (enVittem)
                {
                    _vm.StatuszUzenet = "Ezt te vitted! Hívhatsz.";
                    _vm.JatekosPont += _vm.EllensegHivottLap.pont + _vm.JatekosHivottLap.pont;
                    // Bejegyezzük, hogy hoztunk egy ütést
                    _vm.JatekosNyertUtesek++;
                }
                else
                {
                    _vm.StatuszUzenet = "A Gép vitte az ütést! ? hív...";
                    _vm.EllensegPont += _vm.EllensegHivottLap.pont + _vm.JatekosHivottLap.pont;
                    _vm.EllensegNyertUtesek++;
                }

                // Számoljuk, hányadik ütés volt (els? után már nincs snapszer és nyertem meg taloncsere)
                _vm.KiertekeltUtesek++;
                if (_vm.KiertekeltUtesek > 0)
                {
                    _vm.LehetSnapszer = false;
                }

                // Ha valaki bemondta a snapszert, nézzük meg bebukta-e
                if (_vm.PlayerDeclaredSnapszer || _vm.EnemyDeclaredSnapszer)
                {
                    // Ha mi mondtuk a snapszert, de nem visszük az ütést -> azonnal bukta
                    if (_vm.PlayerDeclaredSnapszer && !enVittem)
                    {
                        const int TotalPointsInGame = 120;
                        int remaining = TotalPointsInGame - (_vm.JatekosPont + _vm.EllensegPont);
                        if (remaining < 0) remaining = 0;
                        _vm.EllensegPont += remaining;
                        _vm.Pakli.SumRemainingPointsAndClear();
                        
                        _vm.MerkozesNyertEllenseg += 7; // Bukott snapszer vastagon 7 pontot ad ellennek
                        
                        _vm.EllensegHivottLap = null;
                        _vm.JatekosHivottLap = null;
                        _vm.JatekosLapok.Clear();
                        _vm.EllensegLapok.Clear();
                        _vm.PakliLezarasa();

                        ShowRoundResultWithPrompt("Elbuktad a Snapszert!", "A gép kap 7 pontot.", false);
                        return;
                    }

                    // A gép mondott snapszert, és mi hoztuk az ütést -> bed?lt a gépi snapszer, hurrá!
                    if (_vm.EnemyDeclaredSnapszer && enVittem)
                    {
                        const int TotalPointsInGame = 120;
                        int remaining = TotalPointsInGame - (_vm.JatekosPont + _vm.EllensegPont);
                        if (remaining < 0) remaining = 0;
                        _vm.JatekosPont += remaining;
                        _vm.Pakli.SumRemainingPointsAndClear();
                        
                        _vm.MerkozesNyertJatekos += 7; // Hopp, egy ingyen 7-es
                        
                        _vm.EllensegHivottLap = null;
                        _vm.JatekosHivottLap = null;
                        _vm.JatekosLapok.Clear();
                        _vm.EllensegLapok.Clear();
                        _vm.PakliLezarasa();

                        ShowRoundResultWithPrompt("A gép bukta a Snapszert!", "Kaptál 7 pontot.", true);
                        return;
                    }

                    // Ha a mi snapszerünk hibátlan és elfogytak a lapjaink (tehát minden kört elvittünk)
                    if (_vm.PlayerDeclaredSnapszer && enVittem && _vm.JatekosLapok.Count == 0 && _vm.EllensegLapok.Count == 0)
                    {
                        const int TotalPointsInGame = 120;
                        int remaining = TotalPointsInGame - (_vm.JatekosPont + _vm.EllensegPont);
                        if (remaining < 0) remaining = 0;
                        _vm.JatekosPont += remaining;
                        
                        _vm.MerkozesNyertJatekos += 7; // Sikeres 7 pont
                        if (_vm.EllensegPont >= 33) _vm.MerkozesNyertEllenseg += 1;
                        
                        _vm.PakliLezarasa();

                        ShowRoundResultWithPrompt("Snapszer Siker!", $"Kaptál 7 pontot. (Gép: {(_vm.EllensegPont >= 33 ? "+1" : "0")} pont)", true);
                        return;
                    }

                    // Ugyanez a gépre nézve
                    if (_vm.EnemyDeclaredSnapszer && !enVittem && _vm.JatekosLapok.Count == 0 && _vm.EllensegLapok.Count == 0)
                    {
                        const int TotalPointsInGame = 120;
                        int remaining = TotalPointsInGame - (_vm.JatekosPont + _vm.EllensegPont);
                        if (remaining < 0) remaining = 0;
                        _vm.EllensegPont += remaining;
                        
                        _vm.MerkozesNyertEllenseg += 7; // Gép 7 pontos gy?zelme
                        if (_vm.JatekosPont >= 33) _vm.MerkozesNyertJatekos += 1;

                        _vm.Pakli.SumRemainingPointsAndClear();
                        _vm.PakliLezarasa();

                        ShowRoundResultWithPrompt("Gép Snapszer Sikerült", $"Gép kapott 7 pontot. (Te: {(_vm.JatekosPont >= 33 ? "+1" : "0")} pont)", false);
                        return;
                    }
                }

                // Takarítás az asztalról az értékelés után
                _vm.EllensegHivottLap = null;
                _vm.JatekosHivottLap = null;

                // Mindketten húzunk egyet, aki vitte az ütést az húz el?ször
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
                    _vm.PakliLezarasa(); // Nincs több lap, zár a pakli, mától kötelez? színt rakni
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
             if (!_jatekFolyamatban) return; // Ha már vége, ne spammeljünk

             // Checkeljük hogy valakinek megvan-e a 66 pont, vagy elfogytak-e a lapok
             if (_vm.JatekosPont >= 66 || _vm.EllensegPont >= 66 || (_vm.JatekosLapok.Count == 0 && _vm.EllensegLapok.Count == 0))
             {
                 _jatekFolyamatban = false;
                 _vm.EnKovetkezem = false;

                 // Aki az utolsó lapot leviszi az 10 bónuszt kap (ha nem értük még el a 66-ot különben is)
                 if (_vm.JatekosLapok.Count == 0 && _vm.EllensegLapok.Count == 0 && _vm.JatekosPont < 66 && _vm.EllensegPont < 66)
                 {
                     if(isUtolsoBekerult) _vm.JatekosPont += 10;
                     else _vm.EllensegPont += 10;
                 }

                 // Csendes snapszer: ha úgy vitt el egy játékos mindent, hogy nem mondott rá snapszert
                 const int SilentSnapszerBonus = 30; // Erre a plusz bünti pontra érdemes odafigyelni
                 if (!_vm.PlayerDeclaredSnapszer && !_vm.EnemyDeclaredSnapszer)
                 {
                     if (_vm.EllensegNyertUtesek == 0 && _vm.KiertekeltUtesek > 0 && _vm.JatekosNyertUtesek == _vm.KiertekeltUtesek)
                     {
                         _vm.JatekosPont += SilentSnapszerBonus;
                         _vm.StatuszUzenet = "Csendes snapszer: plusz pont a végelszámolásnál.";
                     }
                     else if (_vm.JatekosNyertUtesek == 0 && _vm.KiertekeltUtesek > 0 && _vm.EllensegNyertUtesek == _vm.KiertekeltUtesek)
                     {
                         _vm.EllensegPont += SilentSnapszerBonus;
                         _vm.StatuszUzenet = "Gép csendes snapszert ért el; extra pontot kapott.";
                     }
                 }

                 if (_vm.JatekosPont >= 66 || _vm.JatekosPont > _vm.EllensegPont)
                 {
                     _vm.StatuszUzenet = $"NYERTEL! Eredmény: Te {_vm.JatekosPont} - {_vm.EllensegPont} Gép.";
                 }
                 else
                 {
                     _vm.StatuszUzenet = $"VESZTETTÉL! Eredmény: Te {_vm.JatekosPont} - {_vm.EllensegPont} Gép.";
                 }

                 // Ideje osztani a meccspontokat
                 if (_vm.JatekosPont >= 66 || _vm.JatekosPont > _vm.EllensegPont)
                 {
                     _vm.MerkozesNyertJatekos += 2; // Sima gy?zelem mindig 2 pont
                     if (_vm.EllensegPont >= 33) _vm.MerkozesNyertEllenseg += 1;

                     ShowRoundResultWithPrompt("Kör megnyerve!", $"Kaptál 2 pontot. (Gép: {(_vm.EllensegPont >= 33 ? "+1" : "0")} pont)", true);
                 }
                 else
                 {
                     _vm.MerkozesNyertEllenseg += 2;
                     if (_vm.JatekosPont >= 33) _vm.MerkozesNyertJatekos += 1;

                     ShowRoundResultWithPrompt("Kört elvesztetted!", $"A gép kapott 2 pontot. (Te: {(_vm.JatekosPont >= 33 ? "+1" : "0")} pont)", false);
                 }
             }
         }

         private void ShowRoundResultWithPrompt(string title, string subtitle, bool playerWon)
         {
             var result = MessageBox.Show($"{title}\n{subtitle}\n\nSzeretnél új kört kezdeni? (Nem = Kilépés)", 
                                          "Kör Vége", 
                                          MessageBoxButton.YesNo, 
                                          MessageBoxImage.Question);
             
             if (result == MessageBoxResult.Yes)
             {
                 EndRound(playerWon);
             }
             else
             {
                 this.Close();
             }
         }

         private async void EndRound(bool playerWonRound)
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

            // Letisztítjuk a placcot a kövibe
            _vm.ResetRoundState();
            _vm.Pakli = new KartyaCsomag();
            _vm.Pakli.PakliKeveres();

            // Ujra kirakunk mindenkit
            for (int i = 0; i < 5; i++)
            {
                var p = _vm.Pakli.Huzas(); if (p != null) { _vm.JatekosLapok.Add(p); _vm.NotifyCardDrawn(p); }
                var e = _vm.Pakli.Huzas(); if (e != null) { _vm.EllensegLapok.Add(e); _vm.NotifyCardDrawn(e); }
            }

            // Talon fixálás
            _vm.FelforditottKartya = _vm.Pakli.PeekLast();

            // Véletlenszer?en d?l el, ki kezdi most
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
            // Kiírjuk a szöveget és leállunk picinyt, hogy a user elolvashassa
            if (is40)
            {
                BemondasText.Text = "BEMONDÁS: 40";
            }
            else
            {
                BemondasText.Text = "BEMONDÁS: 20";
            }

            BemondasSzinText.Text = _vm.LastBemondottSzin?.ToString() ?? string.Empty;

            var sbShow = (Storyboard)FindResource("BemonasAppear");
            var sbHide = (Storyboard)FindResource("BemonasDisappear");

            BemondasPanel.BeginStoryboard(sbShow);
            BemondasPanel.BeginStoryboard(sbHide);

            // Csinálunk egy álkártyát, ami átrepül a pakliból az asztalra
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

                // Rákerül egy invisible overlay
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

                // Indítunk rajta egy mozgás animet középre
                var pakliPos = PakliImage.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
                Canvas.SetLeft(img, pakliPos.X);
                Canvas.SetTop(img, pakliPos.Y);

                var center = new System.Windows.Point((this.ActualWidth / 2) - img.Width / 2, (this.ActualHeight / 2) - img.Height / 2);

                var leftAnim = new DoubleAnimation(pakliPos.X, center.X, new Duration(System.TimeSpan.FromMilliseconds(900)));
                var topAnim = new DoubleAnimation(pakliPos.Y, center.Y, new Duration(System.TimeSpan.FromMilliseconds(900)));

                leftAnim.FillBehavior = FillBehavior.Stop;
                topAnim.FillBehavior = FillBehavior.Stop;

                leftAnim.Completed += (s, e) =>
                {
                    overlay.Children.Remove(img);

                    // A megkapott plusz lap bekerül a játszó kezébe
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

         private void ShowRoundResult(string title, string subtitle)
         {
             RoundResultTitle.Text = title;
             RoundResultSubtitle.Text = subtitle;
             RoundResultPanel.Visibility = Visibility.Visible;

             var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
             var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5)) { BeginTime = TimeSpan.FromSeconds(3) };
             
             fadeOut.Completed += (s, e) => {
                 RoundResultPanel.Visibility = Visibility.Collapsed;
             };

             RoundResultPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);
             Task.Delay(3000).ContinueWith(_ => Dispatcher.Invoke(() => RoundResultPanel.BeginAnimation(UIElement.OpacityProperty, fadeOut)));
         }
    }
}