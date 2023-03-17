using Interpreter.AST.Node;

namespace Interpreter.AST;
public class AbstractSyntaxTree
{
    public ASTNode Root { get; set; }

    public AbstractSyntaxTree(ASTNode root)
    {
        Root = root;
    }
}