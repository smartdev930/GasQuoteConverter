using GasQuoteConverter.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Created By : Gjergji Dano.
// Creation Date : 21, Jan, 2021.
// This program is reading inputed .csv file, convert it as some required logic, and write result as "output.csv" file.
// 

namespace GasQuoteConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Input the csv file path to convert.");
            string filepath = string.Empty;
            string[] lines = null;
            while (lines == null || lines.Length < 1)
            {
                filepath = Console.ReadLine();
                try
                {
                    lines = File.ReadAllLines(filepath).Skip(1).ToArray();                    
                }
                catch
                {
                    Console.WriteLine("Invalid csv file. Input file path again.");
                    continue;
                }
                if(lines.Length < 1)
                {
                    Console.WriteLine("No data in file. Input file path again.");
                }
            }

            FileStream fs;
            StreamWriter sw;
            TextWriter twOldOut= Console.Out;   //  Save old out to set it later.
            try
            {
                if(!Directory.Exists("Log"))
                {
                    Directory.CreateDirectory("Log");
                }
                fs = new FileStream($"Log\\log_{DateTime.Now.ToString("yyyyMMddTHHmmss")}.txt", FileMode.OpenOrCreate, FileAccess.Write);
                sw = new StreamWriter(fs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open log.txt for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(sw);     // Set console's out as streamwriter for log.txt.

            Console.WriteLine("Start converting csv file...");
            Console.WriteLine("FilePath : {0}", filepath);
            Console.WriteLine("Timestamp : {0}", DateTime.Now.ToString());
            Console.WriteLine();

            GasQuoteConvertService qcService = new GasQuoteConvertService();
            var outputBuffer = qcService.ConvertGasQuoteData(lines);
            using (StreamWriter file = new StreamWriter("output.csv", false))
            {
                file.Write(outputBuffer);
            }

            Console.WriteLine("Converted successfully....");

            Console.SetOut(twOldOut);   // Set console's out as original.
            sw.Close();
            fs.Close();
            Console.WriteLine("Done");
        }


    }
}
