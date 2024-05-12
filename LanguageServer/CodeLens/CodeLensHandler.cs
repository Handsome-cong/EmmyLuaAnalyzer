﻿using LanguageServer.Configuration;
using LanguageServer.Server;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.CodeLens;

public class CodeLensHandler(ServerContext context) : CodeLensHandlerBase
{
    private CodeLensBuilder Builder { get; } = new();
    
    protected override CodeLensRegistrationOptions CreateRegistrationOptions(CodeLensCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new CodeLensRegistrationOptions()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(context.LuaWorkspace),
            ResolveProvider = true,
        };
    }

    public override Task<CodeLensContainer?> Handle(CodeLensParams request, CancellationToken cancellationToken)
    {
        CodeLensContainer? container = null;
        var config = context.SettingManager.GetCodeLensConfig();
        if (!config.Enable)
        {
            return Task.FromResult(container);
        }
        var uri = request.TextDocument.Uri.ToUri().AbsoluteUri;
        
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                container = Builder.Build(semanticModel, context);
            }
        });
        
        return Task.FromResult(container);
    }

    public override Task<OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeLens> Handle(
        OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeLens request, CancellationToken cancellationToken)
    {
        context.ReadyRead(() =>
        {
            request = Builder.Resolve(request, context);
        });

        return Task.FromResult(request);
    }
}