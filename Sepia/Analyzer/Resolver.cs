using Sepia.AST.Node.Expression;
using Sepia.AST.Node.Statement;
using Sepia.AST.Node;
using Sepia.AST;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Utility;
using Sepia.Value.Type;
using Sepia.Lex.Literal;
using Sepia.Value;
using Sepia.Callable;
using static System.Formats.Asn1.AsnWriter;
using System.Diagnostics.CodeAnalysis;

namespace Sepia.Analyzer;
public class Resolver : IASTNodeVisitor<AbstractSyntaxTree>,
    IASTNodeVisitor<ASTNode>,
    IASTNodeVisitor<ProgramNode>,
    IASTNodeVisitor<ExpressionNode>,
    IASTNodeVisitor<StatementNode>
{
    private readonly Evaluator evaluator;

    public Scope global;
    public Scope scope;

    private List<ResolveInfo> returns = new();
    private List<SepiaControlFlow> controls = new();

    public Resolver(Evaluator evaluator)
    {
        this.evaluator = evaluator;
        InitScope();
    }

    [MemberNotNull(nameof(global), nameof(scope))]
    private void InitScope()
    {
        global = new();
        scope = new(global);

        foreach (var key in evaluator.globals.Keys)
        {
            var type = evaluator.globals.Type(key, 0);

            if (evaluator.globals.Initialized(key, 0))
            {
                var value = evaluator.globals.Get(key, 0);

                if (value.Value != null && value.Value is ISepiaCallable callable)
                {
                    global.Declare(key, new(new FunctionResolveInfo(callable.ReturnType, callable.argumentTypes
                        .Select(a => (ResolveInfo)new ResolveInfo(a)).ToList()), key, true));
                }
                else
                {
                    global.Declare(key, new(new ResolveInfo(type), key, true));
                }
            }
            else
            {
                global.Declare(key, new(new ResolveInfo(type), key, false));
            }
        }
    }

    public void Visit(AbstractSyntaxTree node)
    {
        Visit(node.Root);
    }

    public void Visit(ASTNode node)
    {
        if (node is ProgramNode pnode)
            Visit(pnode);
        else if (node is ExpressionNode expressionNode)
            Visit(expressionNode);
        else if (node is StatementNode snode)
            Visit(snode);
        else
            throw new NotImplementedException();

        node.ResolveInfo.Type = SepiaTypeInfo.Void;
    }

    public void Visit(ProgramNode node)
    {
        foreach (var statement in node.statements)
            Visit(statement);

        node.ResolveInfo.Type = SepiaTypeInfo.Void;
    }

    public void Visit(ExpressionNode node)
    {
        if (node is BinaryExprNode binaryNode)
            Visit(binaryNode);
        else if (node is GroupExprNode groupNode)
            Visit(groupNode);
        else if (node is UnaryPrefixExprNode unaryPrefixNode)
            Visit(unaryPrefixNode);
        else if (node is InterpolatedStringExprNode interpolatedNode)
            Visit(interpolatedNode);
        else if (node is LiteralExprNode literalNode)
            Visit(literalNode);
        else if (node is IdentifierExprNode idNode)
            Visit(idNode);
        else if (node is AssignmentExprNode assignnode)
            Visit(assignnode);
        else if (node is CallExprNode callnode)
            Visit(callnode);
        else if (node is FunctionExpressionNode funcNode)
            Visit(funcNode);
        else if (node is ValueExpressionNode valueNode)
            Visit(valueNode);
        else
            throw new SepiaException(new AnalyzerError($"Cannot resolve node of type '{node.GetType().Name}'."));
    }

    public void Visit(StatementNode node)
    {
        if (node is ExpressionStmtNode exprnode)
            Visit(exprnode);
        else if (node is DeclarationStmtNode decnode)
            Visit(decnode);
        else if (node is Block block)
            Visit(block);
        else if (node is ConditionalStatementNode condnode)
            Visit(condnode);
        else if (node is WhileStatementNode whilenode)
            Visit(whilenode);
        else if (node is ControlFlowStatementNode controlnode)
            Visit(controlnode);
        else if (node is ReturnStatementNode returnnode)
            Visit(returnnode);
        else if (node is ForStatementNode fornode)
            Visit(fornode);
        else if (node is FunctionDeclarationStmtNode funcnode)
            Visit(funcnode);
        else
            throw new SepiaException(new EvaluateError($"Cannot resolve node of type '{node.GetType().Name}'."));
    }

    private void Visit(BinaryExprNode node)
    {
        Visit(node.Left);
        Visit(node.Right);

        node.ResolveInfo = node.Left.ResolveInfo.Clone();
    }

    private void Visit(GroupExprNode node)
    {
        Visit(node.Inner);
        node.ResolveInfo = node.Inner.ResolveInfo.Clone();
    }

    private void Visit(UnaryPrefixExprNode node)
    {
        Visit(node.Right);
        node.ResolveInfo = node.Right.ResolveInfo.Clone();
    }

    private void Visit(InterpolatedStringExprNode node)
    {
        foreach (var segment in node.Segments)
            Visit(segment);

        node.ResolveInfo.Type = SepiaTypeInfo.String;
    }

    private void Visit(LiteralExprNode node)
    {
        var literal = node.Literal;

        if (literal is BooleanLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.Boolean;
        }
        else if (literal is CommentLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.Void;
        }
        else if (literal is IdLiteral)
        {
            throw new NotImplementedException();
        }
        else if (literal is NullLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.Null;
        }
        else if (literal is VoidLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.Void;
        }
        else if (literal is NumberLiteral numliteral)
        {
            switch (numliteral.NumberType)
            {
                case NumberType.INTEGER:
                    node.ResolveInfo.Type = SepiaTypeInfo.Integer;
                    break;
                case NumberType.FLOAT:
                default:
                    node.ResolveInfo.Type = SepiaTypeInfo.Float;
                    break;
            }
        }
        else if (literal is StringLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.String;
        }
        else if (literal is WhitespaceLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.Void;
        }
    }

    private void Visit(IdentifierExprNode node)
    {
        if(scope.TryGet(node.Id.Value, out var info, out _, out _))
        {
            node.ResolveInfo = info.ResolveInfo.Clone();
        }
        //Identifier is not defined
        else
        {
            throw new SepiaException(new AnalyzerError($"Cannot access identifier '{node.Id.Value}' before it is declared."));
        }
    }

    private void Visit(CallExprNode node)
    {
        Visit(node.Callable);

        foreach(var arg in node.Arguments)
        {
            Visit(arg);
        }

        if(node.Callable.ResolveInfo is FunctionResolveInfo fresolve)
        {
            //Type is function's return type
            node.ResolveInfo.Type = fresolve.ReturnType;

            //Make sure arguments match
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                var arg = node.Arguments[i];

                //Make sure the argument exists on the callable
                if(fresolve.Arguments.Count <= i)
                {
                    throw new SepiaException(new AnalyzerError($"Too many arguments."));
                }
                else
                {
                    var callable_arg = fresolve.Arguments[i];

                    //Compare return types
                    if(arg.ResolveInfo.Type != callable_arg.Type)
                    {

                    }
                }
            }

            //Check if missing arguments
            if(fresolve.Arguments.Count > node.Arguments.Count)
            {
                throw new SepiaException(new AnalyzerError($"Missing arguments."));
            }
        }
        //Not callable
        else
        {
            throw new SepiaException(new AnalyzerError($"Not callable."));
        }
    }

    private void Visit(ValueExpressionNode node)
    {
        node.ResolveInfo.Type = node.Value.Type;
    }

    private void Visit(FunctionExpressionNode node)
    {
        var current = scope;
        try
        {
            scope = new(current);

            List<ResolveInfo> arguments = new();

            foreach (var arg in node.Arguments)
            {
                ResolveInfo info = new(arg.type);

                scope.Declare(arg.id.Value, new(info, arg.id.Value, true));

                arguments.Add(info);
            }

            //Visit function body
            Visit(node.Body);

            //If no returns, should be a void function
            if (returns.Count == 0)
            {
                if (node.ReturnType != SepiaTypeInfo.Void)
                {
                    throw new SepiaException(new AnalyzerError($"No return statements found."));
                }
            }
            //Make sure all returns match function return value
            else
            {
                foreach (var ret in returns.Where(r => r.Type != node.ReturnType))
                {
                    throw new SepiaException(new AnalyzerError($"Mismatched return types."));
                }

                returns.Clear();
            }

            node.ResolveInfo = new FunctionResolveInfo(node.ReturnType, arguments);
        }
        finally
        {
            scope = current;
        }
    }

    private void Visit(ExpressionStmtNode node)
    {
        Visit(node.Expression);
        node.ResolveInfo.Type = SepiaTypeInfo.Void;
    }

    private void Visit(DeclarationStmtNode node)
    {
        if (node.Assignment != null)
            Visit(node.Assignment);

        //Make sure that if the type isn't explicitly given, that it can be inferred
        if(node.Assignment == null && node.Type == null)
        {
            throw new SepiaException(new AnalyzerError($"Cannot infer type of '{node.Id.Value}'."));
        }

        var id_type = node.Assignment?.ResolveInfo?.Type?? node.Type?? throw new InvalidOperationException("");

        scope.Declare(node.Id.Value, new(new ResolveInfo(id_type), node.Id.Value, node.Assignment != null));

        node.ResolveInfo.Type = SepiaTypeInfo.Void;
    }

    private void Visit(AssignmentExprNode node)
    { 
        if(scope.TryGet(node.Id.Value, out ScopeInfo? info, out _, out _))
        {
            Visit(node.Assignment);

            info.Initialized = true;

            //Make sure assignment type matches variable type
            if (info.ResolveInfo != node.Assignment.ResolveInfo)
            {
                throw new SepiaException(new AnalyzerError($"Cannot assign type to '{node.Id.Value}'."));
            }

            node.ResolveInfo = node.Assignment.ResolveInfo.Clone();
        }
        //Cannot assign to variable before it is defined
        else
        {
            throw new SepiaException(new AnalyzerError($"Cannot assign value to '{node.Id.Value}' before it is declared."));
        }
    }

    private void Visit(Block node)
    {
        var current = scope;
        try
        {
            scope = new(current);

            foreach (var statement in node.Statements)
                Visit(statement);

            node.ResolveInfo.Type = SepiaTypeInfo.Void;
        }
        finally
        {
            scope = current;
        }
    }

    private void Visit(ConditionalStatementNode node)
    {
        foreach(var branch in node.Branches)
        {
            Visit(branch.condition);
            Visit(branch.body);
        }

        if(node.Else != null)
        {
            Visit(node.Else);
        }

        node.ResolveInfo.Type = SepiaTypeInfo.Void;
    }

    private void Visit(WhileStatementNode node)
    {
        Visit(node.Condition);
        Visit(node.Body);

        controls.Clear();

        node.ResolveInfo.Type = SepiaTypeInfo.Void;
    }

    private void Visit(ControlFlowStatementNode node)
    {
        node.ResolveInfo.Type = SepiaTypeInfo.Void;
        controls.Add(new(node.Token));
    }

    private void Visit(ForStatementNode node)
    {
        if(node.Declaration != null)
            Visit(node.Declaration);
        if (node.Condition != null)
            Visit(node.Condition);
        if(node.Action != null)
            Visit(node.Action);

        Visit(node.Body);

        controls.Clear();

        node.ResolveInfo.Type = SepiaTypeInfo.Void;
    }

    private void Visit(FunctionDeclarationStmtNode node)
    {
        Visit(node.Function);

        scope.Declare(node.Id.Value, new(node.Function.ResolveInfo.Clone(), node.Id.Value, true));

        node.ResolveInfo.Type = SepiaTypeInfo.Void;
    }

    private void Visit(ReturnStatementNode node)
    {
        Visit(node.Expression);
        returns.Add(node.Expression.ResolveInfo);

        node.ResolveInfo.Type = SepiaTypeInfo.Void;
    }
}
