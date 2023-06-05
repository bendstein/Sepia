namespace Interpreter.AST.Node;

public class InterpolatedStringNode : ExpressionNode, IASTNodeVisitable<InterpolatedStringNode>
{
    public List<ExpressionNode> Segments { get; init; }

    public InterpolatedStringNode(List<ExpressionNode> Segments)
    {
        this.Segments = Segments;
    }

    public void Accept(IASTNodeVisitor<InterpolatedStringNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<InterpolatedStringNode, TReturn> visitor) => visitor.Visit(this);
}