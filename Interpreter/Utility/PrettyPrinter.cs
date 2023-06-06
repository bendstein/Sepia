using Interpreter.AST.Node;
using Interpreter.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpreter.Lex.Literal;
using Interpreter.Lex;
using Interpreter.AST.Node.Expression;
using Interpreter.AST.Node.Statement;

namespace Interpreter.Utility;

public class PrettyPrinter :
    IASTNodeVisitor<ASTNode>,
    IASTNodeVisitor<ProgramNode>,
    IASTNodeVisitor<ExpressionNode>,
    IASTNodeVisitor<BinaryNode>,
    IASTNodeVisitor<GroupNode>,
    IASTNodeVisitor<UnaryPrefixNode>,
    IASTNodeVisitor<InterpolatedStringNode>,
    IASTNodeVisitor<LiteralNode>,
    IASTNodeVisitor<StatementNode>,
    IASTNodeVisitor<ExpressionStatementNode>,
    IASTNodeVisitor<PrintStatementNode>
{
    private readonly StringWriter StringWriter;

    public PrettyPrinter(StringWriter stringWriter)
    {
        StringWriter = stringWriter;
    }

    public void Visit(ASTNode node)
    {
        if (node is ProgramNode pnode)
            Visit(pnode);
        else if (node is ExpressionNode enode)
            Visit(enode);
        else if (node is StatementNode snode)
            Visit(snode);
        else
            throw new NotImplementedException($"Cannot pretty print node of type '{node.GetType().Name}'");
    }

    public void Visit(ProgramNode node)
    {
        foreach (var statement in node.statements)
        {
            Visit(statement);
            StringWriter.WriteLine();
        }
    }

    public void Visit(ExpressionNode node)
    {
        if (node is LiteralNode lnode)
            Visit(lnode);
        else if (node is UnaryPrefixNode upnode)
            Visit(upnode);
        else if (node is GroupNode gnode)
            Visit(gnode);
        else if (node is BinaryNode bnode)
            Visit(bnode);
        else if (node is InterpolatedStringNode enode)
            Visit(enode);
        else
            throw new NotImplementedException($"Cannot pretty print node of type '{node.GetType().Name}'");
    }

    public void Visit(BinaryNode node)
    {
        Visit(node.Left);
        StringWriter.Write($" {node.Operator.TokenType.GetSymbol()} ");
        Visit(node.Right);
    }

    public void Visit(GroupNode node)
    {
        StringWriter.Write(TokenType.L_PAREN.GetSymbol());
        Visit(node.Inner);
        StringWriter.Write(TokenType.R_PAREN.GetSymbol());
    }

    public void Visit(UnaryPrefixNode node)
    {
        StringWriter.Write(node.Operator.TokenType.GetSymbol());
        Visit(node.Right);
    }

    public void Visit(InterpolatedStringNode node)
    {
        StringWriter.Write(TokenType.BACKTICK.GetSymbol());

        foreach (var inner in node.Segments)
        {
            if (inner is LiteralNode literal && literal.Literal is StringLiteral sliteral)
            {
                StringWriter.Write(sliteral.Value ?? string.Empty);
            }
            else
            {
                StringWriter.Write(TokenType.L_BRACE.GetSymbol());
                Visit(inner);
                StringWriter.Write(TokenType.R_BRACE.GetSymbol());
            }
        }

        StringWriter.Write(TokenType.BACKTICK.GetSymbol());
    }

    public void Visit(LiteralNode node)
    {
        if (node.Value.Literal == null)
            throw new InvalidOperationException($"Token '{node.Value}' does not represent a literal.");

        var literal = node.Value.Literal;

        if (literal is WhitespaceLiteral) { }
        else if (literal is CommentLiteral) { }
        else if (literal is VoidLiteral) { }
        else if (literal is NullLiteral lnull) StringWriter.Write(lnull);
        else if (literal is BooleanLiteral lbool) StringWriter.Write(lbool);
        else if (literal is IdLiteral lid) StringWriter.Write(lid);
        else if (literal is NumberLiteral lnumber) StringWriter.Write(lnumber);
        else if (literal is StringLiteral lstring) StringWriter.Write(lstring);
        else throw new NotImplementedException($"Cannot pretty print literal of type '{node.Value.Literal.GetType().Name}'");
    }

    public void Visit(StatementNode node)
    {
        if (node is ExpressionStatementNode exprnode)
            Visit(exprnode);
        else if (node is PrintStatementNode printnode)
            Visit(printnode);
        else
            throw new NotImplementedException($"Cannot pretty print node of type '{node.GetType().Name}'");
    }

    public void Visit(ExpressionStatementNode node)
    {
        Visit(node.Expression);
        StringWriter.Write(TokenType.SEMICOLON.GetSymbol());
    }

    public void Visit(PrintStatementNode node)
    {
        StringWriter.Write($"{TokenType.PRINT.GetSymbol()} ");
        Visit(node.Expression);
        StringWriter.Write(TokenType.SEMICOLON.GetSymbol());
    }
}