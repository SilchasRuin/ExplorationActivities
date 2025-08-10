using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics.Enumerations;
using HarmonyLib;

namespace ExplorationActivities;
[HarmonyPatch(typeof(Skills), nameof(Skills.GetSkillDescription))]
internal static class PatchSkillDescription
{
    internal static bool Prefix(Skill skill, ref string __result)
    {
        if (skill != ModData.Skills.WarfareLore) return true;
        __result = "You have studied battlefields, tactics, and strategy.\n\nThis skill is only used in conjunction with the {b}Battle Planner{/b} skill feat and {i}DawnniEx's{/i} {b}Recall Weakness{/b} action.";
        return false;
    }
}
[HarmonyPatch(typeof(Skills), nameof(Skills.SkillToTrait))]
internal static class PatchSkillToTrait
{
    internal static bool Prefix(Skill skill, ref Trait __result)
    {
        if (skill != ModData.Skills.WarfareLore) return true;
        __result = ModData.Traits.WarfareLore;
        return false;
    }
}
[HarmonyPatch(typeof(Skills), nameof(Skills.TraitToSkill))]
internal static class PatchTraitToSkill
{
    internal static bool Prefix(Trait skill, ref Skill? __result)
    {
        if (skill != ModData.Traits.WarfareLore) return true;
        __result = new Skill?(ModData.Skills.WarfareLore);
        return false;
    }
}
[HarmonyPatch(typeof(Skills))]
[HarmonyPatch(nameof(Skills.SkillToFeat), typeof(Skill))]
internal static class PatchSkillToFeat
{
    internal static bool Prefix(Skill skill, ref FeatName __result)
    {
        if (skill != ModData.Skills.WarfareLore) return true;
        __result = ModData.FeatNames.WarfareLore;
        return false;
    }
}
[HarmonyPatch(typeof(Skills))]
[HarmonyPatch(nameof(Skills.SkillToFeat), typeof(Skill), typeof(Proficiency))]
internal static class PatchSkillToFeat2
{
    internal static bool Prefix(Skill skill, Proficiency proficiency, ref FeatName __result)
    {
        if (skill != ModData.Skills.WarfareLore) return true;
        __result = proficiency switch
        {
            Proficiency.Trained => ModData.FeatNames.WarfareLore,
            Proficiency.Expert => ModData.FeatNames.WarfareLoreExpert,
            Proficiency.Master => ModData.FeatNames.WarfareLoreMaster,
            Proficiency.Legendary => ModData.FeatNames.WarfareLoreLegendary,
            _ => throw new ArgumentOutOfRangeException(nameof(proficiency), proficiency, null)
        };
        return false;
    }
}
