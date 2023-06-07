using Sepia.Lex.Literal;

namespace Sepia.AST.Node.Expression;

public class IdentifierExprNode : ExpressionNode, IASTNodeVisitable<IdentifierExprNode>
{
    public IdLiteral Id { get; init; }

    public IdentifierExprNode(IdLiteral Id)
    {
        this.Id = Id;
    }

    public void Accept(IASTNodeVisitor<IdentifierExprNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<IdentifierExprNode, TReturn> visitor) => visitor.Visit(this);
}