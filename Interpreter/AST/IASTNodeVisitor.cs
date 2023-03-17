using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.AST;

public interface IASTNodeVisitor<TNode>
{
    void Visit(TNode node);
}

public interface IASTNodeVisitor<TNode, TReturn>
{
    TReturn Visit(TNode node);
}