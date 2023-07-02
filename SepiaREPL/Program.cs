using Sepia.Analyzer;
using Sepia.AST;
using Sepia.AST.Node;
using Sepia.AST.Node.Statement;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Lex;
using Sepia.Parse;

Console.WriteLine($"Welcome to the Sepia REPL console. Please enter your statement and then press enter to submit.");

Evaluator interpreter = new Evaluator(
        (IEnumerable<Token> tokens) => new Parser(tokens),
        (string input) => new Lexer(input)
    )
    .RegisterFunctions(SepiaStandardLibrary.Function.Functions);

Resolver resolver = new(interpreter);

string? input;

while(true)
{
    try
    {
        Console.Write("> ");
        input = Console.ReadLine();
        if (input == null) return;
    }
    catch (Exception e)
    {
        Console.Error.WriteLine($"Failed to read input; {e.Message}");
        continue;
    }

    //Implicit semicolon in repl
    if (!(input.Trim().EndsWith(TokenType.SEMICOLON.GetSymbol()) || input.Trim().EndsWith(TokenType.R_BRACE.GetSymbol())))
    {
        input += TokenType.SEMICOLON.GetSymbol();
    }

    Lexer lexer = new(input);
    IEnumerable<Token> tokens;

    try
    {
        tokens = lexer.Scan();

        var lex_errors = tokens.Where(t => t.TokenType == TokenType.ERROR);

        if(lex_errors.Any())
        {
            foreach(var err in lex_errors)
            {
                Console.Error.WriteLine(err.Error?.ToString()?? $"A lexical error occurred.");
            }
            continue;
        }
    }
    catch (Exception e) 
    {
        Console.Error.WriteLine($"A lexical error occurred; {e.Message}");
        continue;
    }

    Parser parser = new Parser(tokens);
    bool parse_success = parser.TryParse(out AbstractSyntaxTree? parsed, out List<SepiaError> errors);

    if(parsed == null || !parse_success)
    {
        if(errors.Any())
        {
            foreach(var err in errors)
                Console.Error.WriteLine(err.Message);
        }
        else
        {
            Console.Error.WriteLine($"A syntax error occurred.");
        }

        continue;
    }

    //Make a clone of the current state of the resolver so that if it fails, it still works on future iterations
    Resolver currentResolverState = resolver.Clone();

    resolver.Visit(parsed);

    if(resolver.errors.Any())
    {
        foreach(var error in resolver.errors)
        {
            Console.Error.WriteLine(error);
        }

        resolver = currentResolverState;

        continue;
    }

    try
    {
        //Evaluate and print expression
        if (parsed.Root is ProgramNode program && program.statements.Count == 1 && program.statements[0] is ExpressionStmtNode exprStmt)
        {
            var result = interpreter.Visit(exprStmt.Expression);

            if (result != null)
            {
                string? result_output = result.ToString();

                if (!string.IsNullOrWhiteSpace(result_output))
                {
                    Console.WriteLine(result_output);
                }
            }
        }
        //Evaluate statement
        else
        {
            _ = interpreter.Visit(parsed);
        }
    }
    catch (Exception e)
    {
        Console.Error.WriteLine($"A runtime error occurred; {e.Message}");
    }
}