using Sepia.AST.Node.Expression;
using Sepia.Callable;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Lex;
using Sepia.Parse;
using Sepia.Value;
using Sepia.Value.Type;

namespace SepiaStandardLibrary;

public static class Function
{
    public static readonly Dictionary<string, ISepiaCallable> Functions = new()
    {
        {
            nameof(Write),
            new SepiaDelegateCallable(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String
            }, SepiaTypeInfo.Void, Write)
        },
        {
            nameof(WriteLine),
            new SepiaDelegateCallable(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String
            }, SepiaTypeInfo.Void, WriteLine)
        },
        {
            nameof(Exit),
            new SepiaDelegateCallable(new List<SepiaTypeInfo>(), SepiaTypeInfo.Void, Exit)
        },
        {
            nameof(Clear),
            new SepiaDelegateCallable(new List<SepiaTypeInfo>(), SepiaTypeInfo.Void, Clear)
        },
        {
            nameof(Time),
            new SepiaDelegateCallable(new List<SepiaTypeInfo>(), SepiaTypeInfo.Integer, Time)
        },
        {
            nameof(Sleep),
            new SepiaDelegateCallable(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.Integer
            }, SepiaTypeInfo.Void, Sleep)
        },
        {
            nameof(Random),
            new SepiaDelegateCallable(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.Integer,
                SepiaTypeInfo.Integer
            }, SepiaTypeInfo.Integer, Random)
        },
        {
            nameof(ReadChar),
            new SepiaDelegateCallable(new List<SepiaTypeInfo>(), SepiaTypeInfo.String, ReadChar)
        },
        {
            nameof(ReadLine),
            new SepiaDelegateCallable(new List<SepiaTypeInfo>(), SepiaTypeInfo.String, ReadLine)
        },
        {
            nameof(ParseInt),
            new SepiaDelegateCallable(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String
            }, SepiaTypeInfo.Integer, ParseInt)
        }
    };

    private static SepiaValue Write(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string s = args.First().Value!.ToString()!;

        Console.Write(s);
        return SepiaValue.Void;
    }

    private static SepiaValue WriteLine(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string s = args.First().Value!.ToString()!;

        Console.WriteLine(s);
        return SepiaValue.Void;
    }

    private static SepiaValue Exit(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        System.Environment.Exit(0);
        return SepiaValue.Void;
    }

    private static SepiaValue Clear(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        Console.Clear();
        return SepiaValue.Void;
    }

    private static SepiaValue Time(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        return new(DateTime.Now.Ticks, SepiaTypeInfo.Integer);
    }

    private static SepiaValue Sleep(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        long ms = (long)args.First().Value!;

        Thread.Sleep((int)Math.Min(ms, int.MaxValue));

        return SepiaValue.Void;
    }

    private static SepiaValue Random(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        long min = (long)args.First().Value!;
        long max = (long)args.ElementAt(1).Value!;

        return new(System.Random.Shared.NextInt64(min, max), SepiaTypeInfo.Integer);
    }

    private static SepiaValue ReadChar(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        int c = Console.Read();

        if (c < 0)
            return new(string.Empty, SepiaTypeInfo.String);
        else
            return new(((char)c).ToString(), SepiaTypeInfo.String);
    }

    private static SepiaValue ReadLine(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string line = Console.ReadLine()?? string.Empty;

        return new(line, SepiaTypeInfo.String);
    }

    private static SepiaValue ParseInt(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!.Trim();

        Lexer lexer = interpreter.createLexer(input);
        var tokens = lexer.Scan();
        var lexerr = tokens.Where(t => t.TokenType == TokenType.ERROR);

        if(lexerr.Any())
        {
            throw new SepiaException(new EvaluateError());
        }
        else if(lexerr.Count() > 1)
        {
            throw new SepiaException(new EvaluateError());
        }

        SepiaValue evaluated = interpreter.Visit(new LiteralExprNode(tokens.First()));
        if(evaluated.Type == SepiaTypeInfo.Integer)
        {
            return evaluated;
        }
        else
        {
            throw new SepiaException(new EvaluateError());
        }
    }
}