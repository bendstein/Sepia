using Sepia.Lex.Literal;

namespace Sepia.AST.Node.Statement;
public class ClassDeclarationStmtNode : StatementNode, IASTNodeVisitable<ClassDeclarationStmtNode>
{
    public IdLiteral Id { get; init; }

    public List<StatementNode> ClassMembers { get; init; }

    public ClassDeclarationStmtNode(IdLiteral id, List<StatementNode> classMembers)
    {
        Id = id;
        ClassMembers = classMembers;
    }

    public void Accept(IASTNodeVisitor<ClassDeclarationStmtNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ClassDeclarationStmtNode, TReturn> visitor) => visitor.Visit(this);
}