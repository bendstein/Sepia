namespace Interpreter.AST.Node;

public class GroupNode : ExpressionNode, IASTNodeVisitable<GroupNode>
{
    public ExpressionNode Inner { get; init; }

    public GroupNode(ExpressionNode inner)
    {
        Inner = inner;
    }

    public void Accept(IASTNodeVisitor<GroupNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<GroupNode, TReturn> visitor) => visitor.Visit(this);
}
