using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PLC_Lab10
{
	public class VirtualMachine
	{
		private Stack<object> stack = new Stack<object>();
		private List<string[]> instructions;
		private Dictionary<string, object> memory = new Dictionary<string, object>();
		private Dictionary<int, int> labels = new Dictionary<int, int>();
		private int instructionPointer = 0;
		public VirtualMachine(string code)
		{
			instructions = ParseInstructions(code);
		}

		private List<string[]> ParseInstructions(string code)
		{
			var parsedInstructions = new List<string[]>();
			var lines = code.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				var trimmedLine = line.Trim();
				if (string.IsNullOrEmpty(trimmedLine))
					continue;

				// Handle pushing strings with spaces
				if (trimmedLine.StartsWith("push S"))
				{
					var firstQuote = trimmedLine.IndexOf('"');
					var lastQuote = trimmedLine.LastIndexOf('"');
					string beforeQuote = trimmedLine.Substring(0, firstQuote).Trim();
					string stringLiteral = trimmedLine.Substring(firstQuote, lastQuote - firstQuote + 1);
					parsedInstructions.Add(new[] { "push", "S", stringLiteral });
				}
				else
				{
					var parts = trimmedLine.Split(new[] { ' ' }, 3);
					parsedInstructions.Add(parts);
				}
			}

			return parsedInstructions;
		}

		public void Run()
		{
			PreprocessLabels();
			while (instructionPointer < instructions.Count)
			{
				var instruction = instructions[instructionPointer];
				switch (instruction[0].ToLower())
				{
					case "jmp":
						instructionPointer = labels[int.Parse(instruction[1])];
						break;
					case "fjmp":
						bool condition = (bool)stack.Pop();
						if (!condition)
						{
							instructionPointer = labels[int.Parse(instruction[1])];
						}
						else
						{
							instructionPointer++;
						}
						break;
					case "push":
						HandlePush(instruction);
						break;
					case "print":
						HandlePrint(instruction);
						break;
					case "save":
						HandleSave(instruction);
						break;
					case "load":
						HandleLoad(instruction);
						break;
					case "read":
						HandleRead(instruction);
						break;
					case "uminus":
						HandleUMinus();
						break;
					case "lt":
					case "gt":
					case "eq":
					case "and":
					case "or":
						HandleComparison(instruction);
						break;
					case "itof":
						HandleIToF();
						break;
					case "not":
						HandleNot();
						break;
					case "add":
					case "sub":
					case "mul":
					case "div":
					case "mod":
					case "concat":
						HandleArithmetic(instruction);
						break;
				}
				if (!instruction[0].ToLower().Equals("jmp") && !instruction[0].ToLower().Equals("fjmp"))
				{
					instructionPointer++;
				}
			}
		}


		private void PreprocessLabels()
		{
			for (int i = 0; i < instructions.Count; i++)
			{
				if (instructions[i][0].ToLower() == "label")
				{
					int labelNum = int.Parse(instructions[i][1]);
					labels[labelNum] = i;
				}
			}
		}
		private void HandleUMinus()
		{
			if (stack.Count > 0)
			{
				var top = stack.Pop();
				if (top is int intValue)
				{
					stack.Push(-intValue);
				}
				else if (top is float floatValue)
				{
					stack.Push(-floatValue);
				}
				else
				{
					throw new InvalidOperationException("Unary minus can only be applied to integers or floats.");
				}
			}
			else
			{
				throw new InvalidOperationException("Stack is empty, cannot perform unary minus.");
			}
		}


		private void HandlePush(string[] instruction)
		{
			switch (instruction[1])
			{
				case "I":
					stack.Push(int.Parse(instruction[2]));
					break;
				case "F":
					stack.Push(float.Parse(instruction[2], CultureInfo.InvariantCulture));
					break;
				case "B":
					stack.Push(bool.Parse(instruction[2]));
					break;
				case "S":
					stack.Push(instruction[2].Trim('"'));
					break;
			}
		}

		private void HandlePrint(string[] instruction)
		{
			int count = int.Parse(instruction[1]);
			List<string> output = new List<string>();
			for (int i = 0; i < count; i++)
			{
				if (stack.Count > 0)
					output.Add(stack.Pop().ToString());
			}
			Console.WriteLine(string.Join("", output.Reverse<string>()));
		}


		private void HandleComparison(string[] instruction)
		{
			object right = stack.Pop();
			object left = stack.Pop();
			switch (instruction[0])
			{
				case "lt":
					if (instruction[1] == "I")
						stack.Push((int)left < (int)right);
					else if (instruction[1] == "F")
						stack.Push((float)left < (float)right);
					break;
				case "gt":
					if (instruction[1] == "F" && left is float && right is float)
						stack.Push((float)left > (float)right);
					break;
				case "eq":
					switch (instruction[1])
					{
						case "S":
							stack.Push(left.ToString() == right.ToString());
							break;
						case "F":
							stack.Push((float)left == (float)right);
							break;
						case "I":
							stack.Push((int)left == (int)right);
							break;
					}
					break;
				case "and":
					stack.Push((bool)left && (bool)right);
					break;
				case "or":
					stack.Push((bool)left || (bool)right);
					break;
			}
		}


		private void HandleIToF()
		{
			if (stack.Peek() is int intValue)
			{
				stack.Pop();
				stack.Push((float)intValue);
			}
		}

		private void HandleNot()
		{
			if (stack.Peek() is bool boolValue)
			{
				stack.Pop();
				stack.Push(!boolValue);
			}
		}
		private void HandleSave(string[] instruction)
		{
			string variableName = instruction[1];
			if (stack.Count > 0)
			{
				var value = stack.Peek();
				memory[variableName] = value;
			}
			else
			{
				throw new InvalidOperationException("Cannot save, stack is empty");
			}
		}



		private void HandleLoad(string[] instruction)
		{
			string variableName = instruction[1];
			if (memory.ContainsKey(variableName))
			{
				stack.Push(memory[variableName]);
			}
		}

		private void HandleRead(string[] instruction)
		{
			string type = instruction[1];
			switch (type)
			{
				case "I":
					stack.Push(int.Parse(Console.ReadLine()));
					break;
				case "F":
					stack.Push(float.Parse(Console.ReadLine(), CultureInfo.InvariantCulture));
					break;
				case "S":
					stack.Push(Console.ReadLine());
					break;
				case "B":
					stack.Push(bool.Parse(Console.ReadLine()));
					break;
			}
		}


		private void HandleArithmetic(string[] instruction)
		{
			var right = stack.Pop();
			var left = stack.Pop();
			switch (instruction[0])
			{
				case "add":
					stack.Push((dynamic)left + (dynamic)right);
					break;
				case "sub":
					stack.Push((dynamic)left - (dynamic)right);
					break;
				case "mul":
					stack.Push((dynamic)left * (dynamic)right);
					break;
				case "div":
					stack.Push((dynamic)left / (dynamic)right);
					break;
				case "mod":
					stack.Push((int)left % (int)right);
					break;
				case "concat":
					stack.Push(left.ToString() + right.ToString());
					break;
			}
		}


	}
}
