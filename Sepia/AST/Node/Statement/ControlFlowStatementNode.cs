using Sepia.Lex;

namespace Sepia.AST.Node.Statement;

public class ControlFlowStatementNode : StatementNode, IASTNodeVisitable<ControlFlowStatementNode>
{
    public Token Token { get; }

    public ControlFlowStatementNode(Token Token)
    {
        this.Token = Token;
    }

    public void Accept(IASTNodeVisitor<ControlFlowStatementNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ControlFlowStatementNode, TReturn> visitor) => visitor.Visit(this);
}