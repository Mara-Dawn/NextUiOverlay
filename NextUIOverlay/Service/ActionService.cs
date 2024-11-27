namespace NextUIOverlay.Service;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

public static class ActionService
{
    public static string GetActionNameFromBattleChara(IBattleChara battleChara)
    {
        var target = battleChara.TargetObject;
        var targetKind = target?.ObjectKind;

        switch (targetKind)
        {
            case null:
                break;
            case ObjectKind.Aetheryte:
                return "Attuning...";
            case ObjectKind.EventObj:
            case ObjectKind.EventNpc:
                return "Interacting...";
        }

        if (battleChara.CastActionId == 1 && battleChara.CastActionType != (byte)ActionType.Mount)
        {
            return "Interacting...";
        }

        if (battleChara.CastTargetObjectId == 0xE0000000)
        {
            return "Interacting...";
        }

        switch ((ActionType)battleChara.CastActionType)
        {
            case ActionType.PetAction:
            case ActionType.Action:
            case ActionType.BuddyAction:
            // case ActionType.Spell:
            // case ActionType.SquadronAction:
            case ActionType.PvPAction:
            case ActionType.CraftAction:
            case ActionType.Ability:
                var action = NextUIPlugin.dataManager.GetExcelSheet<Action>()?.GetRow(battleChara.CastActionId);
                return action?.Name.ToString() ?? "";
            case ActionType.Mount:
                var mount = NextUIPlugin.dataManager.GetExcelSheet<Mount>()?.GetRow(battleChara.CastActionId);
                return mount?.Singular.ToString() ?? "";
            case ActionType.KeyItem:
            case ActionType.Item:
                var item = NextUIPlugin.dataManager.GetExcelSheet<Item>()?.GetRow(battleChara.CastActionId);
                return item?.Name.ToString() ?? "Using item...";

            case ActionType.Companion:
                var companion =
                    NextUIPlugin.dataManager.GetExcelSheet<Companion>()?.GetRow(battleChara.CastActionId);
                return companion?.Singular.ToString() ?? "";
            case ActionType.None:
            case ActionType.Ornament:
            case ActionType.BgcArmyAction:
            case ActionType.GeneralAction:
            case ActionType.MainCommand:
            case ActionType.FieldMarker:
            case ActionType.ChocoboRaceAbility:
            case ActionType.ChocoboRaceItem:
            case ActionType.Unk_10:
            case ActionType.Unk_12:
            case ActionType.Unk_18:
            default:
                return "Casting...";
        }
    }
}