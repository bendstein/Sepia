namespace Sepia.AST.Node.Expression;

public class GroupExprNode : ExpressionNode, IASTNodeVisitable<GroupExprNode>
{
    public ExpressionNode Inner { get; init; }

    public GroupExprNode(ExpressionNode inner)
    {
        Inner = inner;
    }

    public void Accept(IASTNodeVisitor<GroupExprNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<GroupExprNode, TReturn> visitor) => visitor.Visit(this);
}
