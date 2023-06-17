using Sepia.Evaluate;
using Sepia.Value;
using Sepia.Value.Type;

namespace Sepia.Callable;

public interface ISepiaCallable
{
    public SepiaCallSignature CallSignature { get; }

    public SepiaEnvironment EnclosingEnvironment { get; set; }

    public SepiaValue Call(Evaluator evaluator, IEnumerable<SepiaValue> arguments);

    public ISepiaCallable WithEnvironment(SepiaEnvironment env)
    {
        EnclosingEnvironment = env;
        return this;
    }
}