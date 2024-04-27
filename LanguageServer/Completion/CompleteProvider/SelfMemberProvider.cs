﻿using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LanguageServer.Completion.CompleteProvider;

public class SelfMemberProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaNameExprSyntax)
        {
            return;
        }

        var luaFuncStat = context.TriggerToken.Ancestors.OfType<LuaFuncStatSyntax>().FirstOrDefault();

        if (luaFuncStat is { IsColonFunc: true, IndexExpr.PrefixExpr: { } selfExpr })
        {
            var selfType = context.SemanticModel.Context.Infer(selfExpr);
            var members = context.SemanticModel.Context.GetMembers(selfType);
            foreach (var member in members)
            {
                if (member.DeclarationType is LuaMethodType { ColonDefine: true })
                {
                    context.CreateCompletion($"self:{member.Name}", member.DeclarationType)
                        .WithData(member.Ptr.Stringify)
                        .WithCheckDeprecated(member)
                        .AddToContext();
                }
                else
                {
                    context.CreateCompletion($"self.{member.Name}", member.DeclarationType)
                        .WithData(member.Ptr.Stringify)
                        .WithCheckDeprecated(member)
                        .AddToContext();
                }
            }
        }
    }
}