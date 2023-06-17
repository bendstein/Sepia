using Sepia.AST.Node.Statement;
using Sepia.Lex;

namespace Sepia.Common;

public class SepiaControlFlow : Exception
{
    public ControlFlowStatementNode Control { get; private set; }

    public SepiaControlFlow(ControlFlowStatementNode control)
    {
        Control = control;
    }
}
