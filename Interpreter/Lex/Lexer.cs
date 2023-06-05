using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Interpreter.Lex.Literal;

namespace Interpreter.Lex;
public class Lexer
{
    protected static readonly Regex DIGIT = new Regex(@"^\d$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    protected static readonly Regex BIT = new Regex(@"^[01]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    protected static readonly Regex HEX = new Regex(@"^[0-9a-f]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    protected static readonly Regex WHITESPACE = new Regex(@"^\s$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    protected static readonly Regex NEWLINE = new Regex(@"^\n$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    protected static readonly Regex IDSTART = new Regex(@"^[a-z_]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    protected static readonly Regex IDINNER = new Regex(@"^[0-9a-z_]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    protected readonly string _source;
    protected readonly LexerSettings _settings;

    public Lexer(string source, LexerSettings? settings = null)
    {
        _source = source;
        _settings = settings?? new LexerSettings();
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

        yield return new Token(TokenType.EOF, (column, column, line, line));
        yield break;
    }

    protected virtual Token nextToken(ref int current, ref int column, ref int line)
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

            Token error = new Token(TokenType.ERROR, (column, column, line, line), error: new LexError($"Failed to match token: {current}."));
            moveNext(ref current, ref column, out _);
            return error;
        }

        return new Token(TokenType.EOF, (column, column, line, line));
    }

    protected bool tryMatchWhiteSpace(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? wsToken)
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
            wsToken = new Token(TokenType.WHITESPACE, (column_start, column, line_start, line), literal: new WhitespaceLiteral(buffer));
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

    protected bool tryMatchComment(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? commentToken)
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
                            column = nestedCommentToken.Location.ColumnEnd;
                            line = nestedCommentToken.Location.LineEnd;
                            buffer = buffer.Substring(0, childEnd - start);
                        }
                    }

                    commentToken = new Token(TokenType.COMMENT, (column_start, column, line_start, line), literal: new CommentLiteral(buffer, commentType.Value));
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

    protected bool tryMatchSimpleToken(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? simpleToken)
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
                simpleToken = new Token(TokenType.PLUS, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.MINUS:
                simpleToken = new Token(TokenType.MINUS, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.STAR:
                simpleToken = new Token(TokenType.STAR, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.SLASH:
                simpleToken = new Token(TokenType.SLASH, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.PERCENT:
                simpleToken = new Token(TokenType.PERCENT, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.CARET:
                simpleToken = new Token(TokenType.CARET, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.DOT:
                simpleToken = new Token(TokenType.DOT, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.COMMA:
                simpleToken = new Token(TokenType.COMMA, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.SEMICOLON:
                simpleToken = new Token(TokenType.SEMICOLON, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.COLON:
                simpleToken = new Token(TokenType.COLON, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.L_PAREN:
                simpleToken = new Token(TokenType.L_PAREN, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.R_PAREN:
                simpleToken = new Token(TokenType.R_PAREN, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.L_BRACE:
                simpleToken = new Token(TokenType.L_BRACE, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.R_BRACE:
                simpleToken = new Token(TokenType.R_BRACE, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.UNDERSCORE:
                simpleToken = new Token(TokenType.UNDERSCORE, (column_start, column, line_start, line));
                break;
            case TokenTypeValues.BANG:
                if (peekNext(current, out next))
                {
                    switch (next)
                    {
                        case TokenTypeValues.EQUAL:
                            moveNext(ref current, ref column, out _);
                            simpleToken = new Token(TokenType.BANG_EQUAL, (column_start, column, line_start, line));
                            break;
                        default:
                            simpleToken = new Token(TokenType.BANG, (column_start, column, line_start, line));
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
                            simpleToken = new Token(TokenType.EQUAL_EQUAL, (column_start, column, line_start, line));
                            break;
                        default:
                            simpleToken = new Token(TokenType.EQUAL, (column_start, column, line_start, line));
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
                            simpleToken = new Token(TokenType.GREATER_EQUAL, (column_start, column, line_start, line));
                            break;
                        default:
                            simpleToken = new Token(TokenType.GREATER, (column_start, column, line_start, line));
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
                            simpleToken = new Token(TokenType.LESS_EQUAL, (column_start, column, line_start, line));
                            break;
                        default:
                            simpleToken = new Token(TokenType.LESS, (column_start, column, line_start, line));
                            break;
                    }
                }
                break;
            case TokenTypeValues.EOF:
                simpleToken = new Token(TokenType.EOF, (column_start, column, line_start, line));
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

    protected bool tryMatchNumber(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? numberToken)
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
                                buffer += next;
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

            numberToken = new Token(TokenType.NUMBER, (column_start, column, line_start, line), literal: new NumberLiteral(buffer, foundRadixPoint ? NumberType.FLOAT : NumberType.INTEGER, digitBase));
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

    protected bool tryMatchString(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? stringToken)
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

            int index_at_first_line_break = -1;
            int column_at_first_line_break = -1;

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
                    if(NEWLINE.IsMatch(next))
                    {
                        index_at_first_line_break = current;
                        column_at_first_line_break = column;
                        advanceLine(ref column, ref line);
                    }

                    moveNext(ref current, ref column, out _);
                    buffer += next;
                }
            }

            if(!terminated)
            {
                //String must terminate
                //Set location to end of first line and report error
                if(column_at_first_line_break >= 0)
                {
                    current = index_at_first_line_break;
                    line = line_start;
                    column = column_at_first_line_break;
                }

                stringToken = new Token(TokenType.ERROR, (column_start, column_at_first_line_break < 0 ? column : column_at_first_line_break,
                    line_start, column_at_first_line_break < 0 ? line : line + 1), error: new LexError($"Unterminated string literal: {stringType.Value.AsString()}{buffer}."));
            }
            else
            {
                stringToken = new Token(TokenType.STRING, (column_start, column, line_start, line), literal: new StringLiteral(buffer, stringType.Value));
            }
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

    protected bool tryMatchIdentifierOrKeyword(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? idToken)
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
                    TokenType.TRUE or TokenType.FALSE => new Token(TokenType.BOOL, (column_start, column, line_start, line), new BooleanLiteral(matching_keyword.GetSymbol()!)),
                    _ => new Token(matching_keyword, (column_start, column, line_start, line))
                };
            }
            //Id is not a keyword
            else
            {
                idToken = new Token(TokenType.ID, (column_start, column, line_start, line), literal: new IdLiteral(buffer));
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

    protected bool peekNext(int current, [NotNullWhen(true)] out string? s)
    {
        s = null;

        if (!isAtEnd(current))
        {
            s = _source[current].ToString();
            return true;
        }

        return false;
    }

    protected bool moveNext(ref int current, ref int column, [NotNullWhen(true)] out string? s)
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

    protected void advanceLine(ref int column, ref int line)
    {
        column = 0;
        line++;
    }

    protected bool isDigit(string s, NumberBase nbase)
    {
        return nbase switch
        {
            NumberBase.DECIMAL => DIGIT.IsMatch(s),
            NumberBase.BINARY => BIT.IsMatch(s),
            NumberBase.HEX => HEX.IsMatch(s),
            _ => false
        };
    }

    protected bool numberLah(int current, NumberBase nbase)
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

    protected bool isUnderscoreOrDigit(string s, NumberBase nbase) => s.Equals(TokenTypeValues.UNDERSCORE) || isDigit(s, nbase);

    protected bool tryGetQuoteType(string s, [NotNullWhen(true)] out QuoteType? quoteType)
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

    protected bool isAtEnd(int current) => current >= _source.Length;
}