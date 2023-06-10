using Sepia.AST.Node.Expression;
using Sepia.Lex;
using Sepia.Value;

namespace Sepia.AST.Node.Statement;

public class ReturnStatementNode : StatementNode, IASTNodeVisitable<ReturnStatementNode>
{
    public Token Token { get; }

    public ExpressionNode Expression { get; }

    public ReturnStatementNode(Token Token, ExpressionNode Expression)
    {
        this.Token = Token;
        this.Expression = Expression;
    }

    public void Accept(IASTNodeVisitor<ReturnStatementNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ReturnStatementNode, TReturn> visitor) => visitor.Visit(this);
}