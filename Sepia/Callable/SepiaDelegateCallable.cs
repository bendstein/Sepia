using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Value;
using Sepia.Value.Type;

namespace Sepia.Callable;

public class SepiaDelegateCallable : ISepiaCallable
{
    public IEnumerable<SepiaTypeInfo> argumentTypes { get; private set; }

    public SepiaTypeInfo ReturnType { get; private set; }

    public Func<Evaluator, IEnumerable<SepiaValue>, SepiaValue> Delegate { get; private set; }

    public SepiaEnvironment EnclosingEnvironment { get; set; }

    public SepiaDelegateCallable()
    {
        argumentTypes = Enumerable.Empty<SepiaTypeInfo>();
        ReturnType = SepiaTypeInfo.Void;
        Delegate = (Evaluator interpreter, IEnumerable<SepiaValue> args) => SepiaValue.Void;
        EnclosingEnvironment = new();
    }

    public SepiaDelegateCallable(IEnumerable<SepiaTypeInfo> ArgumentTypes, SepiaTypeInfo returnType,
        Func<Evaluator, IEnumerable<SepiaValue>, SepiaValue> del)
    {
        argumentTypes = ArgumentTypes;
        ReturnType = returnType;
        Delegate = del;
        EnclosingEnvironment = new();
    }

    public SepiaValue Call(Evaluator evaluator, IEnumerable<SepiaValue> arguments)
    {
        var current_env = evaluator.environment;
        try
        {
            evaluator.environment = new(EnclosingEnvironment);

            if (arguments.Count() != argumentTypes.Count())
                throw new SepiaException(new EvaluateError());

            List<Exception> exceptions = new();

            for (int i = 0; i < arguments.Count(); i++)
            {
                var argument = arguments.ElementAt(i);
                var expectedType = argumentTypes.ElementAt(i);

                if (argument.Type != expectedType)
                {
                    exceptions.Add(new SepiaException(new EvaluateError()));
                }
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

            return Delegate(evaluator, arguments);
        }
        finally
        {
            evaluator.environment = current_env;
        }
    }

    public override string ToString() => $"<native function>";
}