using System;
using FlaxEngine;

namespace GridSystem
{
	/// <summary>
	/// 
	/// </summary>
	public class GSystem : GamePlugin
	{
		/// <inheritdoc />
		public GSystem()
		{
			_description = new PluginDescription
			{
				Name = "GridSystem",
				Category = "Other",
				Author = "D1g1Talino",
				AuthorUrl = null,
				HomepageUrl = null,
				RepositoryUrl = "https://github.com/FlaxEngine/GridSystem",
				Description = "This is an example plugin project.",
				Version = new Version(),
				IsAlpha = false,
				IsBeta = false,
			};
		}

		/// <inheritdoc />
		public override void Initialize()
		{
			base.Initialize();
		}

		/// <inheritdoc />
		public override void Deinitialize()
		{
			// Use it to cleanup data

			base.Deinitialize();
		}
	}
}
