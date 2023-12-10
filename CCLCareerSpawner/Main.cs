using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using CCLCareerSpawnerTypes;
using DV;
using System.Collections.Generic;
using System.Linq;

namespace CCLCareerSpawner;

public static class Main
{
	public static UnityModManager.ModEntry.ModLogger? Logger { get; private set; }
	public static List<CommsRadioController> Controllers { get; set; } = new List<CommsRadioController>();

	// Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
	private static bool Load(UnityModManager.ModEntry modEntry)
	{
		// init type to ensure types module gets loaded
		if (!LoadConfirmer.isLoaded)
		{
			return false;
		}
		Harmony? harmony = null;
		Logger = modEntry.Logger;
		try
		{
			
			harmony = new Harmony(modEntry.Info.Id);
			Harmony.DEBUG = true;
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			// Other plugin startup logic
		}
		catch (Exception ex)
		{
			modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
			harmony?.UnpatchAll(modEntry.Info.Id);
			return false;
		}
		WorldStreamingInit.LoadingFinished += Start;
		if (WorldStreamingInit.Instance && WorldStreamingInit.IsLoaded)
		{
			Start();
		}
		return true;
	}
	private static void Start()
	{
		//foreach (var item in Globals.G.Types.Liveries.Where(c => c.prefab != null && c.prefab.name.Contains("S282C")).Select(c => c.prefab))
		//{
		//	Logger?.Log("Attached a spawn cost");
		//	var cost = item.AddComponent<CareerSpawnerCost>();
		//	cost.spawnCost = 10000;
		//}
		Controllers?.ForEach(control =>
		{
			control.carSpawnerControl.UpdateCarLiveriesToSpawn(true);
		});
	}
}
