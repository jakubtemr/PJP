﻿using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Lab8
{
    public class Errors
    {
        private static readonly List<string> ErrorsData = new List<string>();
        static public void ReportError(IToken token, string message)
        {
            ErrorsData.Add($"{token.Line}:{token.Column} - {message}");
        }
        public static int NumberOfErrors {  get { return ErrorsData.Count; } }
        public static void PrintAndClearErrors( out int c)
        {
            c = ErrorsData.Count;
            foreach (var error in ErrorsData)
            {
                Console.WriteLine(error);
            }
            ErrorsData.Clear(); 
        }
    }
}
