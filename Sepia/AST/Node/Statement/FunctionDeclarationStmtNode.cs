using Sepia.AST.Node.Expression;
using Sepia.Lex.Literal;
using Sepia.Value.Type;

namespace Sepia.AST.Node.Statement;

public class FunctionDeclarationStmtNode : StatementNode, IASTNodeVisitable<FunctionDeclarationStmtNode>
{
    public IdLiteral Id { get; init; }

    public FunctionExpressionNode Function { get; init; }

    public FunctionDeclarationStmtNode(IdLiteral id, FunctionExpressionNode function)
    {
        Id = id;
        Function = function;
    }

    public void Accept(IASTNodeVisitor<FunctionDeclarationStmtNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<FunctionDeclarationStmtNode, TReturn> visitor) => visitor.Visit(this);
}