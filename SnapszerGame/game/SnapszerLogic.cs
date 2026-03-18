using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnapszerGame.game
{
    public class SnapszerLogic
    {
        // Ütés nyertesének eldöntése
        public Player GetWinnerOfTrick(Card hivottLap, Card valaszLap, Player hivoJatekos, Player valaszoloJatekos, Szin aduSzin)
        {
            // Válaszoló aduzott (felülütötte a nem-adut)
            if (valaszLap.szin == aduSzin && hivottLap.szin != aduSzin)
            {
                return valaszoloJatekos;
            }

            // Színre színt, a nagyobb viszi
            if (hivottLap.szin == valaszLap.szin)
            {
                return (hivottLap.pont > valaszLap.pont) ? hivoJatekos : valaszoloJatekos;
            }

            // Különben a hívó viszi (kisebb dobás más színből)
            return hivoJatekos;
        }

        // Szabványos lerakás validálása
        public bool SzabalyosKartyaRakas(Card lerakandoKartya, Card hivottLap, Szin aduSzin, bool pakliLezarva, IEnumerable<Card> jatekosKezeben)
        {
            // Saját hívás, bármi lerakható
            if (hivottLap == null)
            {
                return true;
            }

            // Nyitott talonnál nincsenek kényszerek
            if (!pakliLezarva)
            {
                return true;
            }

            // --- LEZÁRT TALON: Színkényszer, ütéskényszer, adukényszer ---

            bool vanHivottSzin = jatekosKezeben.Any(lap => lap.szin == hivottLap.szin);

            if (vanHivottSzin)
            {
                // Színkényszer
                if (lerakandoKartya.szin != hivottLap.szin) return false;

                // Ütéskényszer
                bool vanNagyobbLapa = jatekosKezeben.Any(lap => lap.szin == hivottLap.szin && lap.pont > hivottLap.pont);
                if (vanNagyobbLapa && lerakandoKartya.pont < hivottLap.pont) return false;

                return true;
            }

            // Adukényszer
            bool vanAduSzin = jatekosKezeben.Any(lap => lap.szin == aduSzin);
            if (vanAduSzin)
            {
                if (lerakandoKartya.szin != aduSzin) return false;
                return true;
            }

            // Nincs szín, nincs adu -> szabad dobás
            return true;
        }

        // Kinyeri az elérhető bemondásokat (Király+Felső párok)
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

        // Bemondás pontértéke (sima=20, adu=40)
        public int BemondasErteke(Szin bemondottSzin, Szin aduSzin)
        {
            return (bemondottSzin == aduSzin) ? 40 : 20;
        }
    }
}
