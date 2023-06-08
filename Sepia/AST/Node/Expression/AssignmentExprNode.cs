using Sepia.Lex;
using Sepia.Lex.Literal;

namespace Sepia.AST.Node.Expression;

public class AssignmentExprNode : ExpressionNode, IASTNodeVisitable<AssignmentExprNode>
{
    public IdLiteral Id { get; init; }

    public Token AssignmentType { get; init; }

    public ExpressionNode Assignment { get; init; }

    public AssignmentExprNode(IdLiteral Id, Token AssignmentType, ExpressionNode Assignment)
    {
        this.Id = Id;
        this.AssignmentType = AssignmentType;
        this.Assignment = Assignment;
    }

    public void Accept(IASTNodeVisitor<AssignmentExprNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<AssignmentExprNode, TReturn> visitor) => visitor.Visit(this);
}