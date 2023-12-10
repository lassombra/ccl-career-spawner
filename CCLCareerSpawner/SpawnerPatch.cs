using CCLCareerSpawnerTypes;
using DV;
using DV.InventorySystem;
using DV.Localization;
using DV.ThingTypes;
using DV.UserManagement;
using DV.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CCLCareerSpawner
{
	[HarmonyPatch(typeof(CommsRadioCarSpawner))]
	internal static class SpawnerPatch
	{
		[HarmonyPatch(nameof(CommsRadioCarSpawner.UpdateCarLiveriesToSpawn))]
		[HarmonyPostfix]
		public static void updateCarLiveriesPatch(CommsRadioCarSpawner __instance)
		{
			bool isSandbox = SingletonBehaviour<UserManager>.Instance.CurrentUser.CurrentSession.GameMode.Equals("FreeRoam");
			if (!isSandbox)
			{
				var liveriesField = __instance.GetType().GetField("carLiveriesToSpawn", BindingFlags.NonPublic | BindingFlags.Instance);
				List<TrainCarLivery>? liveries = liveriesField.GetValue(__instance) as List<TrainCarLivery>;
				Main.Logger?.Log("Loaded liveries");
				if (null != liveries)
				{
					liveries = liveries.Where(livery => livery.prefab.GetComponentInChildren<CareerSpawnerCost>() != null).ToList();
					Main.Logger?.Log("Filtered liveries " + liveries.Count);
					liveriesField.SetValue(__instance, liveries);
					Main.Logger?.Log("Applied filtered liveries");
				}
			}
		}

		[HarmonyPatch("SetCarToSpawn")]
		[HarmonyPostfix]
		public static void SetCarToSpawnPatch(CommsRadioCarSpawner __instance)
		{
			if (!SingletonBehaviour<UserManager>.Instance.CurrentUser.CurrentSession.GameMode.Equals("FreeRoam"))
			{
				var carPrefabToSpawn = carPrefabToSpawnField.GetValue(__instance) as GameObject;
				var trainCar = carPrefabToSpawn?.GetComponent<TrainCar>();
				var livery = trainCar?.carLivery;
				if (null != livery && null != carPrefabToSpawn && null != trainCar)
				{
					var spawnerCost = livery.prefab.GetComponentInChildren<CareerSpawnerCost>();
					var price = Mathf.Min(spawnerCost.spawnCost, Globals.G.GameParams.WorkTrainSummonMaxPrice);
					var content = LocalizationAPI.L(livery.localizationKey, Array.Empty<string>()) + "\n" +
						trainCar.InterCouplerDistance.ToString("F") + "m\n" +
						"$" + price.ToString("F");
					__instance.display.SetContent(content);
				}
			}
		}

		[HarmonyPatch(nameof(CommsRadioCarSpawner.OnUpdate))]
		[HarmonyPostfix]
		public static void UpdatePostfix(CommsRadioCarSpawner __instance)
		{
			if (!SingletonBehaviour<UserManager>.Instance.CurrentUser.CurrentSession.GameMode.Equals("FreeRoam") && !Enum.Equals(stateField.GetValue(__instance), enterSpawnMode))
			{
				var carPrefabToSpawn = carPrefabToSpawnField.GetValue(__instance) as GameObject;
				var spawnerCost = carPrefabToSpawn?.GetComponentInChildren<CareerSpawnerCost>();
				var spawnPrice = spawnerCost?.spawnCost ?? 0;
				var price = Mathf.Min(spawnPrice, Globals.G.GameParams.WorkTrainSummonMaxPrice);
				if (price > SingletonBehaviour<Inventory>.Instance.PlayerMoney)
				{
					canSpawnAtPointField.SetValue(__instance, false);
					destinationTrackField.SetValue(__instance, null);
					CarDestinationHighlighter? destHighlighter = destHighlighterField.GetValue(__instance) as CarDestinationHighlighter;
					var trainCar = carPrefabToSpawnField?.GetValue(__instance) as GameObject;
					if (destHighlighter != null && trainCar != null)
					{
						destHighlighter.Highlight(
						__instance.signalOrigin.position + __instance.signalOrigin.forward * 20f,
						__instance.signalOrigin.right,
						trainCar.GetComponentInChildren<TrainCar>().Bounds,
						__instance.invalidMaterial);
					}
					__instance.display.SetAction(CommsRadioLocalization.CANCEL);
					__instance.lcdArrow.TurnOff();
				}
			}
		}
		private static readonly FieldInfo carPrefabToSpawnField = typeof(CommsRadioCarSpawner).GetField("carPrefabToSpawn", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo canSpawnAtPointField = typeof(CommsRadioCarSpawner).GetField("canSpawnAtPoint", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo destinationTrackField = typeof(CommsRadioCarSpawner).GetField("destinationTrack", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo destHighlighterField = typeof(CommsRadioCarSpawner).GetField("destHighlighter", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo stateField = typeof(CommsRadioCarSpawner).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly object enterSpawnMode = typeof(DV.CommsRadioCarSpawner).GetNestedType("State", BindingFlags.NonPublic).GetField("EnterSpawnMode").GetValue(null);
	}
}
