using Flax.Build;

public class DunGenEditorTarget : GameProjectEditorTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for editor
        Modules.Add("DunGen");
        Modules.Add("DunGenEditor");
    }
}
