using Sepia.AST.Node.Statement;
using Sepia.Callable;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Lex.Literal;
using Sepia.Value;
using Sepia.Value.Type;

namespace Sepia.AST.Node.Expression;

public class FunctionExpressionNode : ExpressionNode, IASTNodeVisitable<FunctionExpressionNode>
{
    public IEnumerable<(IdLiteral id, SepiaTypeInfo type)> Arguments { get; init; }

    public SepiaTypeInfo ReturnType { get; init; }

    public Block Body { get; init; }

    public FunctionExpressionNode(IEnumerable<(IdLiteral id, SepiaTypeInfo type)> arguments, SepiaTypeInfo returnType, Block body)
    {
        Arguments = arguments;
        ReturnType = returnType;
        Body = body;
    }

    public void Accept(IASTNodeVisitor<FunctionExpressionNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<FunctionExpressionNode, TReturn> visitor) => visitor.Visit(this);
}