using Sepia.Value;

namespace Sepia.AST.Node.Expression;

public class ValueExpressionNode : ExpressionNode, IASTNodeVisitable<ValueExpressionNode>
{
    public SepiaValue Value { get; init; }

    public ValueExpressionNode(SepiaValue value)
    {
        Value = value;
    }

    public void Accept(IASTNodeVisitor<ValueExpressionNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ValueExpressionNode, TReturn> visitor) => visitor.Visit(this);
}
