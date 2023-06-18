using Sepia.AST.Node.Statement;
using Sepia.Callable;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Lex.Literal;
using Sepia.Value;
using Sepia.Value.Type;

namespace Sepia.AST.Node.Expression;

public class InlineFunctionExpressionNode : ExpressionNode, IASTNodeVisitable<InlineFunctionExpressionNode>
{
    public IEnumerable<(IdLiteral id, SepiaTypeInfo type)> Arguments { get; init; }

    public ExpressionNode Expression { get; init; }

    public InlineFunctionExpressionNode(IEnumerable<(IdLiteral id, SepiaTypeInfo type)> arguments, ExpressionNode expression)
    {
        Arguments = arguments;
        Expression = expression;
    }

    public void Accept(IASTNodeVisitor<InlineFunctionExpressionNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<InlineFunctionExpressionNode, TReturn> visitor) => visitor.Visit(this);
}