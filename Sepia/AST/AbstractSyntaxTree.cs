using Sepia.AST.Node;

namespace Sepia.AST;
public class AbstractSyntaxTree
{
    public ASTNode Root { get; set; }

    public AbstractSyntaxTree(ASTNode root)
    {
        Root = root;
    }
}