using System;

namespace DwellerBot.Models
{
    public class Rate
    {
        public int Cur_ID { get; set; }
        public DateTime Date { get; set; }
        public string Cur_Abbreviation { get; set; }
        public int Cur_Scale { get; set; }
        public string Cur_Name { get; set; }
        public Nullable<decimal> Cur_OfficialRate { get; set; }
    }

    public class Currency
    {
        public int Cur_ID { get; set; }
        public Nullable<int> Cur_ParentID { get; set; }
        public string Cur_Code { get; set; }
        public string Cur_Abbreviation { get; set; }
        public string Cur_Name { get; set; }
        public string Cur_Name_Bel { get; set; }
        public string Cur_Name_Eng { get; set; }
        public string Cur_QuotName { get; set; }
        public string Cur_QuotName_Bel { get; set; }
        public string Cur_QuotName_Eng { get; set; }
        public string Cur_NameMulti { get; set; }
        public string Cur_Name_BelMulti { get; set; }
        public string Cur_Name_EngMulti { get; set; }
        public int Cur_Scale { get; set; }
        public int Cur_Periodicity { get; set; }
        public System.DateTime Cur_DateStart { get; set; }
        public System.DateTime Cur_DateEnd { get; set; }
    }

    public class RateShort
    {
        public int Cur_ID { get; set; }
        public System.DateTime Date { get; set; }
        public Nullable<decimal> Cur_OfficialRate { get; set; }
    }

}