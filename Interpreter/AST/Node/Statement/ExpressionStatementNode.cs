using Interpreter.AST.Node.Expression;

namespace Interpreter.AST.Node.Statement;

public class ExpressionStatementNode : StatementNode, IASTNodeVisitable<ExpressionStatementNode>
{
    public ExpressionNode Expression { get; init; }

    public ExpressionStatementNode(ExpressionNode expression)
    {
        Expression = expression;
    }

    public void Accept(IASTNodeVisitor<ExpressionStatementNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ExpressionStatementNode, TReturn> visitor) => visitor.Visit(this);
}