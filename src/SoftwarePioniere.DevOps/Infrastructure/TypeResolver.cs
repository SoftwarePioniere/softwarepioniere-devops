using System;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps.Infrastructure;

public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));

    public object Resolve(Type type)
    {
        // ArgumentNullException.ThrowIfNull(type);
        return type == null ? null : _provider.GetService(type);
    }

    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}