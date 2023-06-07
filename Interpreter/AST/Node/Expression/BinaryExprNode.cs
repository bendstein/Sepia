using Interpreter.Lex;

namespace Interpreter.AST.Node.Expression;

public class BinaryExprNode : ExpressionNode, IASTNodeVisitable<BinaryExprNode>
{
    public ExpressionNode Left { get; init; }

    public Token Operator { get; init; }

    public ExpressionNode Right { get; init; }

    public BinaryExprNode(ExpressionNode left, Token op, ExpressionNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public void Accept(IASTNodeVisitor<BinaryExprNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<BinaryExprNode, TReturn> visitor) => visitor.Visit(this);
}