﻿namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public enum TypeKind
{
    Unknown,
    Alias,
    Tuple,
    MultiRet,
    Union,
    Array,
    Class,
    Enum,
    Interface,
    Generic,
    Method,
    TypeRef,
}
