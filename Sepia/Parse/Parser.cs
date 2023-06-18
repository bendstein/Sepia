using Sepia.AST;
using Sepia.AST.Node;
using Sepia.AST.Node.Expression;
using Sepia.AST.Node.Statement;
using Sepia.Common;
using Sepia.Lex;
using Sepia.Lex.Literal;
using Sepia.Value.Type;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;

namespace Sepia.Parse;
public class Parser
{
    private readonly IEnumerable<Token> _tokens;

    private readonly ParserSettings _settings;

    public Parser(IEnumerable<Token> tokens, ParserSettings? settings = null)
    {
        _tokens = tokens;
        _settings = settings?? new ParserSettings();
    }

    public bool TryParse([NotNullWhen(true)] out AbstractSyntaxTree? parsed, out List<SepiaError> errors)
    {
        int n = 0;
        List<StatementNode> statements = new();
        errors = new();

        while (!IsAtEnd(n))
        {
            try
            {
                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                if (IsAtEnd(n))
                    break;

                if (TryParseStatement(ref n, errors, out StatementNode? statement))
                {
                    statements.Add(statement);
                }
                else
                {
                    errors.Add(new($"Expected a statement!", Current(n).Location));
                    //Errors have been reported; try to continue to get any other info
                    Synchronize(ref n);
                }
            }
            catch (SepiaException ie)
            {
                errors.Add(ie.Error ?? new($"An error occurred during parsing: {ie.Message}"));

                //Errors have been reported; try to continue to get any other info
                Synchronize(ref n);
            }
            catch (Exception e)
            {
                errors.Add(new($"An error occurred during parsing: {e.Message}"));

                //Errors have been reported; try to continue to get any other info
                Synchronize(ref n);
            }
        }

        parsed = new(new ProgramNode(statements));

        return parsed != null && !errors.Any();
    }

    public bool TryParseStatement(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out StatementNode? node)
    {
        node = null;

        //Try to match a function declaration statement
        if(TryParseFunctionDeclarationStatement(ref n, errors, out FunctionDeclarationStmtNode? funcStmt))
        {
            node = funcStmt;
        }
        //Try to match a declaration statement
        else if (TryParseDeclarationStatement(ref n, errors, out DeclarationStmtNode? decStmt))
        {
            node = decStmt;
        }
        //Try to match an expression statement
        else if (TryParseExpressionStatement(ref n, errors, out ExpressionStmtNode? exprStmt))
        {
            node = exprStmt;
        }
        //Try to match a block
        else if(TryParseBlock(ref n, errors, out Block? block))
        {
            node = block;
        }
        //Try to match a conditional statement
        else if (TryParseConditionalStatement(ref n, errors, out ConditionalStatementNode? condStmt))
        {
            node = condStmt;
        }
        //Try to match a while loop
        else if (TryParseWhileStatement(ref n, errors, out WhileStatementNode? whileStmt))
        {
            node = whileStmt;
        }
        //Try to match a return statement
        else if(TryParseReturnStatement(ref n, errors, out ReturnStatementNode? retStmt))
        {
            node = retStmt;
        }
        //Try to match a control statement
        else if(TryParseControlStatement(ref n, errors, out ControlFlowStatementNode? ctrlStmt))
        {
            node = ctrlStmt;
        }
        //Try to match a for loop
        else if (TryParseForStatement(ref n, errors, out ForStatementNode? forStmt))
        {
            node = forStmt;
        }

        return node != null;
    }

    public bool TryParseReturnStatement(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ReturnStatementNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to match a return token
        if (Peek(n, out Token? token) && token.TokenType == TokenType.RETURN)
        {
            Advance(ref n);
            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            ExpressionNode? expr;

            //Match an optional expression
            if(TryParseExpression(ref n, errors, out expr))
            {
            }
            else
            {
                expr = new ValueExpressionNode(Value.SepiaValue.Void);
            }


            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match a semicolon
            if (Peek(n, out Token? semiToken) && semiToken.TokenType == TokenType.SEMICOLON)
            {
                Advance(ref n);

                node = new(token, expr)
                {
                    AllTokens = _tokens.Skip(start).Take(n - start).ToList()
                };
                return true;
            }
            else
            {
                throw new SepiaException(new ParseError("Expected a semicolon.", (semiToken?.Location ?? _tokens.Last().Location).End()));
            }
        }

        //Not a control statement
        n = start;
        return false;
    }

    public bool TryParseForStatement(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ForStatementNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to match a for
        if (Peek(n, out Token? for_token) && for_token.TokenType == TokenType.FOR)
        {
            Advance(ref n);

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            DeclarationStmtNode? declaration = null;
            ExpressionStmtNode? condition = null;
            ExpressionNode? action = null;

            bool l_paren = false;

            //Optionally match a left paren
            if(Peek(n, out Token? l_paren_token) && l_paren_token.TokenType == TokenType.L_PAREN)
            {
                Advance(ref n);
                l_paren = true;
            }

            //Try to match a semicolon or a declaration statement
            if (Peek(n, out Token? semi_token) && semi_token.TokenType == TokenType.SEMICOLON)
            {
                Advance(ref n);
            }
            else if(!TryParseDeclarationStatement(ref n, errors, out declaration))
            {
                throw new SepiaException(new ParseError($"Expected a declaration or a semicolon!", Current(n)?.Location?? Prev(n)?.Location.End()));
            }

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match a semicolon or an expression statement
            if (Peek(n, out semi_token) && semi_token.TokenType == TokenType.SEMICOLON)
            {
                Advance(ref n);
            }
            else if (!TryParseExpressionStatement(ref n, errors, out condition))
            {
                throw new SepiaException(new ParseError($"Expected an expression or a semicolon!", Current(n)?.Location ?? Prev(n)?.Location.End()));
            }

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Optionally match an action
            _ = TryParseExpression(ref n, errors, out action);

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //If a left-paren was matched, match a right one
            if(l_paren)
            {
                if (Peek(n, out Token? r_paren_token) && r_paren_token.TokenType == TokenType.R_PAREN)
                {
                    Advance(ref n);
                }
                else
                {
                    throw new SepiaException(new ParseError($"Mismatched parentheses!", Current(n)?.Location ?? Prev(n)?.Location.End()));
                }
            }

            //Try to match block
            if (!TryParseBlock(ref n, errors, out Block? body))
            {
                throw new SepiaException(new ParseError($"Expected a block!", Current(n)?.Location ?? Prev(n)?.Location.End()));
            }

            node = new(declaration, condition, action, body)
            {
                AllTokens = _tokens.Skip(start).Take(n - start).ToList()
            };
            return true;
        }

        //This is not a while loop
        n = start;
        return false;
    }

    public bool TryParseControlStatement(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ControlFlowStatementNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        HashSet<TokenType> valid = new()
        {
            TokenType.BREAK,
            TokenType.CONTINUE
        };

        //Try to match a valid token
        if (Peek(n, out Token? token) && valid.Contains(token.TokenType))
        {
            Advance(ref n);
            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match a semicolon
            if (Peek(n, out Token? semiToken) && semiToken.TokenType == TokenType.SEMICOLON)
            {
                Advance(ref n);

                node = new(token)
                {
                    AllTokens = _tokens.Skip(start).Take(n - start).ToList()
                };
                return true;
            }
            else
            {
                throw new SepiaException(new ParseError("Expected a semicolon.", (semiToken?.Location ?? _tokens.Last().Location).End()));
            }
        }

        //Not a control statement
        n = start;
        return false;
    }

    public bool TryParseWhileStatement(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out WhileStatementNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to match a while
        if (Peek(n, out Token? while_token) && while_token.TokenType == TokenType.WHILE)
        {
            Advance(ref n);

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match condition
            if (!TryParseExpression(ref n, errors, out ExpressionNode? condition))
            {
                throw new SepiaException(new ParseError($"Expected an expression!", Current(n)?.Location ?? Prev(n)?.Location.End()));
            }

            //Try to match block
            if(!TryParseBlock(ref n, errors, out Block? body))
            {
                throw new SepiaException(new ParseError($"Expected a block!", Current(n)?.Location ?? Prev(n)?.Location.End()));
            }

            node = new(condition, body)
            {
                AllTokens = _tokens.Skip(start).Take(n - start).ToList()
            };
            return true;
        }

        //This is not a while loop
        n = start;
        return false;
    }

    public bool TryParseConditionalStatement(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ConditionalStatementNode? node)
    {
        int start = n;
        node = null;
        List<(ExpressionNode condition, Block body)> branches = new();
        Block? elseBody = null;

        while (!IsAtEnd(n))
        {
            int start_inner = n;

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            bool matched_else = false;

            //Try to match an else
            if(Peek(n, out Token? else_token) && else_token.TokenType == TokenType.ELSE)
            {
                Advance(ref n);
                matched_else = true;
                if (branches.Count == 0)
                {
                    throw new SepiaException(new ParseError($"{TokenType.ELSE.GetSymbol()} without a matching {TokenType.IF}.", else_token.Location));
                }
            }

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            bool matched_if = false;

            //Try to match an if
            if (Peek(n, out Token? if_token) && if_token.TokenType == TokenType.IF)
            {
                Advance(ref n);
                matched_if = true;
            }

            //Didn't match if or else; not a conditional
            if (!matched_if && !matched_else)
            {
                n = start_inner;
                break;
            }

            //If multiple if statements in a row, this is a separate node
            if(matched_if && !matched_else && branches.Any())
            {
                n = start_inner;
                break;
            }

            ExpressionNode? condition = null;
            Block? body;

            //If this is an if/else if, match the condition
            if (matched_if)
            {
                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                if(!TryParseExpression(ref n, errors, out condition))
                {
                    throw new SepiaException(new ParseError($"Expected an expression!", Current(n)?.Location?? Prev(n)?.Location.End()));
                }
            }

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //For if/else if/else, match the body
            if(!TryParseBlock(ref n, errors, out body))
            {
                throw new SepiaException(new ParseError($"Expected a block!", Current(n)?.Location ?? Prev(n)?.Location.End()));
            }

            if(matched_if)
            {
                branches.Add((condition!, body));
            }
            else
            {
                elseBody = body;
                break;
            }
        }

        if(branches.Any())
        {
            node = new(branches, elseBody)
            {
                AllTokens = _tokens.Skip(start).Take(n - start).ToList()
            };
            return true;
        }
        else
        {
            //This is not a conditional
            n = start;
            return false;
        }
    }

    public bool TryParseBlock(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out Block? node)
    {
        int start = n;
        node = null;
        List<StatementNode> statements = new();

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to match a left curly brace
        if (Peek(n, out Token? l_brace) && l_brace.TokenType == TokenType.L_BRACE)
        {
            Advance(ref n);

            Token? r_brace = null;

            while (!IsAtEnd(n))
            {
                try
                {
                    TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                    if (IsAtEnd(n))
                        break;

                    //If next symbol is a right curly brace, done with block
                    if (Peek(n, out Token? r_brace_inner) && r_brace_inner.TokenType == TokenType.R_BRACE)
                    {
                        r_brace = r_brace_inner;
                        Advance(ref n);
                        break;
                    }
                    else if (TryParseStatement(ref n, errors, out StatementNode? statement))
                    {
                        statements.Add(statement);
                    }
                    else
                    {
                        errors.Add(new($"Expected a statement!", Current(n).Location));
                        //Errors have been reported; try to continue to get any other info
                        Synchronize(ref n);
                    }
                }
                catch (SepiaException ie)
                {
                    errors.Add(ie.Error ?? new($"An error occurred during parsing: {ie.Message}"));

                    //Errors have been reported; try to continue to get any other info
                    Synchronize(ref n);
                }
                catch (Exception e)
                {
                    errors.Add(new($"An error occurred during parsing: {e.Message}"));

                    //Errors have been reported; try to continue to get any other info
                    Synchronize(ref n);
                }
            }

            if(r_brace == null)
            {
                throw new SepiaException(new ParseError("Mismatched braces.", Current(n)?.Location?? Prev(n).Location.End()));
            }

            node = new Block(statements)
            {
                AllTokens = _tokens.Skip(start).Take(n - start).ToList()
            };
            return true;
        }

        n = start;
        return false;
    }

    public bool TryParseExpressionStatement(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionStmtNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to match an expression
        if (TryParseExpression(ref n, errors, out ExpressionNode? expression))
        {
            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match a semicolon
            if (Peek(n, out Token? semiToken) && semiToken.TokenType == TokenType.SEMICOLON)
            {
                Advance(ref n);

                //Successfully matched expression statement
                node = new ExpressionStmtNode(expression)
                {
                    AllTokens = _tokens.Skip(start).Take(n - start).ToList()
                };
                return true;
            }
            else
            {
                throw new SepiaException(new ParseError("Expected a semicolon.", (semiToken?.Location?? _tokens.Last().Location).End()));
            }
        }
        n = start;
        return false;
    }

    public bool TryParseDeclarationStatement(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out DeclarationStmtNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to match the let keyword
        if (Peek(n, out Token? letToken) && letToken.TokenType == TokenType.LET)
        {
            Advance(ref n);

            IdLiteral? idLiteral;
            ExpressionNode? assignedExpression = null;

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match an identifier
            if(Peek(n, out Token? idToken) && idToken.TokenType == TokenType.ID)
            {
                Advance(ref n);

                if(idToken.Literal != null && idToken.Literal is IdLiteral id)
                {
                    idLiteral = id;
                }
                else
                {
                    throw new SepiaException(new ParseError($"Malformed identifier.", idToken.Location));
                }
            }
            else
            {
                throw new SepiaException(new ParseError($"Expected an identifier.", (idToken?? Prev(n)).Location));
            }

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            SepiaTypeInfo? typeInfo = null;

            //Optionally match type declaration
            if(Peek(n, out Token? colon) && colon.TokenType == TokenType.COLON)
            {
                Advance(ref n);

                if(!TryMatchType(ref n, errors, out typeInfo))
                {
                    throw new SepiaException(new ParseError($"Expected a type.", (Current(n) ?? Prev(n)).Location));
                }
            }

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Optionally match assignment
            if (Peek(n, out Token? assignmentToken) && assignmentToken.TokenType == TokenType.EQUAL)
            {
                Advance(ref n);

                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                //Try to match an expression
                if (TryParseExpression(ref n, errors, out ExpressionNode? assignedExpr))
                {
                    assignedExpression = assignedExpr;
                }
                else
                {
                    throw new SepiaException(new ParseError("Expected an expression.", (Current(n)?? Prev(n)).Location));
                }
            }

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match a semicolon
            if (Peek(n, out Token? semiToken) && semiToken.TokenType == TokenType.SEMICOLON)
            {
                Advance(ref n);

                //Finished statement
                node = new(idLiteral, typeInfo, assignedExpression)
                {
                    AllTokens = _tokens.Skip(start).Take(n - start).ToList()
                };
                return true;
            }
            else
            {
                throw new SepiaException(new ParseError("Expected a semicolon.", (semiToken?? Prev(n)).Location));
            }
        }
        
        n = start;
        return false;
    }

    public bool TryParseFunctionDeclarationStatement(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out FunctionDeclarationStmtNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to match the function keyword
        if (Peek(n, out Token? funcToken) && funcToken.TokenType == TokenType.FUNC)
        {
            Advance(ref n);

            IdLiteral idLiteral;
            FunctionExpressionNode assignedFunction;

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match an identifier
            if (Peek(n, out Token? idToken) && idToken.TokenType == TokenType.ID)
            {
                Advance(ref n);

                if (idToken.Literal != null && idToken.Literal is IdLiteral id)
                {
                    idLiteral = id;
                }
                else
                {
                    throw new SepiaException(new ParseError($"Malformed identifier.", idToken.Location));
                }
            }
            else
            {
                throw new SepiaException(new ParseError($"Expected an identifier.", (idToken ?? Prev(n)).Location));
            }

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match a function expression
            if(TryParseFunctionExpression(ref n, errors, out FunctionExpressionNode? funcNode))
            {
                assignedFunction = funcNode;
            }
            else
            {
                throw new SepiaException(new ParseError($"Expected a function.", (idToken ?? Prev(n)).Location));
            }

            node =  new(idLiteral, assignedFunction);
            return true;
        }

        n = start;
        return false;
    }

    public bool TryParseExpression(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        node = null;
        int start = n;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        if(TryParseAssignmentExpression(ref n, errors, out node))
        {
            return true;
        }

        n = start;
        return false;
    }

    public bool TryParseAssignmentExpression(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;
        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.EQUAL,
            TokenType.PLUS_EQUAL,
            TokenType.MINUS_EQUAL,
            TokenType.STAR_EQUAL,
            TokenType.SLASH_EQUAL,
            TokenType.PERCENT_EQUAL,
            TokenType.CARET_EQUAL,
            TokenType.AMP_AMP_EQUAL,
            TokenType.PIPE_PIPE_EQUAL,
            TokenType.AMP_EQUAL,
            TokenType.PIPE_EQUAL,
            TokenType.TILDE_EQUAL,
            TokenType.LESS_LESS_EQUAL,
            TokenType.GREATER_GREATER_EQUAL
        };

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        IdLiteral idLiteral;
        ExpressionNode assignedExpression;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to match an identifier
        if (Peek(n, out Token? idToken) && idToken.TokenType == TokenType.ID)
        {
            Advance(ref n);

            if (idToken.Literal != null && idToken.Literal is IdLiteral id)
            {
                idLiteral = id;
            }
            else
            {
                throw new SepiaException(new ParseError($"Malformed identifier.", idToken.Location));
            }

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Match assignment
            if (Peek(n, out Token? assignmentToken) && allowedOperators.Contains(assignmentToken.TokenType))
            {
                Advance(ref n);

                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                //Try to match an expression
                if (TryParseExpression(ref n, errors, out ExpressionNode? assignedExpr))
                {
                    assignedExpression = assignedExpr;

                    //Finished expression
                    node = new AssignmentExprNode(idLiteral, assignmentToken, assignedExpression)
                    {
                        AllTokens = _tokens.Skip(start).Take(n - start).ToList()
                    };
                    return true;
                }
                else
                {
                    throw new SepiaException(new ParseError("Expected an expression.", (Current(n) ?? Prev(n)).Location));
                }
            }
        }

        //This is not an assignment statement; move to next highest priority
        n = start;
        return TryParseLogicalOr(ref n, errors, out node);
    }

    public bool TryParseLogicalOr(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.PIPE_PIPE
        };

        //Match the prior part of the expression
        if (TryParseLogicalAnd(ref n, errors, out ExpressionNode? first))
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
                    if (TryParseLogicalAnd(ref n, errors, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new SepiaException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryExprNode(node, next.op, next.adj)
                {
                    AllTokens = node.AllTokens.Union(new Token[] { next.op }).Union(next.adj.AllTokens).ToList()
                };
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseLogicalAnd(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.AMP_AMP
        };

        //Match the prior part of the expression
        if (TryParseBitwiseOr(ref n, errors, out ExpressionNode? first))
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
                    if (TryParseBitwiseOr(ref n, errors, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new SepiaException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryExprNode(node, next.op, next.adj)
                {
                    AllTokens = node.AllTokens.Union(new Token[] { next.op }).Union(next.adj.AllTokens).ToList()
                };
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBitwiseOr(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.PIPE
        };

        //Match the prior part of the expression
        if (TryParseBitwiseXor(ref n, errors, out ExpressionNode? first))
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
                    if (TryParseBitwiseXor(ref n, errors, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new SepiaException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryExprNode(node, next.op, next.adj)
                {
                    AllTokens = node.AllTokens.Union(new Token[] { next.op }).Union(next.adj.AllTokens).ToList()
                };
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBitwiseXor(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.CARET
        };

        //Match the prior part of the expression
        if (TryParseBitwiseAnd(ref n, errors, out ExpressionNode? first))
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
                    if (TryParseBitwiseAnd(ref n, errors, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new SepiaException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryExprNode(node, next.op, next.adj)
                {
                    AllTokens = node.AllTokens.Union(new Token[] { next.op }).Union(next.adj.AllTokens).ToList()
                };
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBitwiseAnd(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.AMP
        };

        //Match the prior part of the expression
        if (TryParseBinaryEquality(ref n, errors, out ExpressionNode? first))
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
                    if (TryParseBinaryEquality(ref n, errors, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new SepiaException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryExprNode(node, next.op, next.adj)
                {
                    AllTokens = node.AllTokens.Union(new Token[] { next.op }).Union(next.adj.AllTokens).ToList()
                };
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBinaryEquality(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
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
                        throw new SepiaException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryExprNode(node, next.op, next.adj)
                {
                    AllTokens = node.AllTokens.Union(new Token[] { next.op }).Union(next.adj.AllTokens).ToList()
                };
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBinaryComparision(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
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
        if (TryParseBitwiseShift(ref n, errors, out ExpressionNode? first))
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
                    if (TryParseBitwiseShift(ref n, errors, out ExpressionNode? adj))
                    {
                        adjoining.Push((t, adj));
                        continue;
                    }
                    else
                    {
                        throw new SepiaException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryExprNode(node, next.op, next.adj)
                {
                    AllTokens = node.AllTokens.Union(new Token[] { next.op }).Union(next.adj.AllTokens).ToList()
                };
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBitwiseShift(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.LESS_LESS,
            TokenType.GREATER_GREATER
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
                        throw new SepiaException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryExprNode(node, next.op, next.adj)
                {
                    AllTokens = node.AllTokens.Union(new Token[] { next.op }).Union(next.adj.AllTokens).ToList()
                };
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBinaryTerm(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
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
                        throw new SepiaException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;
            while (adjoining.TryPop(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryExprNode(node, next.op, next.adj)
                {
                    AllTokens = node.AllTokens.Union(new Token[] { next.op }).Union(next.adj.AllTokens).ToList()
                };
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseBinaryFactor(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.SLASH,
            TokenType.STAR,
            TokenType.PERCENT,
            TokenType.AMP,
            TokenType.AMP_AMP
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
                        throw new SepiaException(new ParseError("Expected an expression!", t.Location.End()));
                    }
                }

                n = loop_start;
                finished = true;
            }

            node = first;

            while (adjoining.TryDequeue(out (Token op, ExpressionNode adj) next))
            {
                node = new BinaryExprNode(node, next.op, next.adj)
                {
                    AllTokens = node.AllTokens.Union(new Token[] { next.op }).Union(next.adj.AllTokens).ToList()
                };
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseUnaryPrefix(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        HashSet<TokenType> allowedOperators = new()
        {
            TokenType.BANG,
            TokenType.MINUS,
            TokenType.TILDE
        };

        Token? t;
        if (!Peek(n, out t))
            throw new SepiaException(new ParseError("Sequence contains no more tokens.", Peek(n - 1, out Token? c)? c.Location.End() : null));

        //Match unary prefix
        if (allowedOperators.Contains(t.TokenType))
        {
            Advance(ref n);

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //For precedence's purposes, after a unary operator should come another unary expression
            if(TryParseUnaryPrefix(ref n, errors, out ExpressionNode? nestedUnary))
            {
                node = new UnaryPrefixExprNode(t, nestedUnary)
                {
                    AllTokens = new Token[] { t }.Union(nestedUnary.AllTokens).ToList()
                };
            }
        }

        //Not a unary expression, roll down to next priority
        if(node == null)
        {
            TryParseCall(ref n, errors, out node);
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseCall(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to parse function or primary expression
        if (TryParseFunc(ref n, errors, out ExpressionNode? primary))
        {
            CallExprNode? current = null;

            //Try to match calls
            while(!IsAtEnd(n))
            {
                int start_inner = n;

                List<ExpressionNode> arguments = new();

                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                //Try to match left paren
                if (Peek(n, out Token? l_paren) && l_paren.TokenType == TokenType.L_PAREN)
                {
                    Advance(ref n);

                    TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                    int start_args = n;

                    //Try to match arguments
                    while (!IsAtEnd(n))
                    {
                        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                        if (TryParseExpression(ref n, errors, out ExpressionNode? arg))
                        {
                            arguments.Add(arg);
                        }
                        else
                        {
                            //Not an argument
                            n = start_args;
                            break;
                        }

                        if(Peek(n, out Token? next))
                        {
                            if(next.TokenType == TokenType.COMMA)
                            {
                                //Make sure symbol after comma isn't a right paren
                                if(Peek(n + 1, out Token? next_2) && next_2.TokenType == TokenType.R_PAREN)
                                {
                                    throw new SepiaException(new ParseError());
                                }

                                //Continue to match arguments
                                Advance(ref n);
                            }
                            else if(next.TokenType == TokenType.R_PAREN)
                            {
                                //Done matching arguments
                                break;
                            }
                            else
                            {
                                throw new SepiaException(new ParseError());
                            }
                        }
                        else
                        {
                            throw new SepiaException(new ParseError());
                        }
                    }

                    //Try to match a right paren
                    if (Peek(n, out Token? r_paren) && r_paren.TokenType == TokenType.R_PAREN)
                    {
                        Advance(ref n);
                    }
                    else
                    {
                        throw new SepiaException(new ParseError());
                    }
                }
                //Not a call
                else
                {
                    n = start_inner;
                    break;
                }

                current = new(current?? primary, arguments);
            }

            if(current == null)
            {
                node = primary;
                return true;
            }
            else
            {
                node = current;
                return true;
            }
        }

        n = start;
        return false;
    }

    public bool TryParseFunc(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        if(TryParseFunctionExpression(ref n, errors, out FunctionExpressionNode? funcNode))
        {
            node = funcNode;
            return true;
        }

        //Roll down to primary
        return TryParsePrimary(ref n, errors, out node);
    }

    public bool TryParsePrimary(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        Token? t;
        if (!Peek(n, out t))
            throw new SepiaException(new ParseError("Sequence contains no more tokens.", Peek(n - 1, out Token? c) ? c.Location.End() : null));

        //Match number
        if (t.Literal != null && t.Literal is NumberLiteral _)
        {
            Advance(ref n);

            node = new LiteralExprNode(t)
            {
                AllTokens = new() { t }
            };
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
                            error.Location.LineStart == 1 ? error.Location.ColumnStart + t.Location.ColumnStart - 1 : error.Location.ColumnStart + 1,
                            error.Location.LineEnd == 1 ? error.Location.ColumnEnd + t.Location.ColumnStart - 1 : error.Location.ColumnEnd + 1,
                            error.Location.LineStart + t.Location.LineStart - 1,
                            error.Location.LineEnd + t.Location.LineStart - 1
                        );

                        errors.Add(new LexError(error.Error!.Message, adj_location, error.Error.Data));
                    }

                    node = new LiteralExprNode(t)
                    {
                        AllTokens = new() { t }
                    };
                }
                else
                {
                    interpolatedTokens = interpolatedTokens.TakeWhile(t => t.TokenType != TokenType.EOF)
                        .Select(token => token.Clone((
                            token.Location.LineStart == 1 ? token.Location.ColumnStart + t.Location.ColumnStart - 1 : token.Location.ColumnStart + 1,
                            token.Location.LineEnd == 1 ? token.Location.ColumnEnd + t.Location.ColumnStart - 1 : token.Location.ColumnEnd + 1,
                            token.Location.LineStart + t.Location.LineStart - 1,
                            token.Location.LineEnd + t.Location.LineStart - 1
                        )));

                    List<ExpressionNode> inner = interpolatedTokens.Select(token =>
                    {
                        if (token.Literal != null && token.Literal is StringLiteral sliteral)
                        {
                            return new LiteralExprNode(token)
                            {
                                AllTokens = new()
                                {
                                    token
                                }
                            };
                        }
                        else if (token.Literal != null && token.Literal is InterpolatedExpressionLiteral ieliteral)
                        {
                            //Lex and parse the interpolated expression
                            Lexer lexer = new(ieliteral.Value, _settings.InterpolatedLexerSettings);
                            IEnumerable<Token> interpolatedTokens = lexer.Scan().Select(itoken => itoken.Clone((
                                itoken.Location.LineStart == 1 ? itoken.Location.ColumnStart + token.Location.ColumnStart - 1 : itoken.Location.ColumnStart + 1,
                                itoken.Location.LineEnd == 1 ? itoken.Location.ColumnEnd + token.Location.ColumnStart - 1 : itoken.Location.ColumnEnd + 1,
                                itoken.Location.LineStart + token.Location.LineStart - 1,
                                itoken.Location.LineEnd + token.Location.LineStart - 1
                            )));

                            var interpolatedTokenErrors = interpolatedTokens.Where(t => t.TokenType == TokenType.ERROR);

                            if (interpolatedTokenErrors.Any())
                            {
                                //Report errors and return as string literal
                                foreach (var error in interpolatedTokenErrors)
                                {
                                    ////Move the error location to reflect the outer scope
                                    //Location adj_location = (
                                    //    error.Location.LineStart == 1 ? error.Location.ColumnStart + t.Location.ColumnStart + token.Location.ColumnStart : error.Location.ColumnStart,
                                    //    error.Location.LineEnd == 1 ? error.Location.ColumnEnd + t.Location.ColumnStart + token.Location.ColumnStart : error.Location.ColumnEnd,
                                    //    error.Location.LineStart + t.Location.LineStart - 1 + token.Location.LineStart - 1,
                                    //    error.Location.LineEnd + t.Location.LineStart - 1 + token.Location.LineStart - 1
                                    //);

                                    //errors.Add(new LexError(error.Error!.Message, adj_location, error.Error.Data));
                                    errors.Add(error.Error!);
                                }

                                return new LiteralExprNode(token)
                                {
                                    AllTokens = new()
                                    {
                                        token
                                    }
                                };
                            }

                            //If no tokens except EOF return void
                            if(!interpolatedTokens.Where(t => t.TokenType != TokenType.EOF).Any())
                            {
                                Token eof = new Token(TokenType.EOF, t.Location, VoidLiteral.Instance);
                                return new LiteralExprNode(eof)
                                {
                                    AllTokens = new() { eof }
                                };
                            }

                            Parser interpolatedParser = new(interpolatedTokens, _settings);

                            List<SepiaError> inner_errors = new();
                            int inner_n = 0;
                            ExpressionNode? inner_expression = null;

                            //Try to parse the inner exception
                            bool tryParseExpression = false;

                            try
                            {
                                tryParseExpression = interpolatedParser.TryParseExpression(ref inner_n, inner_errors, out inner_expression);
                            }
                            catch (SepiaException ie)
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
                                    ////Move the error location to reflect the outer scope
                                    //Location adj_location = (
                                    //    error.Location.LineStart == 1? error.Location.ColumnStart + t.Location.ColumnStart + token.Location.ColumnStart : error.Location.ColumnStart,
                                    //    error.Location.LineEnd == 1? error.Location.ColumnEnd + t.Location.ColumnStart + token.Location.ColumnStart : error.Location.ColumnEnd,
                                    //    error.Location.LineStart + t.Location.LineStart - 1 + token.Location.LineStart - 1,
                                    //    error.Location.LineEnd + t.Location.LineStart - 1 + token.Location.LineStart - 1
                                    //);
                                    //errors.Add(new ParseError(error.Message, adj_location, error.Data));
                                    errors.Add(error);
                                }

                                return new LiteralExprNode(token)
                                {
                                    AllTokens = new()
                                    {
                                        token
                                    }
                                };
                            }
                        }
                        else
                        {
                            throw new SepiaException(new ParseError($"Unexpected token at {token.Location} of interpolated string.", t.Location.Bridge(t.Location.Add(token.Location).Add((0, 0, -1, -1)))));
                        }
                    }).ToList();

                    node = new InterpolatedStringExprNode(inner)
                    {
                        AllTokens = _tokens.Skip(start).Take(n + 1 - start).ToList()
                    };
                }
            }
            else
            {
                node = new LiteralExprNode(t)
                {
                    AllTokens = _tokens.Skip(start).Take(n + 1 - start).ToList()
                };
            }

            Advance(ref n);
        }
        //Match boolean
        else if (t.Literal != null && t.Literal is BooleanLiteral _)
        {
            node = new LiteralExprNode(t)
            {
                AllTokens = new() { t }
            };
            Advance(ref n);
        }
        //Match null
        else if (t.Literal != null && t.Literal is NullLiteral _)
        {
            node = new LiteralExprNode(t)
            {
                AllTokens = new() { t }
            };
            Advance(ref n);
        }
        //Match identifier
        else if (t.Literal != null && t.Literal is IdLiteral idliteral)
        {
            node = new IdentifierExprNode(idliteral)
            {
                AllTokens = new() { t }
            };
            Advance(ref n);
        }
        //Match function
        else
        {
            switch (t.TokenType)
            {
                //Match ( Expression ) or a function declaration
                case TokenType.L_PAREN:
                    Advance(ref n);

                    TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                    if (TryParseExpression(ref n, errors, out ExpressionNode? innerExpression))
                    {
                        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                        if (Peek(n, out Token? rParen) && rParen.TokenType == TokenType.R_PAREN)
                        {
                            Advance(ref n);

                            node = new GroupExprNode(innerExpression)
                            {
                                AllTokens = _tokens.Skip(start).Take(n - start).ToList()
                            };
                        }
                        else
                        {
                            throw new SepiaException(new ParseError("Mismatched parentheses.", Peek(n, out Token? c) ? t.Location.Bridge(c.Location.End()) : t.Location));
                        }
                    }
                    else
                    {
                        throw new SepiaException(new ParseError("Expected an expression!", Peek(n, out Token? c) ? t.Location.Bridge(c.Location.End()) : t.Location));
                    }
                    break;
            }
        }

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
    }

    public bool TryParseFunctionExpression(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out FunctionExpressionNode? node) 
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to match a left paren
        if(Peek(n, out Token? lParen) && lParen.TokenType == TokenType.L_PAREN)
        {
            Advance(ref n);

            List<(IdLiteral id, SepiaTypeInfo type)> arguments = new();
            SepiaTypeInfo returnType;
            Block body;
            bool done = false;

            while(!IsAtEnd(n) && !done)
            {
                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                IdLiteral? id;
                SepiaTypeInfo? type;

                //Match id: type,
                if (Peek(n, out Token? token) && token.TokenType == TokenType.ID && token.Literal is IdLiteral idlit)
                {
                    Advance(ref n);
                    id = new(idlit.ResolveInfo.Clone());

                    TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                    if (Peek(n, out Token? colonToken) && colonToken.TokenType == TokenType.COLON)
                    {
                        Advance(ref n);

                        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                        if (TryMatchType(ref n, errors, out type))
                        {
                            //Match comma if not last arg
                            if(Peek(n, out Token? commaToken) && commaToken.TokenType == TokenType.COMMA)
                            {
                                Advance(ref n);
                            }
                            else if(commaToken != null && commaToken.TokenType == TokenType.R_PAREN)
                            {
                                done = true;
                            }
                            else if(arguments.Any())
                            {
                                throw new SepiaException(new ParseError($"Mismatched parentheses.", (Current(n) ?? Prev(n)).Location));
                            }
                            else
                            {
                                n = start;
                                return false;
                            }

                            arguments.Add((id, type));
                        }
                        else
                        {
                            throw new SepiaException(new ParseError($"Expected a type.", (Current(n) ?? Prev(n)).Location));
                        }
                    }
                    else
                    {
                        if (arguments.Any())
                        {
                            throw new SepiaException(new ParseError($"Expected colon.", (Current(n) ?? Prev(n)).Location));
                        }

                        n = start;
                        return false;
                    }
                }   
                else if(!arguments.Any() && token != null && token.TokenType == TokenType.R_PAREN)
                {
                    //Done
                    done = true;
                }
                else
                {
                    if(arguments.Any())
                    {
                        throw new SepiaException(new ParseError($"Expected argument.", (Current(n) ?? Prev(n)).Location));
                    }

                    n = start;
                    return false;
                }
            }

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match a right paren
            if(Peek(n, out Token? rParen) && rParen.TokenType == TokenType.R_PAREN)
            {
                Advance(ref n);
            }
            else
            {
                throw new SepiaException(new ParseError($"Mismatched parentheses.", (Current(n) ?? Prev(n)).Location));
            }

            //Try to match a colon
            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            if(Peek(n, out Token? funcColon) && funcColon.TokenType == TokenType.COLON)
            {
                Advance(ref n);

                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                //Try to match a type
                if(TryMatchType(ref n, errors, out SepiaTypeInfo? fReturnType))
                {
                    returnType = fReturnType;
                }
                else
                {
                    throw new SepiaException(new ParseError($"Expected a type.", (Current(n) ?? Prev(n)).Location));
                }
            }
            else
            {
                returnType = SepiaTypeInfo.Void();
            }

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to accept body
            if(TryParseBlock(ref n, errors, out Block? fBody)) 
            {
                body = fBody;
            }
            else
            {
                throw new SepiaException(new ParseError($"Expected function body.", (Current(n) ?? Prev(n)).Location));
            }

            node = new FunctionExpressionNode(arguments, returnType, body)
            {
                AllTokens = _tokens.Skip(start).Take(n - start).ToList()
            };
            return true;
        }

        n = start;
        return false;
    }

    private bool TryMatchType(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out SepiaTypeInfo? typeInfo)
    {
        int start = n;
        typeInfo = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Match built-in type, or an identifier
        if (Peek(n, out Token? type))
        {
            switch (type.TokenType)
            {
                case TokenType.ID:
                    if (type.Literal != null && type.Literal is IdLiteral id)
                    {
                        typeInfo = new(id.ResolveInfo.Name);
                    }
                    else
                    {
                        throw new SepiaException(new LexError($"Invalid identifier.", type.Location));
                    }
                    Advance(ref n);
                    break;
                case TokenType.TYPE_VOID:
                    typeInfo = SepiaTypeInfo.Void();
                    Advance(ref n);
                    break;
                case TokenType.TYPE_STRING:
                    typeInfo = SepiaTypeInfo.String();
                    Advance(ref n);
                    break;
                case TokenType.TYPE_INT:
                    typeInfo = SepiaTypeInfo.Integer();
                    Advance(ref n);
                    break;
                case TokenType.TYPE_FLOAT:
                    typeInfo = SepiaTypeInfo.Float();
                    Advance(ref n);
                    break;
                case TokenType.TYPE_BOOL:
                    typeInfo = SepiaTypeInfo.Boolean();
                    Advance(ref n);
                    break;
                case TokenType.FUNC:
                    
                    if(!TryMatchCallSignature(ref n, errors, out typeInfo))
                    {
                        throw new SepiaException(new LexError($"Expected a call signature.", type.Location));
                    }

                    break;
            }

            if (typeInfo != null)
            {
                return true;
            }
        }

        n = start;
        return false;
    }

    private bool TryMatchCallSignature(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out SepiaTypeInfo? typeInfo)
    {
        int start = n;
        typeInfo = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to match func keyword
        if (Peek(n, out Token? funcToken) && funcToken.TokenType == TokenType.FUNC)
        {
            Advance(ref n);

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match left paren
            if(Peek(n, out Token? lParen) && lParen.TokenType == TokenType.L_PAREN)
            {
                Advance(ref n);

                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                //Match arguments; return type
                List<SepiaTypeInfo> Arguments = new();
                SepiaTypeInfo ReturnType = SepiaTypeInfo.Void();

                bool arguments_done = false;

                while(!IsAtEnd(n))
                {
                    TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                    if (Peek(n, out Token? next))
                    {
                        if (next.TokenType == TokenType.R_PAREN)
                        {
                            break;
                        }

                        //If still in arguments portion
                        if (!arguments_done)
                        {
                            //Done with arguments. Anything after this semicolon should be for the return type
                            if (next.TokenType == TokenType.SEMICOLON)
                            {
                                arguments_done = true;
                                Advance(ref n);
                                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);
                                continue;
                            }
                            //Match a comma separating args
                            else if (next.TokenType == TokenType.COMMA)
                            {
                                if (Arguments.Count == 0)
                                {
                                    throw new SepiaException(new LexError($"Invalid call signature. Expected an argument type, but got '{TokenType.COMMA.GetSymbol()}'.", next.Location));
                                }
                                Advance(ref n);
                                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);
                            }
                            //If this is not the first argument, then there must be a comman
                            else if (Arguments.Count > 0)
                            {
                                throw new SepiaException(new LexError($"Invalid call signature. Expected '{TokenType.COMMA.GetSymbol()}', but got '{next.TokenType.GetSymbol()}'.", next.Location));
                            }

                            int n_inner = n;

                            //Try to match type
                            if (TryMatchType(ref n, errors, out SepiaTypeInfo? nextType))
                            {
                                Arguments.Add(nextType);
                                continue;
                            }
                            else
                            {
                                var current = Peek(n_inner, out Token? t) ? t : Last();
                                throw new SepiaException(new LexError($"Invalid call signature; expected a type.", current.Location));
                            }
                        }
                        //In return value portion
                        else
                        {
                            int n_inner = n;
                            //Try to match type
                            if (TryMatchType(ref n, errors, out SepiaTypeInfo? retType))
                            {
                                ReturnType = retType;
                                continue;
                            }
                            else
                            {
                                var current = Peek(n_inner, out Token? t) ? t : Last();
                                throw new SepiaException(new LexError($"Invalid call signature; expected a return type.", current.Location));
                            }
                        }
                    }
                }

                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                //Try to match right paren
                if (Peek(n, out Token? rParen) && rParen.TokenType == TokenType.R_PAREN)
                {
                    Advance(ref n);
                }
                else
                {
                    throw new SepiaException(new LexError($"Invalid call signature. Expected '{TokenType.R_PAREN.GetSymbol()}'; got '{(rParen?.TokenType ?? TokenType.EOF).GetSymbol()}'.", rParen?.Location ?? Last().Location));
                }

                typeInfo = SepiaTypeInfo.Function()
                    .WithCallSignature(new(Arguments, ReturnType));
                return true;
            }
        }

        //Not a call signature
        n = start;
        return false;
    }

    private void Advance(ref int n, int amount = 1) => n += amount;

    private bool Peek(int n, [NotNullWhen(true)] out Token? token)
    {
        token = _tokens.ElementAtOrDefault(n);
        return token != null;
    }

    private Token Current(int n) => _tokens.ElementAt(n);

    private Token Prev(int n) => _tokens.ElementAt(n - 1);

    private Token Last() => _tokens.Last();

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