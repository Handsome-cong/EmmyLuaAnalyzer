﻿using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

public static class Index
{
    public static void BuildIndex(StubIndexImpl stubIndexImpl, DocumentId documentId, LuaSyntaxTree tree)
    {
        foreach (var node in tree.SyntaxRoot.Descendants)
        {
            switch (node)
            {
                case LuaDocTagClassSyntax luaDocClassSyntax:
                {
                    ClassIndex(stubIndexImpl, documentId, luaDocClassSyntax);
                    break;
                }
                case LuaDocTagEnumSyntax luaDocEnumSyntax:
                {
                    EnumIndex(stubIndexImpl, documentId, luaDocEnumSyntax);
                    break;
                }
                case LuaDocTagAliasSyntax luaDocAliasSyntax:
                {
                    AliasIndex(stubIndexImpl, documentId, luaDocAliasSyntax);
                    break;
                }
                case LuaDocTagInterfaceSyntax luaDocInterfaceSyntax:
                {
                    InterfaceIndex(stubIndexImpl, documentId, luaDocInterfaceSyntax);
                    break;
                }
                case LuaDocTagFieldSyntax luaDocFieldSyntax:
                {
                    FieldIndex(stubIndexImpl, documentId, luaDocFieldSyntax);
                    break;
                }
                case LuaFuncStatSyntax luaFuncStatSyntax:
                {
                    MethodIndex(stubIndexImpl, documentId, luaFuncStatSyntax);
                    break;
                }
                case LuaForStatSyntax luaForStatSyntax:
                {
                    ForIndex(stubIndexImpl, documentId, luaForStatSyntax);
                    break;
                }
                case LuaForRangeStatSyntax luaForRangeStatSyntax:
                {
                    ForRangeIndex(stubIndexImpl, documentId, luaForRangeStatSyntax);
                    break;
                }
                case LuaLocalStatSyntax luaLocalStatSyntax:
                {
                    LocalIndex(stubIndexImpl, documentId, luaLocalStatSyntax);
                    break;
                }
                case LuaAssignStatSyntax luaAssignStatSyntax:
                {
                    AssignIndex(stubIndexImpl, documentId, luaAssignStatSyntax);
                    break;
                }
                case LuaIndexExprSyntax luaIndexExprSyntax:
                {
                    IndexExprIndex(stubIndexImpl, documentId, luaIndexExprSyntax);
                    break;
                }
                case LuaTableFieldSyntax luaTableFieldSyntax:
                {
                    TableFieldIndex(stubIndexImpl, documentId, luaTableFieldSyntax);
                    break;
                }
                case LuaParamListSyntax luaParamListSyntax:
                {
                    ParamIndex(stubIndexImpl, documentId, luaParamListSyntax);
                    break;
                }
            }
        }
    }

    private static void AddFieldSymbol(
        StubIndexImpl stubIndexImpl, DocumentId documentId,
        IndexKey key, LuaSyntaxElement field,
        LuaSyntaxElement? fieldType, ILuaType containingType)
    {
        switch (key)
        {
            case IndexKey.Ty ty:
                // TODO add function
                break;
            case IndexKey.String str:
            {
                var fieldSymbol = new FieldSymbol(
                    field,
                    str.Value,
                    fieldType,
                    containingType
                );
                stubIndexImpl.ShortNameIndex.AddStub(documentId, str.Value, fieldSymbol);
                stubIndexImpl.Members.AddStub(documentId, containingType, fieldSymbol);
                break;
            }
            case IndexKey.Integer i:
            {
                var fieldSymbol = new IndexFieldSymbol(
                    field,
                    i.Value,
                    fieldType,
                    containingType
                );
                stubIndexImpl.ShortNameIndex.AddStub(documentId, $"[{i.Value}]", fieldSymbol);
                stubIndexImpl.Members.AddStub(documentId, containingType, fieldSymbol);
                break;
            }
        }
    }

    private static void ClassIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocTagClassSyntax luaDocTagClassSyntax)
    {
        if (luaDocTagClassSyntax.Name is { } name)
        {
            var classType = CreateLuaType(stubIndexImpl, documentId, name.RepresentText,
                () => new LuaClass(name.RepresentText));
            var classSymbol = new NamedSymbol(
                luaDocTagClassSyntax,
                name.RepresentText,
                classType,
                stubIndexImpl.Compilation.Builtin.Global
            );
            stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, classSymbol);
            stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, classSymbol);

            if (luaDocTagClassSyntax.Body is not { } body) return;
            var context = stubIndexImpl.Compilation.SearchContext;
            foreach (var field in body.FieldList)
            {
                var indexKey = IndexKey.FromDocTypedField(field, context);
                AddFieldSymbol(stubIndexImpl, documentId, indexKey, field, field.Type, classType);
            }
        }
    }

    private static void EnumIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocTagEnumSyntax luaDocTagEnumSyntax)
    {
        if (luaDocTagEnumSyntax.Name is { } name)
        {
            var enumType = CreateLuaType(stubIndexImpl, documentId, name.RepresentText,
                () => new LuaEnum(name.RepresentText, luaDocTagEnumSyntax.BaseType));
            var enumSymbol = new NamedSymbol(
                luaDocTagEnumSyntax,
                luaDocTagEnumSyntax.Name.RepresentText,
                enumType,
                stubIndexImpl.Compilation.Builtin.Global
            );
            stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, enumSymbol);
            stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, enumSymbol);


            foreach (var field in luaDocTagEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldSymbol = new EnumFieldSymbol(
                        field,
                        fieldName.RepresentText,
                        enumType
                    );

                    stubIndexImpl.ShortNameIndex.AddStub(documentId, fieldName.RepresentText, fieldSymbol);
                    stubIndexImpl.Members.AddStub(documentId, enumType, fieldSymbol);
                }
            }
        }
    }

    private static void InterfaceIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocTagInterfaceSyntax luaDocTagInterfaceSyntax)
    {
        if (luaDocTagInterfaceSyntax.Name is { } name)
        {
            var interfaceType = CreateLuaType(stubIndexImpl, documentId, name.RepresentText,
                () => new LuaInterface(name.RepresentText));
            var interfaceSymbol = new NamedSymbol(
                luaDocTagInterfaceSyntax,
                name.RepresentText,
                interfaceType,
                stubIndexImpl.Compilation.Builtin.Global);

            stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, interfaceSymbol);
            stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, interfaceSymbol);

            if (luaDocTagInterfaceSyntax.Body is not { } body) return;
            var context = stubIndexImpl.Compilation.SearchContext;
            foreach (var field in body.FieldList)
            {
                var indexKey = IndexKey.FromDocTypedField(field, context);
                AddFieldSymbol(stubIndexImpl, documentId, indexKey, field, field.Type, interfaceType);
            }
        }
    }

    private static void AliasIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocTagAliasSyntax luaDocTagAliasSyntax)
    {
        if (luaDocTagAliasSyntax.Name is { } name)
        {
            var context = stubIndexImpl.Compilation.SearchContext;
            var baseTy = context.Infer(luaDocTagAliasSyntax.Type);
            var aliasType = CreateLuaType(stubIndexImpl, documentId, name.RepresentText,
                () => new LuaAlias(name.RepresentText, baseTy));
            var aliasSymbol = new NamedSymbol(
                luaDocTagAliasSyntax,
                name.RepresentText,
                aliasType,
                stubIndexImpl.Compilation.Builtin.Global
            );

            stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, aliasSymbol);
            stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, aliasSymbol);
        }
    }

    private static void FieldIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocTagFieldSyntax luaDocTagFieldSyntax)
    {
        var context = stubIndexImpl.Compilation.SearchContext;
        var indexKey = IndexKey.FromDocField(luaDocTagFieldSyntax, context);
        ILuaType? classType = null;
        if (luaDocTagFieldSyntax.PrevOfType<LuaDocTagClassSyntax>() is { Name : { } className })
        {
            classType = stubIndexImpl.LuaTypeIndex.Get<LuaClass>(className.RepresentText).FirstOrDefault();
        }
        else if (luaDocTagFieldSyntax.PrevOfType<LuaDocTagInterfaceSyntax>() is { Name : { } interfaceName })
        {
            classType = stubIndexImpl.LuaTypeIndex.Get<LuaInterface>(interfaceName.RepresentText).FirstOrDefault();
        }

        if (classType is not null)
        {
            AddFieldSymbol(stubIndexImpl, documentId, indexKey, luaDocTagFieldSyntax, luaDocTagFieldSyntax.Type,
                classType);
        }
    }

    private static void MethodIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaFuncStatSyntax luaFuncStatSyntax)
    {
        // TODO
        // if (luaFuncStatSyntax is { PrefixExpr: { } prefixExpr })
        // {
        //     stubIndexImpl.Members.AddStub(
        //         documentId, prefixExpr, new LuaMember.Function(luaFuncStatSyntax));
        // }
    }

    private static void ForIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaForStatSyntax luaForStatSyntax)
    {
        if (luaForStatSyntax.IteratorName is { Name: { } name } param)
        {
            var docTagParamSyntax = luaForStatSyntax.Comments.FirstOrDefault()?
                .DocList.OfType<LuaDocTagParamSyntax>()
                .FirstOrDefault();
            if (docTagParamSyntax is { Name: { } tagParamName }
                && string.Equals(tagParamName.RepresentText, name.RepresentText, StringComparison.Ordinal))
            {
                var localSymbol = new LocalSymbol(param, name.RepresentText, docTagParamSyntax.Type);
                stubIndexImpl.LocalIndex.AddStub(documentId, param, localSymbol);
            }
            else
            {
                var localSymbol = new LocalSymbol(param, name.RepresentText, luaForStatSyntax.InitExpr);
                stubIndexImpl.LocalIndex.AddStub(documentId, param, localSymbol);
            }
        }
    }

    private static void ForRangeIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaForRangeStatSyntax luaForRangeStatSyntax)
    {
        var docTagParamSyntaxDictionary = luaForRangeStatSyntax.Comments.FirstOrDefault()?
            .DocList.OfType<LuaDocTagParamSyntax>()
            .Where(it => it.Name is not null)
            .ToDictionary(it => it.Name!.RepresentText, it => it.Type);

        var iteratorExprList = luaForRangeStatSyntax.ExprList;
        var iteratorGenerator = new IteratorGenerator(iteratorExprList);
        foreach (var iteratorName in luaForRangeStatSyntax.IteratorNames)
        {
            if (iteratorName is { Name: { } name } param)
            {
                if (docTagParamSyntaxDictionary is not null
                    && docTagParamSyntaxDictionary.TryGetValue(name.RepresentText, out var type))
                {
                    var localSymbol = new LocalSymbol(iteratorName, name.RepresentText, type);
                    stubIndexImpl.LocalIndex.AddStub(documentId, param, localSymbol);
                }
                else
                {
                    var iteratorParamSymbol =
                        new IteratorParamSymbol(iteratorName, name.RepresentText, iteratorGenerator);
                    stubIndexImpl.LocalIndex.AddStub(documentId, param, iteratorParamSymbol);
                }
            }
        }
    }

    private static LuaDocTagSyntax? FindDefineTypeTag(LuaStatSyntax stat, StubIndexImpl stubIndexImpl)
    {
        var docList = stat.Comments.FirstOrDefault()?.DocList;
        if (docList is null) return null;
        foreach (var docTagSyntax in docList)
        {
            switch (docTagSyntax)
            {
                case LuaDocTagClassSyntax luaDocClassSyntax:
                {
                    return luaDocClassSyntax;
                }
                case LuaDocTagEnumSyntax luaDocEnumSyntax:
                {
                    return luaDocEnumSyntax;
                }
                case LuaDocTagInterfaceSyntax luaDocInterfaceSyntax:
                {
                    return luaDocInterfaceSyntax;
                }
                case LuaDocTagTypeSyntax luaDocTagSyntax:
                {
                    return luaDocTagSyntax;
                }
            }
        }

        return null;
    }

    private static void ModifyNameDeclarationSymbol(LuaDocTagSyntax docTagSyntax,
        List<(LuaSyntaxElement, LuaSymbol)> nameList, StubIndexImpl stubIndexImpl)
    {
        var firstLocalNameTuple = nameList.FirstOrDefault()!;
        switch (docTagSyntax)
        {
            case LuaDocTagClassSyntax classSyntax:
            {
                if (classSyntax.Name is { } name)
                {
                    if(TryFindLuaType<LuaClass>(stubIndexImpl, name.RepresentText, out var luaClass) && luaClass is not null)
                    {
                        firstLocalNameTuple.Item2 = new TypeDeclarationSymbol(firstLocalNameTuple.Item1, luaClass);
                    }
                }

                break;
            }
            case LuaDocTagEnumSyntax enumSyntax:
            {
                if (enumSyntax.Name is { } name)
                {
                    if(TryFindLuaType<LuaEnum>(stubIndexImpl, name.RepresentText, out var luaEnum) && luaEnum is not null)
                    {
                        firstLocalNameTuple.Item2 = new TypeDeclarationSymbol(firstLocalNameTuple.Item1, luaEnum);
                    }
                }
                break;
            }
            case LuaDocTagInterfaceSyntax interfaceSyntax:
            {
                if (interfaceSyntax.Name is { } name)
                {
                    if(TryFindLuaType<LuaInterface>(stubIndexImpl, name.RepresentText, out var luaInterface) && luaInterface is not null)
                    {
                        firstLocalNameTuple.Item2 = new TypeDeclarationSymbol(firstLocalNameTuple.Item1, luaInterface);
                    }
                }
                break;
            }
            case LuaDocTagTypeSyntax typeSyntax:
            {
                var idCount = 0;
                foreach (var docTypeSyntax in typeSyntax.TypeList)
                {
                    if (idCount < nameList.Count)
                    {
                        var localNameTuple = nameList[idCount];
                        if (localNameTuple.Item1 is LuaLocalNameSyntax localNameSyntax)
                        {
                            localNameTuple.Item2 = new LocalSymbol(localNameTuple.Item1,
                                localNameSyntax.Name!.RepresentText, docTypeSyntax);
                        }
                    }

                    idCount++;
                }

                break;
            }
        }
    }


    private static void LocalIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaLocalStatSyntax luaLocalStatSyntax)
    {
        var localNameList = new List<(LuaSyntaxElement, LuaSymbol)>();
        var count = luaLocalStatSyntax.NameList.Count();
        var lastValidExprId = -1;
        LuaExprSyntax? lastValidExpr = null;
        for (var i = 0; i < count; i++)
        {
            var localName = luaLocalStatSyntax.NameList.ElementAt(i);
            var expr = luaLocalStatSyntax.ExpressionList.ElementAtOrDefault(i);
            if (expr is not null)
            {
                lastValidExpr = expr;
                lastValidExprId = i;
            }

            if (localName is { Name: { } name })
            {
                var localSymbol = new LocalSymbol(localName, name.RepresentText, lastValidExpr, i - lastValidExprId);
                localNameList.Add((localName, localSymbol));
            }
        }

        if (localNameList.Count == 0)
        {
            return;
        }

        var docTag = FindDefineTypeTag(luaLocalStatSyntax, stubIndexImpl);
        if (docTag is not null)
        {
            ModifyNameDeclarationSymbol(docTag, localNameList, stubIndexImpl);
        }

        foreach (var valueTuple in localNameList)
        {
            stubIndexImpl.LocalIndex.AddStub(documentId, valueTuple.Item1, valueTuple.Item2);
        }
    }

    private static void AssignIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaAssignStatSyntax luaAssignStatSyntax)
    {
        var varSymbols = new List<(LuaSyntaxElement, LuaSymbol)>();
        var count = luaAssignStatSyntax.VarList.Count();
        var lastValidExprId = -1;
        LuaExprSyntax? lastValidExpr = null;
        for (var i = 0; i < count; i++)
        {
            var varName = luaAssignStatSyntax.VarList.ElementAt(i);
            var expr = luaAssignStatSyntax.ExprList.ElementAtOrDefault(i);
            if (expr is not null)
            {
                lastValidExpr = expr;
                lastValidExprId = i;
            }

            // var varSymbol = FindDeclaration(varName);
            // if (varName is {})
            // {
            //     var localSymbol = new LocalSymbol(localName, name.RepresentText, lastValidExpr, i - lastValidExprId);
            //     varSymbols.Add((varName, localSymbol));
            // }
        }

        var docTag = FindDefineTypeTag(luaAssignStatSyntax, stubIndexImpl);
        if (docTag is not null)
        {
            ModifyNameDeclarationSymbol(docTag, varSymbols, stubIndexImpl);
        }

        foreach (var valueTuple in varSymbols)
        {
            stubIndexImpl.LocalIndex.AddStub(documentId, valueTuple.Item1, valueTuple.Item2);
        }
    }

    private static void IndexExprIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaIndexExprSyntax luaIndexExprSyntax)
    {
        // if (luaIndexExprSyntax.PrefixExpr is { } prefixExpr)
        // {
        //     stubIndexImpl.Members.AddStub(
        //         documentId, prefixExpr, new LuaMember.Index(luaIndexExprSyntax));
        // }
    }

    private static void TableFieldIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaTableFieldSyntax luaTableFieldSyntax)
    {
        // switch (luaTableFieldSyntax)
        // {
        //     case { NameKey: { } nameKey }:
        //     {
        //         stubIndexImpl.ShortNameIndex.AddStub(
        //             documentId, nameKey.RepresentText, new LuaShortName.TableField(luaTableFieldSyntax));
        //         break;
        //     }
        //     case { StringKey: { } stringKey }:
        //     {
        //         stubIndexImpl.ShortNameIndex.AddStub(
        //             documentId, stringKey.InnerString, new LuaShortName.TableField(luaTableFieldSyntax));
        //         break;
        //     }
        //     case { NumberKey: { } numberKey }:
        //     {
        //         if (numberKey is LuaIntegerToken integerToken)
        //         {
        //             stubIndexImpl.ShortNameIndex.AddStub(
        //                 documentId, $"[{integerToken.Value}]", new LuaShortName.TableField(luaTableFieldSyntax));
        //             break;
        //         }
        //         else
        //         {
        //             stubIndexImpl.ShortNameIndex.AddStub(
        //                 documentId, $"[{numberKey.RepresentText}]", new LuaShortName.TableField(luaTableFieldSyntax));
        //             break;
        //         }
        //     }
        // }
        //
        // if (luaTableFieldSyntax.Parent is { } parentExpr)
        // {
        //     stubIndexImpl.Members.AddStub(
        //         documentId, parentExpr, new LuaMember.TableField(luaTableFieldSyntax));
        // }
    }

    private static void ParamIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaParamListSyntax luaParamListSyntax)
    {
        foreach (var param in luaParamListSyntax.Params)
        {
            // if (param.Name is { } name)
            // {
            //     stubIndexImpl.ShortNameIndex.AddStub(
            //         documentId, name.RepresentText, new LuaShortName.Param(param));
            // }
        }
    }

    private static bool TryFindLuaType<TLuaType>(StubIndexImpl stubIndexImpl, string name, out TLuaType? type)
        where TLuaType : ILuaNamedType
    {
        var compilation = stubIndexImpl.Compilation;
        var buildInTy = compilation.Builtin.FromName(name);
        if (buildInTy is TLuaType luaClass)
        {
            type = luaClass;
            return true;
        }

        if (stubIndexImpl.LuaTypeIndex.Get<TLuaType>(name).FirstOrDefault() is { } ty)
        {
            type = ty;
            return true;
        }

        type = default(TLuaType);
        return false;
    }

    private static TLuaType CreateLuaType<TLuaType>(StubIndexImpl stubIndexImpl, DocumentId documentId, string name,
        Func<TLuaType> factory)
        where TLuaType : ILuaNamedType
    {
        if (TryFindLuaType<TLuaType>(stubIndexImpl, name, out var ty))
        {
            return ty!;
        }

        var newTy = factory();
        stubIndexImpl.LuaTypeIndex.AddStub(documentId, name, newTy);
        return newTy;
    }

    public static void RemoveIndex(StubIndexImpl stubIndexImpl, DocumentId documentId, LuaSyntaxTree tree)
    {
        stubIndexImpl.ShortNameIndex.RemoveStub(documentId);
        stubIndexImpl.Members.RemoveStub(documentId);
        // stubIndexImpl.Attached.RemoveStub(documentId);
    }
}
