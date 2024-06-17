
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PLC_Lab10;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace PLC_Lab8
{
    public class Program
    {
		private static string NormalizeLineEndings(string input)
		{
			// Normalizace na jednotný styl ukončení řádků
			return input.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
		}
		public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var fileName = "input.txt";
            Console.WriteLine("Parsing: " + fileName);
            var inputFile = new StreamReader(fileName);
            AntlrInputStream input = new AntlrInputStream(inputFile);
            PLC_Lab8_exprLexer lexer = new PLC_Lab8_exprLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            PLC_Lab8_exprParser parser = new PLC_Lab8_exprParser(tokens);

            parser.AddErrorListener(new VerboseListener());

            IParseTree tree = parser.program();

            if (parser.NumberOfSyntaxErrors == 0)
            {
                //Console.WriteLine(tree.ToStringTree(parser));
                ParseTreeWalker walker = new ParseTreeWalker();
                var result = new EvalVisitor().Visit(tree);
				string normalizedResult = NormalizeLineEndings(result.Code);
				int numberOfErrors;
                Errors.PrintAndClearErrors(out numberOfErrors);
                if(numberOfErrors==0)
                {
                    //Console.WriteLine(normalizedResult);
					using var sw = new StreamWriter("../../../Kód.txt", false);
					sw.Write(normalizedResult);
					sw.Close();
					VirtualMachine vm = new VirtualMachine(normalizedResult);
					vm.Run();
				}
                else
                {
                    Console.WriteLine($"There are {numberOfErrors} errors");
                }
                
            }
        }
    }
}