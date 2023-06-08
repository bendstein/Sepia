using Sepia.AST.Node.Expression;

namespace Sepia.AST.Node.Statement;

public class WhileStatementNode : StatementNode, IASTNodeVisitable<WhileStatementNode>
{
    public ExpressionNode Condition { get; init; }

    public Block Body { get; init; }

    public WhileStatementNode(ExpressionNode Condition, Block Body)
    {
        this.Condition = Condition;
        this.Body = Body;
    }

    public void Accept(IASTNodeVisitor<WhileStatementNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<WhileStatementNode, TReturn> visitor) => visitor.Visit(this);
}