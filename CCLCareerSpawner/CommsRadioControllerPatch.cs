using DV;
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
	[HarmonyPatch(typeof(CommsRadioController))]
	internal static class CommsRadioControllerPatch
	{
		[HarmonyPatch(nameof(CommsRadioController.UpdateModesAvailability))]
		[HarmonyPostfix]
		public static void UpdateModesAvailabilityPatch(CommsRadioController __instance)
		{
			bool isSandbox = SingletonBehaviour<UserManager>.Instance.CurrentUser.CurrentSession.GameMode.Equals("FreeRoam");
			if (!isSandbox)
			{
				Main.Controllers.Add(__instance);
				Main.Logger?.Log("Not sandbox, instance: " + __instance);
				var ourType = __instance.GetType();
				Main.Logger?.Log("Type loaded " + ourType);
				var ourField = ourType.GetField("allModes", BindingFlags.NonPublic | BindingFlags.Instance);
				Main.Logger?.Log("FiendLoaded " + ourField);
				var allModes = ourField.GetValue(__instance) as List<ICommsRadioMode>;
				Main.Logger?.Log("Loaded all modes");
				var disabledModeIndices = __instance.GetType().GetField("disabledModeIndices", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as HashSet<int>;
				Main.Logger?.Log("Loaded disabled mode indices");
				if (allModes != null && disabledModeIndices != null)
				{
					Main.Logger?.Log("Found all modes");
					var index = allModes.IndexOf(__instance.carSpawnerControl);
					Main.Logger?.Log("Found carSpawnerControlMode " + index);
					disabledModeIndices.Remove(index);
				}
		}
		}
	}
}
