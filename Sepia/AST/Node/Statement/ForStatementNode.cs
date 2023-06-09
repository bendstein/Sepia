using Sepia.AST.Node.Expression;

namespace Sepia.AST.Node.Statement;

public class ForStatementNode : StatementNode, IASTNodeVisitable<ForStatementNode>
{
    public DeclarationStmtNode? Declaration { get; init; }

    public ExpressionStmtNode? Condition { get; init; }

    public ExpressionNode? Action { get; init; }

    public Block Body { get; init; }

    public ForStatementNode(DeclarationStmtNode? declaration, ExpressionStmtNode? condition, ExpressionNode? action, Block body)
    {
        Declaration = declaration;
        Condition = condition;
        Action = action;
        Body = body;
    }

    public void Accept(IASTNodeVisitor<ForStatementNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ForStatementNode, TReturn> visitor) => visitor.Visit(this);
}