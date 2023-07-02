using Sepia.AST.Node.Expression;
using Sepia.Callable;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Lex;
using Sepia.Parse;
using Sepia.Value;
using Sepia.Value.Type;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SepiaStandardLibrary;

public static class Function
{
    public static readonly Dictionary<string, ISepiaCallable> Functions;

    public static readonly Dictionary<string, Dictionary<string, ISepiaValue>> MemberFunctions;

    static Function() 
    {
        Dictionary<string, ISepiaCallable> functions = new();
        Dictionary<string, Dictionary<string, ISepiaValue>> memberFunctions = new();

        foreach(var method in typeof(Function)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
        {
            var function = (SepiaDelegateCallable.SepiaDelegate)method.CreateDelegate(typeof(SepiaDelegateCallable.SepiaDelegate));

            foreach(var signatureAttr in method.GetCustomAttributes<SepiaFunctionSignatureAttribute>())
            {
                string name = signatureAttr.Name;
                string for_type = signatureAttr.For;

                SepiaCallSignature callSignature = new(new());
                SepiaDelegateCallable callable = new(callSignature, function);

                if (string.IsNullOrWhiteSpace(for_type))
                {
                    functions[name] = callable;
                }
                else
                {
                    if (!memberFunctions.ContainsKey(for_type))
                        memberFunctions[for_type] = new();

                    memberFunctions[for_type][name] = new SepiaValue(callable, SepiaTypeInfo.TypeFunction()
                        .WithCallSignature(callSignature));
                }
            }
        }

        Functions = functions;
        MemberFunctions = memberFunctions;
    }

    [SepiaFunctionSignature("string", Name = "Write")]
    private static SepiaValue Write(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string s = args.First().Value!.ToString()!;

        Console.Write(s);
        return SepiaValue.Void;
    }

    [SepiaFunctionSignature("string", Name = "WriteLine")]
    private static SepiaValue WriteLine(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string s = args.First().Value!.ToString()!;

        Console.WriteLine(s);
        return SepiaValue.Void;
    }

    [SepiaFunctionSignature(Name = "Exit")]
    private static SepiaValue Exit(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        System.Environment.Exit(0);
        return SepiaValue.Void;
    }

    [SepiaFunctionSignature(Name = "Clear")]
    private static SepiaValue Clear(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        Console.Clear();
        return SepiaValue.Void;
    }

    [SepiaFunctionSignature(ReturnType = "int", Name = "Time")]
    private static SepiaValue Time(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        return new(DateTime.Now.Ticks, SepiaTypeInfo.TypeInteger());
    }

    [SepiaFunctionSignature("int", Name = "Sleep")]
    private static SepiaValue Sleep(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        long ms = Convert.ToInt64(args.First().Value!);

        Thread.Sleep(Convert.ToInt32(Math.Min(ms, int.MaxValue)));

        return SepiaValue.Void;
    }

    [SepiaFunctionSignature("int", "int", ReturnType = "int", Name = "Random", For = "int")]
    private static SepiaValue RandomInt(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        long min = Convert.ToInt64(args.First().Value!);
        long max = Convert.ToInt64(args.ElementAt(1).Value!);

        return new(Random.Shared.NextInt64(min, max), SepiaTypeInfo.TypeInteger());
    }

    [SepiaFunctionSignature(ReturnType = "float", Name = "Random", For = "float")]
    private static SepiaValue RandomFloat(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        return new(Random.Shared.NextDouble(), SepiaTypeInfo.TypeFloat());
    }

    [SepiaFunctionSignature(ReturnType = "string", Name = "ReadChar")]
    private static SepiaValue ReadChar(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        int c = Console.Read();

        if (c < 0)
            return new(string.Empty, SepiaTypeInfo.TypeString());
        else
            return new(((char)c).ToString(), SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature(ReturnType = "string", Name = "ReadLine")]
    private static SepiaValue ReadLine(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string line = Console.ReadLine()?? string.Empty;

        return new(line, SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature("this", "int", "int", ReturnType = "string", Name = "substring", For = "string")]
    private static SepiaValue Substring(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;
        int start = Convert.ToInt32(Math.Max(0, Math.Min(Convert.ToInt64(args.ElementAt(1).Value!), input.Length - 1)));
        int length = Convert.ToInt32(Math.Min(input.Length, Convert.ToInt64(args.ElementAt(2).Value!)));

        return new(input.Substring(start, length), SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature("this", "int", ReturnType = "string", Name = "charAt", For = "string")]
    private static SepiaValue CharAt(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;
        int index = Convert.ToInt32(Math.Max(0, Math.Min(Convert.ToInt64(args.ElementAt(1).Value!), input.Length - 1)));

        return new(input[index].ToString(), SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature("this", "string", ReturnType = "string", Name = "concat", For = "string")]
    private static SepiaValue Concat(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string a = args.First().Value!.ToString()!;
        string b = args.ElementAt(1).Value!.ToString()!;

        return new($"{a}{b}", SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature("this", ReturnType = "string", Name = "toUpper", For = "string")]
    private static SepiaValue Upper(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.ToUpper(), SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature("this", ReturnType = "string", Name = "toLower", For = "string")]
    private static SepiaValue Lower(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.ToLower(), SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature("this", ReturnType = "string", Name = "trim", For = "string")]
    private static SepiaValue Trim(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.Trim(), SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature("this", ReturnType = "string", Name = "trimStart", For = "string")]
    private static SepiaValue TrimStart(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.TrimStart(), SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature("this", ReturnType = "string", Name = "trimEnd", For = "string")]
    private static SepiaValue TrimEnd(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.TrimEnd(), SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature("this", "string", "string", ReturnType = "string", Name = "replace", For = "string")]
    private static SepiaValue Replace(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;
        string pattern = args.ElementAt(1).Value!.ToString()!;
        string replacement = args.ElementAt(2).Value!.ToString()!;

        return new(input.Replace(pattern, replacement), SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature("this", "string", "string", ReturnType = "string", Name = "regexReplace", For = "string")]
    private static SepiaValue RegexReplace(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;
        string pattern = args.ElementAt(1).Value!.ToString()!;
        string replacement = args.ElementAt(2).Value!.ToString()!;

        Regex regex = new(pattern);

        return new(regex.Replace(input, replacement), SepiaTypeInfo.TypeString());
    }

    [SepiaFunctionSignature("this", ReturnType = "int", Name = "length", For = "string")]
    private static SepiaValue Length(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;

        return new(input.Length, SepiaTypeInfo.TypeInteger());
    }

    [SepiaFunctionSignature("this", "string", ReturnType = "int", Name = "indexOf", For = "string")]
    private static SepiaValue IndexOf(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!;
        string pattern = args.ElementAt(1).Value!.ToString()!;

        return new(input.IndexOf(pattern), SepiaTypeInfo.TypeInteger());
    }

    [SepiaFunctionSignature("this", "string", ReturnType = "int", Name = "regexIndexOf", For = "string")]
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

        return new(index, SepiaTypeInfo.TypeInteger());
    }

    [SepiaFunctionSignature(ReturnType = "int", Name = "Parse", For = "int")]
    private static SepiaValue ParseInt(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!.Trim();

        return new(long.Parse(input), SepiaTypeInfo.TypeInteger());
    }

    [SepiaFunctionSignature("this", ReturnType = "bool", Name = "isInt", For = "string")]
    private static SepiaValue IsInt(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!.Trim();

        return new(long.TryParse(input, out _), SepiaTypeInfo.TypeBoolean());
    }

    [SepiaFunctionSignature(ReturnType = "float", Name = "Parse", For = "float")]
    private static SepiaValue ParseFloat(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!.Trim();

        return new(double.Parse(input), SepiaTypeInfo.TypeFloat());
    }

    [SepiaFunctionSignature("this", ReturnType = "bool", Name = "isFloat", For = "string")]
    private static SepiaValue IsFloat(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        string input = args.First().Value!.ToString()!.Trim();

        return new(double.TryParse(input, out _), SepiaTypeInfo.TypeBoolean());
    }

    [SepiaFunctionSignature("this", ReturnType = "float", Name = "toFloat", For = "int")]
    private static SepiaValue IntToFloat(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        long input = Convert.ToInt64(args.First().Value!);
        return new(Convert.ToDouble(input), SepiaTypeInfo.TypeFloat());
    }

    [SepiaFunctionSignature("this", ReturnType = "int", Name = "toInt", For = "float")]
    private static SepiaValue FloatToInt(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        double input = Convert.ToDouble(args.First().Value!);
        return new(Convert.ToInt64(input), SepiaTypeInfo.TypeInteger());
    }

    [SepiaFunctionSignature("this", ReturnType = "float", Name = "round", For = "float")]
    private static SepiaValue Round(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        double input = Convert.ToDouble(args.First().Value!);
        return new(Math.Round(input), SepiaTypeInfo.TypeFloat());
    }

    [SepiaFunctionSignature("this", ReturnType = "float", Name = "ceil", For = "float")]
    private static SepiaValue Ceiling(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        double input = Convert.ToDouble(args.First().Value!);
        return new(Math.Ceiling(input), SepiaTypeInfo.TypeFloat());
    }

    [SepiaFunctionSignature("this", ReturnType = "float", Name = "floor", For = "float")]
    private static SepiaValue Floor(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        double input = Convert.ToDouble(args.First().Value!);
        return new(Math.Floor(input), SepiaTypeInfo.TypeFloat());
    }

    [SepiaFunctionSignature("this", ReturnType = "float", Name = "truncate", For = "float")]
    private static SepiaValue Truncate(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        double input = Convert.ToDouble(args.First().Value!);
        return new(Math.Truncate(input), SepiaTypeInfo.TypeFloat());
    }

    [SepiaFunctionSignature("int", "func()", Name = "Delay")]
    private static SepiaValue Delay(Evaluator interpreter, IEnumerable<SepiaValue> args)
    {
        long delay = Convert.ToInt64(args.First().Value!);
        ISepiaCallable callable = (ISepiaCallable)args.ElementAt(1).Value!;

        Thread.Sleep(Convert.ToInt32(Math.Min(delay, int.MaxValue)));

        return callable.Call(interpreter, Enumerable.Empty<SepiaValue>());
    }

    [SepiaFunctionSignature("func()", ReturnType = "int", Name = "Benchmark")]
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

        return new(stopwatch.ElapsedMilliseconds, SepiaTypeInfo.TypeInteger());
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class SepiaFunctionSignatureAttribute : Attribute
{
    public string[] Arguments { get; set; }

    public string ReturnType { get; set; }

    public string Name { get; set; }

    public string For { get; set; }

    public SepiaFunctionSignatureAttribute(params string[] arguments) 
    {
        Arguments = arguments;
        ReturnType = SepiaTypeInfo.NativeType.Void;
        Name = string.Empty;
        For = string.Empty;
    }
}