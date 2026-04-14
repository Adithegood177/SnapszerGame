using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnapszerGame.game
{
    public class SnapszerLogic
    {
        // Segédfüggvény, hogy be tudjuk állítani mi is veri a másikat egy ütésben (mert ez nem mindig pont alapján megy)
        // Amelyiknek nagyobb az értéke, az a lap erősebb.
        private int RankValue(Ertek e)
        {
            return e switch
            {
                Ertek.Asz => 6,
                Ertek.Kiraly => 5, // Itt a király azért veri a tízest ütésben, hogy simábban menjen
                Ertek.Tiz => 4,
                Ertek.Felsokiraly => 3,
                Ertek.Alsokiraly => 2,
                Ertek.Kilenc => 1,
                _ => 0
            };
        }

        // Kitalálja kié lett az ütés a két lerakott lap alapján
        public Player GetWinnerOfTrick(Card hivottLap, Card valaszLap, Player hivoJatekos, Player valaszoloJatekos, Szin aduSzin)
        {
            // Ha a válaszoló rávágta az adut, a hívó viszont csak simát tett
            if (valaszLap.szin == aduSzin && hivottLap.szin != aduSzin)
            {
                return valaszoloJatekos;
            }

            // Fordítva: ha a hívó aduzott egyet, a válaszoló meg nem
            if (hivottLap.szin == aduSzin && valaszLap.szin != aduSzin)
            {
                return hivoJatekos;
            }

            // Ha mindkettő amúgy ugyanaz a szín, akkor nézzük a ranglétrát (mert a pont itt csalóka lehetne)
            if (hivottLap.szin == valaszLap.szin)
            {
                return (RankValue(hivottLap.ertek) > RankValue(valaszLap.ertek)) ? hivoJatekos : valaszoloJatekos;
            }

            // Ha sejtettem hogy különböző szín, plusz senkinek se esett az adujára a dolog, akkor azé aki hívott (a srácunk egyszerű szabálya szerint)
            return hivoJatekos;
        }

        // Itt döntjük el, mit szabad lerakni a srácnak. A user szólt h ne basztassuk a színre színt ha nem muszáj.
        public bool SzabalyosKartyaRakas(Card lerakandoKartya, Card hivottLap, Szin aduSzin, bool pakliLezarva, IEnumerable<Card> jatekosKezeben)
        {
            // Ha elsőnek hívunk, ami jólesik, azt dobjuk
            if (hivottLap == null)
            {
                return true;
            }

            // Ameddig kint a pakli vége, a szabad világ dübörög
            if (!pakliLezarva)
            {
                return true;
            }

            // Mikor már elfogyott a húzó, a kérés szerint kiment a színre szín kötelezettség is az ablakon
            // Szinte akármit rávágunk, boldogok vagyunk.
            return true;
        }

        // Lecsekkolja a bemákolt király-felső kombókat kézben
        public List<Szin> LehetsegesBemondasok(IEnumerable<Card> jatekosKezeben)
        {
            var bemondasok = new List<Szin>();
            var groupedBySzin = jatekosKezeben.GroupBy(k => k.szin);

            foreach (var group in groupedBySzin)
            {
                bool hasKiraly = group.Any(k => k.ertek == Ertek.Kiraly);
                bool hasFelso = group.Any(k => k.ertek == Ertek.Felsokiraly);

                if (hasKiraly && hasFelso)
                {
                    bemondasok.Add(group.Key);
                }
            }

            return bemondasok;
        }
        // Megmondja mennyit ér a bemondás
        public int BemondasErteke(Szin bemondottSzin, Szin aduSzin)
        {
            return (bemondottSzin == aduSzin) ? 40 : 20;
        }
    }
}
