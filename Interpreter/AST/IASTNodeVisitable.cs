namespace Interpreter.AST;

public interface IASTNodeVisitable<TNode>
{
    void Accept(IASTNodeVisitor<TNode> visitor);

    TReturn Accept<TReturn>(IASTNodeVisitor<TNode, TReturn> visitor);
}