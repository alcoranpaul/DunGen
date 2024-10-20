using Flax.Build;

public class DunGenTarget : GameProjectTarget
{
	/// <inheritdoc />
	public override void Init()
	{
		base.Init();

		// Reference the modules for game
		Modules.Add("DunGen");
	}
}
