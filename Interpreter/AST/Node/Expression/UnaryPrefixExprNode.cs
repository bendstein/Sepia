using Interpreter.Lex;

namespace Interpreter.AST.Node.Expression;


public class UnaryPrefixExprNode : ExpressionNode, IASTNodeVisitable<UnaryPrefixExprNode>
{
    public Token Operator { get; init; }

    public ExpressionNode Right { get; init; }

    public UnaryPrefixExprNode(Token op, ExpressionNode right)
    {
        Operator = op;
        Right = right;
    }

    public void Accept(IASTNodeVisitor<UnaryPrefixExprNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<UnaryPrefixExprNode, TReturn> visitor) => visitor.Visit(this);
}
