﻿using Interpreter.AST;
using Interpreter.AST.Node;
using Interpreter.Common;
using Interpreter.Lex;
using Interpreter.Lex.Literal;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Interpreter.Parse;
public class Parser
{
    private readonly IEnumerable<Token> _tokens;

    private readonly ParserSettings _settings;

    public Parser(IEnumerable<Token> tokens, ParserSettings? settings = null)
    {
        _tokens = tokens;
        _settings = settings?? new ParserSettings();
    }

    public bool TryParse([NotNullWhen(true)] out AbstractSyntaxTree? parsed, out List<InterpretError> errors)
    {
        parsed = null;
        int n = 0;
        errors = new();

        try
        {
            if (TryParseExpression(ref n, errors, out ExpressionNode? node))
            {
                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                parsed = new AbstractSyntaxTree(node);

                if (!IsAtEnd(n))
                    throw new InterpretException(new ParseError("Failed to parse.", Current(n).Location.End()));
            }
        }
        catch (InterpretException ie)
        {
            errors.Add(ie.Error?? new($"An error occurred during parsing: {ie.Message}"));
        }
        catch (Exception e)
        {
            errors.Add(new($"An error occurred during parsing: {e.Message}"));
        }

        return parsed != null && !errors.Any();
    }

    public bool TryParseExpression(ref int n, List<InterpretError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        TryParseBinaryEquality(ref n, errors, out node);

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBinaryEquality(ref int n, List<InterpretError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.EQUAL_EQUAL,
            TokenType.BANG_EQUAL
        };

        //Match the prior part of the expression
        if (TryParseBinaryComparision(ref n, errors, out ExpressionNode? first))
        {
            bool finished = false;

            Stack<(Token op, ExpressionNode adj)> adjoining = new();

            //Match 0+ adjoining expressions of same/higher priority
            while (!finished)
            {
                int loop_start = n;

                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                //Try to match the adjoining operator
                Token? t;
                if (Peek(n, out t) && allowedOperators.Contains(t.TokenType))
                {
                    Advance(ref n);

                    TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                    //Try to match the next part of the expression
                    if (TryParseBinaryComparision(ref n, errors, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new InterpretException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryNode(node, next.op, next.adj);
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBinaryComparision(ref int n, List<InterpretError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.GREATER,
            TokenType.GREATER_EQUAL,
            TokenType.LESS,
            TokenType.LESS_EQUAL
        };

        //Match the prior part of the expression
        if (TryParseBinaryTerm(ref n, errors, out ExpressionNode? first))
        {
            bool finished = false;

            Stack<(Token op, ExpressionNode adj)> adjoining = new();

            //Match 0+ adjoining expressions of same/higher priority
            while (!finished)
            {
                int loop_start = n;

                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                //Try to match the adjoining operator
                Token? t;
                if (Peek(n, out t) && allowedOperators.Contains(t.TokenType))
                {
                    Advance(ref n);

                    TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                    //Try to match the next part of the expression
                    if (TryParseBinaryTerm(ref n, errors, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new InterpretException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryNode(node, next.op, next.adj);
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBinaryTerm(ref int n, List<InterpretError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.MINUS,
            TokenType.PLUS
        };

        //Match the prior part of the expression
        if (TryParseBinaryFactor(ref n, errors, out ExpressionNode? first))
        {
            bool finished = false;

            Stack<(Token op, ExpressionNode adj)> adjoining = new();

            //Match 0+ adjoining expressions of same/higher priority
            while (!finished)
            {
                int loop_start = n;

                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                //Try to match the adjoining operator
                Token? t;
                if (Peek(n, out t) && allowedOperators.Contains(t.TokenType))
                {
                    Advance(ref n);

                    TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                    //Try to match the next part of the expression
                    if (TryParseBinaryFactor(ref n, errors, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new InterpretException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryNode(node, next.op, next.adj);
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBinaryFactor(ref int n, List<InterpretError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.SLASH,
            TokenType.STAR,
            TokenType.PERCENT
        };

        //Match the prior part of the expression
        if(TryParseUnaryPrefix(ref n, errors, out ExpressionNode? first))
        {
            bool finished = false;

            Queue<(Token op, ExpressionNode adj)> adjoining = new();

            //Match 0+ adjoining expressions of same/higher priority
            while (!finished)
            {
                int loop_start = n;

                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                //Try to match the adjoining operator
                Token? t;
                if(Peek(n, out t) && allowedOperators.Contains(t.TokenType))
                {
                    Advance(ref n);

                    TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                    //Try to match the next part of the expression
                    if (TryParseUnaryPrefix(ref n, errors, out ExpressionNode? adj))
                    {
                        adjoining.Enqueue((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new InterpretException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;

            while (adjoining.TryDequeue(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryNode(node, next.op, next.adj);
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseUnaryPrefix(ref int n, List<InterpretError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.BANG,
            TokenType.MINUS
        };

        Token? t;
        if (!Peek(n, out t))
            throw new InterpretException(new ParseError("Sequence contains no more tokens.", Peek(n - 1, out Token? c)? c.Location.End() : null));

        //Match unary prefix
        if (allowedOperators.Contains(t.TokenType))
        {
            Advance(ref n);

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //For precedence's purposes, after a unary operator should come another unary expression
            if(TryParseUnaryPrefix(ref n, errors, out ExpressionNode? nestedUnary))
            {
                node = new UnaryPrefixNode(t, nestedUnary);
            }
        }

        //Not a unary expression, roll down to next priority
        if(node == null)
        {
            TryParsePrimary(ref n, errors, out node);
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParsePrimary(ref int n, List<InterpretError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        Token? t;
        if (!Peek(n, out t))
            throw new InterpretException(new ParseError("Sequence contains no more tokens.", Peek(n - 1, out Token? c) ? c.Location.End() : null));

        //Match number
        if (t.Literal != null && t.Literal is NumberLiteral _)
        {
            node = new LiteralNode(t);
            Advance(ref n);
        }
        //Match string
        else if (t.Literal != null && t.Literal is StringLiteral sliteral)
        {
            if(sliteral.StringType == QuoteType.BACKTICK)
            {
                //Lex and parse the inside of the string as an interpolated string
                InterpolatedStringLexer interpolatedLexer = new(sliteral.Value, _settings.InterpolatedLexerSettings);
                IEnumerable<Token> interpolatedTokens = interpolatedLexer.Scan();

                var interpolatedTokenErrors = interpolatedTokens.Where(t => t.TokenType == TokenType.ERROR);

                if(interpolatedTokenErrors.Any())
                {
                    //Report errors and return as string literal
                    foreach(var error in interpolatedTokenErrors)
                    {
                        //Move the error location to reflect the outer scope
                        Location adj_location = (
                            error.Location.LineStart == 1 ? error.Location.ColumnStart + t.Location.ColumnStart : error.Location.ColumnStart,
                            error.Location.LineEnd == 1 ? error.Location.ColumnEnd + t.Location.ColumnStart : error.Location.ColumnEnd,
                            error.Location.LineStart + t.Location.LineStart - 1,
                            error.Location.LineEnd + t.Location.LineStart - 1
                        );

                        errors.Add(new LexError(error.Error!.Message, adj_location, error.Error.Data));
                    }

                    node = new LiteralNode(t);
                }
                else
                {
                    interpolatedTokens = interpolatedTokens.TakeWhile(t => t.TokenType != TokenType.EOF);

                    List<ExpressionNode> inner = interpolatedTokens.Select(token =>
                    {
                        if (token.Literal != null && token.Literal is StringLiteral sliteral)
                        {
                            return new LiteralNode(token);
                        }
                        else if (token.Literal != null && token.Literal is InterpolatedExpressionLiteral ieliteral)
                        {
                            //Lex and parse the interpolated expression
                            Lexer lexer = new(ieliteral.Value, _settings.InterpolatedLexerSettings);
                            IEnumerable<Token> interpolatedTokens = lexer.Scan();

                            var interpolatedTokenErrors = interpolatedTokens.Where(t => t.TokenType == TokenType.ERROR);

                            if (interpolatedTokenErrors.Any())
                            {
                                //Report errors and return as string literal
                                foreach (var error in interpolatedTokenErrors)
                                {
                                    //Move the error location to reflect the outer scope
                                    Location adj_location = (
                                        error.Location.LineStart == 1 ? error.Location.ColumnStart + t.Location.ColumnStart + token.Location.ColumnStart : error.Location.ColumnStart,
                                        error.Location.LineEnd == 1 ? error.Location.ColumnEnd + t.Location.ColumnStart + token.Location.ColumnStart : error.Location.ColumnEnd,
                                        error.Location.LineStart + t.Location.LineStart - 1 + token.Location.LineStart - 1,
                                        error.Location.LineEnd + t.Location.LineStart - 1 + token.Location.LineStart - 1
                                    );

                                    errors.Add(new LexError(error.Error!.Message, adj_location, error.Error.Data));
                                }

                                return new LiteralNode(token);
                            }

                            //If no tokens except EOF return void
                            if(!interpolatedTokens.Where(t => t.TokenType != TokenType.EOF).Any())
                            {
                                return new LiteralNode(new Token(TokenType.EOF, t.Location, VoidLiteral.Instance));
                            }

                            Parser interpolatedParser = new(interpolatedTokens, _settings);

                            List<InterpretError> inner_errors = new();
                            int inner_n = 0;
                            ExpressionNode? inner_expression = null;

                            //Try to parse the inner exception
                            bool tryParseExpression = false;

                            try
                            {
                                tryParseExpression = interpolatedParser.TryParseExpression(ref inner_n, inner_errors, out inner_expression);
                            }
                            catch (InterpretException ie)
                            {
                                tryParseExpression = false;
                                inner_errors.Add(ie.Error ?? new ParseError("Failed to parse interpolated expression.", t.Location.Add(token.Location)));
                            }

                            if (tryParseExpression)
                            {
                                return inner_expression!;
                            }
                            else
                            {
                                //Report errors and return as string literal
                                foreach (var error in inner_errors)
                                {
                                    //Move the error location to reflect the outer scope
                                    Location adj_location = (
                                        error.Location.LineStart == 1? error.Location.ColumnStart + t.Location.ColumnStart + token.Location.ColumnStart : error.Location.ColumnStart,
                                        error.Location.LineEnd == 1? error.Location.ColumnEnd + t.Location.ColumnStart + token.Location.ColumnStart : error.Location.ColumnEnd,
                                        error.Location.LineStart + t.Location.LineStart - 1 + token.Location.LineStart - 1,
                                        error.Location.LineEnd + t.Location.LineStart - 1 + token.Location.LineStart - 1
                                    );
                                    errors.Add(new ParseError(error.Message, adj_location, error.Data));
                                }

                                return new LiteralNode(token);
                            }
                        }
                        else
                        {
                            throw new InterpretException(new ParseError($"Unexpected token at {token.Location} of interpolated string.", t.Location.Bridge(t.Location.Add(token.Location).Add((0, 0, -1, -1)))));
                        }
                    }).ToList();

                    node = new InterpolatedStringNode(inner);
                }
            }
            else
            {
                node = new LiteralNode(t);
            }

            Advance(ref n);
        }
        //Match boolean
        else if (t.Literal != null && t.Literal is BooleanLiteral _)
        {
            node = new LiteralNode(t);
            Advance(ref n);
        }
        //Match null
        else if (t.Literal != null && t.Literal is NullLiteral _)
        {
            node = new LiteralNode(t);
            Advance(ref n);
        }
        else
        {
            switch (t.TokenType)
            {
                //Match ( Expression )
                case TokenType.L_PAREN:
                    Advance(ref n);

                    TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                    if (TryParseExpression(ref n, errors, out ExpressionNode? innerExpression))
                    {
                        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                        if (Peek(n, out Token? rParen) && rParen.TokenType == TokenType.R_PAREN)
                        {
                            Advance(ref n);

                            node = new GroupNode(innerExpression);
                        }
                        else
                        {
                            throw new InterpretException(new ParseError("Mismatched parentheses.", Peek(n, out Token? c) ? t.Location.Bridge(c.Location.End()) : t.Location));
                        }
                    }
                    else
                    {
                        throw new InterpretException(new ParseError("Expected an expression!", Peek(n, out Token? c) ? t.Location.Bridge(c.Location.End()) : t.Location));
                    }
                    break;
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    private void Advance(ref int n, int amount = 1) => n += amount;

    private bool Peek(int n, [NotNullWhen(true)] out Token? token)
    {
        token = _tokens.ElementAtOrDefault(n);
        return token != null;
    }

    private Token Current(int n) => _tokens.ElementAt(n);

    private Token Prev(int n) => _tokens.ElementAt(n - 1);

    private bool TryAcceptMany(ref int n, [NotNullWhen(true)] out IEnumerable<Token>? tokens, params TokenType[] tokenTypes)
    {
        tokens = null;

        List<Token> accepted = new();

        while(Peek(n, out Token? next))
        {
            if(tokenTypes.Contains(next.TokenType))
            {
                Advance(ref n);
                accepted.Add(next);
            }
            else
            {
                break;
            }
        }

        if(accepted.Any())
            tokens = accepted;

        return tokens != null;
    }

    private bool IsAtEnd(int n) => Peek(n, out Token? t) && t.TokenType == TokenType.EOF;

    private void Synchronize(ref int n)
    {
        if (!IsAtEnd(n)) 
            Advance(ref n);

        while(!IsAtEnd(n))
        {
            //If prev token is defined to be a sync token, continue parsing
            if (Prev(n).TokenType.IsSyncPrev())
                return;

            //If next token is defined to be a sync token, continue parsing
            if (Current(n).TokenType.IsSyncNext())
                return;

            Advance(ref n);
        }
    }
}