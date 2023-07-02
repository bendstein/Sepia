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
using Sepia.Value.Type;

namespace Sepia.PrettyPrint;

public class PrettyPrinter : IASTNodeVisitor<AbstractSyntaxTree>,
    IASTNodeVisitor<ASTNode>,
    IASTNodeVisitor<ProgramNode>,
    IASTNodeVisitor<ExpressionNode>,
    IASTNodeVisitor<StatementNode>
{
    private static readonly Regex NEW_LINE = new("\r?\n.+", RegexOptions.Compiled);
    private readonly StringWriter StringWriter;
    private readonly PrettyPrintSettings Settings;
    private int indent = 0;

    public PrettyPrinter(StringWriter stringWriter, PrettyPrintSettings? settings = null)
    {
        StringWriter = stringWriter;
        Settings = settings?? new();
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
        else if (node is InlineFunctionExpressionNode inlineFuncNode)
            Visit(inlineFuncNode);
        else if (node is FunctionExpressionNode funcNode)
            Visit(funcNode);
        else if (node is ValueExpressionNode valueNode)
            Visit(valueNode);
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
        else if (node is ReturnStatementNode retnode)
            Visit(retnode);
        else if (node is FunctionDeclarationStmtNode funcdecnode)
            Visit(funcdecnode);
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
        Write(node.Id.ResolveInfo.Name);
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
        Write(node.Id.ResolveInfo.Name);

        Write($" {node.AssignmentType.TokenType.GetSymbol()} ");
        Visit(node.Assignment);
    }

    private void Visit(CallExprNode node)
    {
        Visit(node.Callable);

        Write($"{TokenType.L_PAREN.GetSymbol()}");

        for (int i = 0; i < node.Arguments.Count; i++)
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
        Write($"{TokenType.LET.GetSymbol()} {node.Id.ResolveInfo.Name}");

        if (node.Type != null)
        {
            Write($"{TokenType.COLON.GetSymbol()} ");
            Visit(node.Type);
        }

        if (node.Assignment != null)
        {
            Write($" {TokenType.EQUAL.GetSymbol()} ");
            Visit(node.Assignment);
        }

        Write(TokenType.SEMICOLON.GetSymbol());
    }

    private void Visit(Block block, bool endWithNewLine = true)
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

        if(endWithNewLine)
        {
            WriteLine(TokenType.R_BRACE.GetSymbol());
        }
        else
        {
            Write(TokenType.R_BRACE.GetSymbol());
        }
    }

    private void Visit(ConditionalStatementNode node)
    {
        for (int i = 0; i < node.Branches.Count; i++)
        {
            var branch = node.Branches[i];

            if (i > 0)
                WriteIndent();
            Write($"{(i == 0 ? string.Empty : $"{TokenType.ELSE.GetSymbol()} ")}{TokenType.IF.GetSymbol()} ");
            Visit(branch.condition);
            WriteLine();
            Visit(branch.body);
        }

        if (node.Else != null)
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

        Write(" ");

        if (node.Condition != null)
            Visit(node.Condition);
        else
            Write($"{TokenType.SEMICOLON.GetSymbol()}");

        Write(" ");

        if (node.Action != null)
            Visit(node.Action);

        WriteLine();
        Visit(node.Body);
    }

    private void Visit(InlineFunctionExpressionNode node)
    {
        Write($"{TokenType.L_PAREN.GetSymbol()}");

        for(int i = 0; i < node.Arguments.Count(); i++)
        {
            var arg = node.Arguments.ElementAt(i);

            if(i > 0)
                Write($"{TokenType.COMMA.GetSymbol()} ");

            Write($"{arg.id.ResolveInfo.Name}{TokenType.COLON.GetSymbol()} ");
            Visit(arg.type);
        }

        Write($"{TokenType.R_PAREN.GetSymbol()} {TokenType.ARROW.GetSymbol()} ");
        Visit(node.Expression);
    }

    private void Visit(FunctionExpressionNode node)
    {
        Write($"{TokenType.L_PAREN.GetSymbol()}");

        for (int i = 0; i < node.Arguments.Count(); i++)
        {
            var arg = node.Arguments.ElementAt(i);

            if (i > 0)
                Write($"{TokenType.COMMA.GetSymbol()} ");

            Write($"{arg.id.ResolveInfo.Name}{TokenType.COLON.GetSymbol()} ");
            Visit(arg.type);
        }

        Write($"{TokenType.R_PAREN.GetSymbol()}");

        if(node.ReturnType != SepiaTypeInfo.TypeVoid(false))
        {
            Write($"{TokenType.COLON.GetSymbol()} ");
            Visit(node.ReturnType);
        }

        WriteLine();

        Visit(node.Body, false);
    }

    private void Visit(ValueExpressionNode node)
    {
        Write(node.Value);
    }

    private void Visit(ReturnStatementNode node)
    {
        Write(TokenType.RETURN.GetSymbol());
        if(node.Expression is ValueExpressionNode vexpr && vexpr.Value == Value.SepiaValue.Void)
        {

        }
        else
        {
            Write(' ');
            Visit(node.Expression);
        }

        Write(TokenType.SEMICOLON.GetSymbol());
    }

    private void Visit(FunctionDeclarationStmtNode node)
    {
        Write($"{TokenType.FUNC.GetSymbol()} {node.Id.ResolveInfo.Name}");
        Visit(node.Function);
        WriteLine();
    }

    private void Visit(SepiaTypeInfo type)
    {
        Write(type.TypeName);

        if(type.CallSignature != null)
        {
            var sig = type.CallSignature;

            Write($"{TokenType.L_PAREN.GetSymbol()}");

            for (int i = 0; i < sig.Arguments.Count; i++)
            {
                var arg = sig.Arguments[i];

                if (i > 0)
                    Write($"{TokenType.COMMA.GetSymbol()} ");

                Visit(arg);
            }

            if (sig.ReturnType != SepiaTypeInfo.TypeVoid(false))
            {
                Write($"{TokenType.SEMICOLON.GetSymbol()} ");
                Visit(sig.ReturnType);
            }

            Write($"{TokenType.R_PAREN.GetSymbol()}");
        }
    }

    private string IndentString()
    {
        StringBuilder sb = new StringBuilder();

        StringBuilder unitBuilder = new();

        if(Settings.TabWidth > 0)
        {
            for (int i = 0; i < Settings.TabWidth; i++)
            {
                unitBuilder.Append(' ');
            }
        }

        string unit = unitBuilder.ToString();

        for (int i = 0; i < indent; i++)
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

        string indented = NEW_LINE.Replace(value?.ToString() ?? string.Empty, $"{Environment.NewLine}{indent_string}");
        StringWriter.Write(indented);
    }

    private void WriteLine(object? value = null) => Write($"{value ?? string.Empty}{Environment.NewLine}");
}