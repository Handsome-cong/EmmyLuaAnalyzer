﻿using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Type;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl.Primitive;

public class StringSymbol : ClassSymbol
{
    public StringSymbol(SearchContext context) : base("string", context, SymbolKind.Table)
    {
    }

    public override bool SubTypeOf(ILuaSymbol symbol, SearchContext context)
        => base.SubTypeOf(symbol, context) || symbol.Kind == SymbolKind.Table;
}
