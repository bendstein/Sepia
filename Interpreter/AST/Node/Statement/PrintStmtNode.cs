using Interpreter.AST.Node.Expression;

namespace Interpreter.AST.Node.Statement;

public class PrintStmtNode : StatementNode, IASTNodeVisitable<PrintStmtNode>
{
    public ExpressionNode Expression { get; init; }

    public PrintStmtNode(ExpressionNode expression)
    {
        Expression = expression;
    }

    public void Accept(IASTNodeVisitor<PrintStmtNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<PrintStmtNode, TReturn> visitor) => visitor.Visit(this);
}