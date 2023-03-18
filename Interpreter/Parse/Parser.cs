using Interpreter.AST;
using Interpreter.AST.Node;
using Interpreter.Common;
using Interpreter.Lex;
using Interpreter.Lex.Literal;
using System.Diagnostics.CodeAnalysis;

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

    public AbstractSyntaxTree Parse()
    {
        int n = 0;

        if(TryParseExpression(ref n, out ExpressionNode? node))
        {
            return new AbstractSyntaxTree(node);
        }

        throw new InterpretException(new ParseError("Failed to parse."));
    }

    public bool TryParseExpression(ref int n, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        TryParseBinaryEquality(ref n, out node);

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBinaryEquality(ref int n, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.EQUAL_EQUAL,
            TokenType.BANG_EQUAL
        };

        //Match the prior part of the expression
        if (TryParseBinaryComparision(ref n, out ExpressionNode? first))
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
                    if (TryParseBinaryComparision(ref n, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new InterpretException(new ParseError("Expected an expression!"));
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

    public bool TryParseBinaryComparision(ref int n, [NotNullWhen(true)] out ExpressionNode? node)
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
        if (TryParseBinaryTerm(ref n, out ExpressionNode? first))
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
                    if (TryParseBinaryTerm(ref n, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new InterpretException(new ParseError("Expected an expression!"));
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

    public bool TryParseBinaryTerm(ref int n, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.MINUS,
            TokenType.PLUS
        };

        //Match the prior part of the expression
        if (TryParseBinaryFactor(ref n, out ExpressionNode? first))
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
                    if (TryParseBinaryFactor(ref n, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new InterpretException(new ParseError("Expected an expression!"));
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

    public bool TryParseBinaryFactor(ref int n, [NotNullWhen(true)] out ExpressionNode? node)
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
        if(TryParseUnaryPrefix(ref n, out ExpressionNode? first))
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
                    if (TryParseUnaryPrefix(ref n, out ExpressionNode? adj))
                    {
                        adjoining.Enqueue((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new InterpretException(new ParseError("Expected an expression!"));
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

    public bool TryParseUnaryPrefix(ref int n, [NotNullWhen(true)] out ExpressionNode? node)
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
            throw new InterpretException(new ParseError("Sequence contains no more tokens."));

        //Match unary prefix
        if (allowedOperators.Contains(t.TokenType))
        {
            Advance(ref n);

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //For precedence's purposes, after a unary operator should come another unary expression
            if(TryParseUnaryPrefix(ref n, out ExpressionNode? nestedUnary))
            {
                node = new UnaryPrefixNode(t, nestedUnary);
            }
        }

        //Not a unary expression, roll down to next priority
        if(node == null)
        {
            TryParsePrimary(ref n, out node);
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParsePrimary(ref int n, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        Token? t;
        if (!Peek(n, out t))
            throw new InterpretException(new ParseError("Sequence contains no more tokens."));

        //Match number
        if (t.Literal != null && t.Literal is NumberLiteral _)
        {
            node = new LiteralNode(t);
            Advance(ref n);
        }
        //Match string
        else if (t.Literal != null && t.Literal is StringLiteral _)
        {
            node = new LiteralNode(t);
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

                    if (TryParseExpression(ref n, out ExpressionNode? innerExpression))
                    {
                        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                        if (Peek(n, out Token? rParen) && rParen.TokenType == TokenType.R_PAREN)
                        {
                            Advance(ref n);

                            node = new GroupNode(innerExpression);
                        }
                        else
                        {
                            throw new InterpretException(new ParseError("Mismatched parentheses."));
                        }
                    }
                    else
                    {
                        throw new InterpretException(new ParseError("Expected an expression!"));
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
}