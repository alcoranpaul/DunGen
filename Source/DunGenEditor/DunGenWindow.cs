#if FLAX_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using DunGen;
using FlaxEditor;
using FlaxEditor.Content;
using FlaxEditor.Content.Settings;
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
	// TODO: make editable via editor
	public string DEBUG_GRID_PREFAB_NAME = "DebugGrid";
	public string PATH_NODE_DEBUG_PREFAB_NAME = "PathNodeDebugObject";
	public string ROOM_PREFAB_NAME = "Room";
	public string FLOOR_PREFAB_NAME = "Floor";
	public string ROOM_MATERIAL_NAME = "Debug Rooms";

	public const string PREFAB_FOLDER_NAME = "Prefabs";
	public const string MATERIAL_FOLDER_NAME = "Material";

	public bool EnableDebugDraw = false;
	private readonly string repoURL = "";

	private DataGenerator dataGenerator;
	private ModelGenerator modelGenerator;


	public DunGenWindow(PluginDescription description)
	{
		// Debug.Log($"DunGenWindow Constructor");
		repoURL = description.RepositoryUrl;
	}
	public override void Initialize(LayoutElementsContainer layout)
	{
		// Debug.Log($"DunGenWindow Initialized");
		// if (generator == null)
		// 	generator = new Generator();

		layout.Label("Dungeon Generation (DunGen)", TextAlignment.Center);
		layout.Space(20);

		// Settings group - location of settings json data
		var settingsGroup = layout.VerticalPanel();


		var settingNameHP = layout.HorizontalPanel();
		settingNameHP.ContainerControl.Height = 20f;
		settingsGroup.AddElement(settingNameHP);

		settingNameHP.AddElement(CreateLabel(layout, $"Settings Name:", marginLeft: 5));
		settingNameHP.AddElement(CreateTextBox(layout, DunGenEditor.SETTINGS_NAME, textboxEnabled: false));

		var settingFolderHP = layout.HorizontalPanel();
		settingFolderHP.ContainerControl.Height = 20f;

		settingsGroup.AddElement(settingFolderHP);

		settingFolderHP.AddElement(CreateLabel(layout, $"Settings Folder:", marginLeft: 5));
		settingFolderHP.AddElement(CreateTextBox(layout, DunGenEditor.SETTINGS_PATH_FOLDER, tooltip: "The folder where the settings is located"));

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
		var saveButton = layout.Button("Open Settings", Color.DarkGray, $"Open settings file @ {DunGenEditor.SettingsPath}");
		saveButton.Button.TextColor = Color.Black;
		saveButton.Button.TextColorHighlighted = Color.Black;
		saveButton.Button.Bold = true;
		saveButton.Button.Clicked += OpenData;

		layout.Space(10);
		var dungeonDataButton = layout.Button("Generate Rooms", Color.DarkRed);
		dungeonDataButton.Button.Enabled = false;
		dungeonDataButton.Button.Clicked += () =>
		{
			// Issue #21
			// action = () => dungeonDataButton.Button.Clicked -= () => GeneratePathfinding(dungeonDataButton);
			// GeneratePathfinding(dungeonDataButton);

		};

		layout.Space(10);
		var spawnDebugButton = layout.Button("Spawn Debug Dungeon", Color.DarkRed);
		spawnDebugButton.Button.Clicked += () => DataGenerator.Instance.SpawnGridDebugDungeon();

		layout.Space(10);
		var destroyButton = layout.Button("Destroy Dungeon", Color.DarkRed);
		destroyButton.Button.Clicked += DestroyDungeon;

		layout.Space(15);
		var generateButton = layout.Button("Generate Final Dungeon", Color.DarkGreen, "Generate the final version of the dungeon");
		generateButton.Button.Clicked += GenerateFinalDungeon;


		layout.Space(20);
		var githubButton = layout.Button("Open Github Repository", Color.DarkKhaki);
		githubButton.Button.Clicked += OpenGitHub;
		githubButton.Button.TextColor = Color.Black;
		githubButton.Button.TextColorHighlighted = Color.Black;
	}


	private void GeneratePathfinding(ButtonElement button)
	{
		// DestroyDungeon();
		// generator = new DataGenerator();
		// generator.SetupActor();
		// generator.GeneratePathfinding();
		// Debug.Log($"Generate Pathfinding");
		// button.Button.Text = "Spawn Rooms";
		// action();
		// button.Button.Clicked += () => SpawnRooms(button);
		// action = () => button.Button.Clicked -= () => SpawnRooms(button);
	}

	private void SpawnRooms(ButtonElement button)
	{

		// if (DataGenerator.Instance == null)
		// {
		// 	Debug.LogWarning("Generator Instance is null");
		// 	return;
		// }
		// Debug.Log($"Spawn Rooms");
		// generator.GenerateRoomData();
		// button.Button.Text = "Connect Rooms";
		// action();
		// action = () => button.Button.Clicked -= () => ConnectRooms(button);
		// button.Button.Clicked += () => ConnectRooms(button);
	}
	private void ConnectRooms(ButtonElement button)
	{

		// if (DataGenerator.Instance == null)
		// {
		// 	Debug.LogWarning("Generator Instance is null");
		// 	return;
		// }
		// Debug.Log($"Connect Rooms");
		// generator.GenerateHallwayPaths();
		// Debug.Log($"GeneratePathfinding Pathfinding null: {generator.Pathfinding == null}");
		// button.Button.Text = "Generate Rooms";
		// action();
		// action = () => button.Button.Clicked -= () => SpawnRooms(button);

		// button.Button.Clicked += () => SpawnRooms(button);

	}


	private void GenerateFinalDungeon()
	{
		DebugDraw.UpdateContext(IntPtr.Zero, float.MaxValue);
		if (DataGenerator.Instance == null)
			dataGenerator = new DataGenerator();
		else
			dataGenerator = DataGenerator.Instance;

		if (ModelGenerator.Instance == null)
			modelGenerator = new ModelGenerator(dataGenerator);
		else
			modelGenerator = ModelGenerator.Instance;


		dataGenerator.GenerateDungeonData();
		modelGenerator.SpawnModels();

		if (!EnableDebugDraw)
			DebugDraw.UpdateContext(IntPtr.Zero, float.MaxValue);
	}

	private void DestroyDungeon()
	{
		DebugDraw.UpdateContext(IntPtr.Zero, float.MaxValue);
		dataGenerator?.DestroyData();
		modelGenerator?.DestroyDungeon();
		Editor.Instance.Windows.SceneWin.Focus();
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




	private void OpenGitHub()
	{
		CreateProcessSettings settings = new CreateProcessSettings();
		settings.FileName = repoURL;
		settings.ShellExecute = true;
		settings.LogOutput = false;
		settings.WaitForEnd = false;

		Platform.CreateProcess(ref settings);


	}

	private void UpdatePlugin()
	{
		// TODO: Implement the update process
		// Define the URL and the output file path
		string url = "https://raw.githubusercontent.com/username/repo/branch/file.txt";
		string savePath = "C:/Your/Desired/Path/file.txt";

		// Set up the CreateProcessSettings for the curl command
		var settings = new CreateProcessSettings
		{
			FileName = "curl",  // Use 'curl' for downloading files
			Arguments = $"-o \"{savePath}\" \"{url}\"",  // Arguments for saving the file
			WorkingDirectory = "",  // You can set the working directory or leave it empty
			LogOutput = true,  // Print the process output to the Flax log
			SaveOutput = false,  // Don't save the process output into the Output array
			WaitForEnd = true,  // Wait for the process to finish before proceeding
			HiddenWindow = true,  // Hide the window (supported on Windows only)
			ShellExecute = false,  // Don't use the operating system shell
								   // Environment = new Dictionary<string, string>()  // You can set custom environment variables here if needed
		};

		// Run the process
		Platform.CreateProcess(ref settings);
	}

	private void OpenData()
	{
		var asset = Content.Load(DunGenEditor.SettingsPath);
		if (asset == null) // If the asset is not found, create a new settings file
		{
			string path = $"../Content{DunGenEditor.SETTINGS_PATH_FOLDER}/{DunGenEditor.SETTINGS_NAME}.json";
			Debug.LogWarning($"Failed to load settings @ {DunGenEditor.SettingsPath} ");

			DungeonGenSettings settings = new DungeonGenSettings();
			settings.DebugSetting = GetDebugAssets();

			Editor.SaveJsonAsset(DunGenEditor.SettingsPath, settings);
			GameSettings.SetCustomSettings(DunGenEditor.SETTINGS_NAME, Content.LoadAsync<JsonAsset>(DunGenEditor.SettingsPath));

			MessageBox.Show($"Newly created settings @ {path}\n You can now open settings via the DunGen Window");
			return;
		}


		if (asset is not JsonAsset) Debug.LogWarning($"Settings @ {DunGenEditor.SettingsPath} is not a JsonAsset");
		// Open the settings asset in the editor
		Editor.Instance.ContentEditing.Open(asset);

	}

	private DungeonGenSettings.DebugSettings GetDebugAssets()
	{
		// Construct the paths to the prefab and material directories based on the DunGenEditor.DebugPath
		string prefabPath = Path.Combine(DunGenEditor.DebugPath, PREFAB_FOLDER_NAME);
		string materialPath = Path.Combine(DunGenEditor.DebugPath, MATERIAL_FOLDER_NAME);

		// Check if the material and prefab directories exist, and log warnings if they don't
		if (!Directory.Exists(materialPath)) Debug.LogWarning($"Debug Material Path does not exist @ {materialPath}");
		if (!Directory.Exists(prefabPath)) Debug.LogWarning($"Debug Prefab Path does not exist @ {prefabPath}");

		// Initialize an empty DebugSettings object to hold the loaded assets
		DungeonGenSettings.DebugSettings debugSettings = new DungeonGenSettings.DebugSettings();

		// ------------------ Load Debug Materials ------------------

		// Get all material files from the material directory (including subdirectories)
		string[] materialFiles = Directory.GetFiles(materialPath, "*", SearchOption.AllDirectories);
		List<Material> debugMaterials = new List<Material>();

		// Iterate through each material file, retrieve its asset information, and load it asynchronously
		foreach (string file in materialFiles)
		{
			if (!Content.GetAssetInfo(file, out var assetInfo)) continue; // Skip if asset info is not found
			debugMaterials.Add(Content.LoadAsync<Material>(assetInfo.Path));
		}

		// Assign the room material if the file name matches the predefined ROOM_MATERIAL_NAME
		foreach (var item in debugMaterials)
		{
			string fileName = Path.GetFileNameWithoutExtension(item.Path);
			if (fileName == ROOM_MATERIAL_NAME) // Check for the specific material used for the room
			{
				debugSettings.Material = item; // Set the room material in the debug settings
			}
		}

		// ------------------ Load Debug Prefabs ------------------

		// Get all prefab files from the prefab directory (including subdirectories)
		string[] prefabFiles = Directory.GetFiles(prefabPath, "*.prefab", SearchOption.AllDirectories);
		List<Prefab> debugPrefabs = new List<Prefab>();

		// Iterate through each prefab file, retrieve its asset information, and load it asynchronously
		foreach (string file in prefabFiles)
		{
			if (!Content.GetAssetInfo(file, out var assetInfo)) continue; // Skip if asset info is not found
			debugPrefabs.Add(Content.LoadAsync<Prefab>(assetInfo.Path));
		}

		// Assign the appropriate prefab to the corresponding field in the debug settings based on the file name
		foreach (var item in debugPrefabs)
		{
			string fileName = Path.GetFileNameWithoutExtension(item.Path);

			if (fileName == DEBUG_GRID_PREFAB_NAME)
			{
				debugSettings.DebugGridPrefab = item; // Set the debug grid prefab
			}
			else if (fileName == PATH_NODE_DEBUG_PREFAB_NAME)
			{
				debugSettings.PathfindingDebugPrefab = item; // Set the pathfinding debug object prefab
			}
			else if (fileName == ROOM_PREFAB_NAME)
			{
				debugSettings.RoomPrefab = item; // Set the room prefab
			}
			else if (fileName == FLOOR_PREFAB_NAME)
			{
				debugSettings.FloorPrefab = item; // Set the floor prefab
			}
		}

		// Return the populated debug settings containing all the necessary prefabs and materials
		return debugSettings;
	}



}

#endif