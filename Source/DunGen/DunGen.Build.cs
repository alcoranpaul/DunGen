using Flax.Build;
using Flax.Build.NativeCpp;

public class DunGen : GameModule
{
    /// <inheritdoc />
    public override void Setup(BuildOptions options)
    {
        base.Setup(options);
        options.ScriptingAPI.IgnoreMissingDocumentationWarnings = true;
        options.PrivateDependencies.Add("GridSystem");
        // Here you can modify the build options for your game module
        // To reference another module use: options.PublicDependencies.Add("Audio");
        // To add C++ define use: options.PublicDefinitions.Add("COMPILE_WITH_FLAX");
        // To learn more see scripting documentation.
        BuildNativeCode = false;
    }
}
