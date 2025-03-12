using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace Linkoid.Repo.HostOnlyStart;

[HarmonyPatch(typeof(TruckScreenText))]
internal static class TruckScreenTextPatches
{
    [HarmonyTranspiler, HarmonyPatch(nameof(TruckScreenText.HoldChat))]
    private static IEnumerable<CodeInstruction> HoldChat_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var smeth_ProtonNetwork_get_IsMasterClient = AccessTools.PropertyGetter(typeof(PhotonNetwork), nameof(PhotonNetwork.IsMasterClient));
        var smeth_HostOnlyStartCheck = AccessTools.DeclaredMethod(typeof(TruckScreenTextPatches), nameof(HostOnlyStartCheck));

        var head = new CodeMatcher(instructions);

        // Match:
        //     if (!PhotonNetwork.IsMasterClient) return;
        CodeMatch[] notMasterClient_return =
        {
            new(OpCodes.Call, smeth_ProtonNetwork_get_IsMasterClient),
            new(x => x.opcode == OpCodes.Brfalse),
        };
        head.MatchForward(useEnd: true, notMasterClient_return);
        head.ThrowIfInvalid($"Could not match {nameof(notMasterClient_return)}");

        // Get return label
        HostOnlyStart.Logger.LogDebug($"head.Instruction.opcode: {head.Instruction.opcode.Value}");
        HostOnlyStart.Logger.LogDebug($"head.Instruction.operand: {head.Instruction.operand}");
        var returnLabel = head.Instruction.operand;

        head.Advance(1);

        // Insert:
        //     if (!HostOnlyStartCheck(this)) return;
        head.Insert(
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, smeth_HostOnlyStartCheck),
            new CodeInstruction(OpCodes.Brfalse, returnLabel)
        );

        return head.InstructionEnumeration();
    }

    private static bool HostOnlyStartCheck(TruckScreenText __instance)
    {
        HostOnlyStart.Logger.LogDebug($"HostOnlyStartCheck");
        if (RunManager.instance == null)
            return false;

        Level[] hostOnlyStartLevels =
        {
            RunManager.instance.levelArena,
            RunManager.instance.levelLobby,
            RunManager.instance.levelLobbyMenu,
            RunManager.instance.levelRecording,
            RunManager.instance.levelShop,
            RunManager.instance.levelMainMenu,
        };

        HostOnlyStart.Logger.LogDebug($"hostOnlyStartLevels.Contains(RunManager.instance.levelCurrent) = {hostOnlyStartLevels.Contains(RunManager.instance.levelCurrent)}");
        HostOnlyStart.Logger.LogDebug($"!__instance.staticGrabObject.playerGrabbing.Contains(PlayerAvatar.instance.physGrabber) = {!__instance.staticGrabObject.playerGrabbing.Contains(PlayerAvatar.instance.physGrabber)}");

        if (hostOnlyStartLevels.Contains(RunManager.instance.levelCurrent)
            && !__instance.staticGrabObject.playerGrabbing.Contains(PlayerAvatar.instance.physGrabber)
            && !PlayerAvatar.instance.deadSet)
            return false;

        return true;
    }
}