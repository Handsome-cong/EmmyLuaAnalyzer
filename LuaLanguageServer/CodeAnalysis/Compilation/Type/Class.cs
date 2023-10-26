﻿using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Class : LuaType, ILuaNamedType
{
    public string Name { get; }

    public Class(string name) : base(TypeKind.Class)
    {
        Name = name;
    }

    public override IEnumerable<ClassMember> GetMembers(SearchContext context)
    {
        var syntaxElement = context.Compilation
            .StubIndexImpl.ShortNameIndex.Get<LuaShortName.Class>(Name).FirstOrDefault()?.ClassSyntax;
        if (syntaxElement is null)
        {
            yield break;
        }

        var memberIndex = context.Compilation.StubIndexImpl.Members;
        foreach (var classField in memberIndex.Get<LuaMember.ClassDocField>(syntaxElement))
        {
            // yield return context.Infer(classField.ClassDocFieldSyntax);
        }

        // TODO attach variable
    }
}

public class ClassMember : LuaTypeMember
{
    public IndexKey Key { get; }

    public LuaSyntaxElement SyntaxElement { get; }

    public ClassMember(IndexKey key, LuaSyntaxElement syntaxElement, Class containingType) : base(containingType)
    {
        Key = key;
        SyntaxElement = syntaxElement;
    }

    public override ILuaType? GetType(SearchContext context)
    {
        return SyntaxElement switch
        {
            LuaDocTypedFieldSyntax typeField => context.Infer(typeField.Type),
            LuaDocFieldSyntax field => context.Infer(field.Type),
            _ => null
        };
    }

    public override bool MatchKey(IndexKey key, SearchContext context)
    {
        return (key, Key) switch
        {
            (IndexKey.Integer i1, IndexKey.Integer i2) => i1.Value == i2.Value,
            (IndexKey.String s1, IndexKey.String s2) => s1.Value == s2.Value,
            (IndexKey.Ty t1, IndexKey.Ty t2) => t1.Value == t2.Value,
            _ => false
        };
    }
}