namespace Interpreter.AST.Node;

public abstract class ExpressionNode : ASTNode, IASTNodeVisitable<ExpressionNode>
{
    public void Accept(IASTNodeVisitor<ExpressionNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ExpressionNode, TReturn> visitor) => visitor.Visit(this);
}