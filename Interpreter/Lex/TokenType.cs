namespace Interpreter.Lex;

public enum TokenType
{
    /// <summary>
    /// Token representing '+'
    /// </summary>
    PLUS,
    /// <summary>
    /// Token representing '-'
    /// </summary>
    MINUS,
    /// <summary>
    /// Token representing '*'
    /// </summary>
    STAR,
    /// <summary>
    /// Token representing '/'
    /// </summary>
    SLASH,
    /// <summary>
    /// Token representing '%'
    /// </summary>
    PERCENT,
    /// <summary>
    /// Token representing '^'
    /// </summary>
    CARET,
    /// <summary>
    /// Token representing '.'
    /// </summary>
    DOT,
    /// <summary>
    /// Token representing ','
    /// </summary>
    COMMA,
    /// <summary>
    /// Token representing ';'
    /// </summary>
    SEMICOLON,
    /// <summary>
    /// Token representing ':'
    /// </summary>
    COLON,
    /// <summary>
    /// Token representing '('
    /// </summary>
    L_PAREN,
    /// <summary>
    /// Token representing ')'
    /// </summary>
    R_PAREN,
    /// <summary>
    /// Token representing '{'
    /// </summary>
    L_BRACE,
    /// <summary>
    /// Token representing '}'
    /// </summary>
    R_BRACE,
    /// <summary>
    /// Token representing '_'
    /// </summary>
    UNDERSCORE,
    /// <summary>
    /// Token representing '
    /// </summary>
    S_QUOTE,
    /// <summary>
    /// Token representing "
    /// </summary>
    D_QUOTE,
    /// <summary>
    /// Token representing `
    /// </summary>
    BACKTICK,
    /// <summary>
    /// Token representing '!'
    /// </summary>
    BANG,
    /// <summary>
    /// Token representing '!='
    /// </summary>
    BANG_EQUAL,
    /// <summary>
    /// Token representing '='
    /// </summary>
    EQUAL,
    /// <summary>
    /// Token representing '=='
    /// </summary>
    EQUAL_EQUAL,
    /// <summary>
    /// Token representing '>'
    /// </summary>
    GREATER,
    /// <summary>
    /// Token representing '>='
    /// </summary>
    GREATER_EQUAL,
    /// <summary>
    /// Token representing '<'
    /// </summary>
    LESS,
    /// <summary>
    /// Token representing '<='
    /// </summary>
    LESS_EQUAL,
    /// <summary>
    /// Token representing '0b'
    /// </summary>
    ZERO_B,
    /// <summary>
    /// Token representing '0x'
    /// </summary>
    ZERO_X,
    /// <summary>
    /// Token representing an identifier literal (variable name, function name, etc)
    /// </summary>
    ID,
    /// <summary>
    /// Token representing a string literal
    /// </summary>
    STRING,
    /// <summary>
    /// Token representing a number literal
    /// </summary>
    NUMBER,
    /// <summary>
    /// Token representing a boolean literal
    /// </summary>
    BOOL,
    /// <summary>
    /// Token representing whitespace
    /// </summary>
    WHITESPACE,
    /// <summary>
    /// Token representing the end of the file
    /// </summary>
    EOF,
    /// <summary>
    /// Token representing keyword boolean False
    /// </summary>
    FALSE,
    /// <summary>
    /// Token representing keyword boolean True
    /// </summary>
    TRUE,
    /// <summary>
    /// Token representing keyword null
    /// </summary>
    NULL,
    /// <summary>
    /// Token representing keyword boolean AND
    /// </summary>
    AND,
    /// <summary>
    /// Token representing keyword boolean OR
    /// </summary>
    OR,
    /// <summary>
    /// Token representing keyword if
    /// </summary>
    IF,
    /// <summary>
    /// Token representing keyword else
    /// </summary>
    ELSE,
    /// <summary>
    /// Token representing keyword while
    /// </summary>
    WHILE,
    /// <summary>
    /// Token representing keyword do
    /// </summary>
    DO,
    /// <summary>
    /// Token representing keyword for
    /// </summary>
    FOR,
    /// <summary>
    /// Token representing keyword break
    /// </summary>
    BREAK,
    /// <summary>
    /// Token representing keyword continue
    /// </summary>
    CONTINUE,
    /// <summary>
    /// Token representing keyword switch
    /// </summary>
    SWITCH,
    /// <summary>
    /// Token representing keyword case
    /// </summary>
    CASE,
    /// <summary>
    /// Token representing keyword func
    /// </summary>
    FUNC,
    /// <summary>
    /// Token representing keyword return
    /// </summary>
    RETURN,
    /// <summary>
    /// Token representing keyword class
    /// </summary>
    CLASS,
    /// <summary>
    /// Token representing keyword this
    /// </summary>
    THIS,
    /// <summary>
    /// Token representing keyword base
    /// </summary>
    BASE,
    /// <summary>
    /// Token representing keyword abstract
    /// </summary>
    ABSTRACT,
    /// <summary>
    /// Token representing keyword interface
    /// </summary>
    INTERFACE,
    /// <summary>
    /// Token representing a comment
    /// </summary>
    COMMENT
}

public static class TokenTypeValues
{
    public const string
        PLUS = "+",
        MINUS = "-",
        STAR = "*",
        SLASH = "/",
        PERCENT = "%",
        CARET = "^",
        DOT = ".",
        COMMA = ",",
        SEMICOLON = ";",
        COLON = ":",
        L_PAREN = "(",
        R_PAREN = ")",
        L_BRACE = "{",
        R_BRACE = "}",
        UNDERSCORE = "_",
        S_QUOTE = "'",
        D_QUOTE = "\"",
        BACKTICK = "`",
        BANG = "!",
        BANG_EQUAL = "!=",
        EQUAL = "=",
        EQUAL_EQUAL = "==",
        GREATER = ">",
        GREATER_EQUAL = ">=",
        LESS = "<",
        LESS_EQUAL = "<=",
        ZERO_B = "0b",
        ZERO_X = "0x",
        FALSE = "false",
        TRUE = "true",
        NULL = "null",
        AND = "and",
        OR = "or",
        IF = "if",
        ELSE = "else",
        WHILE = "while",
        DO = "do",
        FOR = "for",
        BREAK = "break",
        CONTINUE = "continue",
        SWITCH = "switch",
        CASE = "case",
        FUNC = "func",
        RETURN = "return",
        CLASS = "class",
        THIS = "this",
        BASE = "base",
        ABSTRACT = "abstract",
        INTERFACE = "interface",
        EOF = "";
}