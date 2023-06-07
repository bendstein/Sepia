using Interpreter.AST.Node.Expression;
using Interpreter.Lex.Literal;

namespace Interpreter.AST.Node.Statement;

public class DeclarationStmtNode : StatementNode, IASTNodeVisitable<DeclarationStmtNode>
{
    public IdLiteral Id { get; init; }

    public ExpressionNode? Assignment { get; init; }

    public DeclarationStmtNode(IdLiteral Id, ExpressionNode? Assignment = null)
    {
        this.Id = Id;
        this.Assignment = Assignment;
    }

    public void Accept(IASTNodeVisitor<DeclarationStmtNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<DeclarationStmtNode, TReturn> visitor) => visitor.Visit(this);
}