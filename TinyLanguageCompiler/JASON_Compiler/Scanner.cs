using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System;

public enum TokenClass
{
    DATATYPEINT, DATATYPEFLOAT, DATATYPESTRING, IF, ELSE, ELSEIF, UNTIL, READ, RETURN, WRITE, REPEAT,
    ENDLINE, END, ASSIGNMENTOPERATOR, IDENTIFIER, SEMICOLON, NUMBER, THEN, PLUSOPERATOR, MINUSOPERATOR,
    STRING, EQUALTO, MULTIPLICATIONOPERATOR, LESSTHAN, DIVISIONOPERATOR, GREATERTHAN, LEFTCURLYBRACKETS,
    RIGHTCURLYBRACKETS, LEFTPARENTHESES, RIGHTPARENTHESES, COMMA, NOTEQUAL, AND, OR, MAIN
}

namespace JASON_Compiler
{


    public class Token
    {
        public string lex;
        public TokenClass token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, TokenClass> ReservedWords = new Dictionary<string, TokenClass>();
        Dictionary<string, TokenClass> Operators = new Dictionary<string, TokenClass>();

        public Scanner()
        {
            ReservedWords.Add("int", TokenClass.DATATYPEINT);
            ReservedWords.Add("float", TokenClass.DATATYPEFLOAT);
            ReservedWords.Add("string", TokenClass.DATATYPESTRING);
            ReservedWords.Add("if", TokenClass.IF);
            ReservedWords.Add("elseif", TokenClass.ELSEIF);
            ReservedWords.Add("else", TokenClass.ELSE);
            ReservedWords.Add("until", TokenClass.UNTIL);
            ReservedWords.Add("read", TokenClass.READ);
            ReservedWords.Add("return", TokenClass.RETURN);
            ReservedWords.Add("write", TokenClass.WRITE);
            ReservedWords.Add("repeat", TokenClass.REPEAT);
            ReservedWords.Add("endl", TokenClass.ENDLINE);
            ReservedWords.Add("end", TokenClass.END);
            ReservedWords.Add("then", TokenClass.THEN);
            ReservedWords.Add("main", TokenClass.MAIN);

            Operators.Add(";", TokenClass.SEMICOLON);
            Operators.Add("+", TokenClass.PLUSOPERATOR);
            Operators.Add("-", TokenClass.MINUSOPERATOR);
            Operators.Add("*", TokenClass.MULTIPLICATIONOPERATOR);
            Operators.Add("/", TokenClass.DIVISIONOPERATOR);
            Operators.Add(":=", TokenClass.ASSIGNMENTOPERATOR);
            Operators.Add("=", TokenClass.EQUALTO);
            Operators.Add("<>", TokenClass.NOTEQUAL);
            Operators.Add("<", TokenClass.LESSTHAN);
            Operators.Add(">", TokenClass.GREATERTHAN);
            Operators.Add("(", TokenClass.LEFTPARENTHESES);
            Operators.Add(")", TokenClass.RIGHTPARENTHESES);
            Operators.Add("{", TokenClass.LEFTCURLYBRACKETS);
            Operators.Add("}", TokenClass.RIGHTCURLYBRACKETS);
            Operators.Add(",", TokenClass.COMMA);
            Operators.Add("&&", TokenClass.AND);
            Operators.Add("||", TokenClass.OR);
        }

        public void StartScanning(string SourceCode)
        {
            for (int i = 0; i < SourceCode.Length; i++)
            {
                int j = i;
                string CurrentLexeme = SourceCode[j++].ToString();

                if (SourceCode[i] == ' ' || SourceCode[i] == '\r' || SourceCode[i] == '\n' || SourceCode[i] == '\t')
                    continue;

                if (SourceCode[i] == '/' && SourceCode[i + 1] == '*')
                {
                    while (j < SourceCode.Length && !CurrentLexeme.EndsWith("*/"))
                        CurrentLexeme += SourceCode[j++];
                    i = j - 1;
                }
                else if (SourceCode[i] == '"')
                {
                    while (j < SourceCode.Length)
                    {
                        CurrentLexeme += SourceCode[j];
                        if (SourceCode[j] == '"')
                            break;
                        j++;
                    }
                    i = j;
                }
                else if (((SourceCode[i] >= 'a' && SourceCode[i] <= 'z') || (SourceCode[i] >= 'A' && SourceCode[i] <= 'Z')) || ((SourceCode[i] >= '0' && SourceCode[i] <= '9') || SourceCode[i] == '.'))
                {
                    while (j < SourceCode.Length && (((SourceCode[j] >= 'a' && SourceCode[j] <= 'z') || (SourceCode[j] >= 'A' && SourceCode[j] <= 'Z')) || ((SourceCode[j] >= '0' && SourceCode[j] <= '9')) || SourceCode[j] == '.'))
                        CurrentLexeme += SourceCode[j++];
                    i = j - 1;
                }
                else
                {
                    if ((SourceCode[i] == ':' && SourceCode[j] == '=')
                        || (SourceCode[i] == '&' && SourceCode[j] == '&')
                        || (SourceCode[i] == '|' && SourceCode[j] == '|')
                        || (SourceCode[i] == '<' && SourceCode[j] == '>')
                        || (SourceCode[i] == '>' && SourceCode[j] == '<')
                        || (SourceCode[i] == '<' && SourceCode[j] == '=')
                        || (SourceCode[i] == '>' && SourceCode[j] == '='))
                        CurrentLexeme += SourceCode[j++];
                    i = j - 1;
                }
                FindTokenClass(CurrentLexeme);
            }
            JASON_Compiler.TokenStream = Tokens;
        }

        void FindTokenClass(string Lex)
        {
            TokenClass TC;
            Token Tok = new Token();
            Tok.lex = Lex;
            //Is it a reserved word?
            if (ReservedWords.Keys.Contains(Lex))
            {
                Tok.token_type = ReservedWords[Lex];
                Tokens.Add(Tok);
            }
            //Is it an operator?
            else if (Operators.Keys.Contains(Lex))
            {
                Tok.token_type = Operators[Lex];
                Tokens.Add(Tok);
            }
            //Is it a Comment?
            else if (Lex.StartsWith("/*") && Lex.EndsWith("*/"))
            {
                //Do Nothing  
            }
            //Is it a String?
            else if (Lex.StartsWith("\"") && Lex.EndsWith("\""))
            {
                Tok.token_type = TokenClass.STRING;
                Tokens.Add(Tok);
            }
            //Is it an Invalid  Comment?
            else if ((Lex.StartsWith("/*") && !Lex.EndsWith("*/")))
                Errors.Error_List.Add("Invalid Comment : " + Lex);
            //Is it an Invalic String?
            else if (Lex.StartsWith("\"") && !Lex.EndsWith("\""))
                Errors.Error_List.Add("Invalid String : " + Lex);
            //Is it an identifier?
            else if (isIdentifier(Lex))
            {
                if (Lex[0] >= '0' && Lex[0] <= '9')
                    Errors.Error_List.Add("Invalid Identifier : " + Lex);
                else
                {
                    Tok.token_type = TokenClass.IDENTIFIER;
                    Tokens.Add(Tok);
                }
            }
            //Is it a Number Or Not?
            else if (isNumber(Lex))
            {
                if (Lex.Count(t => t == '.') <= 1 && !Lex.StartsWith(".") && !Lex.EndsWith("."))
                {
                    Tok.token_type = TokenClass.NUMBER;
                    Tokens.Add(Tok);
                }
                else
                    Errors.Error_List.Add("Invalid Number : " + Lex);
            }
            //Is it an undefined?
            else
                Errors.Error_List.Add("ERROR : " + Lex);
        }

        bool isIdentifier(string lex)
        {
            // Check if the lex is an identifier or not.
            var Idetifier = new Regex("([a-z]|[A-Z])([a-z]|[A-Z]|[0-9])*", RegexOptions.Compiled);
            return Idetifier.IsMatch(lex);
        }
        bool isNumber(string lex)
        {
            // Check if the lex is a constant (Number) or not.
            var Constant = new Regex("[0-9]+(.[0-9]+)?", RegexOptions.Compiled);
            return Constant.IsMatch(lex);
        }
    }
}