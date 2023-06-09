﻿namespace Sepia.Value.Type;

public class SepiaTypeInfo
{
    public static readonly SepiaTypeInfo
        Void = new(NativeType.Void),
        Null = new(NativeType.Null),
        Integer = new(NativeType.Integer),
        Float = new(NativeType.Float),
        String = new(NativeType.String),
        Boolean = new(NativeType.Boolean);

    public string TypeName { get; set; }

    public SepiaTypeInfo(string typeName)
    {
        TypeName = typeName;
    }

    public static class NativeType
    {
        public const string
            Void = "void",
            Null = "null",
            Integer = "int",
            Float = "float",
            String = "string",
            Boolean = "bool";
    }

    public static bool operator ==(SepiaTypeInfo a, SepiaTypeInfo b) => a.Equals(b);

    public static bool operator !=(SepiaTypeInfo a, SepiaTypeInfo b) => !a.Equals(b);

    public override string ToString()
    {
        return TypeName.ToString();
    }

    public override bool Equals(object? obj)
    {
        if (this is null) return obj is null;
        else if (obj is null) return false;
        else if(obj is SepiaTypeInfo other)
        {
            return this.TypeName.Equals(other.TypeName);
        }
        else
        {
            return base.Equals(obj);
        }
    }

    public override int GetHashCode()
    {
        return this.TypeName.GetHashCode();
    }
}