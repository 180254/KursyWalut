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

        public DateTime Day { get; }
        public Currency Currency { get; }
        public double AverageRate { get; }

        public override string ToString()
        {
            return $"[Day: {Day}, Currency: {Currency}, AverageRate: {AverageRate}]";
        }
    }
}