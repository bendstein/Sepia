using Interpreter.Lex;

namespace Interpreter.AST.Node;


public class UnaryPrefixNode : ExpressionNode, IASTNodeVisitable<UnaryPrefixNode>
{
    public Token Operator { get; init; }

    public ExpressionNode Right { get; init; }

    public UnaryPrefixNode(Token op, ExpressionNode right)
    {
        Operator = op;
        Right = right;
    }

    public void Accept(IASTNodeVisitor<UnaryPrefixNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<UnaryPrefixNode, TReturn> visitor) => visitor.Visit(this);
}
