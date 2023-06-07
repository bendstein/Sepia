namespace Interpreter.AST.Node.Expression;

public class InterpolatedStringExprNode : ExpressionNode, IASTNodeVisitable<InterpolatedStringExprNode>
{
    public List<ExpressionNode> Segments { get; init; }

    public InterpolatedStringExprNode(List<ExpressionNode> Segments)
    {
        this.Segments = Segments;
    }

    public void Accept(IASTNodeVisitor<InterpolatedStringExprNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<InterpolatedStringExprNode, TReturn> visitor) => visitor.Visit(this);
}