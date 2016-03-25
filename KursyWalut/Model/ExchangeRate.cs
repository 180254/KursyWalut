using System;

namespace KursyWalut.Model
{
    public class ExchangeRate
    {
        public ExchangeRate(DateTime day, Currency currency, double averageRate)
        {
            Day = day;
            Currency = currency;
            AverageRate = averageRate;
        }

        public double AverageRate { get; }

        public DateTime Day { get; }
        public Currency Currency { get; }

        public string AvarageRateF => string.Format("{0:0.000}", AverageRate);

        public override string ToString()
        {
            return $"[Day: {Day}, Currency: {Currency}, AverageRate: {AvarageRateF}]";
        }
    }
}