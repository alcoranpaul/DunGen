#if FLAX_EDITOR
using System;
using System.IO;
using DunGen;
using FlaxEditor;
using FlaxEditor.Content;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEditor.GUI.Docking;
using FlaxEngine;
using FlaxEngine.GUI;

namespace DunGenEditor;

/// <summary>
/// DunGenWindow Script.
/// </summary>
public class DunGenWindow : CustomEditorWindow
{


	private const string SETTINGS_NAME = "DunGenSettings";
	public string SETTINGS_PATH_FOLDER = "Data";
	public string SettingsPath => Path.Combine(Globals.ProjectContentFolder + "/" + SETTINGS_PATH_FOLDER, SETTINGS_NAME + ".json");

	public bool EnableDebugDraw = false;
	private readonly string repoURL = "";

	public DunGenWindow(PluginDescription description)
	{
		Debug.Log($"DunGenWindow Constructor");
		repoURL = description.RepositoryUrl;
	}
	public override void Initialize(LayoutElementsContainer layout)
	{
		Debug.Log($"DunGenWindow Initialized");
		layout.Label("Dungeon Generation (DunGen)", TextAlignment.Center);
		layout.Space(20);

		// Settings group - location of settings json data
		var settingsGroup = layout.VerticalPanel();


		var settingNameHP = layout.HorizontalPanel();
		settingNameHP.ContainerControl.Height = 20f;
		settingsGroup.AddElement(settingNameHP);

		settingNameHP.AddElement(CreateLabel(layout, $"Settings Name:", marginLeft: 5));
		settingNameHP.AddElement(CreateTextBox(layout, SETTINGS_NAME, textboxEnabled: false));

		var settingFolderHP = layout.HorizontalPanel();
		settingFolderHP.ContainerControl.Height = 20f;

		settingsGroup.AddElement(settingFolderHP);

		settingFolderHP.AddElement(CreateLabel(layout, $"Settings Folder:", marginLeft: 5));
		settingFolderHP.AddElement(CreateTextBox(layout, SETTINGS_PATH_FOLDER, tooltip: "The folder where the settings is located"));

		var enabbleDebugHP = layout.HorizontalPanel();
		enabbleDebugHP.ContainerControl.Height = 20f;
		enabbleDebugHP.ContainerControl.Width = 800f;
		settingsGroup.AddElement(enabbleDebugHP);
		enabbleDebugHP.AddElement(CreateLabel(layout, $"Enable Debug Drawing:", marginLeft: 5));
		var enableDebugBox = layout.Checkbox("Enable Debug Draw");
		enabbleDebugHP.AddElement(enableDebugBox);
		enableDebugBox.CheckBox.StateChanged += ToggleDebugDraw;



		// Buttons group - Load, Save, Generate
		layout.Space(20);
		var saveButton = layout.Button("Open Settings", Color.DarkGray, $"Open settings file @ {SettingsPath}");
		saveButton.Button.TextColor = Color.Black;
		saveButton.Button.TextColorHighlighted = Color.Black;
		saveButton.Button.Bold = true;
		saveButton.Button.Clicked += OpenData;

		layout.Space(10);
		var dungeonDataButton = layout.Button("Generate Dungeon Data", Color.DarkRed);
		dungeonDataButton.Button.Clicked += GenerateData;

		layout.Space(10);
		var spawnRoomsButton = layout.Button("Spawn Rooms", Color.DarkRed);
		spawnRoomsButton.Button.Clicked += SpawnRooms;

		layout.Space(10);
		var spawnHallwaysButton = layout.Button("Spawn Hallways", Color.DarkRed);
		spawnHallwaysButton.Button.Clicked += SpawnHallways;

		layout.Space(10);
		var destroyButton = layout.Button("Destroy Dungeon", Color.DarkRed);
		destroyButton.Button.Clicked += DestroyDungeon;

		layout.Space(15);
		var generateButton = layout.Button("Generate Final Dungeon", Color.DarkGreen, "Generate the final version of the dungeon");
		generateButton.Button.Clicked += GenerateDungeon;


		layout.Space(20);
		var githubButton = layout.Button("Open Github Repository", Color.DarkKhaki);
		githubButton.Button.Clicked += OpenGitHub;
		githubButton.Button.TextColor = Color.Black;
		githubButton.Button.TextColorHighlighted = Color.Black;
	}


	private void SpawnHallways()
	{

	}

	private void SpawnRooms()
	{

	}

	private void GenerateData()
	{

	}

	private void ToggleDebugDraw(CheckBox box)
	{
		EnableDebugDraw = box.Checked;
		Debug.Log($"Debug Draw is {EnableDebugDraw}");
	}

	private TextBoxElement CreateTextBox(LayoutElementsContainer layout, string textValue, string tooltip = "", bool textboxEnabled = true)
	{
		var retVal = layout.TextBox();
		retVal.Text = textValue;
		retVal.TextBox.Enabled = textboxEnabled;
		if (!string.IsNullOrEmpty(tooltip))
			retVal.TextBox.TooltipText = tooltip;
		return retVal;
	}

	private LabelElement CreateLabel(LayoutElementsContainer layout, string name, int marginLeft = 10, int marginRight = 20, string tooltip = "", bool textboxEnabled = true)
	{
		var retVal = layout.Label(name);
		retVal.Label.Margin = new Margin(marginLeft, marginRight, 0, 0);
		retVal.Label.AutoWidth = true;
		retVal.Label.Enabled = textboxEnabled;
		if (!string.IsNullOrEmpty(tooltip))
			retVal.Label.TooltipText = tooltip;
		return retVal;
	}

	private void GenerateDungeon()
	{
		DebugDraw.UpdateContext(IntPtr.Zero, float.MaxValue);
		Generator generator = new Generator();
		generator.GenerateDungeon();

		if (!EnableDebugDraw)
			DebugDraw.UpdateContext(IntPtr.Zero, float.MaxValue);
	}

	private void DestroyDungeon()
	{
		DebugDraw.UpdateContext(IntPtr.Zero, float.MaxValue);
		Actor DungeonGenActor = Level.FindActor("DungeonGenActor");
		// If Actor has children, destroy them
		if (DungeonGenActor != null && DungeonGenActor.ChildrenCount > 0)
		{
			DungeonGenActor.DestroyChildren();
		}
	}


	private void OpenGitHub()
	{
		CreateProcessSettings settings = new CreateProcessSettings();
		settings.FileName = repoURL;
		settings.ShellExecute = true;
		settings.LogOutput = false;
		settings.WaitForEnd = false;
		Platform.CreateProcess(ref settings);
	}

	private void OpenData()
	{
		var asset = Content.Load(SettingsPath);
		if (asset == null) Debug.LogWarning($"Failed to load settings @ {SettingsPath}");
		// TODO: Add auto create settings if a bool is enabled

		if (asset is not JsonAsset) Debug.LogWarning($"Settings @ {SettingsPath} is not a JsonAsset");
		Editor.Instance.ContentEditing.Open(asset);
	}



}

#endif