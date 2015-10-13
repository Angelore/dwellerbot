using System;

namespace DwellerBot.Models
{
    public class CurrencyContainer
    {
        public Query query { get; set; }
    }

    public class Query
    {
        public int count { get; set; }
        public DateTime created { get; set; }
        public string lang { get; set; }
        public Results results { get; set; }
    }

    public class Results
    {
        public rate[] rate { get; set; }
    }

    public class rate
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string Rate { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Ask { get; set; }
        public string Bid { get; set; }
    }
}
