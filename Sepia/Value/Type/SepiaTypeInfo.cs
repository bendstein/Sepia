namespace Sepia.Value.Type;

public class SepiaTypeInfo : ISepiaValue
{
    public static readonly SepiaTypeInfo
        VOID = new(NativeType.Void),
        NULL = new(NativeType.Null),
        INTEGER = new(NativeType.Integer),
        FLOAT = new(NativeType.Float),
        STRING = new(NativeType.String),
        BOOLEAN = new(NativeType.Boolean),
        TYPE = new(NativeType.Type),
        THIS = new(NativeType.This),
        FUNCTION = new(NativeType.Function);

    public static readonly IEnumerable<SepiaTypeInfo> NativeTypes = new SepiaTypeInfo[]
    {
        VOID,
        INTEGER,
        FLOAT,
        STRING,
        BOOLEAN,
        FUNCTION,
        TYPE
    };

    public string TypeName { get; set; }

    public SepiaCallSignature? CallSignature { get; set; } = null;

    public Dictionary<string, ISepiaValue> Members { get; set; }

    public SepiaTypeInfo Type => TypeType(false);

    public object? Value => TypeName;

    public SepiaTypeInfo(string typeName, SepiaCallSignature? callSignature = null, Dictionary<string, ISepiaValue>? members = null)
    {
        TypeName = typeName;
        CallSignature = callSignature;
        Members = members?? new();
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

    public SepiaTypeInfo WithMembers(Dictionary<string, ISepiaValue> members)
    {
        Members = members;
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
        if (a.CallSignature != null && b.CallSignature != null && !a.CallSignature.Equals(b.CallSignature))
            return false;
        if (a.Members.Count != b.Members.Count)
            return false;

        foreach(var pair in a.Members)
        {
            if(b.Members.TryGetValue(pair.Key, out var bmember))
            {
                if(!pair.Value.Equals(bmember))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TypeName, CallSignature, Members);
    }

    public SepiaTypeInfo Clone() => new(TypeName, CallSignature?.Clone(), Members.ToDictionary(m => m.Key, m => m.Value.Clone()));

    ISepiaValue ISepiaValue.Clone() => Clone();

    public static void RegisterMembers(Dictionary<string, Dictionary<string, ISepiaValue>> members)
    {
        foreach (var memberPair in members)
        {
            var matching = NativeTypes.Where(t => t.TypeName == memberPair.Key);

            if(matching.Any())
            {
                var type = matching.First();

                foreach(var member in memberPair.Value)
                {
                    type.Members[member.Key] = member.Value;
                }
            }
        }
    }

    public static SepiaTypeInfo TypeVoid(bool clone = true) => clone? VOID.Clone() : VOID;
    public static SepiaTypeInfo TypeType(bool clone = true) => clone? TYPE.Clone() : TYPE;
    public static SepiaTypeInfo TypeNull(bool clone = true) => clone ? NULL.Clone() : NULL;
    public static SepiaTypeInfo TypeInteger(bool clone = true) => clone ? INTEGER.Clone() : INTEGER;
    public static SepiaTypeInfo TypeFloat(bool clone = true) => clone ? FLOAT.Clone() : FLOAT;
    public static SepiaTypeInfo TypeString(bool clone = true) => clone ? STRING.Clone() : STRING;
    public static SepiaTypeInfo TypeBoolean(bool clone = true) => clone ? BOOLEAN.Clone() : BOOLEAN;
    public static SepiaTypeInfo TypeFunction(bool clone = true) => clone ? FUNCTION.Clone() : FUNCTION;
    public static SepiaTypeInfo TypeThis(bool clone = true) => clone ? THIS.Clone() : THIS;

    public static class NativeType
    {
        public const string
            Type = "type",
            Void = "void",
            Null = "null",
            Integer = "int",
            Float = "float",
            String = "string",
            Boolean = "bool",
            Function = "func",
            This = "this";
    }
}