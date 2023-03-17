using Interpreter.AST;
using Interpreter.AST.Node;
using Interpreter.Lex;
using Interpreter.Lex.Literal;
using Interpreter.Utility;
using System.Diagnostics;
using System.Text;

AbstractSyntaxTree tree = new AbstractSyntaxTree(
    new UnaryPrefixNode(new Token(TokenType.BANG, (0, 0, 0, 0)), new GroupNode(new BinaryNode(
    new GroupNode(new LiteralNode(new Token(TokenType.ID, (0, 0, 0, 0), literal: new IdLiteral("AAAA")))),
    new Token(TokenType.PLUS, (0, 0, 0, 0)),
    new UnaryPrefixNode(new Token(TokenType.MINUS, (0, 0, 0, 0)), 
    new LiteralNode(new Token(TokenType.NUMBER, (0, 0, 0, 0), literal: new NumberLiteral("0FA1", NumberType.INTEGER, NumberBase.HEX))))
    )))
);

StringBuilder prettyPrinted = new StringBuilder();
using(StringWriter sw = new StringWriter(prettyPrinted))
{
    PrettyPrinter prettyPrinter = new PrettyPrinter(sw);
    prettyPrinter.Visit(tree.Root);
}

Console.WriteLine(prettyPrinted);

Stopwatch stopwatch = new Stopwatch();

foreach(var s in new string[]
{
    
})
{
    Console.WriteLine(@"[\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/]");

    try
    {
        WriteLine($"Tokenizing the following input:\r\n{s}");
        Lexer scanner = new Lexer(s);

        stopwatch.Restart();

        List<Token> tokens = scanner.Scan().ToList();

        stopwatch.Stop();

        WriteLine();
        WriteLine($"Finished tokenizing.");
        WriteLine();

        IEnumerable<Token> token_errors = tokens.Where(t => t.TokenType == TokenType.ERROR);

        if(token_errors.Any())
        {
            WriteLine($"Encountered the following errors during tokenization:");
            foreach (Token error in token_errors)
                WriteLine(error.ToString());
        }
        else
        {
            WriteLine($"No errors encountered during tokenization.");
        }

        WriteLine($"Elapsed Time: {stopwatch.Elapsed.TotalMilliseconds}ms.");
    }
    catch (Exception e) 
    {
        WriteLine(e.Message);
    }

    Console.WriteLine(@"[\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/]");
}

void WriteLine(string? s = null) => Console.WriteLine($"# {(s?? string.Empty).Replace("\n", "\n# ").ReplaceLineEndings()}");