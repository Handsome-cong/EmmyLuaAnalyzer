﻿using System.Diagnostics;
using LuaLanguageServer.CodeAnalysis.Compilation.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Semantic;

public class SemanticModel
{
    private LuaCompilation _compilation;

    private LuaSyntaxTree _tree;

    public SemanticModel(LuaCompilation compilation, LuaSyntaxTree tree)
    {
        _compilation = compilation;
        _tree = tree;
    }

    public ILuaSymbol GetSymbol(LuaSyntaxElement element)
    {
        return _compilation.SearchContext.Infer(element);
    }
}
