using CCLCareerSpawnerTypes;
using DV;
using DV.InventorySystem;
using DV.UserManagement;
using DV.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CCLCareerSpawner
{
	[HarmonyPatch(typeof(CarSpawner))]
	internal static class CarSpawnerPatch
	{
		[HarmonyPatch(nameof(CarSpawner.SpawnCar))]
		[HarmonyPrefix]
		public static bool SpawnCar(GameObject carToSpawn, RailTrack track, Vector3 position, Vector3 forward, ref bool playerSpawnedCar, ref TrainCar __result)
		{
			if (SingletonBehaviour<UserManager>.Instance.CurrentUser.CurrentSession.GameMode.Equals("FreeRoam") || !playerSpawnedCar)
			{
				return true;
			}
			var spawnPrice = Mathf.Min(
				carToSpawn.GetComponentInChildren<CareerSpawnerCost>()?.spawnCost ?? 0,
				Globals.G.GameParams.WorkTrainSummonMaxPrice
				);
			if ( spawnPrice > SingletonBehaviour<Inventory>.Instance.PlayerMoney) {
				__result = null;
				return false;
			}
			if (spawnPrice > 0)
			{
				SingletonBehaviour<Inventory>.Instance.RemoveMoney( spawnPrice );
				playerSpawnedCar = false;
			}
			return true;
		}
	}
}
