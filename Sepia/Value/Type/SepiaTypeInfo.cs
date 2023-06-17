namespace Sepia.Value.Type;

public class SepiaTypeInfo
{
    private static readonly SepiaTypeInfo
        VOID = new(NativeType.Void),
        NULL = new(NativeType.Null),
        INTEGER = new(NativeType.Integer),
        FLOAT = new(NativeType.Float),
        STRING = new(NativeType.String),
        BOOLEAN = new(NativeType.Boolean),
        FUNCTION = new(NativeType.Function);

    public string TypeName { get; set; }

    public SepiaCallSignature? CallSignature { get; set; } = null;

    public SepiaTypeInfo(string typeName, SepiaCallSignature? callSignature = null)
    {
        TypeName = typeName;
        CallSignature = callSignature;
    }

    public SepiaTypeInfo WithTypeName(string typeName)
    {
        TypeName = typeName;
        return this;
    }

    public SepiaTypeInfo WithCallSignature(SepiaCallSignature? callSignature)
    {
        CallSignature = callSignature;
        return this;
    }

    public static bool operator ==(SepiaTypeInfo? a, SepiaTypeInfo? b) => Equals(a, b);

    public static bool operator !=(SepiaTypeInfo? a, SepiaTypeInfo? b) => !Equals(a, b);

    public override string ToString()
    {
        return $"{TypeName}{(CallSignature == null ? string.Empty : CallSignature.ToString())}";
    }

    public override bool Equals(object? obj)
    {
        if (this is null) return obj is null;
        else if (obj is null) return false;
        else if (obj is SepiaTypeInfo other)
        {
            return Equals(this, other);
        }
        else
        {
            return base.Equals(obj);
        }
    }

    public static bool Equals(SepiaTypeInfo? a, SepiaTypeInfo? b)
    {
        if(a is null)
            return b is null;
        if(b is null)
            return false;
        if (a.TypeName != b.TypeName)
            return false;
        if (a.CallSignature == null ^ b.CallSignature == null)
            return false;
        if (a.CallSignature == null)
            return true;

        return a.CallSignature.Equals(b.CallSignature);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TypeName, CallSignature);
    }

    public SepiaTypeInfo Clone() => new(TypeName, CallSignature?.Clone());

    public static SepiaTypeInfo Void(bool clone = true) => clone? VOID.Clone() : VOID;
    public static SepiaTypeInfo Null(bool clone = true) => clone ? NULL.Clone() : NULL;
    public static SepiaTypeInfo Integer(bool clone = true) => clone ? INTEGER.Clone() : INTEGER;
    public static SepiaTypeInfo Float(bool clone = true) => clone ? FLOAT.Clone() : FLOAT;
    public static SepiaTypeInfo String(bool clone = true) => clone ? STRING.Clone() : STRING;
    public static SepiaTypeInfo Boolean(bool clone = true) => clone ? BOOLEAN.Clone() : BOOLEAN;
    public static SepiaTypeInfo Function(bool clone = true) => clone ? FUNCTION.Clone() : FUNCTION;

    public static class NativeType
    {
        public const string
            Void = "void",
            Null = "null",
            Integer = "int",
            Float = "float",
            String = "string",
            Boolean = "bool",
            Function = "func";
    }

}