using Sepia.AST.Node.Expression;

namespace Sepia.AST.Node.Statement;

public class ConditionalStatementNode : StatementNode, IASTNodeVisitable<ConditionalStatementNode>
{
    public List<(ExpressionNode condition, Block body)> Branches { get; init; }

    public Block? Else { get; init; }

    public ConditionalStatementNode(List<(ExpressionNode condition, Block body)> Branches, Block? Else = null)
    {
        this.Branches = Branches;
        this.Else = Else;
    }

    public void Accept(IASTNodeVisitor<ConditionalStatementNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ConditionalStatementNode, TReturn> visitor) => visitor.Visit(this);
}