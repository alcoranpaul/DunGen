#if FLAX_EDITOR

using System;
using System.IO;
using DunGen;
using FlaxEditor;
using FlaxEditor.Content;
using FlaxEditor.Content.Settings;
using FlaxEditor.GUI;
using FlaxEngine;
namespace DunGenEditor;

/// <summary>
/// DunGenEditor Script.
/// </summary>

public class DunGenEditor : EditorPlugin
{
	private CustomSettingsProxy assetProxy;
	private const string SETTINGS_NAME = "DunGenSettings";
	private const string SETTINGS_PATH_FOLDER = "/Data";
	public static string SettingsPath => Path.Combine(Globals.ProjectContentFolder + SETTINGS_PATH_FOLDER, SETTINGS_NAME + ".json");

	private ToolStripButton _button;

	public DunGenEditor()
	{
		_description = new PluginDescription
		{
			Name = "DunGenEditor",
			Category = "Procedural",
			Author = "D1g1Talino",
			AuthorUrl = "https://github.com/alcoranpaul",
			HomepageUrl = "https://github.com/alcoranpaul",
			RepositoryUrl = "https://github.com/alcoranpaul/DunGen",
			Description = "Editor for DunGen",
			Version = new Version(0, 1),
			IsAlpha = true,
			IsBeta = false,

		};
	}
	/// <inheritdoc />
	public override void InitializeEditor()
	{
		base.InitializeEditor();
		_button = Editor.UI.ToolStrip.AddButton("DunGen");
		_button.Clicked += () => new DunGenWindow(_description).Show();

	}

	/// <inheritdoc />
	public override void DeinitializeEditor()
	{
		if (_button != null)
		{
			_button.Dispose();
			_button = null;
		}

		// Debug.Log("DunGenEditor Deinitialize");
		base.DeinitializeEditor();
	}
}
#endif