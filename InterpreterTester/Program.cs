using Interpreter.AST;
using Interpreter.AST.Node;
using Interpreter.Lex;
using Interpreter.Lex.Literal;
using Interpreter.Parse;
using Interpreter.Utility;
using System.Diagnostics;
using System.Text;

Stopwatch stopwatch = new Stopwatch();

foreach(var s in new string[]
{
    "5 + 6'aaa'"
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

        double tokenization_time = stopwatch.Elapsed.TotalMilliseconds;

        WriteLine();
        WriteLine($"Finished tokenizing.");

        IEnumerable<Token> token_errors = tokens.Where(t => t.TokenType == TokenType.ERROR);

        if(token_errors.Any())
        {
            WriteLine();
            WriteLine($"Encountered the following errors during tokenization:");
            foreach (Token error in token_errors)
                WriteLine(error.ToString());
        }
        else
        {
            WriteLine($"No errors encountered during tokenization.");
        }

        WriteLine($"Parsing the resulting tokens.");
        Parser parser = new Parser(tokens);

        stopwatch.Restart();

        AbstractSyntaxTree? parsed = null;

        try
        {
            parsed = parser.Parse();
        }
        catch (Exception e)
        {
            WriteLine($"{e.Message}");
        }

        stopwatch.Stop();

        if (parsed != null)
        {
            StringBuilder sb = new();
            using (StringWriter sw = new StringWriter(sb))
            {
                PrettyPrinter prettyPrinter = new PrettyPrinter(sw);
                prettyPrinter.Visit(parsed.Root);
            }

            WriteLine(sb.ToString());
        }

        double parser_time = stopwatch.Elapsed.TotalMilliseconds;

        WriteLine($"Elapsed Time:");
        WriteLine($"\tTokenize: {tokenization_time}ms.");
        WriteLine($"\tParse: {parser_time}ms.");
    }
    catch (Exception e) 
    {
        WriteLine(e.Message);
    }

    Console.WriteLine(@"[\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/]");
}

void WriteLine(string? s = null) => Console.WriteLine($"# {(s?? string.Empty).Replace("\n", "\n# ").ReplaceLineEndings()}");