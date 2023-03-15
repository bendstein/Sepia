using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Interpreter.Lex;
public class Scanner
{
    private readonly string _source = string.Empty;
    private static readonly Regex DIGIT = new Regex(@"^\d$", RegexOptions.Compiled);
    private static readonly Regex BIT = new Regex(@"^[01]$", RegexOptions.Compiled);
    private static readonly Regex HEX = new Regex(@"^[0-9a-f]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public Scanner(string source)
    {
        _source = source;
    }

    public IEnumerator<Token> GetEnumerator()
    {
        int current = 0;
        int line = 1;

        while (current < _source.Length)
        {
            yield return nextToken(ref current, ref line);
        }

        yield return new Token(TokenType.EOF, line: line);
        yield break;
    }

    private Token nextToken(ref int current, ref int line)
    {
        if(tryMatchSimpleToken(ref current, ref line, out Token? simpleToken))
            return simpleToken;
        else if (tryMatchNumber(ref current, ref line, out Token? numberToken))
            return numberToken;

        //TODO
        return new Token(TokenType.EOF, line: line);
    }

    private bool tryMatchSimpleToken(ref int current, ref int line, [NotNullWhen(true)] out Token? simpleToken)
    {
        int start = current;
        int line_start = line;

        simpleToken = null;

        string buffer = _source[current++].ToString();

        switch (buffer)
        {
            case TokenTypeValues.PLUS:
                simpleToken = new Token(TokenType.PLUS, line: line);
                break;
            case TokenTypeValues.MINUS:
                simpleToken = new Token(TokenType.MINUS, line: line);
                break;
            case TokenTypeValues.STAR:
                simpleToken = new Token(TokenType.STAR, line: line);
                break;
            case TokenTypeValues.SLASH:
                simpleToken = new Token(TokenType.SLASH, line: line);
                break;
            case TokenTypeValues.PERCENT:
                simpleToken = new Token(TokenType.PERCENT, line: line);
                break;
            case TokenTypeValues.CARET:
                simpleToken = new Token(TokenType.CARET, line: line);
                break;
            case TokenTypeValues.DOT:
                simpleToken = new Token(TokenType.DOT, line: line);
                break;
            case TokenTypeValues.COMMA:
                simpleToken = new Token(TokenType.COMMA, line: line);
                break;
            case TokenTypeValues.SEMICOLON:
                simpleToken = new Token(TokenType.SEMICOLON, line: line);
                break;
            case TokenTypeValues.COLON:
                simpleToken = new Token(TokenType.COLON, line: line);
                break;
            case TokenTypeValues.L_PAREN:
                simpleToken = new Token(TokenType.L_PAREN, line: line);
                break;
            case TokenTypeValues.R_PAREN:
                simpleToken = new Token(TokenType.R_PAREN, line: line);
                break;
            case TokenTypeValues.L_BRACE:
                simpleToken = new Token(TokenType.L_BRACE, line: line);
                break;
            case TokenTypeValues.R_BRACE:
                simpleToken = new Token(TokenType.R_BRACE, line: line);
                break;
            case TokenTypeValues.UNDERSCORE:
                simpleToken = new Token(TokenType.UNDERSCORE, line: line);
                break;
            case TokenTypeValues.BANG:
                switch (_source[current].ToString())
                {
                    case TokenTypeValues.EQUAL:
                        current++;
                        simpleToken = new Token(TokenType.BANG_EQUAL, line: line);
                        break;
                    default:
                        simpleToken = new Token(TokenType.BANG, line: line);
                        break;
                }
                break;
            case TokenTypeValues.EQUAL:
                switch (_source[current].ToString())
                {
                    case TokenTypeValues.EQUAL:
                        current++;
                        simpleToken = new Token(TokenType.EQUAL_EQUAL, line: line);
                        break;
                    default:
                        simpleToken = new Token(TokenType.EQUAL, line: line);
                        break;
                }
                break;
            case TokenTypeValues.GREATER:
                switch (_source[current].ToString())
                {
                    case TokenTypeValues.EQUAL:
                        current++;
                        simpleToken = new Token(TokenType.GREATER_EQUAL, line: line);
                        break;
                    default:
                        simpleToken = new Token(TokenType.GREATER, line: line);
                        break;
                }
                break;
            case TokenTypeValues.LESS:
                switch (_source[current].ToString())
                {
                    case TokenTypeValues.EQUAL:
                        current++;
                        simpleToken = new Token(TokenType.LESS_EQUAL, line: line);
                        break;
                    default:
                        simpleToken = new Token(TokenType.LESS, line: line);
                        break;
                }
                break;
            case TokenTypeValues.EOF:
                simpleToken = new Token(TokenType.EOF, line: line);
                break;
        }

        if (simpleToken == null)
        {
            current = start;
            line = line_start;
            return false;
        }

        return true;
    }

    private bool tryMatchNumber(ref int current, ref int line, [NotNullWhen(true)] out Token? numberToken)
    {
        int start = current;
        int line_start = line;

        numberToken = null;

        string buffer = _source[current++].ToString();

        if (DIGIT.IsMatch(buffer))
        {
            //The base of the number
            NumberBase digitBase = NumberBase.DECIMAL;

            //Whether a decimal point has been seen yet
            bool foundRadixPoint = false;

            //If true, done reading number
            bool done = false;

            if (current < _source.Length) 
            {
                string next = _source[current].ToString();

                //If second symbol isn't a number, check if it's clarifying the number's base or a radix point
                if (!DIGIT.IsMatch(next))
                {
                    switch(next)
                    {
                        case TokenTypeValues.DOT:
                            foundRadixPoint = true;
                            buffer += next;
                            current++;
                            break;
                        default:
                            string temp = buffer + next;

                            switch(temp)
                            {
                                case TokenTypeValues.ZERO_B:
                                    digitBase = NumberBase.BINARY;
                                    buffer = temp;
                                    current++;
                                    break;
                                case TokenTypeValues.ZERO_X:
                                    digitBase = NumberBase.HEX;
                                    buffer = temp;
                                    current++;
                                    break;
                                default:
                                    done = true;
                                    break;
                            }
                            break;
                    }
                }

                //Whether a digit has been seen before the decimal point
                //In base decimal, there isn't a prefix, so the buffer already contains a digit
                bool atLeastOneDigitBeforeRadix = digitBase == NumberBase.DECIMAL;

                //Whether a digit has been seen after the decimal point
                bool atLeastOneDigitAfterRadix = false;

                //Keep consuming characters until the lexeme is built
                while (!done && current < _source.Length)
                {
                    string next_char = _source[current].ToString();

                    //Check if {next_digit} matches the pattern for a digit/bit/hex digit in the given base
                    bool is_digit = false;

                    switch (digitBase)
                    {
                        //If base 10, match 0-9
                        case NumberBase.DECIMAL:
                            if (DIGIT.IsMatch(next_char))
                                is_digit = true;
                            break;
                        //If base 2, match 0-1
                        case NumberBase.BINARY:
                            if (BIT.IsMatch(next_char))
                                is_digit = true;
                            break;
                        //If base 16, match 0-9a-f
                        case NumberBase.HEX:
                            if (HEX.IsMatch(next_char))
                                is_digit = true;
                            break;
                    }

                    //If valid digit for base, record its position wrt the decimal point
                    //And continue
                    if (is_digit)
                    {
                        if (foundRadixPoint) atLeastOneDigitAfterRadix = true;
                        else atLeastOneDigitBeforeRadix = true;

                        buffer += next_char;
                        current++;
                    }
                    //If {next digit} isn't a number, check if it's a decimal point
                    else
                    {
                        switch (next_char)
                        {
                            case TokenTypeValues.DOT:
                                if (!atLeastOneDigitBeforeRadix)
                                    throw new Exception("Cannot have a number literal without a leading digit.");
                                else if (foundRadixPoint)
                                    throw new Exception("Cannot have multiple decimal points in a number literal.");
                                else if (digitBase != NumberBase.DECIMAL)
                                    throw new Exception("Decimal points are only allowed on base 10 number literals.");

                                foundRadixPoint = true;
                                current++;
                                break;
                            default:
                                done = true;
                                break;
                        }
                    }
                }

                //If a radix point was seen, but not digits were found after
                if (foundRadixPoint && !atLeastOneDigitAfterRadix)
                    throw new Exception("Cannot terminate a number literal with a decimal point.");
                else if (!atLeastOneDigitBeforeRadix)
                    throw new Exception("Cannot have empty number literal.");
            }

            numberToken = new Token(TokenType.NUMBER, new NumberLiteral(buffer, foundRadixPoint? NumberType.FLOAT : NumberType.INTEGER, digitBase), line);
        }

        if(numberToken == null)
        {
            current = start;
            line = line_start;
            return false;
        }

        return true;
    }
}