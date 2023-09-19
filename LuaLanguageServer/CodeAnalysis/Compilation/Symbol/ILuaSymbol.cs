﻿using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public interface ILuaSymbol
{
    public ILuaSymbol ContainingSymbol { get; }

    public SymbolKind Kind { get; }

    public string Name { get; }

    IEnumerable<LuaLocation> Locations { get; }
}