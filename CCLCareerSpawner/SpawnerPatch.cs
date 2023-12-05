using CCLCareerSpawnerTypes;
using DV;
using DV.ThingTypes;
using DV.UserManagement;
using DV.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
	}
}
