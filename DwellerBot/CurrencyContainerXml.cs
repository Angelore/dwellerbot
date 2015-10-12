using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwellerBot
{
    public class CurrencyContainerXml
    {
        public DailyExRates DailyRates { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class DailyExRates
        {

            private DailyExRatesCurrency[] currencyField;

            private string dateField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("Currency")]
            public DailyExRatesCurrency[] Currency
            {
                get
                {
                    return this.currencyField;
                }
                set
                {
                    this.currencyField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Date
            {
                get
                {
                    return this.dateField;
                }
                set
                {
                    this.dateField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class DailyExRatesCurrency
        {

            private ushort numCodeField;

            private string charCodeField;

            private ushort scaleField;

            private string nameField;

            private decimal rateField;

            private ushort idField;

            /// <remarks/>
            public ushort NumCode
            {
                get
                {
                    return this.numCodeField;
                }
                set
                {
                    this.numCodeField = value;
                }
            }

            /// <remarks/>
            public string CharCode
            {
                get
                {
                    return this.charCodeField;
                }
                set
                {
                    this.charCodeField = value;
                }
            }

            /// <remarks/>
            public ushort Scale
            {
                get
                {
                    return this.scaleField;
                }
                set
                {
                    this.scaleField = value;
                }
            }

            /// <remarks/>
            public string Name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            public decimal Rate
            {
                get
                {
                    return this.rateField;
                }
                set
                {
                    this.rateField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public ushort Id
            {
                get
                {
                    return this.idField;
                }
                set
                {
                    this.idField = value;
                }
            }
        }

    }
}
