using Sepia.AST.Node;
using Sepia.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sepia.Lex.Literal;
using Sepia.Lex;
using Sepia.AST.Node.Expression;
using Sepia.AST.Node.Statement;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Sepia.Utility;

public class PrettyPrinter : IASTNodeVisitor<AbstractSyntaxTree>,
    IASTNodeVisitor<ASTNode>,
    IASTNodeVisitor<ProgramNode>,
    IASTNodeVisitor<ExpressionNode>,
    IASTNodeVisitor<StatementNode>
{
    private static readonly Regex NEW_LINE = new("\r?\n.+", RegexOptions.Compiled);
    private readonly StringWriter StringWriter;
    private int indent = 0;

    public PrettyPrinter(StringWriter stringWriter)
    {
        StringWriter = stringWriter;
    }

    public void Visit(AbstractSyntaxTree ast)
    {
        Visit(ast.Root);
    }

    public void Visit(ASTNode node)
    {
        if (node is ProgramNode pnode)
            Visit(pnode);
        else if (node is ExpressionNode enode)
            Visit(enode);
        else if (node is StatementNode snode)
            Visit(snode);
        else
            throw new NotImplementedException($"Cannot pretty print node of type '{node.GetType().Name}'");
    }

    public void Visit(ProgramNode node)
    {
        foreach (var statement in node.statements)
        {
            Visit(statement);
            WriteLine();
        }
    }

    public void Visit(ExpressionNode node)
    {
        if (node is LiteralExprNode lnode)
            Visit(lnode);
        else if (node is UnaryPrefixExprNode upnode)
            Visit(upnode);
        else if (node is GroupExprNode gnode)
            Visit(gnode);
        else if (node is BinaryExprNode bnode)
            Visit(bnode);
        else if (node is InterpolatedStringExprNode enode)
            Visit(enode);
        else if (node is IdentifierExprNode idnode)
            Visit(idnode);
        else if (node is AssignmentExprNode assignnode)
            Visit(assignnode);
        else if (node is CallExprNode callnode)
            Visit(callnode);
        else
            throw new NotImplementedException($"Cannot pretty print node of type '{node.GetType().Name}'");
    }

    public void Visit(StatementNode node)
    {
        WriteIndent();

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
        else if (node is ForStatementNode fornode)
            Visit(fornode);
        else
            throw new NotImplementedException($"Cannot pretty print node of type '{node.GetType().Name}'");
    }

    private void Visit(BinaryExprNode node)
    {
        Visit(node.Left);
        Write($" {node.Operator.TokenType.GetSymbol()} ");
        Visit(node.Right);
    }

    private void Visit(GroupExprNode node)
    {
        Write(TokenType.L_PAREN.GetSymbol());
        Visit(node.Inner);
        Write(TokenType.R_PAREN.GetSymbol());
    }

    private void Visit(UnaryPrefixExprNode node)
    {
        Write(node.Operator.TokenType.GetSymbol());
        Visit(node.Right);
    }

    private void Visit(InterpolatedStringExprNode node)
    {
        Write(TokenType.BACKTICK.GetSymbol());

        foreach (var inner in node.Segments)
        {
            if (inner is LiteralExprNode literal && literal.Literal is StringLiteral sliteral)
            {
                Write(sliteral.Value ?? string.Empty);
            }
            else
            {
                Write(TokenType.L_BRACE.GetSymbol());
                Visit(inner);
                Write(TokenType.R_BRACE.GetSymbol());
            }
        }

        Write(TokenType.BACKTICK.GetSymbol());
    }

    private void Visit(IdentifierExprNode node)
    {
        Write(node.Id.Value);
    }

    private void Visit(LiteralExprNode node)
    {
        if (node.Value.Literal == null)
            throw new InvalidOperationException($"Token '{node.Value}' does not represent a literal.");

        var literal = node.Value.Literal;

        if (literal is WhitespaceLiteral) { }
        else if (literal is CommentLiteral) { }
        else if (literal is VoidLiteral) { }
        else if (literal is NullLiteral lnull) Write(lnull);
        else if (literal is BooleanLiteral lbool) Write(lbool);
        else if (literal is IdLiteral lid) Write(lid);
        else if (literal is NumberLiteral lnumber) Write(lnumber);
        else if (literal is StringLiteral lstring) Write(lstring);
        else throw new NotImplementedException($"Cannot pretty print literal of type '{node.Value.Literal.GetType().Name}'");
    }

    private void Visit(AssignmentExprNode node)
    {
        Write(node.Id.Value);

        Write($" {node.AssignmentType.TokenType.GetSymbol()} ");
        Visit(node.Assignment);
    }

    private void Visit(CallExprNode node)
    {
        Visit(node.Callable);

        Write($"{TokenType.L_PAREN.GetSymbol()}");

        for(int i = 0; i < node.Arguments.Count; i++)
        {
            Visit(node.Arguments[i]);
            if (i < node.Arguments.Count - 1)
                Write($"{TokenType.COMMA.GetSymbol()} ");
        }

        Write($"{TokenType.R_PAREN.GetSymbol()}");
    }

    private void Visit(ExpressionStmtNode node)
    {
        Visit(node.Expression);
        Write(TokenType.SEMICOLON.GetSymbol());
    }

    private void Visit(DeclarationStmtNode node)
    {
        Write($"{TokenType.LET.GetSymbol()} {node.Id.Value}");

        if(node.Type != null)
        {
            Write($"{TokenType.COLON.GetSymbol()} {node.Type.TypeName}");
        }

        if(node.Assignment != null)
        {
            Write($" {TokenType.EQUAL.GetSymbol()} ");
            Visit(node.Assignment);
        }

        Write(TokenType.SEMICOLON.GetSymbol());
    }

    private void Visit(Block block)
    {
        WriteIndent();
        WriteLine(TokenType.L_BRACE.GetSymbol());
        indent++;
        try
        {
            foreach (var statement in block.Statements)
            {
                Visit(statement);
                WriteLine();
            }
        }
        finally
        {
            indent--;
        }
        WriteIndent();
        WriteLine(TokenType.R_BRACE.GetSymbol());
    }

    private void Visit(ConditionalStatementNode node)
    {
        for(int i = 0; i < node.Branches.Count; i++)
        {
            var branch = node.Branches[i];

            if (i > 0) 
                WriteIndent();
            Write($"{(i == 0 ? string.Empty : $"{TokenType.ELSE.GetSymbol()} ")}{TokenType.IF.GetSymbol()} ");
            Visit(branch.condition);
            WriteLine();
            Visit(branch.body);
        }

        if(node.Else != null)
        {
            WriteIndent();
            Write(TokenType.ELSE.GetSymbol());
            WriteLine();
            Visit(node.Else);
        }
    }

    private void Visit(WhileStatementNode node)
    {
        Write($"{TokenType.WHILE.GetSymbol()} ");
        Visit(node.Condition);
        WriteLine();
        Visit(node.Body);
    }

    private void Visit(ControlFlowStatementNode node)
    {
        Write($"{node.Token.TokenType.GetSymbol()}{TokenType.SEMICOLON.GetSymbol()}");
    }

    private void Visit(ForStatementNode node)
    {
        Write($"{TokenType.FOR.GetSymbol()} ");

        if (node.Declaration != null)
            Visit(node.Declaration);
        else
            Write($"{TokenType.SEMICOLON.GetSymbol()}");

        Write($" ");

        if (node.Condition != null)
            Visit(node.Condition);
        else
            Write($"{TokenType.SEMICOLON.GetSymbol()}");

        Write($" ");

        if (node.Action != null)
            Visit(node.Action);

        WriteLine();
        Visit(node.Body);
    }

    private string IndentString()
    {
        StringBuilder sb = new StringBuilder();
        string unit = "    ";
        for(int i = 0; i < indent; i++)
        {
            sb.Append(unit);
        }

        return sb.ToString();
    }

    private void WriteIndent()
    {
        StringWriter.Write(IndentString());
    }

    private void Write(object? value = null)
    {
        string indent_string = IndentString();

        string indented = NEW_LINE.Replace(value?.ToString()?? string.Empty, $"{System.Environment.NewLine}{indent_string}");
        StringWriter.Write(indented);
    }

    private void WriteLine(object? value = null) => Write($"{value?? string.Empty}{System.Environment.NewLine}");
}