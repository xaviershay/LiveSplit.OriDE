﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
namespace LiveSplit.OriDE.Memory {
	public partial class OriMemory {
		private ProgramPointer gameWorld, gameplayCamera, worldEvents, seinCharacter, scenesManager, gameController, gameStateMachine;
		private float[] junkX = { 992.36f, 725.25f, 540.18f, 1513.69f, 1082.25f, 422.82f, 1494.81f, 692.56f, 1559.98f };
		private float[] junkY = { 6039.27f, 6288.26f, 6407.79f, 6441.79f, 6484.88f, 6676.19f, 6999.13f, 7093.40f, 7325.27f };
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;

		public OriMemory() {
			gameWorld = new ProgramPointer(this, "GameWorld") { IsStatic = true };
			gameplayCamera = new ProgramPointer(this, "GameplayCamera") { IsStatic = true };
			worldEvents = new ProgramPointer(this, "WorldEvents") { IsStatic = false };
			seinCharacter = new ProgramPointer(this, "SeinCharacter") { IsStatic = false };
			scenesManager = new ProgramPointer(this, "ScenesManager") { IsStatic = true };
			gameController = new ProgramPointer(this, "GameController") { IsStatic = true };
			gameStateMachine = new ProgramPointer(this, "GameStateMachine") { IsStatic = true };
		}

		public PointF GetCameraTargetPosition() {
			if (!IsHooked) { return new PointF(0, 0); }

			float px = gameplayCamera.Read<float>(0x14, 0x10);
			float py = gameplayCamera.Read<float>(0x14, 0x14);
			return new PointF(px, py);
		}
		public Dictionary<string, bool> GetEvents() {
			IntPtr start = worldEvents.Value + 0x40;

			Dictionary<string, bool> results = new Dictionary<string, bool>();
			foreach (var pair in events) {
				results[pair.Key] = Program.Read<bool>(start + pair.Value);
			}
			return results;
		}
		public Dictionary<string, bool> GetKeys() {
			IntPtr start = worldEvents.Value;

			Dictionary<string, bool> results = new Dictionary<string, bool>();
			foreach (var pair in keys) {
				results[pair.Key] = Program.Read<bool>(start + pair.Value);
			}
			return results;
		}
		public Dictionary<string, bool> GetAbilities() {
			IntPtr start = seinCharacter.Read<IntPtr>(0x00, 0x4c);

			Dictionary<string, bool> results = new Dictionary<string, bool>();
			foreach (var pair in abilities) {
				results[pair.Key] = Program.Read<bool>(start, pair.Value * 4, 0x08);
			}
			return results;
		}
		public Area[] GetMapCompletion() {
			IntPtr current = gameWorld.Read<IntPtr>(0x1c);
			Area currentArea = GetArea(current);
			IntPtr listHead = gameWorld.Read<IntPtr>(0x18, 0x08);
			int listSize = gameWorld.Read<int>(0x18, 0x0c);

			List<Area> areas = new List<Area>();
			for (var i = 0; i < listSize; i++) {
				IntPtr gameWorldAreaHead = Program.Read<IntPtr>(listHead, 0x10 + (i * 4));

				Area area = GetArea(gameWorldAreaHead);
				if (area.name.Equals(currentArea.name, StringComparison.OrdinalIgnoreCase)) {
					area.current = true;
				}
				areas.Add(area);
			}

			return areas.ToArray();
		}
		public decimal GetTotalMapCompletion() {
			IntPtr listHead = gameWorld.Read<IntPtr>(0x18, 0x08);
			int listSize = gameWorld.Read<int>(0x18, 0x0c);

			decimal total = 0;
			for (var i = 0; i < listSize; i++) {
				IntPtr gameWorldAreaHead = Program.Read<IntPtr>(listHead, 0x10 + (i * 4));
				Area area = GetArea(gameWorldAreaHead);
				total += area.progress;
			}

			return total;
		}
		private Area GetArea(IntPtr areaAddress) {
			float completionAmount = Program.Read<float>(areaAddress, 0x14);
			string areaName = Program.GetString(Program.Read<IntPtr>(areaAddress, 0x08, 0x1c));
			if (areaName.IndexOf("Mangrove", StringComparison.OrdinalIgnoreCase) >= 0) {
				areaName = "Black Root";
			}

			Area area = new Area();
			area.name = areaName;
			area.progress = Math.Round((decimal)completionAmount * 100, 2, MidpointRounding.AwayFromZero);
			area.current = false;
			return area;
		}
		public Scene[] GetScenes() {
			IntPtr activeScenesHead = scenesManager.Read<IntPtr>(0x14);
			int listSize = Program.Read<int>(activeScenesHead, 0x0C);

			List<Scene> scenes = new List<Scene>();
			for (var i = 0; i < listSize; i++) {
				IntPtr sceneManagerHead = Program.Read<IntPtr>(activeScenesHead, 0x08, 0x10 + (i * 4));
				IntPtr runtimeSceneHead = Program.Read<IntPtr>(sceneManagerHead, 0x0c, 0x08);

				Scene scene = new Scene();
				scene.name = Program.GetString(runtimeSceneHead);
				scene.hasStartBeenCalled = Program.Read<bool>(sceneManagerHead, 0x10);
				scene.state = (SceneState)Program.Read<int>(sceneManagerHead, 0x14);
				scenes.Add(scene);
			}

			return scenes.ToArray();
		}
		public bool IsEnteringGame() {
			return gameController.Read<bool>(0x6a) || gameController.Read<bool>(0x6b);
		}
		public GameState GetGameState() {
			return (GameState)gameStateMachine.Read<int>(0x14);
		}
		public int GetKeyStones() {
			return seinCharacter.Read<int>(0x00, 0x2c, 0x1c);
		}
		public int GetMapStones() {
			return seinCharacter.Read<int>(0x00, 0x2c, 0x20);
		}
		public int GetAbilityCells() {
			return seinCharacter.Read<int>(0x00, 0x2c, 0x24);
		}
		public int GetSkillPointsAvailable() {
			return seinCharacter.Read<int>(0x00, 0x38, 0x24);
		}
		public int GetCurrentLevel() {
			return seinCharacter.Read<int>(0x00, 0x38, 0x28);
		}
		public int GetExperience() {
			return seinCharacter.Read<int>(0x00, 0x38, 0x2c);
		}
		public float GetCurrentHP() {
			return seinCharacter.Read<float>(0x00, 0x40, 0x0c, 0x1c);
		}
		public int GetCurrentHPMax() {
			return seinCharacter.Read<int>(0x00, 0x40, 0x0c, 0x20);
		}
		public float GetCurrentEN() {
			return seinCharacter.Read<float>(0x00, 0x3c, 0x20);
		}
		public float GetCurrentENMax() {
			return seinCharacter.Read<float>(0x00, 0x3c, 0x24);
		}

		public bool HookProcess() {
			if (Program == null || Program.HasExited) {
				Process[] processes = Process.GetProcessesByName("OriDE");
				Program = processes.Length == 0 ? null : processes[0];
				if (processes.Length == 0 || Program.HasExited) {
					IsHooked = false;
					return IsHooked;
				}

				IsHooked = true;
			}

			return IsHooked;
		}
		public void Dispose() {
			if (Program != null) { this.Program.Dispose(); }
		}

		public static Dictionary<string, int> keys = new Dictionary<string, int>()
		{
			{"Water Vein",   0},
			{"Gumon Seal",   1},
			{"Sunstone",     2},
		};
		public static Dictionary<string, int> events = new Dictionary<string, int>()
		{
			{"Ginso Tree Entered",   0},
			{"Mist Lifted",          1},
			{"Clean Water",          2},
			{"Wind Restored",        3},
			{"Gumo Free",            4},
			{"Spirit Tree Reached",  5},
			{"Warmth Returned",      6},
			{"Darkness Lifted",      7}
		};
		public static Dictionary<string, int> abilities = new Dictionary<string, int>()
		{
			{"Bash",                     5},
			{"Charge Flame",             6},
			{"Wall Jump",                7},
			{"Stomp",                    8},
			{"Double Jump",              9},
			{"Charge Jump",              10},
			{"Magnet",                   11},
			{"Ultra Magnet",             12},
			{"Climb",                    13},
			{"Glide",                    14},
			{"Spirit Flame",             15},
			{"Rapid Fire",               16},
			{"Soul Efficiency",          17},
			{"Water Breath",             18},
			{"Charge Flame Blast",       19},
			{"Charge Flame Burn",        20},
			{"Double Jump Upgrade",      21},
			{"Bash Upgrade",             22},
			{"Ultra Defense",            23},
			{"Health Efficiency",        24},
			{"Sense",                    25},
			{"Stomp Upgrade",            26},
			{"Quick Flame",              27},
			{"Map Markers",              28},
			{"Energy Efficiency",        29},
			{"Health Markers",           30},
			{"Energy Markers",           31},
			{"Ability Markers",          32},
			{"Rekindle",                 33},
			{"Regroup",                  34},
			{"Charge Flame Efficiency",  35},
			{"Ultra Soul Flame",         36},
			{"Soul Flame Efficiency",    37},
			{"Split Flame",              38},
			{"Spark Flame",              39},
			{"Cinder Flame",             40},
			{"Ultra Split Flame",        41},
			{"Grenade",                  42},
			{"Dash",                     43},
			{"Grenade Upgrade",          44},
			{"Charge Dash",              45},
			{"Air Dash",                 46},
			{"Grenade Efficiency",       47}
		};
		public static Dictionary<string, int> skills = new Dictionary<string, int>()
		{
			{"Bash",                     5},
			{"Charge Flame",             6},
			{"Wall Jump",                7},
			{"Stomp",                    8},
			{"Double Jump",              9},
			{"Charge Jump",              10},
			{"Climb",                    13},
			{"Glide",                    14},
			{"Grenade",                  41},
			{"Dash",                     42}
		};
	}
	public class ProgramPointer {
		private static string[] versions = new string[1] { "v1.0" };
		private static Dictionary<string, Dictionary<string, string>> funcPatterns = new Dictionary<string, Dictionary<string, string>>() {
			{"v1.0", new Dictionary<string, string>() {
					{"GameController",        "83EC78C745B000000000C745B400000000C745B800000000C745BC00000000C745C000000000C745AC00000000C745A8000000008B05????????83EC086A0050|-10"},
					{"ScenesManager",         "558BEC5783EC148B7D08B8????????893883EC0C57E8????????83C4108B05????????8B40208B40308945EC85FF0F84????????83EC0C68????????E8????????83C4108BC88B45EC897910|-65"},
					{"GameStateMachine",      "558BEC5783EC048B7D08B8????????8938E8????????83EC0868????????50E8????????83C41085C0740CC7471400000000E94D000000E8????????83EC0868????????50E8????????|-63"},
					{"WorldEvents",           "558BEC83EC08B8????????C60000B8????????C60000B8????????C60000B8????????C60000B8????????C60000B8????????C60000B8????????C60000B8????????C6000083EC0C6A00E8????????83C410B8????????C60000B8????????C60000B8????????C60000C9C3|-94"},
					{"SeinCharacter",         "558BEC5783EC048B7D08B8????????8938B8????????893883EC0C68????????E8????????83C41083EC08578945F850E8????????83C4108B45F889473483EC0C57E8????????83C41083EC085057E8????????83C4108D65FC5FC9C3|-82"},
					{"GameplayCamera",        "05480000008B08894DE88B4804894DEC8B40088945F08B05|-28"},
					{"GameWorld",             "558BEC53575683EC0C8B7D08B8????????89388B47|-8"},
			}},
		};
		private IntPtr pointer;
		public OriMemory Memory { get; set; }
		public string Name { get; set; }
		public bool IsStatic { get; set; }
		private int lastID;
		private DateTime lastTry;
		public ProgramPointer(OriMemory memory, string name) {
			this.Memory = memory;
			this.Name = name;
			this.IsStatic = true;
			lastID = memory.Program == null ? -1 : memory.Program.Id;
			lastTry = DateTime.MinValue;
		}

		public IntPtr Value {
			get {
				if (!Memory.IsHooked) {
					pointer = IntPtr.Zero;
				} else {
					GetPointer(ref pointer, Name);
				}
				return pointer;
			}
		}
		public T Read<T>(params int[] offsets) {
			if (!Memory.IsHooked) { return default(T); }
			return Memory.Program.Read<T>(Value, offsets);
		}
		public string ReadString(params int[] offsets) {
			if (!Memory.IsHooked) { return string.Empty; }
			IntPtr p = Memory.Program.Read<IntPtr>(Value, offsets);
			return Memory.Program.GetString(p);
		}
		private void GetPointer(ref IntPtr ptr, string name) {
			if (Memory.IsHooked) {
				if (Memory.Program.Id != lastID) {
					ptr = IntPtr.Zero;
					lastID = Memory.Program.Id;
				}
				if (ptr == IntPtr.Zero && DateTime.Now > lastTry.AddSeconds(1)) {
					lastTry = DateTime.Now;
					ptr = GetVersionedFunctionPointer(name);
					if (ptr != IntPtr.Zero) {
						if (IsStatic) {
							ptr = Memory.Program.Read<IntPtr>(ptr, 0, 0);
						} else {
							ptr = Memory.Program.Read<IntPtr>(ptr, 0);
						}
					}
				}
			}
		}
		public IntPtr GetVersionedFunctionPointer(string name) {
			foreach (string version in versions) {
				if (funcPatterns[version].ContainsKey(name)) {
					return Memory.Program.FindSignatures(funcPatterns[version][name])[0];
				}
			}
			return IntPtr.Zero;
		}
	}
}