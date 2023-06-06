using Interpreter.AST.Node.Expression;

namespace Interpreter.AST.Node.Statement;

public class PrintStatementNode : StatementNode, IASTNodeVisitable<PrintStatementNode>
{
    public ExpressionNode Expression { get; init; }

    public PrintStatementNode(ExpressionNode expression)
    {
        Expression = expression;
    }

    public void Accept(IASTNodeVisitor<PrintStatementNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<PrintStatementNode, TReturn> visitor) => visitor.Visit(this);
}