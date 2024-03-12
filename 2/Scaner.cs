using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2
{
    public class Scaner
    {
        private StreamReader file;
        private int currentChar;
        public Scaner(StreamReader file)
        {
            this.file = file;
            this.currentChar = file.Read();
        }

        public Token NextToken()
        {
            while (char.IsWhiteSpace((char)currentChar) || currentChar == '/')
            {
                if (currentChar == '/')
                {
                    NextChar();
                    if (currentChar == '/')
                    {
                        while (currentChar != '\n' && currentChar != -1)
                        {
                            NextChar();
                        }
                    }
                }
                NextChar();
            }

            if (currentChar == -1)
            {
                return new Token(TokenType.EOF, "");
            }

            if (char.IsLetter((char)currentChar))
            {
                StringBuilder identifier = new StringBuilder();
                while (char.IsLetterOrDigit((char)currentChar))
                {
                    identifier.Append((char)currentChar);
                    NextChar();
                }

                string value = identifier.ToString();
                TokenType type = value switch
                {
                    "div" => TokenType.DIV,
                    "mod" => TokenType.MOD,
                    _ => TokenType.ID,
                };

                return new Token(type, value);
            }

            if (char.IsDigit((char)currentChar))
            {
                StringBuilder number = new StringBuilder();
                while (char.IsDigit((char)currentChar))
                {
                    number.Append((char)currentChar);
                    NextChar();
                }

                return new Token(TokenType.NUM, number.ToString());
            }
            Token? token = currentChar switch
            {
                '+' or '-' or '*' or '/' => new Token(TokenType.OP, ((char)currentChar).ToString()),
                '(' => new Token(TokenType.LPAR, "("),
                ')' => new Token(TokenType.RPAR, ")"),
                ';' => new Token(TokenType.SEMICOLON, ";"),
                _ => null
            };
            NextChar();

            return token;
        }

        private void NextChar()
        {
            currentChar = file.Read();
        }
    }
}
