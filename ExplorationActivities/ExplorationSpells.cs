using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Modding;
using Microsoft.Xna.Framework;

namespace ExplorationActivities;

public abstract class ExplorationSpells
{
    public static SpellId MusicalAccompaniment { get; private set; }
    public static SpellId GlassShield { get; private set; }
    public static void RegisterSpells()
    {
        MusicalAccompaniment = ModManager.RegisterNewSpell("MusicalAccompaniment", 0, (_, _, _, _, _) =>
        {
            return Spells.CreateModern(IllustrationName.CounterPerformance, "Musical Accompaniment", [Trait.Auditory, Trait.Cantrip, Trait.Illusion, Trait.Manipulate, Trait.Concentrate, Trait.Occult, Trait.Arcane], 
                    "You're surrounded by orchestral music that shifts and changes to match your behavior.", "You gain a +1 status bonus to Performance checks. You take a –4 penalty to Stealth checks while the music is playing. You can Dismiss this spell.",
                    Target.Self().WithAdditionalRestriction(cr => cr.HasEffect(ModData.QEffectIds.MusicalAccompanimentQf) ? "You're already under the effect of this spell." : null), 1, null)
                .WithActionCost(2)
                .WithSoundEffect(SfxName.Choir)
                .WithEffectOnSelf(cr =>
                {
                    cr.AddQEffect(new QEffect()
                    {
                        Dismissable = true,
                        ProvideActionIntoPossibilitySection = (Func<QEffect, PossibilitySection, Possibility?>) ((effect, section) => section.PossibilitySectionId == PossibilitySectionId.OtherManeuvers ? new ActionPossibility(new CombatAction(effect.Owner, IllustrationName.CounterPerformance, "Dismiss Musical Accompaniment", [], "Dismiss Musical Accompaniment and end its effects.", Target.Self()).WithActionCost(1).WithSoundEffect(SfxName.AuraDismissal).WithEffectOnEachTarget((_, _, _, _) => Task.FromResult(effect.ExpiresAt = ExpirationCondition.Immediately))) : null),
                        BonusToSkillChecks = (skill, _, _) =>
                        {
                            return skill switch
                            {
                                Skill.Performance => new Bonus(1, BonusType.Status, "Musical Accompaniment"),
                                Skill.Stealth => new Bonus(-4, BonusType.Untyped, "Musical Accompaniment"),
                                _ => null
                            };
                        },
                        Illustration = IllustrationName.CounterPerformance,
                        Name = "Musical Accompaniment",
                        Description = "You gain a +1 status bonus to Performance checks. You take a –4 penalty to Stealth checks while the music is playing.",
                        Id = ModData.QEffectIds.MusicalAccompanimentQf
                    });
                });
        });
        GlassShield = ModManager.RegisterNewSpell("GlassShield", 0, (_, _, level, inCombat, _) =>
        {
            var shieldHardness = level < 5 ? (level + 1) / 2 * 2 : level < 7 ? 7 : (level + 1) / 2 * 2 + 2;
            var str6 = shieldHardness.ToString();
            var diceFormula = level < 3 ? "1d4" : level < 5 ? "3d4" : level < 7 ? "4d4" : level < 9 ? "5d4" : "6d4";
            Target target1 = Target.Self((Func<Creature, AI, float>)((_, ai) => ai.GainBonusToAC(2)))
                .WithAdditionalRestriction(cr =>
                    !cr.HasEffect(ModData.QEffectIds.GlassShieldEffect)
                        ? null
                        : "You're already under the effect of this spell.");
            var description3 =
                $"You gain a +1 circumstance bonus to AC until the start of your next turn.\n\nWhile the spell is in effect, you can use the Shield Block reaction with your magic shield to prevent {str6} damage.\n\nWhen you Shield Block, the shield explodes in a shower of glass. If the creature that broke it is within 5 feet, the shards deal {diceFormula} piercing damage to that creature with a basic Reflex save. After you use Shield Block, the spell ends and you can't cast it again this encounter.";
            return Spells
                .CreateModern(  IllustrationName.SunderShield, "Glass Shield",
                    [Trait.Cantrip, Trait.Concentrate, Trait.Arcane, Trait.Primal, Trait.Earth],
                    "You summon a layer of clear glass to keep you from harm.", description3, target1, level, null)
                .WithActionCost(1).WithSoundEffect(SfxName.ShieldSpell)
                .WithGoodness((_, _, _) => 2f).WithEffectOnEachTarget((spell, caster, _, _) =>
                {
                    string qEffectDescription =
                            "A magical shield of glass grants you a +1 circumstance bonus to AC and grants the Shield Block reaction.";
                        QEffect qEffect = new("Shield", qEffectDescription,
                            ExpirationCondition.ExpiresAtStartOfSourcesTurn, caster, IllustrationName.SunderShield)
                        {
                            CountsAsABuff = true,
                            CountsAsBeneficialToSource = true,
                            Id = ModData.QEffectIds.GlassShieldEffect,
                            CannotExpireThisTurn = true,
                            DoNotShowUpOverhead = true,
                            Tag = false,
                            BonusToDefenses =
                                (Func<QEffect, CombatAction?, Defense, Bonus?>)((qEffect, attack, defense) =>
                                {
                                    if ((bool)qEffect.Tag!)
                                        return null;
                                    return defense != Defense.AC &&
                                           (!defense.IsSavingThrow() || attack == null || !attack.HasTrait(Trait.Spell))
                                        ? null
                                        : new Bonus(1, BonusType.Circumstance, "glass shield");
                                }),
                            YouAreDealtDamage =
                                (Func<QEffect, Creature, DamageStuff, Creature, Task<DamageModification?>>)(async (
                                    qEffect, attacker, damageStuff, you) =>
                                {
                                    if (!damageStuff.Kind.IsPhysical() || damageStuff.Power == null ||
                                         !damageStuff.Power.HasTrait(Trait.Attack))
                                        return null;
                                    int preventHowMuch = Math.Min(shieldHardness, damageStuff.Amount);
                                    if (!await you.Battle.AskToUseReaction(caster,
                                            string.Concat("You would be dealt damage by ", damageStuff.Power?.Name,
                                                $".\nUse your glass shield to resist {preventHowMuch} damage?",
                                                " {i}(You won't be able  to cast Glass Shield again for the rest of the encounter.){/i}")))
                                        return null;
                                    if (attacker.DistanceTo(caster) <= 1)
                                    {
                                        CheckResult checkResult = CommonSpellEffects.RollSpellSavingThrow(attacker, spell, Defense.Reflex);
                                        await CommonSpellEffects.DealBasicDamage(spell, caster, attacker, checkResult, DiceFormula.FromText(diceFormula), DamageKind.Piercing);
                                    }
                                    spell.SpellcastingSource?.Cantrips.RemoveAll(
                                        sp => sp.SpellId == GlassShield);
                                    qEffect.ExpiresAt = ExpirationCondition.Immediately;
                                    caster.Overhead("shield block", Color.White,
                                        $"{caster} uses {{b}}Shield Block{{/b}} to mitigate {{b}}{preventHowMuch.ToString()}{{/b}} damage.");
                                    return new ReduceDamageModification(preventHowMuch, "Shield block (spell)");
                                })
                        };
                        caster.AddQEffect(qEffect);
                        return Task.CompletedTask;
                    }).WithHeightenedAtSpecificLevels(level, inCombat, [3,5,7,9],"The shield blocks 4 damage, and the damage increases to 3d4.", "The shield blocks 7 damage and the damage increases to 4d4.", "The shield blocks 10 damage and the damage increases to 5d4.", "The shield blocks 12 damage and the damage increases to 6d4.");
        });
    }
}