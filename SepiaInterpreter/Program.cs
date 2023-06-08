using Sepia.AST;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Lex;
using Sepia.Parse;

const string
    ARG_PATH = "path";

Dictionary<string, object> arg_pairs = new();
    
for(int i = 0; i < args.Length; i++)
{
    var arg = args[i];

    string[] split = arg.Split('=', 2);

    if(split.Length == 1)
    {
        if (i == 0)
        {
            arg_pairs[ARG_PATH] = split[0];
        }
        else
        {
            arg_pairs[split[0]] = true;
        }
    }
    else
    {
        arg_pairs[split[0]] = split[1];
    }
}

if(!arg_pairs.ContainsKey(ARG_PATH))
{
    throw new Exception($"Missing path.");
}

var path = arg_pairs[ARG_PATH].ToString()?? string.Empty;

if(string.IsNullOrWhiteSpace(path) || !File.Exists(path))
{
    throw new Exception($"Path '{path}' must point to an existing file.");
}

string file_content = File.ReadAllText(path);

Lexer lexer = new(file_content);

IEnumerable<Token> tokens = lexer.Scan();

var lex_errors = tokens.Where(t => t.TokenType == TokenType.ERROR);

if (lex_errors.Any())
{
    throw new AggregateException(lex_errors.Select(e => e.Error)
        .Where(e => e != null)
        .Select(e => new SepiaException(e!))
        .ToList());
}

Parser parser = new Parser(tokens);
bool parse_success = parser.TryParse(out AbstractSyntaxTree? parsed, out List<SepiaError> errors);

if (parsed == null || !parse_success)
{
    if (errors.Any())
    {
        throw new AggregateException(errors.Select(e => new SepiaException(e)).ToList());
    }
    else
    {
        throw new SepiaException($"A syntax error occurred.");
    }
}

Evaluator interpreter = new();
_ = interpreter.Visit(parsed);