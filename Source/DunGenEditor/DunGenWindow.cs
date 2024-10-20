#if FLAX_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
	#region DebugPrefabNames
	public string DEBUG_GRID_PREFAB_NAME = "DebugGrid";
	public string PATH_NODE_DEBUG_PREFAB_NAME = "PathNodeDebugObject";
	public string ROOM_PREFAB_NAME = "Room";
	public string HALLWAY_FLOOR_PREFAB = "HallwayFloor";
	public string ROOM_FLOOR_PREFAB = "RoomFloor";
	public string ROOM_DOOR_FLOOR_PREFAB = "RoomDoorFloor";
	public string ROOM_MATERIAL_NAME = "Debug Rooms";
	#endregion

	#region RoomPrefabNames
	public string ROOM_NWALL_PREFAB = "NWallPrefab";
	public string ROOM_SWALL_PREFAB = "SWallPrefab";
	public string ROOM_EWALL_PREFAB = "EWallPrefab";
	public string ROOM_WWALL_PREFAB = "WWallPrefab";

	public string ROOM_NDOOR_WALL_PREFAB = "NDoorWallPrefab";
	public string ROOM_SDOOR_WALL_PREFAB = "SDoorWallPrefab";
	public string ROOM_EDOOR_WALL_PREFAB = "EDoorWallPrefab";
	public string ROOM_WDOOR_WALL_PREFAB = "WDoorWallPrefab";
	#endregion
	public const string PREFAB_FOLDER_NAME = "Prefabs";
	public const string MATERIAL_FOLDER_NAME = "Material";

	public bool EnableDebugDraw = false;
	public DebugDrawType _DebugDrawType;


	private DataGenerator dataGenerator;
	private ModelGenerator modelGenerator;
	private DungeonGenSettings dunGenSettings;
	private Task _task;


	public override void Initialize(LayoutElementsContainer layout)
	{
		if (Window == null) // So that it does not double initialize
			return;

		// Debug.Log($"DunGenWindow Initialized");
		// if (generator == null)
		// 	generator = new Generator();
		dunGenSettings = GetInstanceSettings();
		if (dunGenSettings.MaxRooms <= 2)
		{
			MessageBox.Show("Max Rooms is less than 2, value will be set to the minimum acceptable number (which is 3)\nOpening Settings...", "Invalid Max Rooms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			dunGenSettings.MaxRooms = 3;
			SaveToSettings();
		}

		layout.Label("Dungeon Generation (DunGen)", TextAlignment.Center);
		layout.Space(20);

		// Settings group - location of settings json data
		CreateSettingsGroup(layout);

		// Buttons group - Load, Save, Generate
		CreateButtons(layout);


	}

	private void CreateButtons(LayoutElementsContainer layout)
	{
		layout.Space(20);
		var saveButton = layout.Button("Open Settings", Color.DarkGray, $"Open settings file @ {DunGenEditor.SettingsPath}");
		saveButton.Button.TextColor = Color.Black;
		saveButton.Button.TextColorHighlighted = Color.Black;
		saveButton.Button.Bold = true;
		saveButton.Button.Clicked += OpenData;

		// layout.Space(10);
		// var saveSettingsButton = layout.Button("Save to Settings", Color.DarkRed);
		// saveSettingsButton.Button.Clicked += () => SaveToSettings();

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
		spawnDebugButton.Button.Clicked += () =>
		{
			if (dataGenerator != null) dataGenerator.SpawnGridDebugDungeon();
		};

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

		layout.Space(10);
		var updateButton = layout.Button("Check for Updates", Color.DarkKhaki);
		updateButton.Button.Clicked += CheckForGitHubUpdate;
		updateButton.Button.TextColor = Color.Black;
		updateButton.Button.TextColorHighlighted = Color.Black;
	}

	private void CreateSettingsGroup(LayoutElementsContainer layout)
	{
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
	}

	private void SaveToSettings()
	{
		var asset = Content.Load(DunGenEditor.SettingsPath);
		if (asset != null)
		{
			JsonAsset json = asset as JsonAsset;
			DungeonGenSettings settings = json.GetInstance<DungeonGenSettings>();
			if (settings == null) Debug.LogWarning("Settings is null");
			settings = dunGenSettings;
			Editor.SaveJsonAsset(DunGenEditor.SettingsPath, settings);
			OpenData();
		}
	}

	private DungeonGenSettings GetInstanceSettings()
	{
		var asset = Content.Load(DunGenEditor.SettingsPath);
		if (asset == null)
		{
			Debug.LogWarning($"Failed to load asset @ {DunGenEditor.SettingsPath} ");
			CreateSettings();
			asset = Content.Load(DunGenEditor.SettingsPath);
		}
		if (asset is not JsonAsset)
		{
			Debug.LogError($"Settings @ {DunGenEditor.SettingsPath} is not a JsonAsset");
			return null;
		}
		JsonAsset json = asset as JsonAsset;
		return json.GetInstance<DungeonGenSettings>();

	}
	private void GenerateFinalDungeon()
	{
		if (dunGenSettings.MaxRooms <= 2)
		{
			MessageBox.Show("Max Rooms is less than 2, value will be set to the minimum acceptable number (which is 3)\nOpening Settings...", "Invalid Max Rooms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			dunGenSettings.MaxRooms = 3;
			SaveToSettings();
			return;
		}

		DebugDraw.UpdateContext(IntPtr.Zero, float.MaxValue);
		dunGenSettings = GetInstanceSettings();
		if (dataGenerator == null)
			dataGenerator = new DataGenerator(dunGenSettings);

		if (modelGenerator == null)
			modelGenerator = new ModelGenerator(dataGenerator, dunGenSettings);

		// RefreshSettings();
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
		if (modelGenerator == null)
		{
			Actor dungeonGenActor = Level.FindActor("DungeonGenActor");
			// If Actor has children, destroy them
			if (dungeonGenActor != null && dungeonGenActor.ChildrenCount > 0)
			{
				dungeonGenActor.DestroyChildren();
			}
		}
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



	private void CheckForGitHubUpdate()
	{
		var gamePlugin = PluginManager.GetPlugin("DunGen");
		if (gamePlugin == null) Debug.LogError("Plugin is null");
		PluginDescription pluginDescription = gamePlugin.Description;

		_task = Task.Run(async () =>
		{
			var latestRelease = await GithubFetcher.FetchLatestReleaseAsync(pluginDescription.Author, PluginManager.GetPlugin("DunGen").Description.Name);
			if (!string.IsNullOrEmpty(latestRelease))
			{
				var version = new Version(latestRelease);
				bool isSameVersion = version.Major == pluginDescription.Version.Major && version.Minor == pluginDescription.Version.Minor;
				if (isSameVersion)
					MessageBox.Show($"You are up to date with the latest release: {latestRelease}", "No Updates", MessageBoxButtons.OK, MessageBoxIcon.Information);
				else
					MessageBox.Show($"A new release is available: {latestRelease}", "Update Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		});

	}
	private void OpenGitHub()
	{
		var gamePlugin = PluginManager.GetPlugin("DunGen");
		if (gamePlugin == null) Debug.LogError("Plugin is null");

		PluginDescription pluginDescription = gamePlugin.Description;
		CreateProcessSettings settings = new CreateProcessSettings();
		settings.FileName = pluginDescription.RepositoryUrl;
		settings.ShellExecute = true;
		settings.LogOutput = false;
		settings.WaitForEnd = false;


		Platform.CreateProcess(ref settings);


	}

	private void OpenData()
	{
		var asset = Content.Load(DunGenEditor.SettingsPath);
		if (asset == null) // If the asset is not found, create a new settings file
		{
			CreateSettings();
			return;
		}


		if (asset is not JsonAsset) Debug.LogWarning($"Settings @ {DunGenEditor.SettingsPath} is not a JsonAsset");
		// Open the settings asset in the editor
		Editor.Instance.ContentEditing.Open(asset);

	}

	private void CreateSettings()
	{
		string path = $"../Content{DunGenEditor.SETTINGS_PATH_FOLDER}/{DunGenEditor.SETTINGS_NAME}.json";
		Debug.LogWarning($"Failed to load settings @ {DunGenEditor.SettingsPath} ");

		DungeonGenSettings settings = new DungeonGenSettings();
		settings.DebugSetting = GetDebugSettings();

		Editor.SaveJsonAsset(DunGenEditor.SettingsPath, settings);
		GameSettings.SetCustomSettings(DunGenEditor.SETTINGS_NAME, Content.LoadAsync<JsonAsset>(DunGenEditor.SettingsPath));

		MessageBox.Show($"Newly created settings @ {path}\n You can now open settings via the DunGen Window");
	}

	// private void RefreshSettings()
	// {
	// 	dataGenerator?.GetSettings();
	// 	modelGenerator?.GetSettings();

	// }

	private DungeonGenSettings.DebugSettings GetDebugSettings()
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
			else if (fileName == HALLWAY_FLOOR_PREFAB)
			{
				debugSettings.HallwayFloorPrefab = item; // Set the floor prefab
			}
			else if (fileName == ROOM_FLOOR_PREFAB)
			{
				debugSettings.RoomFloorPrefab = item; // Set the room floor prefab
			}
			else if (fileName == ROOM_DOOR_FLOOR_PREFAB)
			{
				debugSettings.RoomDoorFloorPrefab = item; // Set the room door floor prefab
			}
			else if (fileName == ROOM_NWALL_PREFAB)
			{
				debugSettings.WallPrefab.NPrefab = item; // Set the room north wall prefab
			}
			else if (fileName == ROOM_SWALL_PREFAB)
			{
				debugSettings.WallPrefab.SPrefab = item; // Set the room south wall prefab
			}
			else if (fileName == ROOM_EWALL_PREFAB)
			{
				debugSettings.WallPrefab.EPrefab = item; // Set the room east wall prefab
			}
			else if (fileName == ROOM_WWALL_PREFAB)
			{
				debugSettings.WallPrefab.WPrefab = item; // Set the room west wall prefab
			}
			else if (fileName == ROOM_NDOOR_WALL_PREFAB)
			{
				debugSettings.DoorPrefab.NPrefab = item; // Set the room north door wall prefab
			}
			else if (fileName == ROOM_SDOOR_WALL_PREFAB)
			{
				debugSettings.DoorPrefab.SPrefab = item; // Set the room south door wall prefab
			}
			else if (fileName == ROOM_EDOOR_WALL_PREFAB)
			{
				debugSettings.DoorPrefab.EPrefab = item; // Set the room east door wall prefab
			}
			else if (fileName == ROOM_WDOOR_WALL_PREFAB)
			{
				debugSettings.DoorPrefab.WPrefab = item; // Set the room west door wall prefab
			}
		}

		// Return the populated debug settings containing all the necessary prefabs and materials
		return debugSettings;
	}

	public enum DebugDrawType
	{
		Grid,
		Pathfinding,
		Rooms,
		All
	}

}

#endif