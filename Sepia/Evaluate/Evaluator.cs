using Sepia.AST;
using Sepia.AST.Node;
using Sepia.AST.Node.Expression;
using Sepia.AST.Node.Statement;
using Sepia.Common;
using Sepia.Lex;
using Sepia.Lex.Literal;
using System.Text;

namespace Sepia.Evaluate;

public class Evaluator :
    IASTNodeVisitor<AbstractSyntaxTree, object>,
    IASTNodeVisitor<ASTNode, object>,
    IASTNodeVisitor<ProgramNode, object>,
    IASTNodeVisitor<ExpressionNode, object>,
    IASTNodeVisitor<BinaryExprNode, object>,
    IASTNodeVisitor<GroupExprNode, object>,
    IASTNodeVisitor<UnaryPrefixExprNode, object>,
    IASTNodeVisitor<InterpolatedStringExprNode, object>,
    IASTNodeVisitor<LiteralExprNode, object>,
    IASTNodeVisitor<IdentifierExprNode, object>,
    IASTNodeVisitor<StatementNode, object>,
    IASTNodeVisitor<ExpressionStmtNode, object>,
    IASTNodeVisitor<PrintStmtNode, object>,
    IASTNodeVisitor<DeclarationStmtNode, object>,
    IASTNodeVisitor<AssignmentExprNode, object>,
    IASTNodeVisitor<Block, object>
{
    private Environment environment = new();

    public object Visit(AbstractSyntaxTree tree)
    {
        return Visit(tree.Root);
    }

    public object Visit(ASTNode node)
    {
        if (node is ProgramNode pnode)
            return Visit(pnode);
        else if (node is ExpressionNode expressionNode)
            return Visit(expressionNode);
        else if (node is StatementNode snode)
            return Visit(snode);
        throw new NotImplementedException();
    }

    public object Visit(ProgramNode node)
    {
        object? last_result = null;
        foreach (var statement in node.statements)
            last_result = Visit(statement);

        return last_result ?? VoidLiteral.Instance;
    }

    public object Visit(ExpressionNode node)
    {
        if (node is BinaryExprNode binaryNode)
            return Visit(binaryNode);
        else if (node is GroupExprNode groupNode)
            return Visit(groupNode);
        else if (node is UnaryPrefixExprNode unaryPrefixNode)
            return Visit(unaryPrefixNode);
        else if (node is InterpolatedStringExprNode interpolatedNode)
            return Visit(interpolatedNode);
        else if (node is LiteralExprNode literalNode)
            return Visit(literalNode);
        else if (node is IdentifierExprNode idNode)
            return Visit(idNode);
        else if (node is AssignmentExprNode assignnode)
            return Visit(assignnode);
        throw new SepiaException(new EvaluateError($"Cannot evaluate node of type '{node.GetType().Name}'."));
    }

    public object Visit(BinaryExprNode node)
    {
        var eval_left = () => Visit(node.Left);
        var eval_right = () => Visit(node.Right);

        switch (node.Operator.TokenType)
        {
            case TokenType.GREATER:
            {
                object left = eval_left();

                if(IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft > iright;
                    }
                    else if (right is float fright)
                    {
                        return ileft > fright;
                    }
                }
                else if (left is float fleft)
                {
                    if (right is int iright)
                    {
                        return fleft > iright;
                    }
                    else if (right is float fright)
                    {
                        return fleft > fright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.GREATER_EQUAL:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft >= iright;
                    }
                    else if (right is float fright)
                    {
                        return ileft >= fright;
                    }
                }
                else if (left is float fleft)
                {
                    if (right is int iright)
                    {
                        return fleft >= iright;
                    }
                    else if (right is float fright)
                    {
                        return fleft >= fright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.LESS:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft < iright;
                    }
                    else if (right is float fright)
                    {
                        return ileft < fright;
                    }
                }
                else if (left is float fleft)
                {
                    if (right is int iright)
                    {
                        return fleft < iright;
                    }
                    else if (right is float fright)
                    {
                        return fleft < fright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.LESS_EQUAL:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft <= iright;
                    }
                    else if (right is float fright)
                    {
                        return ileft <= fright;
                    }
                }
                else if (left is float fleft)
                {
                    if (right is int iright)
                    {
                        return fleft <= iright;
                    }
                    else if (right is float fright)
                    {
                        return fleft <= fright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.EQUAL_EQUAL:
            {
                object left = eval_left();
                object right = eval_right();

                if (IsNull(left))
                {
                    return IsNull(right);
                }
                else if(IsNull(right))
                {
                    return false;
                }

                return left.Equals(right);
            }
            case TokenType.BANG_EQUAL:
            {
                object left = eval_left();
                object right = eval_right();

                if (IsNull(left))
                {
                    return !IsNull(right);
                }
                else if(IsNull(right))
                {
                    return true;
                }

                return !left.Equals(right);
            }
            case TokenType.PLUS:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft + iright;
                    }
                    else if (right is float fright)
                    {
                        return ileft + fright;
                    }
                }
                else if (left is float fleft)
                {
                    if (right is int iright)
                    {
                        return fleft + iright;
                    }
                    else if (right is float fright)
                    {
                        return fleft + fright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.MINUS:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft - iright;
                    }
                    else if (right is float fright)
                    {
                        return ileft - fright;
                    }
                }
                else if (left is float fleft)
                {
                    if (right is int iright)
                    {
                        return fleft - iright;
                    }
                    else if (right is float fright)
                    {
                        return fleft - fright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.SLASH:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft / iright;
                    }
                    else if (right is float fright)
                    {
                        return ileft / fright;
                    }
                }
                else if (left is float fleft)
                {
                    if (right is int iright)
                    {
                        return fleft / iright;
                    }
                    else if (right is float fright)
                    {
                        return fleft / fright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.STAR:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft * iright;
                    }
                    else if (right is float fright)
                    {
                        return ileft * fright;
                    }
                }
                else if (left is float fleft)
                {
                    if (right is int iright)
                    {
                        return fleft * iright;
                    }
                    else if (right is float fright)
                    {
                        return fleft * fright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.PERCENT:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft % iright;
                    }
                    else if (right is float fright)
                    {
                        return ileft % fright;
                    }
                }
                else if (left is float fleft)
                {
                    if (right is int iright)
                    {
                        return fleft % iright;
                    }
                    else if (right is float fright)
                    {
                        return fleft % fright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.AMP_AMP:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }
                else if(left is bool bleft)
                {
                    //Short circuit
                    if(!bleft)
                    {
                        return false;
                    }

                    object right = eval_right();

                    if (IsNull(right))
                    {
                        return NullLiteral.Instance;
                    }

                    if(right is bool bright)
                    {
                        return bright;
                    }
                    else
                    {
                        throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
                    }
                }
                else
                {
                    object right = eval_right();
                    throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
                }
            }
            case TokenType.PIPE_PIPE:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }
                else if (left is bool bleft)
                {
                    //Short circuit
                    if (bleft)
                    {
                        return true;
                    }

                    object right = eval_right();

                    if (IsNull(right))
                    {
                        return NullLiteral.Instance;
                    }

                    if (right is bool bright)
                    {
                        return bright;
                    }
                    else
                    {
                        throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
                    }
                }
                else
                {
                    object right = eval_right();
                    throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
                }
            }
            case TokenType.AMP:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft & iright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.CARET:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft ^ iright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.PIPE:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft | iright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.LESS_LESS:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft << iright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.GREATER_GREATER:
            {
                object left = eval_left();

                if (IsNull(left))
                {
                    return NullLiteral.Instance;
                }

                object right = eval_right();

                if (IsNull(right))
                {
                    return NullLiteral.Instance;
                }

                if (left is int ileft)
                {
                    if (right is int iright)
                    {
                        return ileft >> iright;
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
        }

        throw new SepiaException(new EvaluateError($"'{node.Operator.TokenType.GetSymbol()}' is not a valid binary operator."));
    }

    public object Visit(GroupExprNode node)
    {
        return Visit(node.Inner);
    }

    public object Visit(UnaryPrefixExprNode node)
    {
        object inner = Visit(node.Right);

        switch (node.Operator.TokenType)
        {
            case TokenType.MINUS:
                if (IsNull(inner))
                    return NullLiteral.Instance;
                else if (inner is int i_inner)
                    return -i_inner;
                else if (inner.GetType().IsAssignableTo(typeof(float)))
                    return -(float)inner;
                else
                    throw new SepiaException(new EvaluateError($"Cannot perform the negate operation ('{node.Operator.TokenType.GetSymbol()}') on '{inner}'."));
            case TokenType.BANG:
                if (IsNull(inner))
                    return NullLiteral.Instance;
                else if (inner is bool b_inner)
                    return !b_inner;
                else
                    throw new SepiaException(new EvaluateError($"Cannot perform the invert operation ('{node.Operator.TokenType.GetSymbol()}') on '{inner}'."));
            case TokenType.TILDE:
                if (IsNull(inner))
                    return NullLiteral.Instance;
                else if (inner is int i_inner)
                    return ~i_inner;
                else
                    throw new SepiaException(new EvaluateError($"Cannot perform the bitwise invert operation ('{node.Operator.TokenType.GetSymbol()}') on '{inner}'."));
        }

        throw new SepiaException(new EvaluateError($"'{node.Operator.TokenType.GetSymbol()}' is not a valid unary operator."));
    }

    public object Visit(InterpolatedStringExprNode node)
    {
        StringBuilder aggregator = new();

        foreach (var child in node.Segments)
        {
            object? value = child == null ? null : Visit(child);
            aggregator.Append(value ?? string.Empty);
        }

        return aggregator.ToString();
    }

    public object Visit(LiteralExprNode node)
    {
        var literal = node.Literal;

        if (literal is BooleanLiteral bliteral)
        {
            return bliteral.BooleanValue;
        }
        else if (literal is CommentLiteral)
        {
            return VoidLiteral.Instance;
        }
        else if (literal is IdLiteral)
        {
            throw new NotImplementedException();
        }
        else if (literal is NullLiteral)
        {
            return NullLiteral.Instance;
        }
        else if (literal is VoidLiteral)
        {
            return VoidLiteral.Instance;
        }
        else if (literal is NumberLiteral numliteral)
        {
            switch (numliteral.NumberType)
            {
                case NumberType.INTEGER:
                    return Convert.ToInt32(numliteral.Value, numliteral.NumberBase.GetBaseNum());
                case NumberType.FLOAT:
                default:
                    return float.Parse(numliteral.Value);
            }
        }
        else if (literal is StringLiteral sliteral)
        {
            return sliteral.Value;
        }
        else if (literal is WhitespaceLiteral wliteral)
        {
            return VoidLiteral.Instance;
        }

        throw new SepiaException(new EvaluateError($"Cannot evaluate literal '{literal}'."));
    }

    public object Visit(IdentifierExprNode node)
    {
        return environment[node.Id.Value]!;
    }

    public object Visit(AssignmentExprNode node)
    {
        object assignment;

        //If compound assignment, perform operation between self and operand before assignment
        if(TokenTypeValues.COMPOUND_ASSIGNMENT.TryGetValue(node.AssignmentType.TokenType, out TokenType compound_op))
        {
            assignment = Visit(new BinaryExprNode(new IdentifierExprNode(node.Id), new Token(compound_op, node.AssignmentType.Location), node.Assignment));
        }
        else
        {
            assignment = Visit(node.Assignment);
        }
        
        environment[node.Id.Value, environment.GetCurrent(node.Id.Value)] = assignment;
        return assignment;
    }

    public object Visit(StatementNode node)
    {
        if (node is ExpressionStmtNode exprnode)
            return Visit(exprnode);
        else if (node is PrintStmtNode printnode)
            return Visit(printnode);
        else if (node is DeclarationStmtNode decnode)
            return Visit(decnode);
        else if (node is Block block)
            return Visit(block);
        throw new SepiaException(new EvaluateError($"Cannot evaluate node of type '{node.GetType().Name}'."));
    }

    public object Visit(ExpressionStmtNode node)
    {
        object expression_result = Visit(node.Expression);

        return expression_result;
    }

    public object Visit(PrintStmtNode node)
    {
        var evaluated_expression = Visit(node.Expression);

        if (IsNull(evaluated_expression))
            Console.WriteLine(TokenType.NULL.GetSymbol());
        else
            Console.WriteLine(evaluated_expression);

        return VoidLiteral.Instance;
    }

    public object Visit(DeclarationStmtNode node)
    {
        var assignment = node.Assignment == null ? null : Visit(node.Assignment);
        environment[node.Id.Value] = assignment;
        return VoidLiteral.Instance;
    }

    public object Visit(Block block)
    {
        var parent = environment;
        try
        {
            environment = new(parent);
            foreach (var statement in block.Statements)
                Visit(statement);
        }
        finally
        {
            environment = parent;
        }

        return VoidLiteral.Instance;
    }

    private bool IsNull(object o) => o == null || o is NullLiteral;
}