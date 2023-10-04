﻿using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaSourceSyntax : LuaSyntaxNode
{
    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSourceSyntax(GreenNode greenNode, LuaSyntaxTree tree)
        : base(greenNode, tree, null)
    {
    }
}

public class LuaBlockSyntax : LuaSyntaxNode
{
    public IEnumerable<LuaStatSyntax> StatList => ChildNodes<LuaStatSyntax>();

    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? Enumerable.Empty<LuaCommentSyntax>();

    public LuaBlockSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaParamDef : LuaSyntaxNode
{
    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public bool IsVarArgs => FirstChildToken(LuaTokenKind.TkDots) != null;

    public LuaParamDef(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaParamListSyntax : LuaSyntaxNode
{
    public IEnumerable<LuaParamDef> Params => ChildNodes<LuaParamDef>();

    public bool HasVarArgs => Params.LastOrDefault()?.IsVarArgs == true;

    public LuaParamListSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaAttributeSyntax : LuaSyntaxNode
{
    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public bool IsConst
    {
        get
        {
            if (Name == null)
            {
                return false;
            }

            return Name.Text == "const";
        }
    }

    public bool IsClose
    {
        get
        {
            if (Name == null)
            {
                return false;
            }

            return Name.Text == "close";
        }
    }

    public LuaAttributeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaLocalNameSyntax : LuaSyntaxNode
{
    public LuaAttributeSyntax? Attribute => FirstChild<LuaAttributeSyntax>();

    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public LuaLocalNameSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaCallArgListSyntax : LuaSyntaxNode
{
    public IEnumerable<LuaExprSyntax> ArgList => ChildNodes<LuaExprSyntax>();

    public bool IsSingleArgCall => FirstChildToken(LuaTokenKind.TkLeftParen) != null;

    public LuaExprSyntax? SingleArg => FirstChild<LuaExprSyntax>();

    public LuaCallArgListSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}
