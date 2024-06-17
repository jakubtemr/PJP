using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Lab8
{
    public class EvalVisitor : PLC_Lab8_exprBaseVisitor<(Type Type,string Code)>
    {
        SymbolTable symbolTable = new SymbolTable();
        private int label = -1;

        private int GetLabel()
        {
            label++;
            return label;
        }

        public override (Type Type, string Code) VisitProgram([NotNull] PLC_Lab8_exprParser.ProgramContext context)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var statement in context.statement())
            {
                var code= Visit(statement);
                //sb.Append(code); //s typy
                sb.Append(code.Code);
            }
            return (Type.String,sb.ToString());
        }

        public override (Type Type, string Code) VisitCondition([NotNull] PLC_Lab8_exprParser.ConditionContext context)
        {
            var exp = Visit(context.expr());
            if (exp.Type != Type.Bool) { 
                Errors.ReportError(context.expr().Start, $"Condition{context.expr().GetText()} must be a boolean expression.");
                return (Type.Error, "ERROR: Condition must be boolean");
            }
            return exp;
        }

        public override (Type Type, string Code) VisitExpressionAssignment([NotNull] PLC_Lab8_exprParser.ExpressionAssignmentContext context)
        {
             return Visit(context.assignment_expr());
        }

        public override (Type Type, string Code) VisitUnnaryMinus([NotNull] PLC_Lab8_exprParser.UnnaryMinusContext context)
        {
            var result = Visit(context.expr());
            if (result.Type != Type.Int && result.Type != Type.Float)
            {
                Errors.ReportError(context.expr().Start, $"Unary minus operand {context.expr().GetText()} must be int or float.");
                return (Type.Error, "ERROR: Unary minus type mismatch");
            }
            return (result.Type, result.Code + "uminus\n");
        }
        public override (Type Type, string Code) VisitUnnaryNeg([NotNull] PLC_Lab8_exprParser.UnnaryNegContext context)
        {
            var result = Visit(context.expr());
            if (result.Type != Type.Bool)
            {
                Errors.ReportError(context.expr().Start, $"Unary neg operand {context.expr().GetText()} must be bool.");
                return (Type.Error, "ERROR: Unary negation type mismatch");
            }
            string code = result.Code + "not\n";
            return (Type.Bool, code);
        }


        public override (Type Type, string Code) VisitNestedAss([NotNull] PLC_Lab8_exprParser.NestedAssContext context)
        {
            var right = Visit(context.assignment_expr());
            var variable = symbolTable[context.IDENTIFIER().Symbol];

            if (variable.Type == Type.Float && right.Type == Type.Int)
            {
                right.Code += "itof\n";
            }
            else if (variable.Type != right.Type)
            {
                Errors.ReportError(context.assignment_expr().Start, $"Cannot assign value of type {right.Type} to variable of type {variable.Type}.");
                return (Type.Error, "ERROR: Type mismatch in assignment");
            }

            string code = right.Code + $"save {context.IDENTIFIER().GetText()}\n";

            var nextContext = context.parent as PLC_Lab8_exprParser.NestedAssContext;
            while (nextContext != null)
            {
                var nextVariable = symbolTable[nextContext.IDENTIFIER().Symbol];
                if (nextVariable.Type == Type.Float && variable.Type == Type.Int)
                {
                    code += "itof\n";
                }
                else if (nextVariable.Type != variable.Type)
                {
                    Errors.ReportError(nextContext.assignment_expr().Start, $"Cannot assign value of type {variable.Type} to variable of type {nextVariable.Type}.");
                    return (Type.Error, "ERROR: Type mismatch in nested assignment");
                }
                code += $"save {nextContext.IDENTIFIER().GetText()}\n";
                variable = nextVariable;
                context = nextContext;
                nextContext = context.parent as PLC_Lab8_exprParser.NestedAssContext;
            }

            code += $"load {context.IDENTIFIER().GetText()}\n";


            return (variable.Type, code);
        }

        public override (Type Type, string Code) VisitLeafAss([NotNull] PLC_Lab8_exprParser.LeafAssContext context)
        {
            return Visit(context.assignment_leaf());
        }


        public override (Type Type, string Code) VisitAssTerminal([NotNull] PLC_Lab8_exprParser.AssTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, string Code) VisitAssOr([NotNull] PLC_Lab8_exprParser.AssOrContext context)
        {
            return Visit(context.or_expr());
        }
        public override (Type Type, string Code) VisitNestedOr([NotNull] PLC_Lab8_exprParser.NestedOrContext context)
        {
            var left = Visit(context.or_expr());
            var right = Visit(context.or_leaf());
            if (left.Type != right.Type || left.Type != Type.Bool)
            {
                Errors.ReportError(context.or_expr().Start, $"Operands must be boolean type.");
                return (Type.Error, "ERROR: Operand type mismatch in logical OR");
            }
            string code = $"{left.Code}{right.Code}or\n";
            return (Type.Bool, code);
        }

        public override (Type Type, string Code) VisitLeafOr([NotNull] PLC_Lab8_exprParser.LeafOrContext context)
        {
            return Visit(context.or_leaf());
        }
        public override (Type Type, string Code) VisitOrTerminal([NotNull] PLC_Lab8_exprParser.OrTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, string Code) VisitOrAnd([NotNull] PLC_Lab8_exprParser.OrAndContext context)
        {
            return Visit(context.and_expr());
        }

        public override (Type Type, string Code) VisitNestedAnd([NotNull] PLC_Lab8_exprParser.NestedAndContext context)
        {
            var left = Visit(context.and_expr());
            var right = Visit(context.and_leaf());
            if (left.Type != right.Type || left.Type != Type.Bool)
            {
                Errors.ReportError(context.and_expr().Start, $"Operands must be boolean type.");
                return (Type.Error, "ERROR: Operand type mismatch in logical AND");
            }
            string code = $"{left.Code}{right.Code}and\n";
            return (Type.Bool, code);
        }

        public override (Type Type, string Code) VisitLeafAnd([NotNull] PLC_Lab8_exprParser.LeafAndContext context)
        {
            return Visit(context.and_leaf());
        }

        public override (Type Type, string Code) VisitAndTerminal([NotNull] PLC_Lab8_exprParser.AndTerminalContext context)
        {
            return Visit(context.leaf_common());
        }

        public override (Type Type, string Code) VisitAndComp([NotNull] PLC_Lab8_exprParser.AndCompContext context)
        {
            return Visit(context.comp_expr());
        }

        public override (Type Type, string Code) VisitNestedComp([NotNull] PLC_Lab8_exprParser.NestedCompContext context)
        {
            var left = Visit(context.comp_expr());
            var right = Visit(context.comp_leaf());
            if (!(left.Type == right.Type || (left.Type == Type.Int && right.Type == Type.Float) || (left.Type == Type.Float && right.Type == Type.Int)))
            {
                Errors.ReportError(context.comp_expr().Start, $"Operands must be int or float type.");
                return (Type.Error, "ERROR: Operand type mismatch in comparison");
            }
            string opCode;
            if(left.Type == Type.Float || right.Type == Type.Float)
            {
				opCode = context.op.Text == "==" ? "eq F" : "eq F\nnot";
			}
          
			else if (left.Type == Type.Bool)
            {
				opCode = context.op.Text == "==" ? "eq B" : "eq B\nnot";
			}
			else if (left.Type==Type.String){
				opCode = context.op.Text == "==" ? "eq S" : "eq S\nnot";
			}
			else
            {
				opCode = context.op.Text == "==" ? "eq I" : "eq I\nnot";

			}
			return (Type.Bool, $"{left.Code}{right.Code}{opCode}\n");
		}


        public override (Type Type, string Code) VisitLeafComp([NotNull] PLC_Lab8_exprParser.LeafCompContext context)
        {
            return Visit(context.comp_leaf());
        }

        public override (Type Type, string Code) VisitCompTerminal([NotNull] PLC_Lab8_exprParser.CompTerminalContext context)
        {
            return Visit(context.leaf_common());
        }

        public override (Type Type, string Code) VisitCompRel([NotNull] PLC_Lab8_exprParser.CompRelContext context)
        {
            return Visit(context.rel_expr());
        }

        public override (Type Type, string Code) VisitNestedRel([NotNull] PLC_Lab8_exprParser.NestedRelContext context)
        {
            var left = Visit(context.rel_expr());
            var right = Visit(context.rel_leaf());
            string opCode = context.op.Text switch
            {
                ">" => "gt",
                "<" => "lt",
                _ => throw new InvalidOperationException("Unsupported operation")
            };

            if (left.Type != right.Type)
            {
                if ((left.Type == Type.Float && right.Type == Type.Int) || (left.Type == Type.Int && right.Type == Type.Float))
                {
                    string code =left.Type==Type.Int? $"{left.Code}itof\n{right.Code}{opCode} F\n": $"{left.Code}{right.Code}itof\n{opCode} F\n";
                    return (Type.Bool, code);
                }
                Errors.ReportError(context.rel_expr().Start, "Operands must be of the same type or convertible types (int and float).");
                return (Type.Error, "ERROR: Type mismatch in relational operation");
            }

            if (left.Type != Type.Int && left.Type != Type.Float)
            {
                Errors.ReportError(context.rel_expr().Start, "Operands must be int or float.");
                return (Type.Error, "ERROR: Invalid operand types for relational operation");
            }

            string finalCode = $"{left.Code}{right.Code}{opCode} I\n";
            return (Type.Bool, finalCode);
        }


        public override (Type Type, string Code) VisitLeafRel([NotNull] PLC_Lab8_exprParser.LeafRelContext context)
        {
            return Visit(context.rel_leaf());
        }

        public override (Type Type, string Code) VisitRelTerminal([NotNull] PLC_Lab8_exprParser.RelTerminalContext context)
        {
            return Visit(context.leaf_common());
        }

        public override (Type Type, string Code) VisitRelAdd([NotNull] PLC_Lab8_exprParser.RelAddContext context)
        {
            return Visit(context.add_expr());
        }

        public override (Type Type, string Code) VisitNestedAdd([NotNull] PLC_Lab8_exprParser.NestedAddContext context)
        {
            var left = Visit(context.add_expr());
            var right = Visit(context.add_leaf());
            string opCode = context.op.Text == "+" ? "add" : context.op.Text == "-" ? "sub" : "concat";

            if (opCode == "concat" && (left.Type == Type.String && right.Type == Type.String))
            {
                return (Type.String, $"{left.Code}{right.Code}concat\n");
            }
            else if ((left.Type == Type.Int || left.Type == Type.Float) && (right.Type == Type.Int || right.Type == Type.Float))
            {
                if (left.Type != right.Type)
                {
                    string leftCode = left.Type == Type.Int ? $"{left.Code}itof\n" : left.Code;
                    string rightCode = right.Type == Type.Int ? $"{right.Code}itof\n" : right.Code;
                    return (Type.Float, $"{leftCode}{rightCode}{opCode}\n");
                }
                return (left.Type, $"{left.Code}{right.Code}{opCode}\n");
            }

            Errors.ReportError(context.add_expr().Start, $"Operands must be either both strings for concatenation or both numbers for arithmetic.");
            return (Type.Error, "ERROR: Type mismatch in addition or subtraction");
        }



        public override (Type Type, string Code) VisitLeafAdd([NotNull] PLC_Lab8_exprParser.LeafAddContext context)
        {
            return Visit(context.add_leaf());
        }

        public override (Type Type, string Code) VisitAddTerminal([NotNull] PLC_Lab8_exprParser.AddTerminalContext context)
        {
            return Visit(context.leaf_common());
        }

        public override (Type Type, string Code) VisitAddMul([NotNull] PLC_Lab8_exprParser.AddMulContext context)
        {
            return Visit(context.mul_expr());
        }

        public override (Type Type, string Code) VisitNestedMul([NotNull] PLC_Lab8_exprParser.NestedMulContext context)
        {
            var left = Visit(context.mul_expr());
            var right = Visit(context.mul_leaf());
            if (context.op.Type == PLC_Lab8_exprParser.MUL_OP || context.op.Type == PLC_Lab8_exprParser.DIV_OP)
            {
                string opCode = context.op.Type == PLC_Lab8_exprParser.MUL_OP ? "mul\n" : "div\n";
                if (left.Type != right.Type)
                {
                    if (left.Type == Type.Int && right.Type == Type.Float || left.Type == Type.Float && right.Type == Type.Int)
                    {
                        return (Type.Float, left.Type == Type.Int ? $"{left.Code}itof\n{right.Code}{opCode}": $"{left.Code}{right.Code}itof\n{opCode}");
                    }
                    Errors.ReportError(context.mul_expr().Start, "Operands must be either both int or both float for multiplication or division.");
                    return (Type.Error, "ERROR: Type mismatch");
                }
                return (left.Type, $"{left.Code}{right.Code}{opCode}");
            }
            else if (context.op.Type == PLC_Lab8_exprParser.MOD_OP)
            {
                if (left.Type == Type.Int && right.Type == Type.Int)
                {
                    return (Type.Int, $"{left.Code}{right.Code}mod\n");
                }
                Errors.ReportError(context.mul_expr().Start, "Modulo operator requires both operands to be int type.");
                return (Type.Error, "ERROR: Type mismatch in modulo operation");
            }
            return (Type.Error, "ERROR: Unsupported operation");
        }


        public override (Type Type, string Code) VisitLeafMul([NotNull] PLC_Lab8_exprParser.LeafMulContext context)
        {
            return Visit(context.mul_leaf());
        }



        public override (Type Type, string Code) VisitMulTerminal([NotNull] PLC_Lab8_exprParser.MulTerminalContext context)
            {
                return Visit(context.leaf_common());
            }

        public override (Type Type, string Code) VisitLeafCommon([NotNull] PLC_Lab8_exprParser.LeafCommonContext context)
        {
            return Visit(context.leaf());
        }

        public override (Type Type, string Code) VisitCommonExpr([NotNull] PLC_Lab8_exprParser.CommonExprContext context)
        {
            return Visit(context.expr());
        }

        public override (Type Type, string Code) VisitCommonUnnary([NotNull] PLC_Lab8_exprParser.CommonUnnaryContext context)
        {
            return Visit(context.unnary_expr());
        }

        public override (Type Type, string Code) VisitPrimitiveType([NotNull] PLC_Lab8_exprParser.PrimitiveTypeContext context)
        {
            Type type = context.type.Text switch
            {
                "int" => Type.Int,
                "float" => Type.Float,
                "bool" => Type.Bool,
                "string" => Type.String,
                _ => throw new InvalidOperationException("Unsupported type")
            };
            return (type, "");
        }


        public override (Type Type, string Code) VisitFloat([NotNull] PLC_Lab8_exprParser.FloatContext context)
        {
            string floatLiteral = context.FLOAT().GetText();
            if (!floatLiteral.Contains("."))
            {
                floatLiteral += ".0";
            }
            return (Type.Float, $"push F {floatLiteral}\n");
        }

        public override (Type Type, string Code) VisitInt([NotNull] PLC_Lab8_exprParser.IntContext context)
        {
            return (Type.Int, $"push I {int.Parse(context.INT().GetText())}\n");
        }

        public override (Type Type, string Code) VisitBool([NotNull] PLC_Lab8_exprParser.BoolContext context)
        {
            return (Type.Bool, $"push B {bool.Parse(context.BOOL().GetText())}\n");
        }
        public override (Type Type, string Code) VisitString([NotNull] PLC_Lab8_exprParser.StringContext context)
        {
            return (Type.String, $"push S {context.STRING().GetText()}\n");
        }

        public override (Type Type, string Code) VisitIdentifier([NotNull] PLC_Lab8_exprParser.IdentifierContext context)
        {
            var identifier = context.IDENTIFIER().GetText();
            if (identifier == "true" || identifier == "false") // Special handling for boolean literals
            {
                return (Type.Bool, $"push B {identifier}\n");
            }

            var variable = symbolTable[context.IDENTIFIER().Symbol];
            return (variable.Type, $"load {identifier}\n");
        }

        public override (Type Type, string Code) VisitEmptyStatement([NotNull] PLC_Lab8_exprParser.EmptyStatementContext context)
        {
            return (Type.Error, "");
        }

        public override (Type Type, string Code) VisitDeclaration([NotNull] PLC_Lab8_exprParser.DeclarationContext context)
        {
            var typeResult = Visit(context.primitiveType());
            string code = "";
            foreach (var id in context.IDENTIFIER())
            {
                symbolTable.Add(id.Symbol, typeResult.Type);

                // Výchozí hodnoty pro různé typy
                string defaultValue = typeResult.Type switch
                {
                    Type.Int => "0",
                    Type.Float => "0.0",
                    Type.Bool => "false",
                    Type.String => "\"\"",
                    _ => "null"
                };

                code += $"push {typeResult.Type.ToString().Substring(0, 1).ToUpper()} {defaultValue}\n";
                code += $"save {id.GetText()}\n";
            }
            return (Type.Error, code);
        }


        public override (Type Type, string Code) VisitExpression([NotNull] PLC_Lab8_exprParser.ExpressionContext context)
        {
            var result = Visit(context.expr());
            return (Type.Error, $"{result.Code}pop\n");
        }

		public override (Type Type, string Code) VisitReadCLI([NotNull] PLC_Lab8_exprParser.ReadCLIContext context)
		{
			StringBuilder code = new StringBuilder();
			foreach (var id in context.IDENTIFIER())
			{
				var variable = symbolTable[id.Symbol];
				code.AppendLine($"read {variable.Type.ToString().Substring(0, 1).ToUpper()}");
				code.AppendLine($"save {id.GetText()}");
			}
			return (Type.Void, code.ToString());
		}


		public override (Type Type, string Code) VisitWriteCLI([NotNull] PLC_Lab8_exprParser.WriteCLIContext context)
		{
			StringBuilder code = new StringBuilder();
			int count = 0;
			foreach (var expr in context.expr())
			{
				var result = Visit(expr);
				code.Append(result.Code);
				count++;
			}
			code.AppendLine($"print {count}");
			return (Type.Void, code.ToString());
		}


		public override (Type Type, string Code) VisitCodeBlock([NotNull] PLC_Lab8_exprParser.CodeBlockContext context)
        {
            string code = "";
            foreach (var statement in context.statement())
            {
                var result = Visit(statement);
                code += result.Code;
            }
            return (Type.Error, code);
        }

        public override (Type Type, string Code) VisitIfStatement([NotNull] PLC_Lab8_exprParser.IfStatementContext context)
        {
            var conditionResult = Visit(context.condition());
            var thenCode = Visit(context.statement(0)).Code;
            string elseCode = context.statement().Count() > 1 ? Visit(context.statement(1)).Code : "";

            int labelFalse = GetLabel();
            int labelEnd = GetLabel();
            string code = $"{conditionResult.Code}fjmp {labelFalse}\n{thenCode}jmp {labelEnd}\nlabel {labelFalse}\n{elseCode}label {labelEnd}\n";
            return (Type.Error, code);
        }

        public override (Type Type, string Code) VisitWhileStatement([NotNull] PLC_Lab8_exprParser.WhileStatementContext context)
        {
            var conditionResult = Visit(context.condition());
            var bodyCode = Visit(context.statement()).Code;

            int labelStart = GetLabel();
            int labelEnd = GetLabel();
            string code = $"label {labelStart}\n{conditionResult.Code}fjmp {labelEnd}\n{bodyCode}jmp {labelStart}\nlabel {labelEnd}\n";
            return (Type.Error, code);
        }


        public override (Type Type, string Code) VisitTernaryExpression([NotNull] PLC_Lab8_exprParser.TernaryExpressionContext context)
        {
            var condition = Visit(context.expr(0));
            if (condition.Type != Type.Bool)
            {
                Errors.ReportError(context.expr(0).Start, "Ternary condition must be a boolean expression.");
                return (Type.Error, "ERROR: Ternary condition must be boolean");
            }

            var trueBranch = Visit(context.expr(1));
            var falseBranch = Visit(context.expr(2));

            int labelFalse = GetLabel(); 
            int labelEnd = GetLabel();

            if (trueBranch.Type != falseBranch.Type)
            {
                if ((trueBranch.Type == Type.Int && falseBranch.Type == Type.Float) ||
                    (trueBranch.Type == Type.Float && falseBranch.Type == Type.Int))
                {
                    string conversionCode = trueBranch.Type == Type.Int ? "itof\n" : "";
                    string code = $"{condition.Code}fjmp {labelFalse}\n{trueBranch.Code}{conversionCode}jmp {labelEnd}\nlabel {labelFalse}\n{falseBranch.Code}label {labelEnd}\n";
                    return (Type.Float, code);
                }
                Errors.ReportError(context.expr(1).Start, "Types of true and false branches in ternary expression must match or be convertible.");
                return (Type.Error, "ERROR: Type mismatch in ternary branches");
            }

            string finalCode = $"{condition.Code}fjmp {labelFalse}\n{trueBranch.Code}jmp {labelEnd}\nlabel {labelFalse}\n{falseBranch.Code}label {labelEnd}\n";
            return (trueBranch.Type, finalCode);
        }
    }
}
