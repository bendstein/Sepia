using Interpreter.Lex;
using Interpreter.Lex.Literal;

namespace Interpreter.AST.Node.Expression;

public class LiteralNode : ExpressionNode, IASTNodeVisitable<LiteralNode>
{
    public Token Value { get; init; }

    public LiteralBase Literal => Value.Literal!;

    public LiteralNode(Token value)
    {
        Value = value;
    }

    public void Accept(IASTNodeVisitor<LiteralNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<LiteralNode, TReturn> visitor) => visitor.Visit(this);
}
