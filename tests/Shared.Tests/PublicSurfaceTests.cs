using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Shared.Tests;

[CollectionDefinition("NonParallel", DisableParallelization = true)]
public class NonParallelCollection { }

/// <summary>
/// Basic public surface smoke tests ensuring publicly visible types load and their default/public ctors execute without throwing.
/// This is NOT exhaustive behavioral coverage but asserts that reflection over the assemblies succeeds.
/// </summary>
[Collection("NonParallel")]
public class PublicSurfaceTests
{
    private static readonly Assembly[] TargetAssemblies = new []
    {
        typeof(KC.ITCompanion.CorePolicyEngine.Result).Assembly,
        typeof(Security.IClientAccessPolicy).Assembly,
        typeof(KC.ITCompanion.ClientShared.PolicySettingViewModel).Assembly
    };

    [Fact]
    public void CanEnumeratePublicTypes()
    {
        foreach (var asm in TargetAssemblies)
        {
            var types = asm.GetExportedTypes();
            Assert.NotEmpty(types);
        }
    }

    [Fact]
    public void PublicTypes_DefaultConstructors_DoNotThrow()
    {
        foreach (var asm in TargetAssemblies)
        {
            foreach (var t in asm.GetExportedTypes())
            {
                if (t.IsAbstract || t.IsInterface) continue;
                if (t.IsGenericTypeDefinition) continue;
                var ctor = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                            .FirstOrDefault(c => c.GetParameters().Length == 0);
                if (ctor == null) continue; // skip types requiring args
                try
                {
                    var obj = ctor.Invoke(null);
                    Assert.NotNull(obj);
                }
                catch (TargetInvocationException tie) when (tie.InnerException is System.Runtime.InteropServices.COMException)
                {
                    // Skip WinUI activated types in headless test environment
                    continue;
                }
            }
        }
    }
}
