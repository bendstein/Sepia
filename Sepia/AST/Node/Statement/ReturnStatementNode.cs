using Sepia.AST.Node.Expression;
using Sepia.Lex;
using Sepia.Value;

namespace Sepia.AST.Node.Statement;

public class ReturnStatementNode : ControlFlowStatementNode, IASTNodeVisitable<ReturnStatementNode>
{
    public ExpressionNode Expression { get; }

    public ReturnStatementNode(Token Token, ExpressionNode Expression) : base(Token)
    {
        this.Expression = Expression;
    }

    public void Accept(IASTNodeVisitor<ReturnStatementNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ReturnStatementNode, TReturn> visitor) => visitor.Visit(this);
}