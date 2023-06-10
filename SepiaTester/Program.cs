using Sepia.AST;
using Sepia.AST.Node;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Lex;
using Sepia.Lex.Literal;
using Sepia.Parse;
using Sepia.Utility;
using System.Diagnostics;
using System.Text;

Stopwatch stopwatch = new Stopwatch();

Evaluator interpreter = new Evaluator(
        (IEnumerable<Token> tokens) => new Parser(tokens),
        (string input) => new Lexer(input)
    )
    .RegisterNativeFunctions(SepiaStandardLibrary.Function.Functions);

foreach (var s in new string[]
{
    @"let y = 17 * 2;
let z;
let x = y / (z = 6 / 2);
x = x * 3;
print x;",
})
{
    Console.WriteLine(@"[\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/]");

    try
    {
        WriteLine($"Tokenizing the following input:\r\n{s}");
        Lexer scanner = new Lexer(s);
        bool has_errors = false;
        double tokenization_time = -1, parser_time = -1, evaluate_time = -1;

        stopwatch.Restart();

        List<Token> tokens = scanner.Scan().ToList();

        stopwatch.Stop();

        tokenization_time = stopwatch.Elapsed.TotalMilliseconds;

        WriteLine();
        WriteLine($"Finished tokenizing.");

        IEnumerable<Token> token_errors = tokens.Where(t => t.TokenType == TokenType.ERROR);

        if(token_errors.Any())
        {
            has_errors = true;
            WriteLine();
            WriteLine($"Encountered the following errors during tokenization:");
            foreach (Token error in token_errors)
                WriteLine(error.ToString());
        }
        else
        {
            WriteLine($"No errors encountered during tokenization.");
        }

        if(!has_errors)
        {
            WriteLine($"Parsing the resulting tokens.");
            Parser parser = new Parser(tokens);

            stopwatch.Restart();

            string pretty_printed = string.Empty;

            if (parser.TryParse(out AbstractSyntaxTree? parsed, out List<SepiaError> parseErrors))
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

                has_errors = true;
                WriteLine($"Failed to parse.");

                foreach (var error in parseErrors)
                    WriteLine($"\t{error}");
            }

            parser_time = stopwatch.Elapsed.TotalMilliseconds;

            if(!has_errors)
            {
                WriteLine($"Evaluating the resulting expression.");
                Console.WriteLine();

                stopwatch.Restart();

                if (parsed != null)
                {
                    try
                    {
                        var result = interpreter.Visit(parsed);
                    }
                    catch (Exception e)
                    {
                        WriteLine($"Failed to evaluate expression.");
                        WriteLine($"\t{e.Message}");
                    }
                }

                Console.WriteLine();

                evaluate_time = stopwatch.Elapsed.TotalMilliseconds;
            }
        }

        WriteLine($"Elapsed Time:");
        if(tokenization_time >= 0) WriteLine($"\tTokenize: {tokenization_time}ms.");
        if (parser_time >= 0) WriteLine($"\tParse: {parser_time}ms.");
        if (evaluate_time >= 0) WriteLine($"\tEval: {evaluate_time}ms.");
    }
    catch (Exception e) 
    {
        WriteLine(e.Message);
    }

    Console.WriteLine(@"[\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/]");
}

void WriteLine(string? s = null) => Console.WriteLine($"# {(s?? string.Empty).Replace("\n", "\n# ").ReplaceLineEndings()}");