using Interpreter.Lex;

namespace Interpreter.AST.Node;

public class LiteralNode : ExpressionNode, IASTNodeVisitable<LiteralNode>
{
    public Token Value { get; init; }

    public LiteralNode(Token value)
    {
        Value = value;
    }

    public void Accept(IASTNodeVisitor<LiteralNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<LiteralNode, TReturn> visitor) => visitor.Visit(this);
}
