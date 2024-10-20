#if FLAX_EDITOR

using System;
using System.IO;
using FlaxEditor;

using FlaxEditor.GUI;
using FlaxEditor.GUI.Docking;
using FlaxEngine;
namespace DunGenEditor;

/// <summary>
/// DunGenEditor Script.
/// </summary>

public class DunGenEditor : EditorPlugin
{
	public static bool IsInDebug = true; // True when plugin is in development. Set to False when using plugin for production

	public static readonly string SETTINGS_NAME = "DunGenSettings";
	public static readonly string SETTINGS_PATH_FOLDER = "Data";
	public static readonly string DEBUG_PREFAB_FOLDER = "Debugging";
	public static string DebugPath
	{
		get
		{
			if (IsInDebug)
				return Path.Combine(Globals.ProjectContentFolder, DEBUG_PREFAB_FOLDER);
			else
				return Path.Combine(Globals.ProjectFolder, "Plugins", "DunGen", DEBUG_PREFAB_FOLDER, "Debug");
		}
	}
	public static string SettingsPath
	{
		get
		{
			if (IsInDebug)
				return Path.Combine(Globals.ProjectContentFolder, SETTINGS_PATH_FOLDER, SETTINGS_NAME + ".json");
			else
				return Path.Combine(Globals.ProjectFolder, "Plugins", "DunGen", SETTINGS_PATH_FOLDER, SETTINGS_NAME + ".json");
		}
	}




	private ToolStripButton _button;

	public DunGenEditor()
	{
		_description = new PluginDescription
		{
			Name = "DunGenEditor",
			Category = "Procedural",
			Author = "alcoranpaul",
			AuthorUrl = $"https://github.com/alcoranpaul",
			HomepageUrl = $"https://github.com/alcoranpaul",
			RepositoryUrl = $"https://github.com/alcoranpaul/DunGen",
			Description = "Editor for DunGen",
			Version = new Version(1, 0),
			IsAlpha = true,
			IsBeta = false,

		};
	}
	/// <inheritdoc />
	public override void InitializeEditor()
	{
		base.InitializeEditor();

		_button = Editor.UI.ToolStrip.AddButton("DunGen");
		// if (dunGenWindow == null)
		// 	dunGenWindow = new DunGenWindow(_description);
		// ShowEditorWindow();

		_button.Clicked += ShowEditorWindow;
	}

	private void ShowEditorWindow()
	{
		Debug.Log($"TabsCousnt: {Editor.Instance.Windows.ToolboxWin.ParentDockPanel.TabsCount}");
		if (Editor.Instance.Windows.ToolboxWin.ParentDockPanel.TabsCount <= 1)
		{

			new DunGenWindow().Show(DockState.DockFill, Editor.Instance.Windows.ToolboxWin.ParentDockPanel, false);
		}

		if (Editor.Instance.Windows.ToolboxWin.ParentDockPanel.TabsCount > 1)
			Editor.Instance.Windows.ToolboxWin.ParentDockPanel.SelectTab(1);
	}

	/// <inheritdoc />
	public override void DeinitializeEditor()
	{
		if (_button != null)
		{
			_button.Dispose();
			_button = null;

		}
		base.DeinitializeEditor();
	}


}
#endif