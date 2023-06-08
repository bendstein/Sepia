namespace Sepia.AST.Node.Statement;

public class Block : StatementNode, IASTNodeVisitable<Block>
{
    public List<StatementNode> Statements { get; init; }

    public Block(List<StatementNode> statements)
    {
        Statements = statements;
    }

    public void Accept(IASTNodeVisitor<Block> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<Block, TReturn> visitor) => visitor.Visit(this);
}