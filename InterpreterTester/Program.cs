using Interpreter.AST;
using Interpreter.AST.Node;
using Interpreter.Common;
using Interpreter.Evaluate;
using Interpreter.Lex;
using Interpreter.Lex.Literal;
using Interpreter.Parse;
using Interpreter.Utility;
using System.Diagnostics;
using System.Text;

Stopwatch stopwatch = new Stopwatch();

foreach(var s in new string[]
{
    @"'a' * 'b'",
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

        string pretty_printed = string.Empty;

        if(parser.TryParse(out AbstractSyntaxTree? parsed, out List<InterpretError> parseErrors))
        {
            stopwatch.Stop();

            StringBuilder sb = new();
            using (StringWriter sw = new StringWriter(sb))
            {
                PrettyPrinter prettyPrinter = new PrettyPrinter(sw);
                prettyPrinter.Visit(parsed.Root);
            }

            pretty_printed = sb.ToString();

            WriteLine(pretty_printed);
        }
        else
        {
            stopwatch.Stop();

            WriteLine($"Failed to parse.");

            foreach (var error in parseErrors)
                WriteLine($"\t{error}");
        }

        double parser_time = stopwatch.Elapsed.TotalMilliseconds;

        WriteLine($"Evaluating the resulting expression.");
        Evaluator evaluator = new Evaluator();

        stopwatch.Restart();

        if(parsed != null)
        {
            try
            {
                var result = evaluator.Visit(parsed);

                WriteLine($"{pretty_printed} = {result}");
            }
            catch (Exception e)
            {
                WriteLine($"Failed to evaluate expression.");
                WriteLine($"\t{e.Message}");
            }
        }

        double evaluate_time = stopwatch.Elapsed.TotalMilliseconds;

        WriteLine($"Elapsed Time:");
        WriteLine($"\tTokenize: {tokenization_time}ms.");
        WriteLine($"\tParse: {parser_time}ms.");
        WriteLine($"\tEval: {evaluate_time}ms.");
    }
    catch (Exception e) 
    {
        WriteLine(e.Message);
    }

    Console.WriteLine(@"[\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/]");
}

void WriteLine(string? s = null) => Console.WriteLine($"# {(s?? string.Empty).Replace("\n", "\n# ").ReplaceLineEndings()}");