using Interpreter.AST.Node.Expression;

namespace Interpreter.AST.Node.Statement;

public abstract class StatementNode : ASTNode, IASTNodeVisitable<StatementNode>
{
    public void Accept(IASTNodeVisitor<StatementNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<StatementNode, TReturn> visitor) => visitor.Visit(this);
}