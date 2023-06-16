using Sepia.AST.Node.Expression;
using Sepia.AST.Node.Statement;
using Sepia.AST.Node;
using Sepia.AST;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Utility;

namespace Sepia.Analyzer;
public class Resolver : IASTNodeVisitor<AbstractSyntaxTree>,
    IASTNodeVisitor<ASTNode>,
    IASTNodeVisitor<ProgramNode>,
    IASTNodeVisitor<ExpressionNode>,
    IASTNodeVisitor<BinaryExprNode>,
    IASTNodeVisitor<GroupExprNode>,
    IASTNodeVisitor<UnaryPrefixExprNode>,
    IASTNodeVisitor<InterpolatedStringExprNode>,
    IASTNodeVisitor<LiteralExprNode>,
    IASTNodeVisitor<IdentifierExprNode>,
    IASTNodeVisitor<CallExprNode>,
    IASTNodeVisitor<ValueExpressionNode>,
    IASTNodeVisitor<FunctionExpressionNode>,
    IASTNodeVisitor<StatementNode>,
    IASTNodeVisitor<ExpressionStmtNode>,
    IASTNodeVisitor<DeclarationStmtNode>,
    IASTNodeVisitor<AssignmentExprNode>,
    IASTNodeVisitor<Block>,
    IASTNodeVisitor<ConditionalStatementNode>,
    IASTNodeVisitor<WhileStatementNode>,
    IASTNodeVisitor<ControlFlowStatementNode>,
    IASTNodeVisitor<ForStatementNode>,
    IASTNodeVisitor<FunctionDeclarationStmtNode>,
    IASTNodeVisitor<ReturnStatementNode>
{
    private readonly Evaluator evaluator;
    private readonly Dictionary<string, ScopeInfo> scopes = new();

    public Resolver(Evaluator evaluator)
    {
        this.evaluator = evaluator;
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
    }

    public void Visit(ProgramNode node)
    {
        foreach (var statement in node.statements)
            Visit(statement);
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
            throw new SepiaException(new AnalyzerError($"Cannot evaluate node of type '{node.GetType().Name}'."));
    }

    public void Visit(BinaryExprNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(GroupExprNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(UnaryPrefixExprNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(InterpolatedStringExprNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(LiteralExprNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(IdentifierExprNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(CallExprNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(ValueExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(FunctionExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(StatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(ExpressionStmtNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(DeclarationStmtNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(AssignmentExprNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Block node)
    {
        throw new NotImplementedException();
    }

    public void Visit(ConditionalStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(WhileStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(ControlFlowStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(ForStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(FunctionDeclarationStmtNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(ReturnStatementNode node)
    {
        throw new NotImplementedException();
    }
}
