using System;

namespace KursyWalut.Model
{
    public class ExchangeRate
    {
        public ExchangeRate(DateTimeOffset day, Currency currency, double averageRate)
        {
            Day = day;
            Currency = currency;
            AverageRate = averageRate;
        }

        public DateTimeOffset Day { get; }
        public Currency Currency { get; }
        public double AverageRate { get; }

        public DateTime DayDateTimeUtc => Day.UtcDateTime;

        public override string ToString()
        {
            return $"[Day: {Day}, Currency: {Currency}, AverageRate: {AverageRate}]";
        }
    }
}