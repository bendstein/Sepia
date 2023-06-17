using Sepia.Analyzer;
using Sepia.AST;
using Sepia.AST.Node;
using Sepia.AST.Node.Expression;
using Sepia.AST.Node.Statement;
using Sepia.Callable;
using Sepia.Common;
using Sepia.Lex;
using Sepia.Lex.Literal;
using Sepia.Parse;
using Sepia.Value;
using Sepia.Value.Type;
using System.Text;

namespace Sepia.Evaluate;

public class Evaluator :
    IASTNodeVisitor<AbstractSyntaxTree, SepiaValue>,
    IASTNodeVisitor<ASTNode, SepiaValue>,
    IASTNodeVisitor<ProgramNode, SepiaValue>,
    IASTNodeVisitor<ExpressionNode, SepiaValue>,
    IASTNodeVisitor<StatementNode, SepiaValue>
{
    public readonly SepiaEnvironment globals = new();
    public SepiaEnvironment environment { get; set; }

    public readonly Func<IEnumerable<Token>, Parser> createParser;
    public readonly Func<string, Lexer> createLexer;

    public Evaluator(Func<IEnumerable<Token>, Parser> ParserFactory, Func<string, Lexer> LexerFactory)
    {
        createParser = ParserFactory;
        createLexer = LexerFactory;
        environment = new(globals);
    }

    public Evaluator RegisterNativeFunctions(Dictionary<string, ISepiaCallable> nativeFuncs)
    {
        foreach(var pair in nativeFuncs)
        {
            globals.Define(pair.Key, SepiaTypeInfo.Function, new(pair.Value.WithEnvironment(globals), SepiaTypeInfo.Function));
        }

        return this;
    }

    public SepiaValue Visit(AbstractSyntaxTree tree)
    {
        return Visit(tree.Root);
    }

    public SepiaValue Visit(ASTNode node)
    {
        if (node is ProgramNode pnode)
            return Visit(pnode);
        else if (node is ExpressionNode expressionNode)
            return Visit(expressionNode);
        else if (node is StatementNode snode)
            return Visit(snode);
        throw new NotImplementedException();
    }

    public SepiaValue Visit(ProgramNode node)
    {
        try
        {
            foreach (var statement in node.statements)
                _ = Visit(statement);

            return SepiaValue.Void;
        }
        catch (SepiaControlFlow control)
        {
            throw new SepiaException($"Control token '{control.Token.TokenType.GetSymbol()}' is not allowed here!", control);
        }
    }

    public SepiaValue Visit(ExpressionNode node)
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
        else if (node is CallExprNode callnode)
            return Visit(callnode);
        else if (node is FunctionExpressionNode funcNode)
            return Visit(funcNode);
        else if (node is ValueExpressionNode valueNode)
            return Visit(valueNode);
        throw new SepiaException(new EvaluateError($"Cannot evaluate node of type '{node.GetType().Name}'."));
    }

    public SepiaValue Visit(StatementNode node)
    {
        if (node is ExpressionStmtNode exprnode)
            return Visit(exprnode);
        else if (node is DeclarationStmtNode decnode)
            return Visit(decnode);
        else if (node is Block block)
            return Visit(block);
        else if (node is ConditionalStatementNode condnode)
            return Visit(condnode);
        else if (node is WhileStatementNode whilenode)
            return Visit(whilenode);
        else if (node is ControlFlowStatementNode controlnode)
            return Visit(controlnode);
        else if (node is ReturnStatementNode returnnode)
            return Visit(returnnode);
        else if (node is ForStatementNode fornode)
            return Visit(fornode);
        else if (node is FunctionDeclarationStmtNode funcnode)
            return Visit(funcnode);
        throw new SepiaException(new EvaluateError($"Cannot evaluate node of type '{node.GetType().Name}'."));
    }

    private SepiaValue Visit(BinaryExprNode node)
    {
        var eval_left = () => Visit(node.Left);
        var eval_right = () => Visit(node.Right);

        switch (node.Operator.TokenType)
        {
            case TokenType.GREATER:
            {
                SepiaValue? left = eval_left();

                if(IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if(left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((int)left.Value! > (int)right.Value!, SepiaTypeInfo.Boolean);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((int)left.Value! > (double)right.Value!, SepiaTypeInfo.Boolean);
                    }
                }
                else if(left.Type == SepiaTypeInfo.Float)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((double)left.Value! > (int)right.Value!, SepiaTypeInfo.Boolean);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((double)left.Value! > (double)right.Value!, SepiaTypeInfo.Boolean);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.GREATER_EQUAL:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((long)left.Value! >= (long)right.Value!, SepiaTypeInfo.Boolean);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((long)left.Value! >= (double)right.Value!, SepiaTypeInfo.Boolean);
                    }
                }
                else if (left.Type == SepiaTypeInfo.Float)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((double)left.Value! >= (long)right.Value!, SepiaTypeInfo.Boolean);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((double)left.Value! >= (double)right.Value!, SepiaTypeInfo.Boolean);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.LESS:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((long)left.Value! < (long)right.Value!, SepiaTypeInfo.Boolean);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((long)left.Value! < (double)right.Value!, SepiaTypeInfo.Boolean);
                    }
                }
                else if (left.Type == SepiaTypeInfo.Float)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((double)left.Value! < (long)right.Value!, SepiaTypeInfo.Boolean);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((double)left.Value! < (double)right.Value!, SepiaTypeInfo.Boolean);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.LESS_EQUAL:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((long)left.Value! <= (long)right.Value!, SepiaTypeInfo.Boolean);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((long)left.Value! <= (double)right.Value!, SepiaTypeInfo.Boolean);
                    }
                }
                else if (left.Type == SepiaTypeInfo.Float)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((double)left.Value! <= (long)right.Value!, SepiaTypeInfo.Boolean);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((double)left.Value! <= (double)right.Value!, SepiaTypeInfo.Boolean);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.EQUAL_EQUAL:
            {
                SepiaValue? left = eval_left();
                SepiaValue? right = eval_right();

                if (IsNull(left))
                {
                    return new(IsNull(right), SepiaTypeInfo.Boolean);
                }
                else if(IsNull(right))
                {
                    return new(false, SepiaTypeInfo.Boolean);
                }

                return new(left.Value!.Equals(right.Value!), SepiaTypeInfo.Boolean);
            }
            case TokenType.BANG_EQUAL:
            {
                SepiaValue? left = eval_left();
                SepiaValue? right = eval_right();

                if (IsNull(left))
                {
                    return new(!IsNull(right), SepiaTypeInfo.Boolean);
                }
                else if (IsNull(right))
                {
                    return new(true, SepiaTypeInfo.Boolean);
                }

                return new(!left.Value!.Equals(right.Value!), SepiaTypeInfo.Boolean);
            }
            case TokenType.PLUS:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((long)left.Value! + (long)right.Value!, SepiaTypeInfo.Integer);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((long)left.Value! + (double)right.Value!, SepiaTypeInfo.Float);
                    }
                }
                else if (left.Type == SepiaTypeInfo.Float)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((double)left.Value! + (long)right.Value!, SepiaTypeInfo.Float);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((double)left.Value! + (double)right.Value!, SepiaTypeInfo.Float);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.MINUS:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((long)left.Value! - (long)right.Value!, SepiaTypeInfo.Integer);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((long)left.Value! - (double)right.Value!, SepiaTypeInfo.Float);
                    }
                }
                else if (left.Type == SepiaTypeInfo.Float)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((double)left.Value! - (long)right.Value!, SepiaTypeInfo.Float);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((double)left.Value! - (double)right.Value!, SepiaTypeInfo.Float);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.SLASH:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((long)left.Value! / (long)right.Value!, SepiaTypeInfo.Integer);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((long)left.Value! / (double)right.Value!, SepiaTypeInfo.Float);
                    }
                }
                else if (left.Type == SepiaTypeInfo.Float)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((double)left.Value! / (long)right.Value!, SepiaTypeInfo.Float);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((double)left.Value! / (double)right.Value!, SepiaTypeInfo.Float);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.STAR:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((long)left.Value! * (long)right.Value!, SepiaTypeInfo.Integer);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((long)left.Value! * (double)right.Value!, SepiaTypeInfo.Float);
                    }
                }
                else if (left.Type == SepiaTypeInfo.Float)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((double)left.Value! * (long)right.Value!, SepiaTypeInfo.Float);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((double)left.Value! * (double)right.Value!, SepiaTypeInfo.Float);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.PERCENT:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((long)left.Value! % (long)right.Value!, SepiaTypeInfo.Integer);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((long)left.Value! % (double)right.Value!, SepiaTypeInfo.Float);
                    }
                }
                else if (left.Type == SepiaTypeInfo.Float)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((double)left.Value! % (long)right.Value!, SepiaTypeInfo.Float);
                    }
                    else if (right.Type == SepiaTypeInfo.Float)
                    {
                        return new((double)left.Value! % (double)right.Value!, SepiaTypeInfo.Float);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.AMP_AMP:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Boolean)
                {
                    //Short circuit
                    if (!(bool)left.Value!)
                    {
                        return new SepiaValue(false, SepiaTypeInfo.Boolean);
                    }

                    SepiaValue? right = eval_right();

                    if (IsNull(right))
                    {
                        return SepiaValue.Null;
                    }

                    if (right.Type == SepiaTypeInfo.Boolean)
                    {
                        return new SepiaValue((bool)right.Value!, SepiaTypeInfo.Boolean);
                    }
                    else
                    {
                        throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
                    }
                }
                else
                {
                    SepiaValue? right = eval_right();
                    throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
                }
            }
            case TokenType.PIPE_PIPE:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Boolean)
                {
                    //Short circuit
                    if ((bool)left.Value!)
                    {
                        return new SepiaValue(true, SepiaTypeInfo.Boolean);
                    }

                    SepiaValue? right = eval_right();

                    if (IsNull(right))
                    {
                        return SepiaValue.Null;
                    }

                    if (right.Type == SepiaTypeInfo.Boolean)
                    {
                        return new SepiaValue((bool)right.Value!, SepiaTypeInfo.Boolean);
                    }
                    else
                    {
                        throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
                    }
                }
                else
                {
                    SepiaValue? right = eval_right();
                    throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
                }
            }
            case TokenType.AMP:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if(left.Type == SepiaTypeInfo.Integer)
                {
                    if(right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((long)left.Value! & (long)right.Value!, SepiaTypeInfo.Integer);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.CARET:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((long)left.Value! ^ (long)right.Value!, SepiaTypeInfo.Integer);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.PIPE:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((long)left.Value! | (long)right.Value!, SepiaTypeInfo.Integer);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.LESS_LESS:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((int)left.Value! << (int)right.Value!, SepiaTypeInfo.Integer);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
            case TokenType.GREATER_GREATER:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                SepiaValue? right = eval_right();

                if (IsNull(right))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.Integer)
                {
                    if (right.Type == SepiaTypeInfo.Integer)
                    {
                        return new((int)left.Value! >> (int)right.Value!, SepiaTypeInfo.Integer);
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}."));
            }
        }

        throw new SepiaException(new EvaluateError($"'{node.Operator.TokenType.GetSymbol()}' is not a valid binary operator."));
    }

    private SepiaValue Visit(GroupExprNode node)
    {
        return Visit(node.Inner);
    }

    private SepiaValue Visit(UnaryPrefixExprNode node)
    {
        SepiaValue? inner = Visit(node.Right);

        if(IsNull(inner))
        {
            return SepiaValue.Null;
        }

        switch (node.Operator.TokenType)
        {
            case TokenType.MINUS:
                if (inner.Type == SepiaTypeInfo.Integer)
                    return new(-(long)inner.Value!, SepiaTypeInfo.Integer);
                else if (inner.Type == SepiaTypeInfo.Float)
                    return new(-(double)inner.Value!, SepiaTypeInfo.Float);
                else
                    throw new SepiaException(new EvaluateError($"Cannot perform the negate operation ('{node.Operator.TokenType.GetSymbol()}') on '{inner}'."));
            case TokenType.BANG:
                if (inner.Type == SepiaTypeInfo.Boolean)
                    return new(!(bool)inner.Value!, SepiaTypeInfo.Boolean);
                else
                    throw new SepiaException(new EvaluateError($"Cannot perform the invert operation ('{node.Operator.TokenType.GetSymbol()}') on '{inner}'."));
            case TokenType.TILDE:
                if (inner.Type == SepiaTypeInfo.Integer)
                    return new(~(long)inner.Value!, SepiaTypeInfo.Integer);
                else
                    throw new SepiaException(new EvaluateError($"Cannot perform the bitwise invert operation ('{node.Operator.TokenType.GetSymbol()}') on '{inner}'."));
        }

        throw new SepiaException(new EvaluateError($"'{node.Operator.TokenType.GetSymbol()}' is not a valid unary operator."));
    }

    private SepiaValue Visit(InterpolatedStringExprNode node)
    {
        StringBuilder aggregator = new();

        foreach (var child in node.Segments)
        {
            SepiaValue? value = child == null ? null : Visit(child);
            aggregator.Append(value == null? string.Empty : value.ToString());
        }

        return new(aggregator.ToString(), SepiaTypeInfo.String);
    }

    private SepiaValue Visit(LiteralExprNode node)
    {
        var literal = node.Literal;

        if (literal is BooleanLiteral bliteral)
        {
            return new(bliteral.BooleanValue, SepiaTypeInfo.Boolean);
        }
        else if (literal is CommentLiteral)
        {
            return SepiaValue.Void;
        }
        else if (literal is IdLiteral)
        {
            throw new NotImplementedException();
        }
        else if (literal is NullLiteral)
        {
            return SepiaValue.Null;
        }
        else if (literal is VoidLiteral)
        {
            return SepiaValue.Void;
        }
        else if (literal is NumberLiteral numliteral)
        {
            switch (numliteral.NumberType)
            {
                case NumberType.INTEGER:
                    return new(Convert.ToInt64(numliteral.Value, numliteral.NumberBase.GetBaseNum()), SepiaTypeInfo.Integer);
                case NumberType.FLOAT:
                default:
                    return new(double.Parse(numliteral.Value), SepiaTypeInfo.Float);
            }
        }
        else if (literal is StringLiteral sliteral)
        {
            return new(sliteral.Value, SepiaTypeInfo.String);
        }
        else if (literal is WhitespaceLiteral wliteral)
        {
            return SepiaValue.Void;
        }

        throw new SepiaException(new EvaluateError($"Cannot evaluate literal '{literal}'."));
    }

    private SepiaValue Visit(IdentifierExprNode node)
    {
        return environment.Step(node.ResolveInfo.Steps)
            .Get(node.Id.Value, node.ResolveInfo.Index);
    }

    private SepiaValue Visit(AssignmentExprNode node)
    {
        int index = node.ResolveInfo.Index;

        SepiaValue assignment;

        //If compound assignment, perform operation between self and operand before assignment
        if(TokenTypeValues.COMPOUND_ASSIGNMENT.TryGetValue(node.AssignmentType.TokenType, out TokenType compound_op))
        {
            assignment = Visit(new BinaryExprNode(new IdentifierExprNode(node.Id), new Token(compound_op, node.AssignmentType.Location), node.Assignment));
        }
        else
        {
            assignment = Visit(node.Assignment);
        }

        SepiaTypeInfo varType = environment.Type(node.Id.Value, index);

        if(assignment.Type != varType)
        {
            throw new SepiaException(new EvaluateError($"Cannot assign value '{assignment}' ({assignment.Type}) to variable '{node.Id}' ({varType})."));
        }

        environment.Update(node.Id.Value, assignment, index);

        return assignment;
    }

    private SepiaValue Visit(CallExprNode node)
    {
        SepiaValue inner = Visit(node.Callable);

        if (inner.Value == null)
        {
            throw new SepiaException(new EvaluateError());
        }
        else if (inner.Value is ISepiaCallable callable)
        {
            //ToList to eagerly evaluate, otherwise they'll be evaluated in the scope of the callable
            IEnumerable<SepiaValue> evaluatedArgs = node.Arguments
                .Select(Visit)
                .ToList();

            SepiaValue result;

            try
            {
                result = callable.Call(this, evaluatedArgs);
            }
            catch(SepiaReturn ret)
            {
                result = ret.Value;
            }

            if(result.Type != callable.ReturnType)
            {
                throw new SepiaException(new EvaluateError($"Return value '{result.Type}' doesn't match expected type '{callable.ReturnType}'."));
            }

            return result;
        }
        else
        {
            throw new SepiaException(new EvaluateError());
        }
    }

    private SepiaValue Visit(ValueExpressionNode node)
    {
        return node.Value;
    }

    private SepiaValue Visit(FunctionExpressionNode node)
    {
        return new SepiaValue(new SepiaFunction(node.Arguments, node.ReturnType, node.Body, environment), SepiaTypeInfo.Function);
    }

    private SepiaValue Visit(ExpressionStmtNode node)
    {
        _ = Visit(node.Expression);
        return SepiaValue.Void;
        //return expression_result;
    }

    private SepiaValue Visit(DeclarationStmtNode node)
    {
        var assignment = node.Assignment == null ? null : Visit(node.Assignment);

        SepiaTypeInfo varType;

        if(node.Type is null)
        {
            if(assignment is null)
            {
                throw new SepiaException(new EvaluateError($"Type of '{node.Id}' is ambiguous."));
            }
            else
            {
                varType = assignment.Type;
            }
        }
        else
        {
            varType = node.Type;

            if(assignment is not null && node.Type != assignment.Type)
            {
                throw new SepiaException(new EvaluateError($"Cannot assign value '{assignment}' ({assignment.Type}) to variable '{node.Id}' ({node.Type})."));
            }
        }

        environment.Define(node.Id.Value, varType, assignment);

        return SepiaValue.Void;
        //return assignment?? new(null, varType);
    }

    private SepiaValue Visit(Block block)
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

        return SepiaValue.Void;
    }

    private SepiaValue Visit(ConditionalStatementNode node)
    {
        var parent = environment;
        try
        {
            environment = new(parent);
            bool resolved = false;
            foreach(var branch in node.Branches)
            {
                SepiaValue result = Visit(branch.condition);
                if(result.Type == SepiaTypeInfo.Boolean && (bool)result.Value!)
                {
                    resolved = true;
                    _ = Visit(branch.body);
                    break;
                }
            }

            if(!resolved && node.Else != null)
            {
                _ = Visit(node.Else);
            }
        }
        finally
        {
            environment = parent;
        }

        return SepiaValue.Void;
    }

    private SepiaValue Visit(WhileStatementNode node)
    {
        var parent = environment;
        try
        {
            environment = new(parent);

            var eval_condition = () =>
            {
                var result = Visit(node.Condition);

                if(IsNull(result))
                {
                    return false;
                }
                else if(result.Type == SepiaTypeInfo.Boolean)
                {
                    return (bool)result.Value!;
                }
                else
                {
                    return false;
                }
            };

            while(eval_condition())
            {
                try
                {
                    Visit(node.Body);
                }
                catch (SepiaControlFlow control)
                {
                    if (control.Token.TokenType == TokenType.CONTINUE)
                    {
                        continue;
                    }
                    else if (control.Token.TokenType == TokenType.BREAK)
                    {
                        break;
                    }
                    else
                    {
                        throw new SepiaException($"Invalid control flow token '{control.Token.TokenType.GetSymbol()}'.", control);
                    }
                }
            }
        }
        finally
        {
            environment = parent;
        }

        return SepiaValue.Void;
    }

    private SepiaValue Visit(ControlFlowStatementNode node)
    {
        throw new SepiaControlFlow(node.Token);
    }

    private SepiaValue Visit(ReturnStatementNode node)
    {
        var evaluated = Visit(node.Expression);
        throw new SepiaReturn(node.Token, evaluated);
    }

    private SepiaValue Visit(ForStatementNode node)
    {
        var parent = environment;
        try
        {
            environment = new(parent);

            var eval_condition = () =>
            {
                var result = node.Condition == null? SepiaValue.Null : Visit(node.Condition);

                if (IsNull(result))
                {
                    return false;
                }
                else if (result.Type == SepiaTypeInfo.Boolean)
                {
                    return (bool)result.Value!;
                }
                else
                {
                    return false;
                }
            };

            var eval_action = () =>
            {
                if (node.Action == null) return;
                _ = Visit(node.Action);
            };

            if(node.Declaration != null)
                Visit(node.Declaration);

            while (eval_condition())
            {
                try
                {
                    Visit(node.Body);
                }
                catch (SepiaControlFlow control)
                {
                    if (control.Token.TokenType == TokenType.CONTINUE)
                    {
                        continue;
                    }
                    else if (control.Token.TokenType == TokenType.BREAK)
                    {
                        break;
                    }
                    else
                    {
                        throw new SepiaException($"Invalid control flow token '{control.Token.TokenType.GetSymbol()}'.", control);
                    }
                }
                finally
                {
                    eval_action();
                }
            }
        }
        finally
        {
            environment = parent;
        }

        return SepiaValue.Void;
    }

    private SepiaValue Visit(FunctionDeclarationStmtNode node)
    {
        return Visit(new DeclarationStmtNode(node.Id, SepiaTypeInfo.Function, node.Function));
    }

    private bool IsNull(object o) => o == null || o is NullLiteral || (o is LiteralBase ol && ol.Type == SepiaTypeInfo.Null) || (o is SepiaValue sv && sv.Type == SepiaTypeInfo.Null);
}