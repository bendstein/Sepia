namespace Sepia.Lex;

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
    /// Token representing '&&'
    /// </summary>
    AMP_AMP,
    /// <summary>
    /// Token representing '||'
    /// </summary>
    PIPE_PIPE,
    /// <summary>
    /// Token representing '&'
    /// </summary>
    AMP,
    /// <summary>
    /// Token representing '|'
    /// </summary>
    PIPE,
    /// <summary>
    /// Token representing '~'
    /// </summary>
    TILDE,
    /// <summary>
    /// Token representing '<<'
    /// </summary>
    LESS_LESS,
    /// <summary>
    /// Token representing '>>'
    /// </summary>
    GREATER_GREATER,
    /// <summary>
    /// Token representing '+='
    /// </summary>
    PLUS_EQUAL,
    /// <summary>
    /// Token representing '-='
    /// </summary>
    MINUS_EQUAL,
    /// <summary>
    /// Token representing '*='
    /// </summary>
    STAR_EQUAL,
    /// <summary>
    /// Token representing '/='
    /// </summary>
    SLASH_EQUAL,
    /// <summary>
    /// Token representing '%='
    /// </summary>
    PERCENT_EQUAL,
    /// <summary>
    /// Token representing '^='
    /// </summary>
    CARET_EQUAL,
    /// <summary>
    /// Token representing '&&='
    /// </summary>
    AMP_AMP_EQUAL,
    /// <summary>
    /// Token representing '||='
    /// </summary>
    PIPE_PIPE_EQUAL,
    /// <summary>
    /// Token representing '&='
    /// </summary>
    AMP_EQUAL,
    /// <summary>
    /// Token representing '&='
    /// </summary>
    PIPE_EQUAL,
    /// <summary>
    /// Token representing '~='
    /// </summary>
    TILDE_EQUAL,
    /// <summary>
    /// Token representing '<<='
    /// </summary>
    LESS_LESS_EQUAL,
    /// <summary>
    /// Token representing '>>='
    /// </summary>
    GREATER_GREATER_EQUAL,
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
    /// Token representing the void type keyword
    /// </summary>
    TYPE_VOID,
    /// <summary>
    /// Token representing the string type keyword
    /// </summary>
    TYPE_STRING,
    /// <summary>
    /// Token representing the int type keyword
    /// </summary>
    TYPE_INT,
    /// <summary>
    /// Token representing the long type keyword
    /// </summary>
    TYPE_LONG,
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
    /// Token representing keyword let
    /// </summary>
    LET,
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
    /// Token representing keyword in
    /// </summary>
    IN,
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
    /// Token representing keyword print
    /// </summary>
    PRINT,
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
        AMP_AMP = "&&",
        PIPE_PIPE = "||",
        AMP = "&",
        PIPE = "|",
        TILDE = "~",
        LESS_LESS = "<<",
        GREATER_GREATER = ">>",
        PLUS_EQUAL = "+=",
        MINUS_EQUAL = "-=",
        STAR_EQUAL = "*=",
        SLASH_EQUAL = "/=",
        PERCENT_EQUAL = "%=",
        CARET_EQUAL = "^=",
        AMP_AMP_EQUAL = "&&=",
        PIPE_PIPE_EQUAL = "||=",
        AMP_EQUAL = "&=",
        PIPE_EQUAL = "|=",
        TILDE_EQUAL = "~=",
        LESS_LESS_EQUAL = "<<=",
        GREATER_GREATER_EQUAL = ">>=",
        ZERO = "0",
        B = "b",
        X = "x",
        ZERO_B = "0b",
        ZERO_X = "0x",
        VAR = "var",
        VOID = "void",
        STRING = "string",
        INT = "int",
        LONG = "long",
        FLOAT = "float",
        BOOL = "bool",
        FALSE = "false",
        TRUE = "true",
        NULL = "null",
        LET = "let",
        AND = "and",
        OR = "or",
        IF = "if",
        ELSE = "else",
        WHILE = "while",
        DO = "do",
        FOR = "for",
        IN = "in",
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
        PRINT = "print",
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
        { TokenType.AMP_AMP, AMP_AMP },
        { TokenType.PIPE_PIPE, PIPE_PIPE },
        { TokenType.AMP, AMP },
        { TokenType.PIPE, PIPE },
        { TokenType.TILDE, TILDE },
        { TokenType.LESS_LESS, LESS_LESS },
        { TokenType.GREATER_GREATER, GREATER_GREATER },
        { TokenType.PLUS_EQUAL, PLUS_EQUAL },
        { TokenType.MINUS_EQUAL, MINUS_EQUAL },
        { TokenType.STAR_EQUAL, STAR_EQUAL },
        { TokenType.SLASH_EQUAL, SLASH_EQUAL },
        { TokenType.PERCENT_EQUAL, PERCENT_EQUAL },
        { TokenType.CARET_EQUAL, CARET_EQUAL },
        { TokenType.AMP_AMP_EQUAL, AMP_AMP_EQUAL },
        { TokenType.PIPE_PIPE_EQUAL, PIPE_PIPE_EQUAL },
        { TokenType.AMP_EQUAL, AMP_EQUAL },
        { TokenType.PIPE_EQUAL, PIPE_EQUAL },
        { TokenType.TILDE_EQUAL, TILDE_EQUAL },
        { TokenType.LESS_LESS_EQUAL, LESS_LESS_EQUAL },
        { TokenType.GREATER_GREATER_EQUAL, GREATER_GREATER_EQUAL },
        { TokenType.TYPE_VAR, VAR },
        { TokenType.TYPE_VOID, VOID},
        { TokenType.TYPE_STRING, STRING },
        { TokenType.TYPE_INT, INT },
        { TokenType.TYPE_LONG, LONG },
        { TokenType.TYPE_FLOAT, FLOAT },
        { TokenType.TYPE_BOOL, BOOL },
        { TokenType.FALSE, FALSE },
        { TokenType.TRUE, TRUE },
        { TokenType.NULL, NULL },
        { TokenType.LET, LET },
        { TokenType.AND, AND },
        { TokenType.OR, OR },
        { TokenType.IF, IF },
        { TokenType.ELSE, ELSE },
        { TokenType.WHILE, WHILE },
        { TokenType.DO, DO },
        { TokenType.FOR, FOR },
        { TokenType.IN, IN },
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
        { TokenType.PRINT, PRINT},
        { TokenType.EOF, EOF }
    };

    public static readonly Dictionary<TokenType, TokenType> COMPOUND_ASSIGNMENT = new()
    {
        { TokenType.PLUS_EQUAL, TokenType.PLUS },
        { TokenType.MINUS_EQUAL, TokenType.MINUS },
        { TokenType.STAR_EQUAL, TokenType.STAR },
        { TokenType.SLASH_EQUAL, TokenType.SLASH},
        { TokenType.PERCENT_EQUAL, TokenType.PERCENT },
        { TokenType.CARET_EQUAL, TokenType.CARET },
        { TokenType.AMP_AMP_EQUAL, TokenType.AMP_AMP },
        { TokenType.PIPE_PIPE_EQUAL, TokenType.PIPE_PIPE },
        { TokenType.AMP_EQUAL, TokenType.AMP },
        { TokenType.PIPE_EQUAL, TokenType.PIPE },
        { TokenType.TILDE_EQUAL, TokenType.TILDE },
        { TokenType.LESS_LESS_EQUAL, TokenType.LESS_LESS },
        { TokenType.GREATER_GREATER_EQUAL, TokenType.GREATER_GREATER },
    };

    public static readonly HashSet<TokenType> KEYWORD_TOKENS = new()
    {
        TokenType.TYPE_VAR,
        TokenType.TYPE_VOID,
        TokenType.TYPE_STRING,
        TokenType.TYPE_INT,
        TokenType.TYPE_FLOAT,
        TokenType.TYPE_BOOL,
        TokenType.FALSE,
        TokenType.TRUE,
        TokenType.NULL,
        TokenType.LET,
        TokenType.AND,
        TokenType.OR,
        TokenType.IF,
        TokenType.ELSE,
        TokenType.WHILE,
        TokenType.DO,
        TokenType.FOR,
        TokenType.IN,
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
        TokenType.INTERFACE,
        TokenType.PRINT
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
        TokenType.LET,
        TokenType.AND,
        TokenType.OR,
        TokenType.IF,
        TokenType.ELSE,
        TokenType.WHILE,
        TokenType.DO,
        TokenType.FOR,
        TokenType.IN,
        TokenType.BREAK,
        TokenType.CONTINUE,
        TokenType.SWITCH,
        TokenType.CASE,
        TokenType.FUNC,
        TokenType.RETURN,
        TokenType.CLASS,
        TokenType.ABSTRACT,
        TokenType.INTERFACE,
        TokenType.PRINT
    };
}