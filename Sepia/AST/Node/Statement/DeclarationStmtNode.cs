using Sepia.AST.Node.Expression;
using Sepia.Lex.Literal;
using Sepia.Value.Type;

namespace Sepia.AST.Node.Statement;

public class DeclarationStmtNode : StatementNode, IASTNodeVisitable<DeclarationStmtNode>
{
    public IdLiteral Id { get; init; }

    public SepiaTypeInfo? Type { get; init; }

    public ExpressionNode? Assignment { get; init; }

    public DeclarationStmtNode(IdLiteral Id, SepiaTypeInfo? Type = null, ExpressionNode? Assignment = null)
    {
        this.Id = Id;
        this.Type = Type;
        this.Assignment = Assignment;
    }

    public void Accept(IASTNodeVisitor<DeclarationStmtNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<DeclarationStmtNode, TReturn> visitor) => visitor.Visit(this);
}