using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GasQuoteConverter;
using GasQuoteConverter.Model;
using GasQuoteConverter.Service;

namespace GasQuoteConverterTests
{
    
    // Created By : Gjergji Dano.
    // Creation Date : 21, Jan, 2021.
    
    [TestClass]
    public class GasQuoteConverterTests
    {
        [TestMethod]
        // This test method is to validate the engine to fix wrong shorthands based on dates.
        public void GetGasQuoteFromString_WithWrongShorthand()
        {
            // Arange
            string wrongShorthandLine1 = "02/01/2009,,01/07/2010,30/09/2010,0.511";     // empty shorthand
            string wrongShorthandLine2 = "02/01/2009,Q,01/07/2010,30/09/2010,0.511";    // wrong shorthand 'Q'
            string wrongShorthandLine3 = "02/01/2009,Q5_11,01/07/2010,30/09/2010,0.511";// wrong shorthand 'Q5_11'. Q5 is invalid.
            string expected = "Q3_10";
            GasQuoteConvertService qcService = new GasQuoteConvertService();

            // Act
            GasQuote gasQuote1 = qcService.GetGasQuoteFromString(wrongShorthandLine1);
            GasQuote gasQuote2 = qcService.GetGasQuoteFromString(wrongShorthandLine2);
            GasQuote gasQuote3 = qcService.GetGasQuoteFromString(wrongShorthandLine3);

            // Assert

            Assert.AreEqual(expected, gasQuote1.Shorthand);
            Assert.AreEqual(expected, gasQuote2.Shorthand);
            Assert.AreEqual(expected, gasQuote3.Shorthand);
        }
        [TestMethod]
        // This test method is to validate the engine to fix wrong observation date based on above correct date.
        // Input data is containing 2 wrong observation date. One is invalid date '14/01/1900', another is empty.
        public void ConvertGasQuoteData_WithWrongObservationDate()
        {
            // Arange

            string expectedHeader = "ObservationDate,Q1_14,Q2_14,Q3_14,Q4_14,Q1_15";
            string expectedDataLine = "13/11/2009,0.6855,0.592,0.575,0.6295,0.7145";
            var fileLines = File.ReadAllLines("Data\\WrongObservationDateFile.csv");

            GasQuoteConvertService qcService = new GasQuoteConvertService();

            // Act
            string[] result = qcService.ConvertGasQuoteData(fileLines).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            
            // Assert

            Assert.AreEqual(expectedHeader, result[0]);
            Assert.AreEqual(expectedDataLine, result[1]);

        }
    }
}
