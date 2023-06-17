using Sepia.AST.Node.Statement;
using Sepia.Lex;
using Sepia.Value;

namespace Sepia.Common;
public class SepiaReturn : SepiaControlFlow
{
    public SepiaValue Value { get; init; }

    public SepiaReturn(ControlFlowStatementNode control, SepiaValue value) : base(control)
    {
        Value = value;
    }
}