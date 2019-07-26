using BetfairGameApi.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairGameApi.Cards
{
    public class GameHands
    {
        public List<cardsValue> Hands { get; set; }
        public double Probability { get; set; }
        public string PlayerName { get; set; }
        public bool Isbet { get; set; } =  false;
        public selectionStatusEnum Status { get; set; }
        public selectionSnapshot SelectionSnapshot { get; set; }
        public List<betSnapshotTypeBetSnapshotItem> PlayerBets { get; set; }
        public int PlayerId => SelectionSnapshot?.id ?? 0;
        public double DecimalOdd() => Probability != 0 ? Math.Round(1 / Probability, 2) : 0;

        public string PlayerPocket()
        {
            string H = " ";

            foreach (var item in Hands)
            {
                H += item.Name + " ";
            }

            return H;
        }

        private double[] Table = new double[]
        {
            1.01, 1.02, 1.03, 1.04, 1.05, 1.06, 1.07, 1.08, 1.09,
            1.1, 1.11, 1.12, 1.13, 1.14, 1.15, 1.16, 1.17, 1.18, 1.19, 1.2,
            1.21, 1.22, 1.23, 1.24, 1.25, 1.26, 1.27, 1.28, 1.29, 1.3, 1.31,
            1.32, 1.33, 1.34, 1.35, 1.36, 1.37, 1.38, 1.39, 1.4, 1.41, 1.42,
            1.43, 1.44, 1.45, 1.46, 1.47, 1.48, 1.49, 1.5, 1.51, 1.52, 1.53,
            1.54, 1.55, 1.56, 1.57, 1.58, 1.59, 1.6, 1.61, 1.62, 1.63, 1.64,
            1.65, 1.66, 1.67, 1.68, 1.69, 1.7, 1.71, 1.72, 1.73, 1.74, 1.75,
            1.76, 1.77, 1.78, 1.79, 1.8, 1.81, 1.82, 1.83, 1.84, 1.85, 1.86,
            1.87, 1.88, 1.89, 1.9, 1.91, 1.92, 1.93, 1.94, 1.95, 1.96, 1.97,
            1.98, 1.99, 2.0, 2.02, 2.04, 2.06, 2.08, 2.1, 2.12, 2.14, 2.16,
            2.18, 2.2, 2.22, 2.24, 2.26, 2.28, 2.3, 2.32, 2.34, 2.36, 2.38, 2.4,
            2.42, 2.44, 2.46, 2.48, 2.5, 2.52, 2.54, 2.56, 2.58, 2.6, 2.62,
            2.64, 2.66, 2.68, 2.7, 2.72, 2.74, 2.76, 2.78, 2.8, 2.82, 2.84,
            2.86, 2.88, 2.9, 2.92, 2.94, 2.96, 2.98, 3.0, 3.05, 3.1, 3.15, 3.2,
            3.25, 3.3, 3.35, 3.4, 3.45, 3.5, 3.55, 3.6, 3.65, 3.7, 3.75, 3.8,
            3.85, 3.9, 3.95, 4.0, 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8, 4.9,
            5.0, 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7, 5.8, 5.9, 6.0, 6.2, 6.4,
            6.6, 6.8, 7.0, 7.2, 7.4, 7.6, 7.8, 8.0, 8.2, 8.4, 8.6, 8.8, 9.0,
            9.2, 9.4, 9.6, 9.8, 10.0, 10.5, 11.0, 11.5, 12.0, 12.5, 13.0, 13.5,
            14.0, 14.5, 15.0, 15.5, 16.0, 16.5, 17.0, 17.5, 18.0, 18.5, 19.0,
            19.5, 20.0, 21.0, 22.0, 23.0, 24.0, 25.0, 26.0, 27.0, 28.0, 29.0,
            30.0, 32.0, 34.0, 36.0, 38.0, 40.0, 42.0, 44.0, 46.0, 48.0, 50.0,
            55.0, 60.0, 65.0, 70.0, 75.0, 80.0, 85.0, 90.0, 95.0, 100.0, 110.0,
            120.0, 130.0, 140.0, 150.0, 160.0, 170.0, 180.0, 190.0, 200.0,
            210.0, 220.0, 230.0, 240.0, 250.0, 260.0, 270.0, 280.0, 290.0,
            300.0, 310.0, 320.0, 330.0, 340.0, 350.0, 360.0, 370.0, 380.0,
            390.0, 400.0, 410.0, 420.0, 430.0, 440.0, 450.0, 460.0, 470.0,
            480.0, 490.0, 500.0, 510.0, 520.0, 530.0, 540.0, 550.0, 560.0,
            570.0, 580.0, 590.0, 600.0, 610.0, 620.0, 630.0, 640.0, 650.0,
            660.0, 670.0, 680.0, 690.0, 700.0, 710.0, 720.0, 730.0, 740.0,
            750.0, 760.0, 770.0, 780.0, 790.0, 800.0, 810.0, 820.0, 830.0,
            840.0, 850.0, 860.0, 870.0, 880.0, 890.0, 900.0, 910.0, 920.0,
            930.0, 940.0, 950.0, 960.0, 970.0, 980.0, 990.0, 1000.0
        };

        public double AddPip()
        {
            int index = Array.IndexOf<double>(Table, AjustOdd());
            return Table[++index];
        }

        public double AddPip(int num)
        {

            int index = Array.IndexOf<double>(Table, AjustOdd());
            return Table[index + num];
        }

        public double SubtractPip()
        {
            int index = Array.IndexOf<double>(Table, AjustOdd());
            return Table[--index];
        }

        public double SubtractPip(int num)
        {
            int index = Array.IndexOf<double>(Table, AjustOdd());
            return Table[index - num];
        }

        public double RoundDownToNearestBetfairPrice(double price)
        {
            int index = 0;
            for (int i = 0; i < Table.Length; i++)
            {
                if (Table[i] > price)
                    return Table[index];

                index++;
            }

            return 0.0;
        }

        public double RoundUpToNearestBetfairPrice(double price)
        {
            int index = 0;
            for (int i = 0; i < Table.Length; i++)
            {
                if (Table[i] > price)
                    return Table[index++];

                index++;
            }

            return 0.0;
        }

        public double ApplySpread(double percentage)
        {
            double adjustedPrice = AjustOdd() * percentage;

            if (percentage <= 1.0)
                return RoundDownToNearestBetfairPrice(adjustedPrice);
            else
                return RoundUpToNearestBetfairPrice(adjustedPrice);
        }

        public double AjustOdd()
        {
            if (Probability == 0)
            {
                return Probability;
            }


            var Pro = Math.Round((1 / Probability), 2);

            if (Table.Contains(Pro))
            {
                return Pro;
            }
            else
            {
                return Table.Select(p => new { Value = p, Difference = Math.Abs(p - Pro) })
                 .OrderBy(p => p.Difference).Where(x => x.Value > Pro).First().Value;
            }
        }

        public double BestOddback()
        {
            if (SelectionSnapshot != null)
            {
                if (SelectionSnapshot.bestAvailableToBackPrices != null && SelectionSnapshot.bestAvailableToBackPrices.Count > 0)
                {
                    return SelectionSnapshot.bestAvailableToBackPrices[0].Value;
                }
            }

            return 0;


        }

        public double BestOddlay()
        {
            if (SelectionSnapshot != null)
            {
                if (SelectionSnapshot.bestAvailableToLayPrices != null && SelectionSnapshot.bestAvailableToLayPrices.Count > 0)
                {
                    return SelectionSnapshot.bestAvailableToLayPrices[0].Value;
                }
            }


            return 0;

        }

        public string _ToString()
        {
            string Template = "{0} Status: {1} Cards: [{2}] Stake:{3} \n-> [{4}] {5} :: {6} [{7}]";

            return string.Format(Template,
                PlayerName,
                Status,
                PlayerPocket(),
                PlayerBets.FirstOrDefault()?.size,
                SelectionSnapshot.bestAvailableToBackPrices.Sum(a => a.amountUnmatched).ToString("0").PadLeft(7),
                SelectionSnapshot.bestAvailableToBackPrices.Count > 0 ? SelectionSnapshot.bestAvailableToBackPrices[0].Value.ToString("0.00").PadLeft(6) : "  0.00",
                SelectionSnapshot.bestAvailableToLayPrices.Count > 0 ? SelectionSnapshot.bestAvailableToLayPrices[0].Value.ToString("0.00").PadLeft(6) : "  0.00",
                SelectionSnapshot.bestAvailableToLayPrices.Sum(a => a.amountUnmatched).ToString("0").PadLeft(7));
        }

    }


    public enum CardType
    {
        Spades,
        Hearts,
        Diamonds,
        Clubs
    }

    public class cardsValue
    {
        public int BetfairValue { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public CardType CardType { get; set; }

    }
}
