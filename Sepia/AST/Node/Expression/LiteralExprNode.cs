﻿using Sepia.Lex;
using Sepia.Lex.Literal;

namespace Sepia.AST.Node.Expression;

public class LiteralExprNode : ExpressionNode, IASTNodeVisitable<LiteralExprNode>
{
    public Token Value { get; init; }

    public LiteralBase Literal => Value.Literal!;

    public LiteralExprNode(Token value)
    {
        Value = value;
    }

    public void Accept(IASTNodeVisitor<LiteralExprNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<LiteralExprNode, TReturn> visitor) => visitor.Visit(this);
}
