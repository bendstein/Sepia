using Interpreter.AST.Node.Statement;

namespace Interpreter.AST.Node;

public class ProgramNode : ASTNode, IASTNodeVisitable<ProgramNode>
{
    public List<StatementNode> statements { get; init; }

    public ProgramNode(List<StatementNode> statements)
    {
        this.statements = statements;
    }

    public void Accept(IASTNodeVisitor<ProgramNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ProgramNode, TReturn> visitor) => visitor.Visit(this);
}