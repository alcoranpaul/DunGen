using Flax.Build;

public class GridSystemTarget : GameProjectTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for game
        Modules.Add("GridSystem");
    }
}
