using Sepia.Analyzer;

namespace Sepia.AST.Node;

public abstract class ASTNode : IASTNodeVisitable<ASTNode>
{
    public virtual ResolveInfo ResolveInfo { get; set; } = new();

    public void Accept(IASTNodeVisitor<ASTNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ASTNode, TReturn> visitor) => visitor.Visit(this);
}