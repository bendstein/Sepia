using Sepia.AST;
using Sepia.AST.Node;
using Sepia.AST.Node.Expression;
using Sepia.AST.Node.Statement;
using Sepia.Common;
using Sepia.Lex;
using Sepia.Lex.Literal;
using System.Diagnostics.CodeAnalysis;
using System.Net;

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

        //Try to match a print statement
        if (TryParsePrintStatement(ref n, errors, out PrintStmtNode? printStmt))
        {
            node = printStmt;
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

            node = new(declaration, condition, action, body);
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

                node = new(token);
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

            node = new(condition, body);
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
            node = new(branches, elseBody);
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

            node = new Block(statements);
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
                node = new ExpressionStmtNode(expression);
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

    public bool TryParsePrintStatement(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out PrintStmtNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //Try to match the print keyword
        if(Peek(n, out Token? printToken) && printToken.TokenType == TokenType.PRINT)
        {
            Advance(ref n);

            TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

            //Try to match an expression
            if(TryParseExpression(ref n, errors, out ExpressionNode? expression))
            {
                TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

                //Try to match a semicolon
                if (Peek(n, out Token? semiToken) && semiToken.TokenType == TokenType.SEMICOLON)
                {
                    Advance(ref n);

                    //Successfully matched expression statement
                    node = new PrintStmtNode(expression);
                    return true;
                }
                else
                {
                    throw new SepiaException(new ParseError("Expected a semicolon.", (semiToken?.Location?? _tokens.Last().Location).End()));
                }
            }
            else
            {
                throw new SepiaException(new ParseError("Expected expression.", printToken.Location.End()));
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
                node = new(idLiteral, assignedExpression);
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

    public bool TryParseExpression(ref int n, List<SepiaError> errors, [NotNullWhen(true)] out ExpressionNode? node)
    {
        int start = n;
        node = null;

        TryAcceptMany(ref n, out _, TokenType.COMMENT, TokenType.WHITESPACE);

        //TryParseBinaryEquality(ref n, errors, out node);
        TryParseAssignmentExpression(ref n, errors, out node);

        bool rv = node != null;

        if (!rv) n = start;

        return rv;
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
                    node = new AssignmentExprNode(idLiteral, assignmentToken, assignedExpression);
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
                node = new BinaryExprNode(node, next.op, next.adj);
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
                node = new BinaryExprNode(node, next.op, next.adj);
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
                node = new BinaryExprNode(node, next.op, next.adj);
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
                node = new BinaryExprNode(node, next.op, next.adj);
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
                node = new BinaryExprNode(node, next.op, next.adj);
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
                node = new BinaryExprNode(node, next.op, next.adj);
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
                node = new BinaryExprNode(node, next.op, next.adj);
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
                node = new BinaryExprNode(node, next.op, next.adj);
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
                node = new BinaryExprNode(node, next.op, next.adj);
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
                node = new BinaryExprNode(node, next.op, next.adj);
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
                node = new UnaryPrefixExprNode(t, nestedUnary);
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
            node = new LiteralExprNode(t);
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

                    node = new LiteralExprNode(t);
                }
                else
                {
                    interpolatedTokens = interpolatedTokens.TakeWhile(t => t.TokenType != TokenType.EOF);

                    List<ExpressionNode> inner = interpolatedTokens.Select(token =>
                    {
                        if (token.Literal != null && token.Literal is StringLiteral sliteral)
                        {
                            return new LiteralExprNode(token);
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

                                return new LiteralExprNode(token);
                            }

                            //If no tokens except EOF return void
                            if(!interpolatedTokens.Where(t => t.TokenType != TokenType.EOF).Any())
                            {
                                return new LiteralExprNode(new Token(TokenType.EOF, t.Location, VoidLiteral.Instance));
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
                                    //Move the error location to reflect the outer scope
                                    Location adj_location = (
                                        error.Location.LineStart == 1? error.Location.ColumnStart + t.Location.ColumnStart + token.Location.ColumnStart : error.Location.ColumnStart,
                                        error.Location.LineEnd == 1? error.Location.ColumnEnd + t.Location.ColumnStart + token.Location.ColumnStart : error.Location.ColumnEnd,
                                        error.Location.LineStart + t.Location.LineStart - 1 + token.Location.LineStart - 1,
                                        error.Location.LineEnd + t.Location.LineStart - 1 + token.Location.LineStart - 1
                                    );
                                    errors.Add(new ParseError(error.Message, adj_location, error.Data));
                                }

                                return new LiteralExprNode(token);
                            }
                        }
                        else
                        {
                            throw new SepiaException(new ParseError($"Unexpected token at {token.Location} of interpolated string.", t.Location.Bridge(t.Location.Add(token.Location).Add((0, 0, -1, -1)))));
                        }
                    }).ToList();

                    node = new InterpolatedStringExprNode(inner);
                }
            }
            else
            {
                node = new LiteralExprNode(t);
            }

            Advance(ref n);
        }
        //Match boolean
        else if (t.Literal != null && t.Literal is BooleanLiteral _)
        {
            node = new LiteralExprNode(t);
            Advance(ref n);
        }
        //Match null
        else if (t.Literal != null && t.Literal is NullLiteral _)
        {
            node = new LiteralExprNode(t);
            Advance(ref n);
        }
        //Match identifier
        else if (t.Literal != null && t.Literal is IdLiteral idliteral)
        {
            node = new IdentifierExprNode(idliteral);
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

                            node = new GroupExprNode(innerExpression);
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