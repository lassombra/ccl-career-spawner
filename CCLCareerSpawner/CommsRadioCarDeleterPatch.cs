using CCLCareerSpawnerTypes;
using DV;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CCLCareerSpawner
{
	[HarmonyPatch(typeof(CommsRadioCarDeleter))]
	internal class CommsRadioCarDeleterPatch
	{
		[HarmonyPatch(nameof(CommsRadioCarDeleter.OnUse))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> OnUseTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Stfld && instruction.operand.Equals(removePrice))
				{
					Main.Logger?.Log("instruction found " + instruction);
					var start = new CodeInstruction(OpCodes.Ldarg_0);
					start = start.MoveLabelsFrom(instruction);
					Main.Logger?.Log("instruction " + start);
					yield return start;
					yield return new CodeInstruction(OpCodes.Ldfld, carToDelete);
					yield return new CodeInstruction(OpCodes.Call, typeof(CommsRadioCarDeleterPatch).GetMethod("ClearCost"));
					yield return new CodeInstruction(OpCodes.Stfld, removePrice);
				}
				else
				{
					yield return instruction;
				}
			}
		}
		public static float ClearCost(float calculatedPrice, TrainCar car)
		{
			Main.Logger?.Log("Possibly overriding price " + calculatedPrice);
			Main.Logger?.Log("For car " + car.carLivery.name);
			var careerSpawnCost = car.carLivery.prefab.GetComponentInChildren<CareerSpawnerCost>()?.clearCost ?? calculatedPrice;
			float overridePrice = Mathf.RoundToInt(Mathf.Min(careerSpawnCost, Globals.G.GameParams.DeleteCarMaxPrice));
			Main.Logger?.Log("Calculated price " + overridePrice);
			return car.playerSpawnedCar ? 0f : overridePrice;
		}
		static readonly FieldInfo removePrice = typeof(CommsRadioCarDeleter).GetField("removePrice", BindingFlags.Instance | BindingFlags.NonPublic);
		static readonly FieldInfo carToDelete = typeof(CommsRadioCarDeleter).GetField("carToDelete", BindingFlags.Instance | BindingFlags.NonPublic);
	}
}
