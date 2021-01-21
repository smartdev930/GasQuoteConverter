using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Created By : Gjergji Dano.
// Creation Date : 21, Jan, 2021.
// This class is the model of gas quote data.
//

namespace GasQuoteConverter.Model
{
    public class GasQuote
    {
        public DateTime ObservationDate;        // Observation Date.
        public string sObservationDate;         // string of Observation Date. There is the case which Observation Date is null in input data. so using string for Date.
        public int Year;                        // the year of shorthand. For example, Shorthand is "Q1_10", then Year is 10. it is needed for sorting.
        public int Index;                       // the index of shorthand. then index is 1 in above example.
        public string Shorthand;                // the string of Shorthand.
        public string Price;                    // the string of Price. I used it as string since there is the case Price is null.
    }
}

