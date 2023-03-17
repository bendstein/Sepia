using Interpreter.AST.Node;
using Interpreter.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpreter.Lex.Literal;
using Interpreter.Lex;

namespace Interpreter.Utility;

public class PrettyPrinter :
    IASTNodeVisitor<ASTNode>,
    IASTNodeVisitor<ExpressionNode>,
    IASTNodeVisitor<BinaryNode>,
    IASTNodeVisitor<GroupNode>,
    IASTNodeVisitor<UnaryPrefixNode>,
    IASTNodeVisitor<LiteralNode>
{
    private readonly StringWriter StringWriter;

    public PrettyPrinter(StringWriter stringWriter)
    {
        StringWriter = stringWriter;
    }

    public void Visit(ASTNode node)
    {
        if (node is ExpressionNode enode) Visit(enode);
        else throw new NotImplementedException($"Cannot pretty print node of type '{node.GetType().Name}'");
    }

    public void Visit(ExpressionNode node)
    {
        if (node is LiteralNode lnode) Visit(lnode);
        else if (node is UnaryPrefixNode upnode) Visit(upnode);
        else if (node is GroupNode gnode) Visit(gnode);
        else if (node is BinaryNode bnode) Visit(bnode);
        else throw new NotImplementedException($"Cannot pretty print node of type '{node.GetType().Name}'");
    }

    public void Visit(BinaryNode node)
    {
        Visit(node.Left);
        StringWriter.Write($" {node.Operator.TokenType.GetSymbol()?? string.Empty} ");
        Visit(node.Right);
    }

    public void Visit(GroupNode node)
    {
        StringWriter.Write(TokenType.L_PAREN.GetSymbol() ?? string.Empty);
        Visit(node.Inner);
        StringWriter.Write(TokenType.R_PAREN.GetSymbol() ?? string.Empty);
    }

    public void Visit(UnaryPrefixNode node)
    {
        StringWriter.Write(node.Operator.TokenType.GetSymbol()?? string.Empty);
        Visit(node.Right);
    }

    public void Visit(LiteralNode node)
    {
        if (node.Value.Literal == null)
            throw new InvalidOperationException($"Token '{node.Value}' does not represent a literal.");

        var literal = node.Value.Literal;

        if (literal is WhitespaceLiteral lwhitespace) { }
        else if (literal is CommentLiteral lcomment) { }
        else if (literal is NullLiteral lnull) StringWriter.Write(lnull);
        else if (literal is BooleanLiteral lbool) StringWriter.Write(lbool);
        else if (literal is IdLiteral lid) StringWriter.Write(lid);
        else if (literal is NumberLiteral lnumber) StringWriter.Write(lnumber);
        else if (literal is StringLiteral lstring) StringWriter.Write(lstring);
        else throw new NotImplementedException($"Cannot pretty print literal of type '{node.Value.Literal.GetType().Name}'");
    }
}