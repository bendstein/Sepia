using Interpreter.AST;
using Interpreter.AST.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Interpret;

public class Interpreter :
    IASTNodeVisitor<ASTNode>,
    IASTNodeVisitor<ExpressionNode>,
    IASTNodeVisitor<BinaryNode>,
    IASTNodeVisitor<GroupNode>,
    IASTNodeVisitor<UnaryPrefixNode>,
    IASTNodeVisitor<LiteralNode>
{
    public void Visit(ASTNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(ExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(BinaryNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(GroupNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(UnaryPrefixNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(LiteralNode node)
    {
        throw new NotImplementedException();
    }
}