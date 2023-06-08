using Sepia.Lex.Literal;

namespace Sepia.AST.Node.Expression;

public class AssignmentExprNode : ExpressionNode, IASTNodeVisitable<AssignmentExprNode>
{
    public IdLiteral Id { get; init; }

    public ExpressionNode Assignment { get; init; }

    public AssignmentExprNode(IdLiteral Id, ExpressionNode Assignment)
    {
        this.Id = Id;
        this.Assignment = Assignment;
    }

    public void Accept(IASTNodeVisitor<AssignmentExprNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<AssignmentExprNode, TReturn> visitor) => visitor.Visit(this);
}