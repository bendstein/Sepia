using Interpreter.AST;
using Interpreter.AST.Node;
using Interpreter.Lex.Literal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Interpret;

public class Interpreter :
    IASTNodeVisitor<ASTNode, object>,
    IASTNodeVisitor<ExpressionNode, object>,
    IASTNodeVisitor<BinaryNode, object>,
    IASTNodeVisitor<GroupNode, object>,
    IASTNodeVisitor<UnaryPrefixNode, object>,
    IASTNodeVisitor<LiteralNode, object>
{
    public object Visit(ASTNode node)
    {
        if (node is ExpressionNode expressionNode)
            return Visit(expressionNode);
        else if (node is BinaryNode binaryNode)
            return Visit(binaryNode);
        else if (node is GroupNode groupNode)
            return Visit(groupNode);
        else if (node is UnaryPrefixNode unaryPrefixNode)
            return Visit(unaryPrefixNode);
        else if (node is LiteralNode literalNode)
            return Visit(literalNode);

        throw new NotImplementedException();
    }

    public object Visit(ExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public object Visit(BinaryNode node)
    {
        throw new NotImplementedException();
    }

    public object Visit(GroupNode node)
    {
        throw new NotImplementedException();
    }

    public object Visit(UnaryPrefixNode node)
    {
        throw new NotImplementedException();
    }

    public object Visit(LiteralNode node)
    {
        var literal = node.Literal;

        if(literal is BooleanLiteral bliteral)
        {
            
        }
        else if(literal is CommentLiteral commliteral)
        {

        }
        else if(literal is IdLiteral idliteral)
        {

        }
        else if(literal is NullLiteral nliteral)
        {

        }
        else if(literal is NumberLiteral numliteral)
        {

        }
        else if(literal is StringLiteral sliteral)
        {

        }
        else if(literal is WhitespaceLiteral wliteral)
        {

        }
        else
        {

        }

        throw new NotImplementedException();
    }
}