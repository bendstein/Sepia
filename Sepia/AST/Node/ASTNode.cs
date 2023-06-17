using Sepia.Analyzer;
using Sepia.Common;
using Sepia.Lex;

namespace Sepia.AST.Node;

public abstract class ASTNode : IASTNodeVisitable<ASTNode>
{
    public List<Token> AllTokens = new();

    public virtual ResolveInfo ResolveInfo { get; set; } = new();

    public virtual Location Location
    {
        get
        {
            if (AllTokens.Count == 0)
            {
                return new();
            }
            else if(AllTokens.Count == 1)
            {
                return AllTokens.First().Location;
            }
            else
            {
                var important_tokens = AllTokens.Where(t => !(t.TokenType == TokenType.WHITESPACE || t.TokenType == TokenType.COMMENT));

                return important_tokens.First().Location.Bridge(important_tokens.Last().Location);
            }
        }
    }

    public void Accept(IASTNodeVisitor<ASTNode> visitor) => visitor.Visit(this);

    public TReturn Accept<TReturn>(IASTNodeVisitor<ASTNode, TReturn> visitor) => visitor.Visit(this);
}