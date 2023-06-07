namespace Sepia.AST;

public interface IASTNodeVisitor<TNode>
{
    void Visit(TNode node);
}

public interface IASTNodeVisitor<TNode, TReturn>
{
    TReturn Visit(TNode node);
}