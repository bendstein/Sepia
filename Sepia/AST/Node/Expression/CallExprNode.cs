namespace Sepia.AST.Node.Expression;

public class CallExprNode : ExpressionNode, IASTNodeVisitable<CallExprNode>
{
    public ExpressionNode Callable { get; set; }

    public List<ExpressionNode> Arguments { get; set; }

    public CallExprNode(ExpressionNode callable, List<ExpressionNode> arguments)
    {
        Callable = callable;
        Arguments = arguments;
    }

    public void Accept(IASTNodeVisitor<CallExprNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<CallExprNode, TReturn> visitor) => visitor.Visit(this);
}