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
        RegisterTypes(SepiaTypeInfo.NativeTypes);
    }

    public Evaluator RegisterFunctions(Dictionary<string, ISepiaCallable> funcs)
    {
        foreach(var pair in funcs)
        {
            var type = SepiaTypeInfo.TypeFunction()
                .WithCallSignature(pair.Value.CallSignature.Clone());

            globals.Define(pair.Key, type, new(pair.Value.WithEnvironment(globals), type));
        }

        return this;
    }

    public Evaluator RegisterTypes(IEnumerable<SepiaTypeInfo> types)
    {
        foreach(var type in types)
        {
            var typeType = SepiaTypeInfo.TypeType();
            globals.Define(type.TypeName, typeType, new(type.Clone(), typeType));
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
        throw new SepiaException(new EvaluateError($"Cannot evaluate node of type '{node.GetType().Name}'.", node.Location));
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
            throw new SepiaException(new EvaluateError($"Control token '{control.Control.Token.TokenType.GetSymbol()}' is not allowed here!", control.Control.Location), control);
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
        else if (node is InlineFunctionExpressionNode inlineFuncNode)
            return Visit(inlineFuncNode);
        else if (node is FunctionExpressionNode funcNode)
            return Visit(funcNode);
        else if (node is ValueExpressionNode valueNode)
            return Visit(valueNode);
        throw new SepiaException(new EvaluateError($"Cannot evaluate node of type '{node.GetType().Name}'.", node.Location));
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
        throw new SepiaException(new EvaluateError($"Cannot evaluate node of type '{node.GetType().Name}'.", node.Location));
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

                if(left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) > Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToInt64(left.Value!) > Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                }
                else if(left.Type == SepiaTypeInfo.TypeFloat(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToDouble(left.Value!) > Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToDouble(left.Value!) > Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) >= Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToInt64(left.Value!) >= Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                }
                else if (left.Type == SepiaTypeInfo.TypeFloat(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToDouble(left.Value!) >= Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToDouble(left.Value!) >= Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) < Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToInt64(left.Value!) < Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                }
                else if (left.Type == SepiaTypeInfo.TypeFloat(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToDouble(left.Value!) < Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToDouble(left.Value!) < Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) <= Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToInt64(left.Value!) <= Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                }
                else if (left.Type == SepiaTypeInfo.TypeFloat(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToDouble(left.Value!) <= Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToDouble(left.Value!) <= Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeBoolean());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
            }
            case TokenType.EQUAL_EQUAL:
            {
                SepiaValue? left = eval_left();
                SepiaValue? right = eval_right();

                if (IsNull(left))
                {
                    return new(IsNull(right), SepiaTypeInfo.TypeBoolean());
                }
                else if(IsNull(right))
                {
                    return new(false, SepiaTypeInfo.TypeBoolean());
                }

                return new(left.Value!.Equals(right.Value!), SepiaTypeInfo.TypeBoolean());
            }
            case TokenType.BANG_EQUAL:
            {
                SepiaValue? left = eval_left();
                SepiaValue? right = eval_right();

                if (IsNull(left))
                {
                    return new(!IsNull(right), SepiaTypeInfo.TypeBoolean());
                }
                else if (IsNull(right))
                {
                    return new(true, SepiaTypeInfo.TypeBoolean());
                }

                return new(!left.Value!.Equals(right.Value!), SepiaTypeInfo.TypeBoolean());
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) + Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeInteger());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToInt64(left.Value!) + Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                }
                else if (left.Type == SepiaTypeInfo.TypeFloat(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToDouble(left.Value!) + Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToDouble(left.Value!) + Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) - Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeInteger());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToInt64(left.Value!) - Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                }
                else if (left.Type == SepiaTypeInfo.TypeFloat(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToDouble(left.Value!) - Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToDouble(left.Value!) - Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) / Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeInteger());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToInt64(left.Value!) / Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                }
                else if (left.Type == SepiaTypeInfo.TypeFloat(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToDouble(left.Value!) / Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToDouble(left.Value!) / Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) * Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeInteger());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToInt64(left.Value!) * Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                }
                else if (left.Type == SepiaTypeInfo.TypeFloat(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToDouble(left.Value!) * Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToDouble(left.Value!) * Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) % Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeInteger());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToInt64(left.Value!) % Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                }
                else if (left.Type == SepiaTypeInfo.TypeFloat(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToDouble(left.Value!) % Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                    else if (right.Type == SepiaTypeInfo.TypeFloat(false))
                    {
                        return new(Convert.ToDouble(left.Value!) % Convert.ToDouble(right.Value!), SepiaTypeInfo.TypeFloat());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
            }
            case TokenType.AMP_AMP:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.TypeBoolean(false))
                {
                    //Short circuit
                    if (!Convert.ToBoolean(left.Value))
                    {
                        return new SepiaValue(false, SepiaTypeInfo.TypeBoolean());
                    }

                    SepiaValue? right = eval_right();

                    if (IsNull(right))
                    {
                        return SepiaValue.Null;
                    }

                    if (right.Type == SepiaTypeInfo.TypeBoolean(false))
                    {
                        return new SepiaValue(Convert.ToBoolean(right.Value), SepiaTypeInfo.TypeBoolean());
                    }
                    else
                    {
                        throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
                    }
                }
                else
                {
                    SepiaValue? right = eval_right();
                    throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
                }
            }
            case TokenType.PIPE_PIPE:
            {
                SepiaValue? left = eval_left();

                if (IsNull(left))
                {
                    return SepiaValue.Null;
                }

                if (left.Type == SepiaTypeInfo.TypeBoolean(false))
                {
                    //Short circuit
                    if (Convert.ToBoolean(left.Value))
                    {
                        return new SepiaValue(true, SepiaTypeInfo.TypeBoolean());
                    }

                    SepiaValue? right = eval_right();

                    if (IsNull(right))
                    {
                        return SepiaValue.Null;
                    }

                    if (right.Type == SepiaTypeInfo.TypeBoolean(false))
                    {
                        return new SepiaValue(Convert.ToBoolean(right.Value), SepiaTypeInfo.TypeBoolean());
                    }
                    else
                    {
                        throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
                    }
                }
                else
                {
                    SepiaValue? right = eval_right();
                    throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if(left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if(right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) & Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeInteger());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) ^ Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeInteger());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        return new(Convert.ToInt64(left.Value!) | Convert.ToInt64(right.Value!), SepiaTypeInfo.TypeInteger());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        var long_left = Convert.ToInt64(left.Value!);
                        var long_right = Convert.ToInt64(right.Value!);

                        if (long_left > int.MaxValue || long_left < int.MinValue)
                        {
                            throw new SepiaException(new EvaluateError($"{left} is out of range ([{int.MinValue}, {int.MaxValue}]) for operator '{node.Operator.TokenType.GetSymbol()}'.", node.Location));
                        }

                        if (long_right > int.MaxValue || long_right < int.MinValue)
                        {
                            throw new SepiaException(new EvaluateError($"{right} is out of range ([{int.MinValue}, {int.MaxValue}]) for operator '{node.Operator.TokenType.GetSymbol()}'.", node.Location));
                        }

                        return new(Convert.ToInt32(long_left) << Convert.ToInt32(long_right), SepiaTypeInfo.TypeInteger());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
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

                if (left.Type == SepiaTypeInfo.TypeInteger(false))
                {
                    if (right.Type == SepiaTypeInfo.TypeInteger(false))
                    {
                        var long_left = Convert.ToInt64(left.Value!);
                        var long_right = Convert.ToInt64(right.Value!);

                        if (long_left > int.MaxValue || long_left < int.MinValue)
                        {
                            throw new SepiaException(new EvaluateError($"{left} is out of range ([{int.MinValue}, {int.MaxValue}]) for operator '{node.Operator.TokenType.GetSymbol()}'.", node.Location));
                        }

                        if (long_right > int.MaxValue || long_right < int.MinValue)
                        {
                            throw new SepiaException(new EvaluateError($"{right} is out of range ([{int.MinValue}, {int.MaxValue}]) for operator '{node.Operator.TokenType.GetSymbol()}'.", node.Location));
                        }

                        return new(Convert.ToInt32(long_left) >> Convert.ToInt32(long_right), SepiaTypeInfo.TypeInteger());
                    }
                }

                throw new SepiaException(new EvaluateError($"Cannot perform operation '{node.Operator.TokenType.GetSymbol()}' on {left} and {right}.", node.Location));
            }
        }

        throw new SepiaException(new EvaluateError($"'{node.Operator.TokenType.GetSymbol()}' is not a valid binary operator.", node.Location));
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
                if (inner.Type == SepiaTypeInfo.TypeInteger(false))
                    return new(-Convert.ToInt64(inner.Value)!, SepiaTypeInfo.TypeInteger());
                else if (inner.Type == SepiaTypeInfo.TypeFloat(false))
                    return new(-Convert.ToDouble(inner.Value)!, SepiaTypeInfo.TypeFloat());
                else
                    throw new SepiaException(new EvaluateError($"Cannot perform the negate operation ('{node.Operator.TokenType.GetSymbol()}') on '{inner}'.", node.Location));
            case TokenType.BANG:
                if (inner.Type == SepiaTypeInfo.TypeBoolean(false))
                    return new(!Convert.ToBoolean(inner.Value)!, SepiaTypeInfo.TypeBoolean());
                else
                    throw new SepiaException(new EvaluateError($"Cannot perform the invert operation ('{node.Operator.TokenType.GetSymbol()}') on '{inner}'.", node.Location));
            case TokenType.TILDE:
                if (inner.Type == SepiaTypeInfo.TypeInteger(false))
                    return new(~Convert.ToInt64(inner.Value)!, SepiaTypeInfo.TypeInteger());
                else
                    throw new SepiaException(new EvaluateError($"Cannot perform the bitwise invert operation ('{node.Operator.TokenType.GetSymbol()}') on '{inner}'.", node.Location));
        }

        throw new SepiaException(new EvaluateError($"'{node.Operator.TokenType.GetSymbol()}' is not a valid unary operator.", node.Location));
    }

    private SepiaValue Visit(InterpolatedStringExprNode node)
    {
        StringBuilder aggregator = new();

        foreach (var child in node.Segments)
        {
            SepiaValue? value = child == null ? null : Visit(child);
            aggregator.Append(value == null? string.Empty : value.ToString());
        }

        return new(aggregator.ToString(), SepiaTypeInfo.TypeString());
    }

    private SepiaValue Visit(LiteralExprNode node)
    {
        var literal = node.Literal;

        if (literal is BooleanLiteral bliteral)
        {
            return new(bliteral.BooleanValue, SepiaTypeInfo.TypeBoolean());
        }
        else if (literal is CommentLiteral)
        {
            return SepiaValue.Void;
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
                    return new(Convert.ToInt64(numliteral.Value, numliteral.NumberBase.GetBaseNum()), SepiaTypeInfo.TypeInteger());
                case NumberType.FLOAT:
                default:
                    return new(double.Parse(numliteral.Value), SepiaTypeInfo.TypeFloat());
            }
        }
        else if (literal is StringLiteral sliteral)
        {
            return new(sliteral.Value, SepiaTypeInfo.TypeString());
        }
        else if (literal is WhitespaceLiteral wliteral)
        {
            return SepiaValue.Void;
        }

        throw new SepiaException(new EvaluateError($"Cannot evaluate literal '{literal}'.", node.Location));
    }

    private SepiaValue Visit(IdentifierExprNode node)
    {
        return environment.Step(node.ResolveInfo.Steps)
            .Get(node.Id.ResolveInfo.Name, node.ResolveInfo.Index);
    }

    private SepiaValue Visit(AssignmentExprNode node)
    {
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

        SepiaTypeInfo varType = environment.Type(node.Id.ResolveInfo.Name, node.Id.ResolveInfo.Index);

        if(assignment.Type != varType)
        {
            throw new SepiaException(new EvaluateError($"Cannot assign value '{assignment}' ({assignment.Type}) to variable '{node.Id}' ({varType}).", node.Location));
        }

        environment.Update(node.Id.ResolveInfo.Name, assignment, node.Id.ResolveInfo.Index);

        return assignment;
    }

    private SepiaValue Visit(CallExprNode node)
    {
        SepiaValue inner = Visit(node.Callable);

        if (inner.Value == null)
        {
            throw new SepiaException(new EvaluateError($"Value '{inner}' is not callable.", node.Location));
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

            if(result.Type != callable.CallSignature.ReturnType)
            {
                throw new SepiaException(new EvaluateError($"Return value '{result.Type}' doesn't match expected type '{callable.CallSignature.ReturnType}'.", node.Location));
            }

            return result;
        }
        else
        {
            throw new SepiaException(new EvaluateError($"Value '{inner}' is not callable.", node.Location));
        }
    }

    private SepiaValue Visit(ValueExpressionNode node)
    {
        return node.Value;
    }

    private SepiaValue Visit(FunctionExpressionNode node)
    {
        var function = new SepiaFunction(node.Arguments, node.ReturnType, node.Body, environment);
        return new SepiaValue(function, SepiaTypeInfo.TypeFunction()
            .WithCallSignature(function.CallSignature.Clone()));
    }

    private SepiaValue Visit(InlineFunctionExpressionNode node)
    {
        var function = new SepiaInlineFunction(node.Arguments, node.Expression, environment);
        return new SepiaValue(function, SepiaTypeInfo.TypeFunction()
            .WithCallSignature(function.CallSignature.Clone()));
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
                throw new SepiaException(new EvaluateError($"Type of '{node.Id}' is ambiguous.", node.Location));
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
                throw new SepiaException(new EvaluateError($"Cannot assign value '{assignment}' ({assignment.Type}) to variable '{node.Id}' ({node.Type}).", node.Location));
            }
        }

        environment.Define(node.Id.ResolveInfo.Name, varType, assignment);

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
                if(result.Type == SepiaTypeInfo.TypeBoolean(false) && Convert.ToBoolean(result.Value!))
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
                else if(result.Type == SepiaTypeInfo.TypeBoolean(false))
                {
                    return Convert.ToBoolean(result.Value);
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
                catch (SepiaControlFlow control) when (!(control.Control is ReturnStatementNode))
                {
                    if (control.Control.Token.TokenType == TokenType.CONTINUE)
                    {
                        continue;
                    }
                    else if (control.Control.Token.TokenType == TokenType.BREAK)
                    {
                        break;
                    }
                    else
                    {
                        throw new SepiaException(new EvaluateError($"Invalid control flow token '{control.Control.Token.TokenType.GetSymbol()}'.", control.Control.Location), control);
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
        if(node is ReturnStatementNode rsnode)
        {
            return Visit(rsnode);
        }

        throw new SepiaControlFlow(node);
    }

    private SepiaValue Visit(ReturnStatementNode node)
    {
        var evaluated = Visit(node.Expression);
        throw new SepiaReturn(node, evaluated);
    }

    private SepiaValue Visit(ForStatementNode node)
    {
        var parent = environment;
        try
        {
            environment = new(parent);

            var eval_condition = () =>
            {
                var result = node.Condition?.Expression == null? SepiaValue.Null : Visit(node.Condition.Expression);

                if (IsNull(result))
                {
                    return false;
                }
                else if (result.Type == SepiaTypeInfo.TypeBoolean(false))
                {
                    return Convert.ToBoolean(result.Value);
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
                catch (SepiaControlFlow control) when (!(control.Control is ReturnStatementNode))
                {
                    if (control.Control.Token.TokenType == TokenType.CONTINUE)
                    {
                        continue;
                    }
                    else if (control.Control.Token.TokenType == TokenType.BREAK)
                    {
                        break;
                    }
                    else
                    {
                        throw new SepiaException(new EvaluateError($"Invalid control flow token '{control.Control.Token.TokenType.GetSymbol()}'.", control.Control.Location), control);
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
        return Visit(new DeclarationStmtNode(node.Id, SepiaTypeInfo.TypeFunction()
            .WithCallSignature(node.Function.ResolveInfo.Type.CallSignature), node.Function));
    }

    private bool IsNull(object o) => o == null || o is NullLiteral || (o is LiteralBase ol && ol.Type == SepiaTypeInfo.TypeNull(false)) || (o is SepiaValue sv && sv.Type == SepiaTypeInfo.TypeNull(false));
}