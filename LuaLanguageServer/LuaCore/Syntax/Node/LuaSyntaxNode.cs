﻿using LuaLanguageServer.LuaCore.Kind;
using LuaLanguageServer.LuaCore.Syntax.Location;
using LuaLanguageServer.LuaCore.Syntax.Visitor;

namespace LuaLanguageServer.LuaCore.Syntax.Node;

// 实现一个语法节点, 类似于roslyn的语法节点
public abstract class LuaSyntaxNode
{
    public LuaSyntaxKind Kind { get; }

    public LuaSyntaxNode(LuaSyntaxKind kind)
    {
        Kind = kind;
    }
    //
    // public abstract void Accept(LuaSyntaxVisitor visitor);
    //
    // public abstract LuaSyntaxNode Accept(LuaSyntaxRewriter rewriter);
    //
    // public abstract LuaSyntaxNode Update(LuaSyntaxNode node);
    //
    // public abstract LuaSyntaxNode WithLeadingTrivia(LuaSyntaxNode node);
    //
    // public abstract LuaSyntaxNode WithTrailingTrivia(LuaSyntaxNode node);
    //
    // public abstract LuaSyntaxNode WithDiagnostics(LuaSyntaxNode node);
    //
    // public abstract LuaSyntaxNode WithDiagnostics(IEnumerable<LuaDiagnostic> diagnostics);
    //
    // public abstract LuaSyntaxNode WithDiagnostics(params LuaDiagnostic[] diagnostics);
    //
    // public abstract LuaSyntaxNode WithLeadingTrivia(IEnumerable<LuaSyntaxTrivia> trivia);
    //
    // public abstract LuaSyntaxNode WithLeadingTrivia(params LuaSyntaxTrivia[] trivia);
    //
    // public abstract LuaSyntaxNode WithTrailingTrivia(IEnumerable<LuaSyntaxTrivia> trivia);
    //
    // public abstract LuaSyntaxNode WithTrailingTrivia(params LuaSyntaxTrivia[] trivia);
    //
    // public abstract LuaSyntaxNode WithLeadingTrivia(LuaSyntaxTrivia trivia);
    //
    // public abstract LuaSyntaxNode WithTrailingTrivia(LuaSyntaxTrivia trivia);
    //
    // public abstract LuaSyntaxNode WithDiagnostics(LuaDiagnostic diagnostic);
    //
    // public abstract LuaSyntaxNode WithDiagnostics(string message, LuaDiagnosticSeverity severity = LuaDiagnosticSeverity.Error);
    //
    // public abstract LuaSyntaxNode WithDiagnostics(string message, LuaDiagnosticSeverity severity, LuaSourceRange range);
    //
    // public abstract LuaSyntaxNode WithDiagnostics(string message, LuaDiagnosticSeverity severity, LuaLocation location);
    //
    // public abstract LuaSyntaxNode WithDiagnostics(string message, LuaDiagnosticSeverity severity, LuaLocation location, LuaSourceRange range);
    //
    // public abstract LuaSyntaxNode WithDiagnostics(string message, LuaDiagnosticSeverity severity, LuaSourceRange range, LuaLocation location);
    //
    // public abstract LuaSyntaxNode WithDiagnostics(string message, LuaDiagnosticSeverity severity, LuaSourceRange range, LuaSourceRange range2);

}
