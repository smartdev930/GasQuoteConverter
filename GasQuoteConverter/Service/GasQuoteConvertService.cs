using GasQuoteConverter.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;


// Created By : Gjergji Dano.
// Creation Date : 21, Jan, 2021.
// This class is the service to convert gas quote data by some algorithm.
// 
namespace GasQuoteConverter.Service
{
    public class GasQuoteConvertService
    {
        private DateTime dtMin;
        private DateTime dtMax;
        private DateTime dtLastProcessed;
        private string[] dateFormats;
        public GasQuoteConvertService()
        {
            dtMin = new DateTime(2000, 1, 1); // If ObservationDate < 1/1/2000, it's invalid.
            dtMax = DateTime.Now;             // If ObservationDate > Now, it's invalid.
            dtLastProcessed = DateTime.Now;  // It will be needed when ObservationDate is null or invalid. In that case we will use last processed observation date instead.
            dateFormats = new[] { "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "dd/MM/yyyy" };
        }

        public string ConvertGasQuoteData(string[] csvLines)
        {
            StringBuilder outputString = new StringBuilder();
            
            //Reading CSV File and Sort by ObservationDate, Year, and Index. (if shorthand = Q1_10, then year = 10, Index = 1)
            List<GasQuote> values = csvLines.Select(c => GetGasQuoteFromString(c))
                                           .Where(c => c != null)
                                           .OrderBy(c => c.ObservationDate)
                                           .ThenBy(c => c.Year)
                                           .ThenBy(c => c.Index)
                                           .ToList();

            // To get header of CSV, sort Shorthands by Year and Index
            string[] shorthands = values.OrderBy(c => c.Year).ThenBy(c => c.Index).Select(c => c.Shorthand).Distinct().ToArray();

            // Appending CSV Header.
            outputString.AppendLine("ObservationDate," + string.Join(",", shorthands));


            int curIndex = 0;   // current Column Number in row.
            string line = "";   // string value of one row.
            for (int i = 0; i < values.Count; i++)
            {
                if (curIndex == 0)
                {
                    line = values[i].sObservationDate; // column0 is ObservationDate
                    curIndex++;
                }
                while (curIndex <= shorthands.Length && (shorthands[curIndex - 1] != values[i].Shorthand))
                {
                    line += ",";    // if not corresponding column, then skip with comma.
                    curIndex++;
                }

                if (curIndex > shorthands.Length)
                {
                    i--;
                    curIndex = 0;
                    outputString.AppendLine(line);   //if can not find corresponding column, then move to next row.
                    line = "";
                    continue;
                }
                else
                {
                    line += "," + values[i].Price;  // if find corresponding column, write price.
                    if (curIndex == shorthands.Length)
                    {
                        outputString.AppendLine(line);       // if corresponding column is last column, then move to next row.
                        line = "";
                        curIndex = 0;
                    }
                    else
                    {
                        curIndex++;
                    }

                }
            }
            return outputString.ToString();
        }

        // This function is getting GasQuote type data from CSV line.
        public GasQuote GetGasQuoteFromString(string csvLine)
        {
            string[] values = csvLine.Split(',');
            if (values.Length < 5)
            {
                Console.WriteLine("Invalid Line : {0}", csvLine);
                Console.WriteLine();
                return null;
            }
            GasQuote item = new GasQuote();
            
            if (!DateTime.TryParseExact(values[0], dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out item.ObservationDate))
            {
                item.ObservationDate = dtLastProcessed;    // When observationdata is invalid, we will set it as last processed observation date                
                Console.WriteLine("Found invalid observation date. So set the observationd date as previous line's observation date");
                Console.WriteLine("Line Content : {0}", csvLine);
                Console.WriteLine("Original Wrong Observation Date : {0}", values[0]);
                Console.WriteLine("New Modified Observation Date : {0}", dtLastProcessed.ToString("dd/MM/yyyy"));
                Console.WriteLine();
            }
            if (!CheckObservationDateValid(item.ObservationDate))
            {
                item.ObservationDate = dtLastProcessed;
                Console.WriteLine("Found invalid observation date. So set the observationd date as previous line's observation date");
                Console.WriteLine("Line Content : {0}", csvLine);
                Console.WriteLine("Original Wrong Observation Date : {0}", values[0]);
                Console.WriteLine("New Modified Observation Date : {0}", dtLastProcessed.ToString("dd/MM/yyyy"));
                Console.WriteLine();
            }                
            item.sObservationDate = item.ObservationDate.ToString("dd/MM/yyyy");
            
            dtLastProcessed = item.ObservationDate;
            
            item.Shorthand = values[1];
            string[] codes = values[1].Split('_');      // it is needed for getting Year and Index.
            try
            {
                item.Year = Convert.ToInt32(codes[1]);
                item.Index = Convert.ToInt32(codes[0].Substring(1));
                if (!ValidateShortHandWithFromDate(values[2], item.Year, item.Index))
                {
                    throw new Exception();
                }
            }
            catch
            {
                bool bValid = GetShortHandsFromDate(values[2], out item.Year, out item.Index);
                if (bValid)
                {
                    item.Shorthand = $"Q{item.Index}_{item.Year.ToString("D2")}";
                    Console.WriteLine("Found invalid shorthand. So get the shorthand from dates.");
                    Console.WriteLine("Line Content : {0}", csvLine);
                    Console.WriteLine("Original Wrong Shorthand : {0}", values[1]);
                    Console.WriteLine("New Modified Shorthand : {0}", item.Shorthand);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Found invalid shorthand. Could not fix this wrong shorthand.");
                    Console.WriteLine("Line Content : {0}", csvLine);
                    Console.WriteLine();
                    return null;
                }
                
            }
            
            item.Price = values[4];

            return item;
        }
        private bool CheckObservationDateValid(DateTime dt)
        {
            return (dt > dtMin && dt < dtMax);
        }
        private bool ValidateShortHandWithFromDate(string fromDate, int year, int index)
        {
            if (year > 100 || index > 4 || index == 0)
            {
                return false;
            }
            DateTime dtFrom;
            if (!DateTime.TryParseExact(fromDate, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtFrom))
            {
                return true;
            }
            if (dtFrom.Year % 100 != year)
            {
                return false;
            }
            if ((dtFrom.Month - 1) / 3 + 1 != index)
            {
                return false;
            }

            return true;

        }
        private bool GetShortHandsFromDate(string fromDate, out int year, out int index)
        {
            DateTime dtFrom;
            year = 0;
            index = 0;
            if (!DateTime.TryParseExact(fromDate, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtFrom))
            {
                return false;
            }
            year = dtFrom.Year % 100;
            index = (dtFrom.Month - 1) / 3 + 1;
            
            return true;
        }
    }
}
