using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnapszerGame.game
{
    public class SnapszerLogic
    {
        // Helper ranking for trick-taking (separate from scoring points).
        // Higher number = higher rank in trick-taking.
        private int RankValue(Ertek e)
        {
            return e switch
            {
                Ertek.Asz => 6,
                Ertek.Kiraly => 5, // King now ranks above Ten
                Ertek.Tiz => 4,
                Ertek.Felsokiraly => 3,
                Ertek.Alsokiraly => 2,
                Ertek.Kilenc => 1,
                _ => 0
            };
        }

        public Player GetWinnerOfTrick(Card hivottLap, Card valaszLap, Player hivoJatekos, Player valaszoloJatekos, Szin aduSzin)
        {
            // If the responder played trump while leader didn't, responder wins
            if (valaszLap.szin == aduSzin && hivottLap.szin != aduSzin)
            {
                return valaszoloJatekos;
            }

            // If leader played trump and responder didn't, leader wins
            if (hivottLap.szin == aduSzin && valaszLap.szin != aduSzin)
            {
                return hivoJatekos;
            }

            // If both cards are of same suit (including both trump), compare rank using RankValue
            if (hivottLap.szin == valaszLap.szin)
            {
                return (RankValue(hivottLap.ertek) > RankValue(valaszLap.ertek)) ? hivoJatekos : valaszoloJatekos;
            }

            // Otherwise, if neither is trump and suits differ, the leader wins (per simplified rules)
            return hivoJatekos;
        }

        // Per user request: do not force following suit when the deck is closed; allow playing any card (trump may be played on anything).
        public bool SzabalyosKartyaRakas(Card lerakandoKartya, Card hivottLap, Szin aduSzin, bool pakliLezarva, IEnumerable<Card> jatekosKezeben)
        {
            // If there is no leading card, any card can be played
            if (hivottLap == null)
            {
                return true;
            }

            // If the deck is not closed, free play (no obligation)
            if (!pakliLezarva)
            {
                return true;
            }

            // When the deck is closed, per the requested rule we do not enforce following-suit.
            // Trump can be played on any card and players are free to choose any card.
            return true;
        }

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

        public int BemondasErteke(Szin bemondottSzin, Szin aduSzin)
        {
            return (bemondottSzin == aduSzin) ? 40 : 20;
        }
    }
}
