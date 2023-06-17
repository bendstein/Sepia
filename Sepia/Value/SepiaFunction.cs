using Sepia.AST.Node.Expression;
using Sepia.AST.Node.Statement;
using Sepia.Callable;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Lex;
using Sepia.Lex.Literal;
using Sepia.Value.Type;

namespace Sepia.Value;
public class SepiaFunction : ISepiaCallable
{
    public IEnumerable<(IdLiteral id, SepiaTypeInfo type)> Arguments { get; init; }

    public SepiaTypeInfo ReturnType { get; init; }

    public SepiaCallSignature CallSignature => new(Arguments.Select(a => a.type).ToList(), ReturnType);

    public Block Body { get; init; }

    public SepiaEnvironment EnclosingEnvironment { get; set; }

    public SepiaFunction(IEnumerable<(IdLiteral id, SepiaTypeInfo type)> arguments, 
        SepiaTypeInfo returnType, Block body, SepiaEnvironment environment)
    {
        Arguments = arguments;
        ReturnType = returnType;
        Body = body;
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

            return evaluator.Visit(Body);
        }
        finally
        {
            evaluator.environment = current_env;
        }
    }

    public override string ToString()
    {
        var args_string = !Arguments.Any() ? string.Empty : Arguments.Select(a => $"{a.id.Value}{TokenType.COLON.GetSymbol()} {a.type}")
            .Aggregate((a, b) => $"{a}{TokenType.COMMA.GetSymbol()} {b}");
        var return_string = ReturnType == SepiaTypeInfo.Void(false) ? string.Empty : ReturnType.ToString();

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
