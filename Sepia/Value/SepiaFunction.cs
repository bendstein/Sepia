﻿using Sepia.AST.Node.Expression;
using Sepia.AST.Node.Statement;
using Sepia.Callable;
using Sepia.Common;
using Sepia.Evaluate;
using Sepia.Lex.Literal;
using Sepia.Value.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sepia.Value;
public class SepiaFunction : ISepiaCallable
{
    public IEnumerable<(IdLiteral id, SepiaTypeInfo type)> Arguments { get; init; }

    public SepiaTypeInfo ReturnType { get; init; }

    public Block Body { get; init; }

    public IEnumerable<SepiaTypeInfo> argumentTypes => Arguments.Select(a => a.type);

    public SepiaFunction(IEnumerable<(IdLiteral id, SepiaTypeInfo type)> arguments, 
        SepiaTypeInfo returnType, Block body)
    {
        Arguments = arguments;
        ReturnType = returnType;
        Body = body;
    }

    public SepiaValue Call(Evaluator evaluator, IEnumerable<SepiaValue> arguments)
    {
        if (arguments.Count() != this.Arguments.Count())
            throw new SepiaException(new EvaluateError());

        List<Exception> exceptions = new();

        for (int i = 0; i < arguments.Count(); i++)
        {
            var argument = arguments.ElementAt(i);
            (var id, var expectedType) = this.Arguments.ElementAt(i);

            if (argument.Type != expectedType)
            {
                exceptions.Add(new SepiaException(new EvaluateError()));
            }

            evaluator.Visit(new DeclarationStmtNode(id, expectedType, new ValueExpressionNode(argument)));
        }

        if (exceptions.Any())
        {
            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }
            else
            {
                throw new AggregateException(exceptions);
            }
        }

        return evaluator.Visit(Body);
    }
}