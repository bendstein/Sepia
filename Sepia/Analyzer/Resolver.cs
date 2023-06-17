using Sepia.AST.Node.Expression;
using Sepia.AST.Node.Statement;
using Sepia.AST.Node;
using Sepia.AST;
using Sepia.Evaluate;
using Sepia.Utility;
using Sepia.Value.Type;
using Sepia.Lex.Literal;
using Sepia.Callable;
using System.Diagnostics.CodeAnalysis;
using Sepia.Lex;

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
    public List<AnalyzerError> errors = new();

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

            global.Declare(key, new(new ResolveInfo(type), key, evaluator.globals.Initialized(key, 0)));
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
            errors.Add(new($"Cannot resolve node of type '{node.GetType().Name}'.", node.Location));

        node.ResolveInfo.Type = SepiaTypeInfo.Void();
    }

    public void Visit(ProgramNode node)
    {
        foreach (var statement in node.statements)
            Visit(statement);

        node.ResolveInfo.Type = SepiaTypeInfo.Void();
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
            errors.Add(new AnalyzerError($"Cannot resolve node of type '{node.GetType().Name}'.", node.Location));
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
            errors.Add(new AnalyzerError($"Cannot resolve node of type '{node.GetType().Name}'.", node.Location));
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

        node.ResolveInfo.Type = SepiaTypeInfo.String();
    }

    private void Visit(LiteralExprNode node)
    {
        var literal = node.Literal;

        if (literal is BooleanLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.Boolean();
        }
        else if (literal is CommentLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.Void();
        }
        else if (literal is NullLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.Null();
        }
        else if (literal is VoidLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.Void();
        }
        else if (literal is NumberLiteral numliteral)
        {
            switch (numliteral.NumberType)
            {
                case NumberType.INTEGER:
                    node.ResolveInfo.Type = SepiaTypeInfo.Integer();
                    break;
                case NumberType.FLOAT:
                default:
                    node.ResolveInfo.Type = SepiaTypeInfo.Float();
                    break;
            }
        }
        else if (literal is StringLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.String();
        }
        else if (literal is WhitespaceLiteral)
        {
            node.ResolveInfo.Type = SepiaTypeInfo.Void();
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
            errors.Add(new AnalyzerError($"Cannot access identifier '{node.Id.Value}' before it is declared.", node.Location));
        }
    }

    private void Visit(CallExprNode node)
    {
        Visit(node.Callable);

        foreach(var arg in node.Arguments)
        {
            Visit(arg);
        }

        if(node.Callable.ResolveInfo.Type.CallSignature != null)
        {
            var callsignature = node.Callable.ResolveInfo.Type.CallSignature;

            //Type will be the return type of the callable
            node.ResolveInfo.Type = callsignature.ReturnType.Clone();

            //Make sure arguments match
            for(int i = 0; i < node.Arguments.Count; i++)
            {
                var arg = node.Arguments[i];

                if(callsignature.Arguments.Count <= i)
                {
                    errors.Add(new AnalyzerError($"Too many arguments.", arg.Location));
                }
                else
                {
                    var callable_arg = callsignature.Arguments[i];

                    //Compare argument types
                    if(arg.ResolveInfo.Type != callable_arg)
                    {
                        errors.Add(new AnalyzerError($"Mismatched argument types; expected '{callable_arg}', got '{arg.ResolveInfo.Type}'."));
                    }
                }
            }

            //Check if missing arguments
            if(callsignature.Arguments.Count > node.Arguments.Count)
            {
                errors.Add(new AnalyzerError($"Too few arguments.", node.Location));
            }
        }
        //Not callable
        else
        {
            errors.Add(new AnalyzerError($"Not callable.", node.Location));
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

            //Set return type for scope
            scope.currentFunctionReturnType = node.ReturnType;

            List<ResolveInfo> arguments = new();

            foreach (var arg in node.Arguments)
            {
                ResolveInfo info = new(arg.type);

                scope.Declare(arg.id.Value, new(info, arg.id.Value, true));

                arguments.Add(info);
            }

            //Visit function body
            Visit(node.Body);

            //If function return type isn't void, every path must definitively return
            if(node.ReturnType != SepiaTypeInfo.Void(false) && !node.Body.ResolveInfo.AlwaysReturns)
            {
                errors.Add(new AnalyzerError($"Not all paths return!", node.Location));
            }

            node.ResolveInfo = new(SepiaTypeInfo.Function()
                .WithCallSignature(new SepiaCallSignature(arguments.Select(a => a.Type).ToList(), node.ReturnType)));
        }
        finally
        {
            scope = current;
        }
    }

    private void Visit(ExpressionStmtNode node)
    {
        Visit(node.Expression);
        node.ResolveInfo.Type = SepiaTypeInfo.Void();
    }

    private void Visit(DeclarationStmtNode node)
    {
        if (node.Assignment != null)
            Visit(node.Assignment);

        var id_type = node.Assignment?.ResolveInfo?.Type?? node.Type;

        //Make sure that if the type isn't explicitly given, that it can be inferred
        if (id_type == null)
        {
            errors.Add(new AnalyzerError($"Cannot infer type of '{node.Id.Value}'.", node.Location));
        }

        var resolveInfo = new ResolveInfo((id_type ?? SepiaTypeInfo.Void()).Clone());

        scope.Declare(node.Id.Value, new(resolveInfo, node.Id.Value, node.Assignment != null));

        node.ResolveInfo.Type = SepiaTypeInfo.Void();
    }

    private void Visit(AssignmentExprNode node)
    { 
        if(scope.TryGet(node.Id.Value, out ScopeInfo? info, out _, out _))
        {
            Visit(node.Assignment);

            info.Initialized = true;

            //Make sure assignment type matches variable type
            if(!info.ResolveInfo.TypeEqual(node.Assignment.ResolveInfo))
            {
                errors.Add(new AnalyzerError($"Cannot assign type '{node.Assignment.ResolveInfo.Type}' to '{node.Id.Value}' (type '{info.ResolveInfo.Type}').", node.Location));
            }

            node.ResolveInfo = node.Assignment.ResolveInfo.Clone();
        }
        //Cannot assign to variable before it is defined
        else
        {
            errors.Add(new AnalyzerError($"Cannot assign value to '{node.Id.Value}' before it is declared.", node.Location));
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

            node.ResolveInfo.Type = SepiaTypeInfo.Void();
            node.ResolveInfo.AlwaysReturns = node.Statements.Any(s => s.ResolveInfo.AlwaysReturns);
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

        node.ResolveInfo.Type = SepiaTypeInfo.Void();
        node.ResolveInfo.AlwaysReturns = node.Else != null && node.Else.ResolveInfo.AlwaysReturns &&
            node.Branches.All(b => b.body.ResolveInfo.AlwaysReturns);
    }

    private void Visit(WhileStatementNode node)
    {
        Visit(node.Condition);

        bool allowedLoopControls = scope.allowLoopControls;
        try
        {
            scope.allowLoopControls = true;
            Visit(node.Body);
        }
        finally
        {
            scope.allowLoopControls = allowedLoopControls;
        }

        node.ResolveInfo.Type = SepiaTypeInfo.Void();
    }

    private void Visit(ControlFlowStatementNode node)
    {
        if(node is ReturnStatementNode rnode)
        {
            Visit(rnode);
        }
        else
        {
            node.ResolveInfo.Type = SepiaTypeInfo.Void();

            if (!scope.allowLoopControls)
            {
                errors.Add(new AnalyzerError($"Control statement '{node.Token.TokenType.GetSymbol()}' is not allowed here.", node.Location));
            }
        }
    }

    private void Visit(ForStatementNode node)
    {
        if(node.Declaration != null)
            Visit(node.Declaration);
        if (node.Condition != null)
            Visit(node.Condition);
        if(node.Action != null)
            Visit(node.Action);

        bool allowedLoopControls = scope.allowLoopControls;
        try
        {
            scope.allowLoopControls = true;
            Visit(node.Body);
        }
        finally
        {
            scope.allowLoopControls = allowedLoopControls;
        }

        node.ResolveInfo.Type = SepiaTypeInfo.Void();

        node.ResolveInfo.Type = SepiaTypeInfo.Void();
    }

    private void Visit(FunctionDeclarationStmtNode node)
    {
        Visit(node.Function);

        scope.Declare(node.Id.Value, new(node.Function.ResolveInfo.Clone(), node.Id.Value, true));

        node.ResolveInfo.Type = SepiaTypeInfo.Void();
    }

    private void Visit(ReturnStatementNode node)
    {
        if(scope.currentFunctionReturnType == null)
        {
            errors.Add(new AnalyzerError($"Cannot '{node.Token.TokenType.GetSymbol()}' outside of a function body.", node.Location));
        }

        Visit(node.Expression);

        if(node.Expression.ResolveInfo.Type != scope.currentFunctionReturnType)
        {
            errors.Add(new AnalyzerError($"Cannot return type '{node.Expression.ResolveInfo.Type}' inside of a function returning type '{scope.currentFunctionReturnType}'.", node.Location));
        }

        node.ResolveInfo.Type = SepiaTypeInfo.Void();
        node.ResolveInfo.AlwaysReturns = true;
    }
}
