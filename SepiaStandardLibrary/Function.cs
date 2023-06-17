using Sepia.AST.Node.Expression;
using Sepia.Callable;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Lex;
using Sepia.Parse;
using Sepia.Value;
using Sepia.Value.Type;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SepiaStandardLibrary;

public static class Function
{
    public static readonly Dictionary<string, ISepiaCallable> Functions = new()
    {
        {
            nameof(Write),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.String() }), Write)
        },
        {
            nameof(WriteLine),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.String() }), WriteLine)
        },
        {
            nameof(Exit),
            new SepiaDelegateCallable(new SepiaCallSignature(), Exit)
        },
        {
            nameof(Clear),
            new SepiaDelegateCallable(new SepiaCallSignature(), Clear)
        },
        {
            nameof(Time),
            new SepiaDelegateCallable(new SepiaCallSignature(SepiaTypeInfo.Integer()), Time)
        },
        {
            nameof(Sleep),
            new SepiaDelegateCallable(new SepiaCallSignature(SepiaTypeInfo.Integer()), Sleep)
        },
        {
            nameof(Random),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.Integer(), SepiaTypeInfo.Integer()}, SepiaTypeInfo.Integer()), Random)
        },
        {
            nameof(ReadChar),
            new SepiaDelegateCallable(new SepiaCallSignature(SepiaTypeInfo.String()), ReadChar)
        },
        {
            nameof(ReadLine),
            new SepiaDelegateCallable(new SepiaCallSignature(SepiaTypeInfo.String()), ReadLine)
        },
        {
            nameof(ParseInt),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.String() }, SepiaTypeInfo.Integer()), ParseInt)
        },
        {
            nameof(IsInt),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.String() }, SepiaTypeInfo.Boolean()), IsInt)
        },
        {
            nameof(ParseFloat),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.String() }, SepiaTypeInfo.Float()), ParseFloat)
        },
        {
            nameof(IsFloat),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.String() }, SepiaTypeInfo.Boolean()), IsFloat)
        },
        {
            nameof(IntToFloat),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.Integer() }, SepiaTypeInfo.Float()), IntToFloat)
        },
        {
            nameof(FloatToInt),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.Float() }, SepiaTypeInfo.Integer()), FloatToInt)
        },
        {
            nameof(Round),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.Float() }, SepiaTypeInfo.Float()), Round)
        },
        {
            nameof(Floor),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.Float() }, SepiaTypeInfo.Float()), Floor)
        },
        {
            nameof(Ceiling),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.Float() }, SepiaTypeInfo.Float()), Ceiling)
        },
        {
            nameof(Truncate),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>() { SepiaTypeInfo.Float() }, SepiaTypeInfo.Float()), Truncate)
        },
        {
            nameof(Delay),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.Integer(),
                SepiaTypeInfo.Function().WithCallSignature(new SepiaCallSignature())
            }), Delay)
        },
        {
            nameof(Benchmark),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.Function().WithCallSignature(new SepiaCallSignature())
            }, SepiaTypeInfo.Integer()), Benchmark)
        },
        {
            nameof(Substring),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String(),
                SepiaTypeInfo.Integer(),
                SepiaTypeInfo.Integer()
            }, SepiaTypeInfo.String()), Substring)
        },
        {
            nameof(CharAt),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String(),
                SepiaTypeInfo.Integer()
            }, SepiaTypeInfo.String()), CharAt)
        },
        {
            nameof(Concat),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String(),
                SepiaTypeInfo.String()
            }, SepiaTypeInfo.String()), Concat)
        },
        {
            nameof(Upper),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String()
            }, SepiaTypeInfo.String()), Upper)
        },
        {
            nameof(Lower),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String()
            }, SepiaTypeInfo.String()), Lower)
        },
        {
            nameof(Trim),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String()
            }, SepiaTypeInfo.String()), Trim)
        },
        {
            nameof(TrimStart),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String()
            }, SepiaTypeInfo.String()), TrimStart)
        },
        {
            nameof(TrimEnd),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String()
            }, SepiaTypeInfo.String()), TrimEnd)
        },
        {
            nameof(Replace),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String(),
                SepiaTypeInfo.String(),
                SepiaTypeInfo.String()
            }, SepiaTypeInfo.String()), Replace)
        },
        {
            nameof(RegexReplace),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String(),
                SepiaTypeInfo.String(),
                SepiaTypeInfo.String()
            }, SepiaTypeInfo.String()), RegexReplace)
        },
        {
            nameof(Length),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String()
            }, SepiaTypeInfo.Integer()), Length)
        },
        {
            nameof(IndexOf),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String(),
                SepiaTypeInfo.String()
            }, SepiaTypeInfo.Integer()), IndexOf)
        },
        {
            nameof(RegexIndexOf),
            new SepiaDelegateCallable(new SepiaCallSignature(new List<SepiaTypeInfo>()
            {
                SepiaTypeInfo.String(),
                SepiaTypeInfo.String()
            }, SepiaTypeInfo.Integer()), RegexIndexOf)
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
        return new(DateTime.Now.Ticks, SepiaTypeInfo.Integer());
    }

    private static SepiaValue Sleep(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        long ms = Convert.ToInt64(args.First().Value!);

        Thread.Sleep(Convert.ToInt32(Math.Min(ms, int.MaxValue)));

        return SepiaValue.Void;
    }

    private static SepiaValue Random(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        long min = Convert.ToInt64(args.First().Value!);
        long max = Convert.ToInt64(args.ElementAt(1).Value!);

        return new(System.Random.Shared.NextInt64(min, max), SepiaTypeInfo.Integer());
    }

    private static SepiaValue ReadChar(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        int c = Console.Read();

        if (c < 0)
            return new(string.Empty, SepiaTypeInfo.String());
        else
            return new(((char)c).ToString(), SepiaTypeInfo.String());
    }

    private static SepiaValue ReadLine(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string line = Console.ReadLine()?? string.Empty;

        return new(line, SepiaTypeInfo.String());
    }

    private static SepiaValue Substring(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;
        int start = Convert.ToInt32(Math.Max(0, Math.Min(Convert.ToInt64(args.ElementAt(1).Value!), input.Length - 1)));
        int length = Convert.ToInt32(Math.Min(input.Length, Convert.ToInt64(args.ElementAt(2).Value!)));

        return new(input.Substring(start, length), SepiaTypeInfo.String());
    }

    private static SepiaValue CharAt(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;
        int index = Convert.ToInt32(Math.Max(0, Math.Min(Convert.ToInt64(args.ElementAt(1).Value!), input.Length - 1)));

        return new(input[index].ToString(), SepiaTypeInfo.String());
    }

    private static SepiaValue Concat(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string a = args.First().Value!.ToString()!;
        string b = args.ElementAt(1).Value!.ToString()!;

        return new($"{a}{b}", SepiaTypeInfo.String());
    }

    private static SepiaValue Upper(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.ToUpper(), SepiaTypeInfo.String());
    }

    private static SepiaValue Lower(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.ToLower(), SepiaTypeInfo.String());
    }

    private static SepiaValue Trim(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.Trim(), SepiaTypeInfo.String());
    }

    private static SepiaValue TrimStart(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.TrimStart(), SepiaTypeInfo.String());
    }

    private static SepiaValue TrimEnd(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.TrimEnd(), SepiaTypeInfo.String());
    }

    private static SepiaValue Replace(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;
        string pattern = args.ElementAt(1).Value!.ToString()!;
        string replacement = args.ElementAt(2).Value!.ToString()!;

        return new(input.Replace(pattern, replacement), SepiaTypeInfo.String());
    }

    private static SepiaValue RegexReplace(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;
        string pattern = args.ElementAt(1).Value!.ToString()!;
        string replacement = args.ElementAt(2).Value!.ToString()!;

        Regex regex = new(pattern);

        return new(regex.Replace(input, replacement), SepiaTypeInfo.String());
    }

    private static SepiaValue Length(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.Length, SepiaTypeInfo.Integer());
    }

    private static SepiaValue IndexOf(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;
        string pattern = args.ElementAt(1).Value!.ToString()!;

        return new(input.IndexOf(pattern), SepiaTypeInfo.Integer());
    }

    private static SepiaValue RegexIndexOf(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        var index = 0;

        if(!string.IsNullOrEmpty(input))
        {
            string pattern = args.ElementAt(1).Value!.ToString()!;

            Regex regex = new(pattern);
            index = regex.Matches(input).FirstOrDefault()?.Index?? -1;
        }

        return new(index, SepiaTypeInfo.Integer());
    }

    private static SepiaValue ParseInt(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!.Trim();

        return new(long.Parse(input), SepiaTypeInfo.Integer());
    }

    private static SepiaValue IsInt(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!.Trim();

        return new(long.TryParse(input, out _), SepiaTypeInfo.Boolean());
    }

    private static SepiaValue ParseFloat(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!.Trim();

        return new(double.Parse(input), SepiaTypeInfo.Float());
    }

    private static SepiaValue IsFloat(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!.Trim();

        return new(double.TryParse(input, out _), SepiaTypeInfo.Boolean());
    }

    private static SepiaValue IntToFloat(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        long input = Convert.ToInt64(args.First().Value!);
        return new(Convert.ToDouble(input), SepiaTypeInfo.Float());
    }

    private static SepiaValue FloatToInt(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        double input = Convert.ToDouble(args.First().Value!);
        return new(Convert.ToInt64(input), SepiaTypeInfo.Integer());
    }

    private static SepiaValue Round(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        double input = Convert.ToDouble(args.First().Value!);
        return new(Math.Round(input), SepiaTypeInfo.Float());
    }

    private static SepiaValue Ceiling(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        double input = Convert.ToDouble(args.First().Value!);
        return new(Math.Ceiling(input), SepiaTypeInfo.Float());
    }

    private static SepiaValue Floor(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        double input = Convert.ToDouble(args.First().Value!);
        return new(Math.Floor(input), SepiaTypeInfo.Float());
    }

    private static SepiaValue Truncate(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        double input = Convert.ToDouble(args.First().Value!);
        return new(Math.Truncate(input), SepiaTypeInfo.Float());
    }

    private static SepiaValue Delay(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        long delay = Convert.ToInt64(args.First().Value!);
        ISepiaCallable callable = (ISepiaCallable)args.ElementAt(1).Value!;

        Thread.Sleep(Convert.ToInt32(Math.Min(delay, int.MaxValue)));

        return callable.Call(interpreter, Enumerable.Empty<SepiaValue>());
    }

    private static SepiaValue Benchmark(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        try
        {
            ISepiaCallable callable = (ISepiaCallable)args.First().Value!;
            _ = callable.Call(interpreter, Enumerable.Empty<SepiaValue>());
        }
        finally
        {
            stopwatch.Stop();
        }

        return new(stopwatch.ElapsedMilliseconds, SepiaTypeInfo.Integer());
    }
}