using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2
{
    public enum TokenType
    {
        EOF,
        OP,
        NUM,
        LPAR,
        DIV,
        RPAR,
        SEMICOLON,
        MOD,
        ID
    }
    public class Token
    {
        public TokenType Type;
        public string Value;
        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case TokenType.LPAR:
                case TokenType.RPAR:
                case TokenType.SEMICOLON:
                case TokenType.MOD:
                case TokenType.DIV:
                    return Type.ToString();

                default:
                    return $"{Type}: {Value}";
            }
        }
    }
}
