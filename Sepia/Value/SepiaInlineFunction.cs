using Sepia.AST.Node.Expression;
using Sepia.AST.Node.Statement;
using Sepia.Callable;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Lex;
using Sepia.Lex.Literal;
using Sepia.Value.Type;

namespace Sepia.Value;
public class SepiaInlineFunction : ISepiaCallable
{
    public IEnumerable<(IdLiteral id, SepiaTypeInfo type)> Arguments { get; init; }

    public ExpressionNode Expression { get; init; }

    public SepiaCallSignature CallSignature => new(Arguments.Select(a => a.type).ToList(), Expression.ResolveInfo.Type);

    public SepiaEnvironment EnclosingEnvironment { get; set; }

    public SepiaInlineFunction(IEnumerable<(IdLiteral id, SepiaTypeInfo type)> arguments,
        ExpressionNode expression, SepiaEnvironment environment)
    {
        Arguments = arguments;
        Expression = expression;
        EnclosingEnvironment = environment;
    }

    public SepiaValue Call(Evaluator evaluator, IEnumerable<SepiaValue> arguments)
    {
        var current_env = evaluator.environment;
        try
        {
            evaluator.environment = new(EnclosingEnvironment);
            if (arguments.Count() != Arguments.Count())
                throw new SepiaException(new EvaluateError());

            List<Exception> exceptions = new();

            for (int i = 0; i < arguments.Count(); i++)
            {
                var argument = arguments.ElementAt(i);
                (var id, var expectedType) = Arguments.ElementAt(i);

                if (argument.Type != expectedType)
                {
                    exceptions.Add(new SepiaException(new EvaluateError()));
                }

                evaluator.Visit(new DeclarationStmtNode(id, expectedType, new ValueExpressionNode(argument)));
            }

            if (exceptions.Any())
            {
                if (exceptions.Count == 1)
                {
                    throw exceptions[0];
                }
                else
                {
                    throw new AggregateException(exceptions);
                }
            }

            return evaluator.Visit(Expression);
        }
        finally
        {
            evaluator.environment = current_env;
        }
    }

    public override string ToString()
    {
        var args_string = !Arguments.Any() ? string.Empty : Arguments.Select(a => $"{a.id.ResolveInfo.Name}{TokenType.COLON.GetSymbol()} {a.type}")
            .Aggregate((a, b) => $"{a}{TokenType.COMMA.GetSymbol()} {b}");
        var return_string = Expression.ResolveInfo.Type == SepiaTypeInfo.TypeVoid(false) ? string.Empty : Expression.ResolveInfo.Type.ToString();

        if (string.IsNullOrWhiteSpace(args_string))
        {
            if (string.IsNullOrWhiteSpace(return_string))
            {
                return $"{TokenType.FUNC.GetSymbol()}{TokenType.L_PAREN.GetSymbol()}{TokenType.R_PAREN.GetSymbol()}";
            }
            else
            {
                return $"{TokenType.FUNC.GetSymbol()}{TokenType.L_PAREN.GetSymbol()}{TokenType.R_PAREN.GetSymbol()}{TokenType.COLON.GetSymbol()} {return_string}";
            }
        }
        else if (string.IsNullOrWhiteSpace(return_string))
        {
            return $"{TokenType.FUNC.GetSymbol()}{TokenType.L_PAREN.GetSymbol()}{args_string}{TokenType.R_PAREN.GetSymbol()}";
        }
        else
        {
            return $"{TokenType.FUNC.GetSymbol()}{TokenType.L_PAREN.GetSymbol()}{args_string}{TokenType.R_PAREN.GetSymbol()}{TokenType.COLON.GetSymbol()} {return_string}";
        }
    }
}
