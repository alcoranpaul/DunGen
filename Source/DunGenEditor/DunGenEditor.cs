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
	public static readonly string SETTINGS_NAME = "DunGenSettings";
	public static readonly string SETTINGS_PATH_FOLDER = "/Data";
	public static readonly string DEBUG_PREFAB_FOLDER = "/Debugging";
	public static string DebugPath => Path.Combine(Globals.ProjectContentFolder + DEBUG_PREFAB_FOLDER);
	public static string SettingsPath => Path.Combine(Globals.ProjectContentFolder + SETTINGS_PATH_FOLDER, SETTINGS_NAME + ".json");




	private ToolStripButton _button;

	private DunGenWindow dunGenWindow;
	private bool isWindowShown;

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


		isWindowShown = false;
		_button = Editor.UI.ToolStrip.AddButton("DunGen");
		dunGenWindow = new DunGenWindow(_description);
		ShowEditorWindow();

		_button.Clicked += ShowEditorWindow;

	}

	private void ShowEditorWindow()
	{
		if (isWindowShown)
		{
			if (Editor.Instance.Windows.ToolboxWin.ParentDockPanel.TabsCount > 1)
				Editor.Instance.Windows.ToolboxWin.ParentDockPanel.SelectTab(1);
			return;
		}

		if (dunGenWindow == null)
			dunGenWindow = new DunGenWindow(_description);
		dunGenWindow.Show(DockState.DockFill, Editor.Instance.Windows.ToolboxWin.ParentDockPanel, false);

		isWindowShown = true;
	}

	/// <inheritdoc />
	public override void DeinitializeEditor()
	{
		if (_button != null)
		{
			_button.Dispose();
			_button = null;
		}

		if (dunGenWindow != null)
		{
			dunGenWindow = null;
		}

		base.DeinitializeEditor();
	}
}
#endif