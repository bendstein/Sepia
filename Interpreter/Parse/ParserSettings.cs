using Interpreter.Lex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Parse;
public class ParserSettings
{
    /// <summary>
    /// Settings to use while lexing an interpolated string during parsing
    /// </summary>
    public LexerSettings InterpolatedLexerSettings { get; set; } = new();
}