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
    /// Token representing an identifier literal (variable name, function name, etc)
    /// </summary>
    ID,
    /// <summary>
    /// Token representing a string literal
    /// </summary>
    STRING,
    /// <summary>
    /// Token representing an interpolated expression literal
    /// </summary>
    INTERPOLATED,
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
    /// Token representing the var type keyword
    /// </summary>
    TYPE_VAR,
    /// <summary>
    /// Token representing the string type keyword
    /// </summary>
    TYPE_STRING,
    /// <summary>
    /// Token representing the int type keyword
    /// </summary>
    TYPE_INT,
    /// <summary>
    /// Token representing the float type keyword
    /// </summary>
    TYPE_FLOAT,
    /// <summary>
    /// Token representing the bool type keyword
    /// </summary>
    TYPE_BOOL,
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
    COMMENT,
    /// <summary>
    /// Represents a tokenization error
    /// </summary>
    ERROR
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
        ZERO = "0",
        B = "b",
        X = "x",
        ZERO_B = "0b",
        ZERO_X = "0x",
        VAR = "var",
        STRING = "string",
        INT = "int",
        FLOAT = "float",
        BOOL = "bool",
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
        FUNC = "function",
        RETURN = "return",
        CLASS = "class",
        THIS = "this",
        BASE = "base",
        ABSTRACT = "abstract",
        INTERFACE = "interface",
        EOF = "";

    public static readonly Dictionary<TokenType, string> TOKEN_SYMBOLS = new()
    {
        { TokenType.PLUS, PLUS },
        { TokenType.MINUS, MINUS },
        { TokenType.STAR, STAR },
        { TokenType.SLASH, SLASH },
        { TokenType.PERCENT, PERCENT },
        { TokenType.CARET, CARET },
        { TokenType.DOT, DOT },
        { TokenType.COMMA, COMMA },
        { TokenType.SEMICOLON, SEMICOLON },
        { TokenType.COLON, COLON },
        { TokenType.L_PAREN, L_PAREN },
        { TokenType.R_PAREN, R_PAREN },
        { TokenType.L_BRACE, L_BRACE },
        { TokenType.R_BRACE, R_BRACE },
        { TokenType.UNDERSCORE, UNDERSCORE },
        { TokenType.S_QUOTE, S_QUOTE },
        { TokenType.D_QUOTE, D_QUOTE },
        { TokenType.BACKTICK, BACKTICK },
        { TokenType.BANG, BANG },
        { TokenType.BANG_EQUAL, BANG_EQUAL },
        { TokenType.EQUAL, EQUAL },
        { TokenType.EQUAL_EQUAL, EQUAL_EQUAL },
        { TokenType.GREATER, GREATER },
        { TokenType.GREATER_EQUAL, GREATER_EQUAL },
        { TokenType.LESS, LESS },
        { TokenType.LESS_EQUAL, LESS_EQUAL },
        { TokenType.TYPE_VAR, VAR },
        { TokenType.TYPE_STRING, STRING },
        { TokenType.TYPE_INT, INT },
        { TokenType.TYPE_FLOAT, FLOAT },
        { TokenType.TYPE_BOOL, BOOL },
        { TokenType.FALSE, FALSE },
        { TokenType.TRUE, TRUE },
        { TokenType.NULL, NULL },
        { TokenType.AND, AND },
        { TokenType.OR, OR },
        { TokenType.IF, IF },
        { TokenType.ELSE, ELSE },
        { TokenType.WHILE, WHILE },
        { TokenType.DO, DO },
        { TokenType.FOR, FOR },
        { TokenType.BREAK, BREAK },
        { TokenType.CONTINUE, CONTINUE },
        { TokenType.SWITCH, SWITCH },
        { TokenType.CASE, CASE },
        { TokenType.FUNC, FUNC },
        { TokenType.RETURN, RETURN },
        { TokenType.CLASS, CLASS },
        { TokenType.THIS, THIS },
        { TokenType.BASE, BASE },
        { TokenType.ABSTRACT, ABSTRACT },
        { TokenType.INTERFACE, INTERFACE },
        { TokenType.EOF, EOF }
    };

    public static readonly HashSet<TokenType> KEYWORD_TOKENS = new()
    {
        TokenType.TYPE_VAR,
        TokenType.TYPE_STRING,
        TokenType.TYPE_INT,
        TokenType.TYPE_FLOAT,
        TokenType.TYPE_BOOL,
        TokenType.FALSE,
        TokenType.TRUE,
        TokenType.NULL,
        TokenType.AND,
        TokenType.OR,
        TokenType.IF,
        TokenType.ELSE,
        TokenType.WHILE,
        TokenType.DO,
        TokenType.FOR,
        TokenType.BREAK,
        TokenType.CONTINUE,
        TokenType.SWITCH,
        TokenType.CASE,
        TokenType.FUNC,
        TokenType.RETURN,
        TokenType.CLASS,
        TokenType.THIS,
        TokenType.BASE,
        TokenType.ABSTRACT,
        TokenType.INTERFACE
    };

    public static readonly HashSet<TokenType> TOKEN_SYNC_PREV = new()
    {
        TokenType.SEMICOLON,
        TokenType.R_BRACE,
        TokenType.R_PAREN
    };

    public static readonly HashSet<TokenType> TOKEN_SYNC_NEXT = new()
    {
        TokenType.TYPE_VAR,
        TokenType.TYPE_STRING,
        TokenType.TYPE_INT,
        TokenType.TYPE_FLOAT,
        TokenType.TYPE_BOOL,
        TokenType.AND,
        TokenType.OR,
        TokenType.IF,
        TokenType.ELSE,
        TokenType.WHILE,
        TokenType.DO,
        TokenType.FOR,
        TokenType.BREAK,
        TokenType.CONTINUE,
        TokenType.SWITCH,
        TokenType.CASE,
        TokenType.FUNC,
        TokenType.RETURN,
        TokenType.CLASS,
        TokenType.ABSTRACT,
        TokenType.INTERFACE
    };
}