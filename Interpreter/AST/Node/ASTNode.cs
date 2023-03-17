namespace Interpreter.AST.Node;

public abstract class ASTNode : IASTNodeVisitable<ASTNode>
{
    public void Accept(IASTNodeVisitor<ASTNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ASTNode, TReturn> visitor) => visitor.Visit(this);
}