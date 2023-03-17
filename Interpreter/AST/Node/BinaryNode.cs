using Interpreter.Lex;

namespace Interpreter.AST.Node;

public class BinaryNode : ExpressionNode, IASTNodeVisitable<BinaryNode>
{
    public ExpressionNode Left { get; init; }

    public Token Operator { get; init; }

    public ExpressionNode Right { get; init; }

    public BinaryNode(ExpressionNode left, Token op, ExpressionNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public void Accept(IASTNodeVisitor<BinaryNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<BinaryNode, TReturn> visitor) => visitor.Visit(this);
}