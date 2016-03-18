using System;

namespace KursyWalut.Model
{
    internal class ExchangeRate
    {
        public readonly DateTime Day;
        public readonly Currency Currency;
        public readonly double AverageRate;

        public ExchangeRate(DateTime day, Currency currency, double averageRate)
        {
            Day = day;
            Currency = currency;
            AverageRate = averageRate;
        }

        public override string ToString()
        {
            return $"[Day: {Day}, Currency: {Currency}, AverageRate: {AverageRate}]";
        }
    }
}