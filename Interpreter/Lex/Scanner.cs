using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Interpreter.Lex.Literal;

namespace Interpreter.Lex;
public class Scanner
{
    private readonly string _source = string.Empty;
    private static readonly Regex DIGIT = new Regex(@"^\d$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BIT = new Regex(@"^[01]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HEX = new Regex(@"^[0-9a-f]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex WHITESPACE = new Regex(@"^\s$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NEWLINE = new Regex(@"^\n$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IDSTART = new Regex(@"^[a-z_]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IDINNER = new Regex(@"^[0-9a-z_]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public Scanner(string source)
    {
        _source = source;
    }

    public IEnumerable<Token> Scan()
    {
        List<Token> tokens = new();
        foreach (var token in this)
            tokens.Add(token);
        return tokens;
    }

    public IEnumerator<Token> GetEnumerator()
    {
        int current = 0;
        int column = 0;
        int line = 1;

        while (!isAtEnd(current))
        {
            Token next = nextToken(ref current, ref column, ref line);

            yield return next;
        }

        yield return new Token(TokenType.EOF, col_start: column, col_end: column, line_start: line, line_end: line);
        yield break;
    }

    private Token nextToken(ref int current, ref int column, ref int line)
    {
        if (!isAtEnd(current))
        {
            if (tryMatchWhiteSpace(ref current, ref column, ref line, out Token? wsToken))
                return wsToken;
            else if (tryMatchComment(ref current, ref column, ref line, out Token? commentToken))
                return commentToken;
            else if (tryMatchString(ref current, ref column, ref line, out Token? stringToken))
                return stringToken;
            else if (tryMatchIdentifierOrKeyword(ref current, ref column, ref line, out Token? idToken))
                return idToken;
            else if (tryMatchNumber(ref current, ref column, ref line, out Token? numberToken))
                return numberToken;
            else if (tryMatchSimpleToken(ref current, ref column, ref line, out Token? simpleToken))
                return simpleToken;
        }

        return new Token(TokenType.EOF, col_start: column, col_end: column, line_start: line, line_end: line);
    }

    private bool tryMatchWhiteSpace(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? wsToken)
    {
        int start = current;
        int column_start = column;
        int line_start = line;

        wsToken = null;

        string? next;
        string buffer = string.Empty;

        while (peekNext(current, out next) && WHITESPACE.IsMatch(next))
        {
            moveNext(ref current, ref column, out _);

            if (NEWLINE.IsMatch(next))
            {
                advanceLine(ref column, ref line);
            }

            buffer += next;
        }

        if (buffer.Length > 0)
        {
            wsToken = new Token(TokenType.WHITESPACE, literal: buffer, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
        }

        if (wsToken == null)
        {
            current = start;
            line = line_start;
            column = column_start;
            return false;
        }

        return true;
    }

    private bool tryMatchComment(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? commentToken)
    {
        int start = current;
        int column_start = column;
        int line_start = line;

        commentToken = null;

        string? next;
        if (!peekNext(current, out next)) return false;

        if(next.Equals(TokenTypeValues.SLASH))
        {
            moveNext(ref current, ref column, out _);
            string buffer = next;

            if(peekNext(current, out next))
            {
                CommentType? commentType = null;

                switch (next)
                {
                    case TokenTypeValues.SLASH:
                        commentType = CommentType.Line;
                        break;
                    case TokenTypeValues.STAR:
                        commentType = CommentType.Block;
                        break;
                }

                if(commentType != null)
                {
                    moveNext(ref current, ref column, out _);
                    buffer += next;

                    bool foundEnd = false;
                    Token? nestedCommentToken = null;
                    int childEnd = -1;

                    while (!foundEnd && peekNext(current, out next))
                    {
                        if(NEWLINE.IsMatch(next))
                        {
                            switch(commentType)
                            {
                                case CommentType.Block:
                                    moveNext(ref current, ref column, out _);
                                    buffer += next;
                                    advanceLine(ref column, ref line);
                                    break;
                                //Line comment ends at new line
                                case CommentType.Line:
                                default:
                                    foundEnd = true;
                                    break;
                            }
                        }
                        else
                        {
                            //If block comment, and next symbols are /*, recursively handle subcomment
                            if (commentType == CommentType.Block && next.Equals(TokenTypeValues.SLASH)
                                && peekNext(current + 1, out string? following) && following.Equals(TokenTypeValues.STAR)
                                && tryMatchComment(ref current, ref column, ref line, out nestedCommentToken))
                            {
                                if (nestedCommentToken.Literal != null && nestedCommentToken.Literal is CommentLiteral comment)
                                {
                                    childEnd = current;
                                    buffer += comment.Value;
                                }
                            }
                            else
                            {
                                moveNext(ref current, ref column, out _);
                                buffer += next;

                                //Check if at end of block comment
                                if (commentType == CommentType.Block && next.Equals(TokenTypeValues.STAR) && peekNext(current, out next) && next.Equals(TokenTypeValues.SLASH))
                                {
                                    moveNext(ref current, ref column, out _);
                                    buffer += next;
                                    foundEnd = true;
                                    break;
                                }
                            }
                        }
                    }

                    //If the end of the block comment wasn't matched,
                    //return error, unless there is a nested block comment,
                    //in which case the end of the subcomment can be used
                    if(commentType == CommentType.Block && !foundEnd)
                    {
                        if(nestedCommentToken == null)
                        {
                            throw new Exception("Unterminated comment.");
                        }
                        else
                        {
                            current = childEnd;
                            column = nestedCommentToken.ColumnEnd;
                            line = nestedCommentToken.LineEnd;
                            buffer = buffer.Substring(0, childEnd - start);
                        }
                    }

                    commentToken = new Token(TokenType.COMMENT, literal: new CommentLiteral(buffer, commentType.Value),
                        col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                }
            }
        }

        if (commentToken == null)
        {
            current = start;
            line = line_start;
            column = column_start;
            return false;
        }

        return true;
    }

    private bool tryMatchSimpleToken(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? simpleToken)
    {
        int start = current;
        int line_start = line;
        int column_start = column;

        simpleToken = null;

        string? next;

        if (!moveNext(ref current, ref column, out next)) return false;

        string buffer = next;

        switch (buffer)
        {
            case TokenTypeValues.PLUS:
                simpleToken = new Token(TokenType.PLUS, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.MINUS:
                simpleToken = new Token(TokenType.MINUS, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.STAR:
                simpleToken = new Token(TokenType.STAR, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.SLASH:
                simpleToken = new Token(TokenType.SLASH, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.PERCENT:
                simpleToken = new Token(TokenType.PERCENT, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.CARET:
                simpleToken = new Token(TokenType.CARET, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.DOT:
                simpleToken = new Token(TokenType.DOT, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.COMMA:
                simpleToken = new Token(TokenType.COMMA, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.SEMICOLON:
                simpleToken = new Token(TokenType.SEMICOLON, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.COLON:
                simpleToken = new Token(TokenType.COLON, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.L_PAREN:
                simpleToken = new Token(TokenType.L_PAREN, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.R_PAREN:
                simpleToken = new Token(TokenType.R_PAREN, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.L_BRACE:
                simpleToken = new Token(TokenType.L_BRACE, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.R_BRACE:
                simpleToken = new Token(TokenType.R_BRACE, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.UNDERSCORE:
                simpleToken = new Token(TokenType.UNDERSCORE, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
            case TokenTypeValues.BANG:
                if (peekNext(current, out next))
                {
                    switch (next)
                    {
                        case TokenTypeValues.EQUAL:
                            moveNext(ref current, ref column, out _);
                            simpleToken = new Token(TokenType.BANG_EQUAL, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                            break;
                        default:
                            moveNext(ref current, ref column, out _);
                            simpleToken = new Token(TokenType.BANG, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                            break;
                    }
                }
                break;
            case TokenTypeValues.EQUAL:
                if (peekNext(current, out next))
                {
                    switch (next)
                    {
                        case TokenTypeValues.EQUAL:
                            moveNext(ref current, ref column, out _);
                            simpleToken = new Token(TokenType.EQUAL_EQUAL, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                            break;
                        default:
                            moveNext(ref current, ref column, out _);
                            simpleToken = new Token(TokenType.EQUAL, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                            break;
                    }
                }
                break;
            case TokenTypeValues.GREATER:
                if (peekNext(current, out next))
                {
                    switch (next)
                    {
                        case TokenTypeValues.EQUAL:
                            moveNext(ref current, ref column, out _);
                            simpleToken = new Token(TokenType.GREATER_EQUAL, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                            break;
                        default:
                            moveNext(ref current, ref column, out _);
                            simpleToken = new Token(TokenType.GREATER, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                            break;
                    }
                }
                break;
            case TokenTypeValues.LESS:
                if (peekNext(current, out next))
                {
                    switch (next)
                    {
                        case TokenTypeValues.EQUAL:
                            moveNext(ref current, ref column, out _);
                            simpleToken = new Token(TokenType.LESS_EQUAL, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                            break;
                        default:
                            moveNext(ref current, ref column, out _);
                            simpleToken = new Token(TokenType.LESS, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                            break;
                    }
                }
                break;
            case TokenTypeValues.EOF:
                simpleToken = new Token(TokenType.EOF, col_start: column_start, col_end: column, line_start: line_start, line_end: line);
                break;
        }

        if (simpleToken == null)
        {
            current = start;
            line = line_start;
            column = column_start;
            return false;
        }

        return true;
    }

    private bool tryMatchNumber(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? numberToken)
    {
        int start = current;
        int line_start = line;
        int column_start = column;

        numberToken = null;

        string? next;

        if (!moveNext(ref current, ref column, out next)) return false;

        string buffer = next;

        //First character is a digit; this is a number
        if (DIGIT.IsMatch(buffer))
        {
            //The base of the number
            NumberBase digitBase = NumberBase.DECIMAL;

            //Whether a decimal point has been seen yet
            bool foundRadixPoint = false;

            //If true, done reading number
            bool done = false;

            if (peekNext(current, out next))
            {
                //If second symbol isn't a number, check if it's clarifying the number's base, an underscore, or a radix point
                if (!DIGIT.IsMatch(next))
                {
                    switch (next)
                    {
                        //Radix point
                        case TokenTypeValues.DOT:
                            //If next character is not a digit or underscore, no longer matches
                            //a number; done
                            if (!peekNext(current + 1, out string? following) || !isUnderscoreOrDigit(following, digitBase))
                            {
                                done = true;
                                break;
                            }

                            moveNext(ref current, ref column, out _);
                            buffer += next;
                            foundRadixPoint = true;
                            break;
                        //Underscore; visual seperator
                        case TokenTypeValues.UNDERSCORE:
                            //Check if following characters can be part of a number in base 10
                            //(If underscore is second character, there isn't a base prefix)
                            //If so, then underscore is allowed
                            if (numberLah(current + 1, NumberBase.DECIMAL))
                            {
                                moveNext(ref current, ref column, out _);
                                //Don't include underscore in token
                            }
                            //Otherwise, the underscore isn't used as a visual seperator
                            else
                            {
                                done = true;
                            }
                            break;
                        default:

                            //If previous symbol was 0 and current symbol is b or x
                            if(buffer.Equals(TokenTypeValues.ZERO))
                            {
                                NumberBase predictedBase = NumberBase.DECIMAL;
                                switch (next)
                                {
                                    case TokenTypeValues.B:
                                        predictedBase = NumberBase.BINARY;
                                        break;
                                    case TokenTypeValues.X:
                                        predictedBase = NumberBase.HEX;
                                        break;
                                    //Isn't a valid base specifier. Done.
                                    default:
                                        done = true;
                                        break;
                                }

                                //Check if following characters can be part of a number in the given base.
                                //If so, then this is a base prefix
                                if(numberLah(current + 1, predictedBase))
                                {
                                    digitBase = predictedBase;
                                    moveNext(ref current, ref column, out _);
                                    //Don't include prefix in token
                                    buffer = string.Empty;
                                }
                                else
                                {
                                    done = true;
                                }
                            }
                            break;
                    }
                }

                //Keep consuming characters until the lexeme is built
                while (!done && peekNext(current, out next))
                {
                    //If valid digit for base, record its position wrt the decimal point
                    //And continue
                    if (isDigit(next, digitBase))
                    {
                        moveNext(ref current, ref column, out _);
                        buffer += next;
                    }
                    //If {next digit} isn't a number, check if it's a decimal point or underscore
                    else
                    {
                        switch (next)
                        {
                            case TokenTypeValues.DOT:
                                //A radix point has been seen, or base is not 10
                                //No longer matches number; done
                                if (foundRadixPoint || digitBase != NumberBase.DECIMAL)
                                {
                                    done = true;
                                    break;
                                }

                                //If next character is not a digit or underscore, no longer matches
                                //a number; done
                                if (!peekNext(current + 1, out string? following) || !isUnderscoreOrDigit(following, digitBase))
                                {
                                    done = true;
                                    break;
                                }

                                moveNext(ref current, ref column, out _);
                                foundRadixPoint = true;
                                break;
                            case TokenTypeValues.UNDERSCORE:
                                //Check if following characters can be part of a number in the given base.
                                //If so, then this underscore is allowed in a number
                                if (numberLah(current + 1, digitBase))
                                {
                                    moveNext(ref current, ref column, out _);
                                    //Don't include underscore in token
                                }
                                //This underscore isn't used as a visual seperator
                                else
                                {
                                    done = true;
                                }
                                break;
                            default:
                                done = true;
                                break;
                        }
                    }
                }
            }

            numberToken = new Token(TokenType.NUMBER, literal: new NumberLiteral(buffer, foundRadixPoint ? NumberType.FLOAT : NumberType.INTEGER, digitBase),
                col_start: column_start, col_end: column, line_start: line_start, line_end: line);
        }

        if (numberToken == null)
        {
            current = start;
            line = line_start;
            column = column_start;
            return false;
        }

        return true;
    }

    private bool tryMatchString(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? stringToken)
    {
        int start = current;
        int column_start = column;
        int line_start = line;

        stringToken = null;

        string? next;
        if (!peekNext(current, out next)) return false;

        if(tryGetQuoteType(next, out QuoteType? stringType))
        {
            //Advance, but don't append start quote to string
            moveNext(ref current, ref column, out _);
            string buffer = string.Empty;
            bool terminated = false;

            while (!terminated && peekNext(current, out next))
            {
                if (tryGetQuoteType(next, out QuoteType? terminatingStringType) && stringType == terminatingStringType)
                {
                    //Advance, but don't append end quote to string
                    moveNext(ref current, ref column, out _);
                    terminated = true;
                }
                else
                {
                    moveNext(ref current, ref column, out _);
                    buffer += next;
                }
            }

            if(!terminated)
            {
                throw new Exception("Unterminated string literal.");
            }

            stringToken = new Token(TokenType.STRING, literal: new StringLiteral(buffer, stringType.Value), col_start: column_start, col_end: column, line_start: line_start, line_end: line);
        }

        if (stringToken == null)
        {
            current = start;
            line = line_start;
            column = column_start;
            return false;
        }

        return true;
    }

    private bool tryMatchIdentifierOrKeyword(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? idToken)
    {
        int start = current;
        int line_start = line;
        int column_start = column;

        idToken = null;

        string? next;

        if (!moveNext(ref current, ref column, out next)) return false;

        if(IDSTART.IsMatch(next))
        {
            string buffer = next;

            while(peekNext(current, out next))
            {
                if(IDINNER.IsMatch(next))
                {
                    moveNext(ref current, ref column, out _);
                    buffer += next;
                }
                else
                {
                    break;
                }
            }

            //Check if this id is a keyword
            var matching_keywords = TokenTypeValues.KEYWORD_TOKENS
                .Where(t => t.TryGetSymbol(out string? symbol) && buffer.Equals(symbol));

            //Id is a keyword, return a keyword token instead of an id
            if(matching_keywords.Any())
            {
                var matching_keyword = matching_keywords.First();

                idToken = matching_keyword switch
                {
                    //If true/false, return boolean literal token instead
                    TokenType.TRUE or TokenType.FALSE => new Token(TokenType.BOOL, new BooleanLiteral(matching_keyword.GetSymbol()!), col_start: column_start, col_end: column, line_start: line_start, line_end: line),
                    _ => new Token(matching_keyword, col_start: column_start, col_end: column, line_start: line_start, line_end: line)
                };
            }
            //Id is not a keyword
            else
            {
                idToken = new Token(TokenType.ID, literal: new IdLiteral(buffer), col_start: column_start, col_end: column, line_start: line_start, line_end: line);
            }
        }

        if (idToken == null)
        {
            current = start;
            line = line_start;
            column = column_start;
            return false;
        }

        return true;
    }

    private bool peekNext(int current, [NotNullWhen(true)] out string? s)
    {
        s = null;

        if (!isAtEnd(current))
        {
            s = _source[current].ToString();
            return true;
        }

        return false;
    }

    private bool moveNext(ref int current, ref int column, [NotNullWhen(true)] out string? s)
    {
        s = null;

        if (!isAtEnd(current))
        {
            s = _source[current++].ToString();
            column += s.Length;
            return true;
        }

        return false;
    }

    private void advanceLine(ref int column, ref int line)
    {
        column = 0;
        line++;
    }

    private bool isDigit(string s, NumberBase nbase)
    {
        return nbase switch
        {
            NumberBase.DECIMAL => DIGIT.IsMatch(s),
            NumberBase.BINARY => BIT.IsMatch(s),
            NumberBase.HEX => HEX.IsMatch(s),
            _ => false
        };
    }

    private bool numberLah(int current, NumberBase nbase)
    {
        for (int lah = 0; peekNext(current + lah, out string? following_digit); lah++)
        {
            //If a digit is found afterward, this can be part of a number
            if (isDigit(following_digit, nbase))
                return true;
            //If underscore is after, it might be part of a number, but it might
            //be an identifier with a missing leading space, keep going until a digit is found
            else if (following_digit.Equals(TokenTypeValues.UNDERSCORE))
            {

            }
            //Not part of a number
            else
                return false;
        }

        return false;
    }

    private bool isUnderscoreOrDigit(string s, NumberBase nbase) => s.Equals(TokenTypeValues.UNDERSCORE) || isDigit(s, nbase);

    private bool tryGetQuoteType(string s, [NotNullWhen(true)] out QuoteType? quoteType)
    {
        quoteType = null;

        if (string.IsNullOrWhiteSpace(s)) return false;

        switch (s)
        {
            case TokenTypeValues.D_QUOTE:
                quoteType = QuoteType.D_QUOTE;
                break;
            case TokenTypeValues.S_QUOTE:
                quoteType = QuoteType.S_QUOTE;
                break;
            case TokenTypeValues.BACKTICK:
                quoteType = QuoteType.BACKTICK;
                break;
        }

        return quoteType != null;
    }

    private bool isAtEnd(int current) => current >= _source.Length;
}