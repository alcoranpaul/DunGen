using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Utilities;


namespace DunGen
{
	/// <summary>
	/// The sample game plugin.
	/// </summary>
	/// <seealso cref="GamePlugin" />
	public class DunGen : GamePlugin
	{
		public int test;
		/// <inheritdoc />
		public DunGen()
		{
			_description = new PluginDescription
			{
				Name = "DunGen",
				Category = "Procedural",
				Author = "alcoranpaul",
				AuthorUrl = "https://github.com/alcoranpaul/DunGen",
				HomepageUrl = "https://github.com/alcoranpaul/DunGen",
				RepositoryUrl = "https://github.com/alcoranpaul/DunGen",
				Description = "Procedural Dungeon Generator",
				Version = new Version(0, 1),
				IsAlpha = true,
				IsBeta = false,

			};
		}

		/// <inheritdoc />
		public override void Initialize()
		{
			base.Initialize();
			// Debug.Log("DunGen Initialize");
			// if (DataGenerator.Instance == null)
			// 	Generator = new DataGenerator();
			// else Generator = DataGenerator.Instance;

		}

		/// <inheritdoc />
		public override void Deinitialize()
		{
			// Use it to cleanup data
			base.Deinitialize();
		}

		public void GenerateDungeon()
		{
			// if (Generator != null)
			// 	Generator.GenerateFinalDungeon();
		}
	}




}
