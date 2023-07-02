using Sepia.Lex;

namespace Sepia.Value.Type;

public class SepiaCallSignature
{
    public SepiaTypeInfo ReturnType { get; set; }

    public List<SepiaTypeInfo> Arguments { get; set; }

    public SepiaCallSignature(List<SepiaTypeInfo> arguments, SepiaTypeInfo? returnType = null)
    {
        Arguments = arguments;
        ReturnType = returnType?? SepiaTypeInfo.TypeVoid();
    }

    public SepiaCallSignature(SepiaTypeInfo? returnType = null) : this(new(), returnType) { }

    public SepiaCallSignature Clone() => new(Arguments.Select(a => a.Clone()).ToList(), ReturnType.Clone());

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;
        if(obj != null && obj is SepiaCallSignature other)
            return Equals(this, other);

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ReturnType, Arguments);
    }

    public static bool Equals(SepiaCallSignature? a, SepiaCallSignature? b)
    {
        if (a == null)
            return b == null;
        if (b == null) 
            return false;
        if(!SepiaTypeInfo.Equals(a.ReturnType, b.ReturnType))
            return false;
        if(a.Arguments.Count != b.Arguments.Count) 
            return false;

        for(int i = 0; i < a.Arguments.Count; i++)
        {
            var arg_a = a.Arguments[i];
            var arg_b = b.Arguments[i];

            if (!SepiaTypeInfo.Equals(arg_a, arg_b))
                return false;
        }

        return true;
    }

    public override string ToString()
    {
        var args_string = Arguments.Count == 0 ? string.Empty : Arguments.Select(a => a.ToString())
            .Aggregate((a, b) => $"{a}{TokenType.COMMA.GetSymbol()} {b}");
        var return_string = ReturnType == SepiaTypeInfo.TypeVoid(false) ? string.Empty : ReturnType.ToString();

        if(string.IsNullOrWhiteSpace(args_string))
        {
            if (string.IsNullOrWhiteSpace(return_string))
            {
                return $"{TokenType.L_PAREN.GetSymbol()}{TokenType.R_PAREN.GetSymbol()}";
            }
            else
            {
                return $"{TokenType.L_PAREN.GetSymbol()}{TokenType.SEMICOLON.GetSymbol()} {return_string}{TokenType.R_PAREN.GetSymbol()}";
            }
        }
        else if(string.IsNullOrWhiteSpace(return_string))
        {
            return $"{TokenType.L_PAREN.GetSymbol()}{args_string}{TokenType.R_PAREN.GetSymbol()}";
        }
        else
        {
            return $"{TokenType.L_PAREN.GetSymbol()}{args_string}{TokenType.SEMICOLON.GetSymbol()} {return_string}{TokenType.R_PAREN.GetSymbol()}";
        }
    }
}