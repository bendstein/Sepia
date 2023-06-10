using Sepia.Evaluate;
using Sepia.Value;
using Sepia.Value.Type;

namespace Sepia.Callable;

public interface ISepiaCallable
{
    public IEnumerable<SepiaTypeInfo> argumentTypes { get; }

    public SepiaTypeInfo ReturnType { get; }

    public SepiaValue Call(Evaluator evaluator, IEnumerable<SepiaValue> arguments);
}