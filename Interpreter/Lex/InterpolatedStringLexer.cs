using Interpreter.Lex.Literal;
using System.Diagnostics.CodeAnalysis;

namespace Interpreter.Lex;
public class InterpolatedStringLexer : Lexer
{
    public InterpolatedStringLexer(string source, LexerSettings? settings = null) : base(source, settings)
    {
    }

    protected override Token nextToken(ref int current, ref int column, ref int line)
    {
        if (!isAtEnd(current))
        {
            if (tryMatchInterpolatedExpression(ref current, ref column, ref line, out Token? interpolatedToken))
                return interpolatedToken;
            else if (tryMatchStringLiteral(ref current, ref column, ref line, out Token? stringToken))
                return stringToken;

            Token error = new Token(TokenType.ERROR, (column, column, line, line), error: new LexError($"Failed to match token: {_source[current]}."));
            moveNext(ref current, ref column, out _);
            return error;
        }

        return new Token(TokenType.EOF, (column, column, line, line));
    }

    protected bool tryMatchStringLiteral(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? stringToken)
    {
        int start = current;
        int column_start = column;
        int line_start = line;
        stringToken = null;
        string buffer = string.Empty;

        while(peekNext(current, out string? next))
        {
            if(next == TokenTypeValues.L_BRACE)
            {
                //End of string literal; left curly brace indicates start of interpolated part
                break;
            }
            else
            {
                if (NEWLINE.IsMatch(next))
                {
                    advanceLine(ref column, ref line);
                }

                moveNext(ref current, ref column, out _);
                buffer += next;
            }
        }

        //if (string.IsNullOrWhiteSpace(buffer))
        //{
        //    current = start;
        //    line = line_start;
        //    column = column_start;
        //    return false;
        //}

        stringToken = new Token(TokenType.STRING, (column_start, column, line_start, line), literal: new StringLiteral(buffer));

        return true;
    }

    protected bool tryMatchInterpolatedExpression(ref int current, ref int column, ref int line, [NotNullWhen(true)] out Token? interpolatedToken)
    {
        int start = current;
        int column_start = column;
        int line_start = line;

        interpolatedToken = null;

        string? next;
        if (!peekNext(current, out next)) return false;

        if (next == TokenTypeValues.L_BRACE)
        {
            //Advance, but don't append start '{' to string
            moveNext(ref current, ref column, out _);
            string buffer = string.Empty;
            bool terminated = false;

            int index_at_first_line_break = -1;
            int column_at_first_line_break = -1;

            while (!terminated && peekNext(current, out next))
            {
                if (next == TokenTypeValues.R_BRACE)
                {
                    //Advance, but don't append '}' to string
                    moveNext(ref current, ref column, out _);
                    terminated = true;
                }
                else
                {
                    if (NEWLINE.IsMatch(next))
                    {
                        index_at_first_line_break = current;
                        column_at_first_line_break = column;
                        advanceLine(ref column, ref line);
                    }

                    moveNext(ref current, ref column, out _);
                    buffer += next;
                }
            }

            if (!terminated)
            {
                //String must terminate
                //Set location to end of first line and report error
                if (column_at_first_line_break >= 0)
                {
                    current = index_at_first_line_break;
                    line = line_start;
                    column = column_at_first_line_break;
                }

                interpolatedToken = new Token(TokenType.ERROR, (column_start, column_at_first_line_break < 0 ? column : column_at_first_line_break,
                    line_start, column_at_first_line_break < 0 ? line : line + 1), error: new LexError($"Unterminated interpolated expression literal: {TokenTypeValues.L_BRACE}{buffer}."));
            }
            else
            {
                interpolatedToken = new Token(TokenType.INTERPOLATED, (column_start, column, line_start, line), literal: new InterpolatedExpressionLiteral(buffer));
            }
        }

        if (interpolatedToken == null)
        {
            current = start;
            line = line_start;
            column = column_start;
            return false;
        }

        return true;
    }
}