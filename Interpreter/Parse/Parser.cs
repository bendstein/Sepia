using Interpreter.Lex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Parse;
public class Parser
{
    private readonly IEnumerable<Token> _tokens;

    private readonly ParserSettings _settings;

    public Parser(IEnumerable<Token> tokens, ParserSettings? settings = null)
    {
        _tokens = tokens;
        _settings = settings?? new ParserSettings();
    }
}