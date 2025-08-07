using System.Reflection;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using HarmonyLib;

namespace ExplorationActivities;

public class ModLoader
{
   [DawnsburyDaysModMainMethod]
    public static void LoadMod()
    {
        var harmony = new Harmony("explorationactivities");
        harmony.PatchAll();
        Type skills = typeof(Skills);
        var myObject = new Skills();
        FieldInfo? field = skills.GetField("relevantAbility", BindingFlags.Static | BindingFlags.NonPublic);
        if (field != null)
        {
            var dict = field.GetValue(myObject) as IDictionary<Skill, Ability>;
            if (dict == null)
            {
                dict = new Dictionary<Skill, Ability>();
                field.SetValue(myObject, dict);
            }
            dict[ModData.Skills.WarfareLore] = Ability.Intelligence;
        }
        foreach (Feat feat in ExplorationActivities.ExplorationFeats())
        {
            ModManager.AddFeat(feat);
        }
        ModManager.RegisterActionOnEachCharacterSheet(sheet =>
        {
            sheet.Calculated.AddAtLevel(1, values =>
            {
                values.AddSelectionOption(new SingleFeatSelectionOption("ExplorationSelections",
                    "Exploration Activity", SelectionOption.PRECOMBAT_PREPARATIONS_LEVEL,
                    feat => feat.HasTrait(ModData.Traits.ExplorationActivity)).WithIsOptional());
            });
        });
        ExplorationSpells.RegisterSpells();
        ModManager.RegisterActionOnEachCreature(cr =>
        {
            if (cr.HasFeat(ModData.FeatNames.WarfareLore))
                cr.AddQEffect(new QEffect()
                {
                    YouBeginAction = (effect, action) =>
                    {
                        Creature self = effect.Owner;
                        Creature? target = action.ChosenTargets.ChosenCreature;
                        var warfareMinusSociety = self.Skills.Get(ModData.Skills.WarfareLore) - self.Skills.Get(Skill.Society);
                        if (target != null && action is { Name: "Recall Weakness" } && warfareMinusSociety > 0
                            && (target.Traits.Contains(Trait.Human) || target.Traits.Contains(Trait.Humanoid) || target.Traits.Contains(Trait.Orc) || target.Traits.Contains(Trait.Kobold) || target.Traits.Contains(Trait.Merfolk)))
                            action.WithActiveRollSpecification(new ActiveRollSpecification(
                                TaggedChecks.SkillCheck(ModData.Skills.WarfareLore),
                                Checks.FlatDC(Checks.LevelBasedDC(target.Level))));
                        return Task.CompletedTask;
                    }
                });
        });
        if (ModManager.TryParse("Fount of Knowledge", out FeatName fountOfKnowledge))
        {
            ModManager.RegisterActionOnEachCreature(cr =>
            {
                if (cr.HasFeat(fountOfKnowledge))
                    cr.AddQEffect(new QEffect()
                    {
                        BonusToSkills = skill => skill == ModData.Skills.WarfareLore ? new Bonus(1, BonusType.Status, "Fount of Knowledge") : null
                    });
            });
        }
    }
}