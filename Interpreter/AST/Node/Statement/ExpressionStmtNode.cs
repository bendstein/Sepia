using Interpreter.AST.Node.Expression;

namespace Interpreter.AST.Node.Statement;

public class ExpressionStmtNode : StatementNode, IASTNodeVisitable<ExpressionStmtNode>
{
    public ExpressionNode Expression { get; init; }

    public ExpressionStmtNode(ExpressionNode expression)
    {
        Expression = expression;
    }

    public void Accept(IASTNodeVisitor<ExpressionStmtNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ExpressionStmtNode, TReturn> visitor) => visitor.Visit(this);
}