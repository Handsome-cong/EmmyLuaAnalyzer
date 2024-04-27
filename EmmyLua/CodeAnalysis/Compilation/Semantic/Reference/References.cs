﻿using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Reference;

public class References(SearchContext context)
{
    public IEnumerable<LuaReference> FindReferences(LuaSyntaxElement element)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(element.DocumentId);
        if (declarationTree is null)
        {
            return Enumerable.Empty<LuaReference>();
        }

        var declaration = declarationTree.FindDeclaration(element, context);
        return declaration switch
        {
            LocalDeclaration localDeclaration => LocalReferences(localDeclaration, declarationTree),
            GlobalDeclaration globalDeclaration => GlobalReferences(globalDeclaration),
            MethodDeclaration methodDeclaration => MethodReferences(methodDeclaration),
            DocFieldDeclaration fieldDeclaration => DocFieldReferences(fieldDeclaration),
            TableFieldDeclaration tableFieldDeclaration => TableFieldReferences(tableFieldDeclaration),
            NamedTypeDeclaration namedTypeDeclaration => NamedTypeReferences(namedTypeDeclaration),
            ParamDeclaration parameterDeclaration => ParameterReferences(parameterDeclaration),
            IndexDeclaration indexDeclaration => IndexExprReferences(indexDeclaration),
            _ => Enumerable.Empty<LuaReference>()
        };
    }

    private IEnumerable<LuaReference> LocalReferences(LuaDeclaration declaration, LuaDeclarationTree declarationTree)
    {
        var references = new List<LuaReference>();
        var declarationNode = declaration.Ptr.ToNode(context);
        var parentBlock = declarationNode?.Ancestors.OfType<LuaBlockSyntax>().FirstOrDefault();
        if (parentBlock is not null)
        {
            references.Add(new LuaReference(declarationNode!.Location, declarationNode));
            foreach (var node in parentBlock.Descendants
                         .Where(it => it.Position > declaration.Position))
            {
                if (node is LuaNameExprSyntax nameExpr && nameExpr.Name?.RepresentText == declaration.Name)
                {
                    if (declarationTree.FindDeclaration(nameExpr, context) == declaration)
                    {
                        references.Add(new LuaReference(nameExpr.Location, nameExpr));
                    }
                }
            }
        }

        return references;
    }

    private IEnumerable<LuaReference> GlobalReferences(LuaDeclaration declaration)
    {
        var references = new List<LuaReference>();
        var globalName = declaration.Name;
        var nameExprs = context.Compilation.DbManager.GetNameExprs(globalName);

        foreach (var nameExpr in nameExprs)
        {
            var declarationTree = context.Compilation.GetDeclarationTree(nameExpr.DocumentId);
            if (declarationTree?.FindDeclaration(nameExpr, context) == declaration)
            {
                references.Add(new LuaReference(nameExpr.Location, nameExpr));
            }
        }

        return references;
    }

    private IEnumerable<LuaReference> FieldReferences(LuaDeclaration declaration, string fieldName)
    {
        var references = new List<LuaReference>();
        var indexExprs = context.Compilation.DbManager.GetIndexExprs(fieldName);
        foreach (var indexExpr in indexExprs)
        {
            var declarationTree = context.Compilation.GetDeclarationTree(indexExpr.DocumentId);
            if (declarationTree?.FindDeclaration(indexExpr, context) == declaration)
            {
                references.Add(new LuaReference(indexExpr.KeyElement.Location, indexExpr.KeyElement));
            }
        }

        return references;
    }

    private IEnumerable<LuaReference> MethodReferences(MethodDeclaration declaration)
    {
        switch (declaration.ScopeFeature)
        {
            case DeclarationScopeFeature.Local:
            {
                var id = declaration.Ptr.DocumentId;
                var declarationTree = context.Compilation.GetDeclarationTree(id);
                if (declarationTree is not null)
                {
                    return LocalReferences(declaration, declarationTree);
                }

                break;
            }
            case DeclarationScopeFeature.Global:
            {
                return GlobalReferences(declaration);
            }
            default:
            {
                if (declaration.IndexExprPtr.ToNode(context) is { Name: { } name })
                {
                    return FieldReferences(declaration, name);
                }

                break;
            }
        }

        return Enumerable.Empty<LuaReference>();
    }

    private IEnumerable<LuaReference> DocFieldReferences(DocFieldDeclaration fieldDeclaration)
    {
        var references = new List<LuaReference>();
        if (fieldDeclaration is
            {
                Name: { } name, FieldDefPtr: { } fieldDefPtr
            }
            && fieldDefPtr.ToNode(context) is { } fieldDef)
        {
            if (fieldDef.FieldElement is { } fieldElement)
            {
                references.Add(new LuaReference(fieldElement.Location, fieldElement));
            }

            var parentType = context.Compilation.DbManager.GetParentType(fieldDef);
            if (parentType is not null)
            {
                var members = context.FindMember(parentType, name);
                foreach (var member in members)
                {
                    if (member is TableFieldDeclaration
                        {
                            Name: { } name2, TableFieldPtr: { } tableFieldPtr
                        }
                        && tableFieldPtr.ToNode(context) is { KeyElement: { } keyElement }
                       )
                    {
                        references.Add(new LuaReference(keyElement.Location, keyElement));
                        break;
                    }
                }
            }

            references.AddRange(FieldReferences(fieldDeclaration, name));
        }

        return references;
    }

    private IEnumerable<LuaReference> TableFieldReferences(TableFieldDeclaration declaration)
    {
        var references = new List<LuaReference>();
        if (declaration is
            {
                Name: { } name, TableFieldPtr: { } tableFieldPtr
            } && tableFieldPtr.ToNode(context) is { } fieldDef)
        {
            if (fieldDef.KeyElement is { } keyElement)
            {
                references.Add(new LuaReference(keyElement.Location, keyElement));
            }

            var parentType = context.Compilation.DbManager.GetParentType(fieldDef);
            if (parentType is not null)
            {
                var members = context.FindMember(parentType, name);
                foreach (var member in members)
                {
                    if (member is DocFieldDeclaration
                        {
                            Name: { } name2, FieldDefPtr: { } fieldDefPtr
                        } && fieldDefPtr.ToNode(context) is { FieldElement: { } fieldElement }
                       )
                    {
                        references.Add(new LuaReference(fieldElement.Location, fieldElement));
                    }
                }
            }

            references.AddRange(FieldReferences(declaration, name));
        }

        return references;
    }

    private IEnumerable<LuaReference> NamedTypeReferences(NamedTypeDeclaration declaration)
    {
        var references = new List<LuaReference>();
        if (declaration is { Name: { } name, TypeDefinePtr: { } typeDefinePtr }
            && typeDefinePtr.ToNode(context) is { Name: { } typeName })
        {
            references.Add(new LuaReference(typeName.Location, typeName));
            var nameTypes = context.Compilation.DbManager.GetNameTypes(name);
            foreach (var nameType in nameTypes)
            {
                var declarationTree = context.Compilation.GetDeclarationTree(nameType.DocumentId);
                if (declarationTree?.FindDeclaration(nameType, context) == declaration && nameType.Name is { } name2)
                {
                    references.Add(new LuaReference(name2.Location, name2));
                }
            }
        }

        return references;
    }

    private IEnumerable<LuaReference> ParameterReferences(ParamDeclaration declaration)
    {
        var references = new List<LuaReference>();
        if (declaration is { ParamDefPtr: { } paramDefPtr, TypedParamPtr: { } typedParamPtr })
        {
            if (paramDefPtr.ToNode(context) is { } paramDef)
            {
                if (DocParameterReferences(paramDef) is { } luaReference)
                {
                    references.Add(luaReference);
                }

                var documentId = paramDefPtr.DocumentId;
                var declarationTree = context.Compilation.GetDeclarationTree(documentId);
                if (declarationTree is not null)
                {
                    references.AddRange(LocalReferences(declaration, declarationTree));
                }
            }
            else if (typedParamPtr.ToNode(context) is { Name: { } typedParamName })
            {
                references.Add(new LuaReference(typedParamName.Location, typedParamName));
            }
        }

        return references;
    }

    private LuaReference? DocParameterReferences(LuaParamDefSyntax paramDefSyntax)
    {
        var paramDefName = paramDefSyntax.Name?.RepresentText;
        var stat = paramDefSyntax.Ancestors.OfType<LuaStatSyntax>().FirstOrDefault();
        if (stat is null || paramDefName is null)
        {
            return null;
        }

        foreach (var comment in stat.Comments)
        {
            foreach (var tagParamSyntax in comment.DocList.OfType<LuaDocTagParamSyntax>())
            {
                if (tagParamSyntax.Name is { RepresentText: { } name } && name == paramDefName)
                {
                    return new LuaReference(tagParamSyntax.Name.Location, tagParamSyntax.Name);
                }
            }
        }

        return null;
    }

    private IEnumerable<LuaReference> IndexExprReferences(IndexDeclaration declaration)
    {
        var references = new List<LuaReference>();
        if (declaration is { IndexExprPtr: { } indexExprPtr }
            && indexExprPtr.ToNode(context) is { Name: { } name })
        {
            references.AddRange(FieldReferences(declaration, name));
        }

        return references;
    }
}
