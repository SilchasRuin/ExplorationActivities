using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Modding;
using Dawnsbury.Mods.DawnniExpanded;

namespace ExplorationActivities;

public abstract class ExplorationActivities
{
    public static IEnumerable<Feat> ExplorationFeats()
    {
        Feat scout = new(ModData.FeatNames.ScoutActivity, "You scout ahead and behind the group to watch danger.", "At the start of the next encounter, every creature in your party gains a +1 circumstance bonus to their initiative rolls.",
            [ModData.Traits.ExplorationActivity], null);
        CreateScoutLogic(scout);
        yield return scout;
        Feat avoidNotice = new(ModData.FeatNames.AvoidNotice, "You move quietly and carefully, avoiding your enemies' detection", "You roll stealth instead of perception for initiative and if you have cover may be hidden at the start of an encounter",
            [ModData.Traits.ExplorationActivity], null);
        CreateAvoidNoticeLogic(avoidNotice);
        yield return avoidNotice;
        Feat hustle = new(ModData.FeatNames.Hustle, "You strain yourself for an extra boost of speed.", "On your first turn, you gain a +5 circumstance bonus to speed, or a +10 circumstance bonus to speed if your Constitution modifier is +4 or higher.", 
            [ModData.Traits.ExplorationActivity, Trait.Homebrew], null);
        CreateHustleLogic(hustle);
        yield return hustle;
        Feat defend = new(ModManager.RegisterFeatName("Defend"), "You keep your shield ahead of you, ready to protect yourself.", "At the start of an encounter, you gain the benefits of Raising a Shield before your first turn begins.",
            [ModData.Traits.ExplorationActivity], null);
        CreateDefendLogic(defend);
        yield return defend;
        Feat castShield = new(ModManager.RegisterFeatName("Repeat a Spell - Shield"), null, "You gain the benefits of casting the given spell at the start of an encounter.",
            [ModData.Traits.ExplorationActivity, Trait.Concentrate], null);
        CreateCastShieldLogic(castShield);
        yield return castShield;
        Feat castMusic = new(ModManager.RegisterFeatName("Repeat a Spell - Musical Accompaniment"), null, "You gain the benefits of casting the given spell at the start of an encounter.",
            [ModData.Traits.ExplorationActivity, Trait.Concentrate], null);
        CreateCastMusicLogic(castMusic);
        yield return castMusic;
        Feat castGlass = new(ModManager.RegisterFeatName("Repeat a Spell - Glass Shield"), null, "You gain the benefits of casting the given spell at the start of an encounter.",
            [ModData.Traits.ExplorationActivity, Trait.Concentrate], null);
        CreateCastGlassLogic(castGlass);
        yield return castGlass;
        if (ModManager.TryParse("DawnniEx", out Trait _))
        {
            Feat investigate = new(ModManager.RegisterFeatName("Investigate"), "You seek out information about the enemies you could face.", "If an enemy is within range at the start of an encounter, you may Recall Weakness as a free action. If you are a strategist, the target of your Recall Weakness is marked as a person of interest (this does not count against the number of times you may declare a person of interest).",
                [ModData.Traits.ExplorationActivity, Trait.Homebrew], null);
            CreateInvestigateLogic(investigate);
            yield return investigate;
        }
        Feat track = new(ModManager.RegisterFeatName("Track"), "You follow tracks and look for signs of enemies.", "You roll survival instead of perception for initiative. If you are a Ranger, at the start of an encounter, you may Hunt Prey as a free action on one creature who acts after you in initiative.",
            [ModData.Traits.ExplorationActivity, Trait.Homebrew], null);
        CreateTrackLogic(track);
        yield return track;
        Feat deceptiveApproach = new TrueFeat(ModData.FeatNames.DeceptiveApproach, 2, "You know how to misdirect people's expectations to get the drop on them.", "You may select the {b}Impersonate{/b} exploration activity, which allows you to roll deception instead of perception for initiative.", [Trait.Homebrew, Trait.General, Trait.Skill])
            .WithPrerequisite(values => values.GetProficiency(Trait.Deception) >= Proficiency.Expert, "You must be an expert in Deception.");
        yield return deceptiveApproach;
        Feat impersonate = new(ModManager.RegisterFeatName("Impersonate"), "You surprise your foes by pretending to be something you're not.", "You roll deception instead of perception for initiative.",
            [ModData.Traits.ExplorationActivity, Trait.Homebrew], null);
        CreateImpersonateLogic(impersonate);
        yield return impersonate;
        Feat gladHand = new TrueFeat(ModData.FeatNames.GladHand, 2, "First impressions are your strong suit, even those hostile to you sometimes hesitate to attack.", "You may select the {b}Make an Impression{/b} exploration activity, which allows you to roll persuasion instead of perception for initiative.", [Trait.Homebrew, Trait.General, Trait.Skill])
            .WithPrerequisite(values => values.GetProficiency(Trait.Diplomacy) >= Proficiency.Expert, "You must be an expert in Diplomacy.");
        yield return gladHand;
        Feat makeAnImpression = new(ModManager.RegisterFeatName("Make an Impression"), "You're so charming your foes may hesitate to attack.", "You roll persuasion instead of perception for initiative.",
            [ModData.Traits.ExplorationActivity, Trait.Homebrew], null);
        CreateMakeAnImpressionLogic(makeAnImpression);
        yield return makeAnImpression;
        Feat imposingPresence = new TrueFeat(ModData.FeatNames.ImposingPresence, 2,
                "One look at you makes your enemies hesitate.",
                "You may select the {b}Coerce{/b} exploration activity, which allows you to roll persuasion instead of perception for initiative.",
                [Trait.General, Trait.Skill, Trait.Homebrew])
            .WithPrerequisite(values => values.GetProficiency(Trait.Intimidation) >= Proficiency.Expert, "You must be an expert in Intimidation.");
        yield return imposingPresence;
        Feat coerce = new(ModManager.RegisterFeatName("Coerce"), "Enemies hesitate to strike due to your ferocious demeanor.", "You roll intimidation instead of perception for initiative.",
            [ModData.Traits.ExplorationActivity, Trait.Homebrew], null);
        CreateCoerceLogic(coerce);
        yield return coerce;
        Feat warfareLore = new SkillSelectionFeat(ModData.FeatNames.WarfareLore, ModData.Skills.WarfareLore, ModData.Traits.WarfareLore);
        yield return warfareLore;
        Feat expertWarfareLore = new SkillIncreaseFeat(ModData.FeatNames.WarfareLoreExpert, ModData.Skills.WarfareLore,ModData.Traits.WarfareLore, Proficiency.Expert, ModData.FeatNames.WarfareLore);
        yield return expertWarfareLore;
        Feat masterWarfareLore = new SkillIncreaseFeat(ModData.FeatNames.WarfareLoreMaster, ModData.Skills.WarfareLore,ModData.Traits.WarfareLore, Proficiency.Master, ModData.FeatNames.WarfareLoreExpert);
        yield return masterWarfareLore;
        Feat legendaryWarfareLore = new SkillIncreaseFeat(ModData.FeatNames.WarfareLoreLegendary, ModData.Skills.WarfareLore,ModData.Traits.WarfareLore, Proficiency.Master, ModData.FeatNames.WarfareLoreMaster);
        yield return legendaryWarfareLore;
        Feat additionalLoreWar = new TrueFeat(ModManager.RegisterFeatName("AdditionalLoreWF", "Additional Lore - Warfare"), 1, "Your knowledge has expanded to encompass a new field.", "You become trained in Warfare Lore. At 3rd level you become an expert in Warfare Lore, at 7th level you become a master in Warfare Lore, and at 15th level, you become legendary in Warfare Lore.", [Trait.General, Trait.Skill]);
        CreateAdditionalLoreLogic(additionalLoreWar);
        yield return additionalLoreWar;
        Feat battlePlanner = new TrueFeat(ModData.FeatNames.BattlePlanner, 2, "You are constantly drawing up plans and battle scenarios, assembling strategies and gathered intelligence for later use.", "If you or one of your allies has taken the scout exploration activity, you roll warfare lore instead of perception for initiative.", [Trait.General, Trait.Skill]);
        CreateBattlePlannerLogic(battlePlanner);
        yield return battlePlanner;
        Feat cadet = new BackgroundSelectionFeat(ModManager.RegisterFeatName("Cadet", "Cadet"), "Once you enrolled in a military academy, where you studied tactics, strategy, the history of battles, and the art of command. Perhaps you lead others in battle yourself, or maybe you never had a chance. Either way, at some point you took the skills you learned and sought to apply them to a life of adventure.", "You are trained in {b}Athletics{/b}. You gain the {b}Additional Lore{/b} skill feat for {b}Warfare Lore{/b}.", [new LimitedAbilityBoost(Ability.Intelligence, Ability.Charisma), new FreeAbilityBoost()])
            .WithOnSheet(values =>
            {
                values.GrantFeat(additionalLoreWar.FeatName);
                values.TrainInThisOrSubstitute(Skill.Athletics);
            });
        cadet.Traits.Add(Trait.Homebrew);
        yield return cadet;
        Feat incredibleScout = new TrueFeat(ModManager.RegisterFeatName("IncredibleScout", "Incredible Scout"), 11, "When you scout, you are particularly alert for danger, granting your allies precious moments to prepare to fight.", "When using the Scout exploration activity, you grant your allies a +2 circumstance bonus to their initiative rolls instead of a +1 circumstance bonus.", [Trait.General])
            .WithPrerequisite(values => values.GetProficiency(Trait.Perception) >= Proficiency.Master, "You must be a master in Perception.").WithPermanentQEffect(null, qf => qf.Id = ModData.QEffectIds.GreaterScoutActivity);
        yield return incredibleScout;
        Feat pickUpThePace = new TrueFeat(ModManager.RegisterFeatName("PickUpThePace", "Pick up the Pace"), 3,
            "You lead by example and can help others push themselves beyond their normal limits.",
            "When using the Hustle exploration activity, you grant your allies the benefit of it as well.",
            [Trait.General, Trait.Homebrew]);
        CreatePickUpThePaceLogic(pickUpThePace);
        yield return pickUpThePace;
    }
    private static void CreateScoutLogic(Feat scout)
    {
        scout.WithPermanentQEffect(null,
            qf =>
            {
                qf.StartOfCombat = qff =>
                {
                    var bonus = qf.Owner.HasEffect(ModData.QEffectIds.GreaterScoutActivity)
                        ? 2
                        : 1;
                    qff.AddGrantingOfTechnical(cr => cr.FriendOf(qf.Owner), qfTech =>
                    {
                        qfTech.BonusToInitiative = _ =>
                            new Bonus(bonus, BonusType.Circumstance, "Scout Activity");
                    });
                    qf.Owner.Battle.Log("You scouted the area, you and all allies gain a +"+bonus+" circumstance bonus to initiative.");
                    return Task.CompletedTask;
                };
            });
    }
    private static void CreateAvoidNoticeLogic(Feat avoidNotice)
    {
        avoidNotice.WithPermanentQEffect(null, effect =>
        {
            Creature self = effect.Owner;
            effect.OfferAlternateSkillForInitiative = _ => Skill.Stealth;
            int stealthMinusPerception = self.Skills.Get(Skill.Stealth) - self.Perception + (ModManager.TryParse("Suli", out Trait suli) && self.HasTrait(suli) ? 1 : 0);
            if (stealthMinusPerception < 0)
            {
                effect.BonusToInitiative = _ => new Bonus(stealthMinusPerception, BonusType.Untyped, "Avoid Notice");
            }
            effect.StartOfCombat = startOfCombat =>
            {
                string coverText = string.Empty;
                if (!startOfCombat.Owner.Battle.AllCreatures.Any(cr =>
                        cr.EnemyOf(startOfCombat.Owner) && cr.Occupies.FogOfWar != FogOfWar.Blackened &&
                        HiddenRules.CountsAsHavingCoverOrConcealment(startOfCombat.Owner, cr)))
                {
                    coverText = ". You don't have cover or concealment from any enemy, so no stealth was applied";
                }

                // Handles the hiding on stealth initiative rolls
                int stealthDc = startOfCombat.Owner.Initiative;
                foreach (Creature enemy in startOfCombat.Owner.Battle.AllCreatures.Where(creature =>
                             !startOfCombat.Owner.FriendOf(creature) &&
                             HiddenRules.HasCoverOrConcealment(startOfCombat.Owner, creature) &&
                             creature.Initiative < stealthDc))
                {
                    startOfCombat.Owner.DetectionStatus.HiddenTo.Add(enemy);
                }
                startOfCombat.Owner.Battle.Log(startOfCombat.Owner.Name + " has rolled Stealth for initiative" +
                                               coverText +
                                               (startOfCombat.Owner.DetectionStatus.EnemiesYouAreHiddenFrom.Any()
                                                   ? "and is hidden to:\n" + string.Join(",",
                                                       startOfCombat.Owner.DetectionStatus.EnemiesYouAreHiddenFrom)
                                                   : "."));
                return Task.CompletedTask;
            };
        });
    }
    private static void CreateHustleLogic(Feat hustle)
    {
        hustle.WithPermanentQEffect(null, qf =>
        {
            Creature self = qf.Owner;
            qf.StartOfCombat = _ =>
            {
                self.AddQEffect(new QEffect(ExpirationCondition.ExpiresAtEndOfYourTurn)
                {
                    BonusToAllSpeeds = _ => new Bonus(self.Abilities.Constitution >= 4 ? 2 : 1, BonusType.Circumstance, "Hustle")
                });
                return Task.CompletedTask;
            };
        });
    }
    private static void CreateDefendLogic(Feat defend)
    {
        defend.WithPermanentQEffect(null, effect =>
        {
            Creature self = effect.Owner;
            effect.StartOfCombat = async _ =>
            {
                if (Possibilities.Create(self)
                        .Filter(ap =>
                        {
                            if (ap.CombatAction.ActionId != ActionId.RaiseShield)
                                return false;
                            ap.CombatAction.ActionCost = 0;
                            ap.RecalculateUsability();
                            return true;
                        }).CreateActions(true).FirstOrDefault(pw =>
                            pw.Action.ActionId == ActionId.RaiseShield) is CombatAction raiseShield) await self.Battle.GameLoop.FullCast(raiseShield);
            };
        })
        .WithPrerequisite(sheet => (sheet.Sheet.Inventory.LeftHand != null && sheet.Sheet.Inventory.LeftHand.HasTrait(Trait.Shield)) || (sheet.Sheet.Inventory.RightHand != null && sheet.Sheet.Inventory.RightHand.HasTrait(Trait.Shield)), "You must be wielding a shield.");
    }
    private static void CreateCastShieldLogic(Feat shieldCast)
    {
        shieldCast.WithPermanentQEffect(null, effect =>
        {
            Creature self = effect.Owner;
            effect.StartOfCombat = async _ =>
            {
                Possibilities shield = Possibilities.Create(self).Filter(ap =>
                {
                    if (ap.CombatAction.SpellId != SpellId.Shield)
                        return false;
                    if (ap.CombatAction.SpellInformation is { PsychicAmpInformation.Amped: true })
                        return false;
                    ap.CombatAction.ActionCost = 0;
                    ap.RecalculateUsability();
                    return true;
                });
                List<Option> actions = await self.Battle.GameLoop.CreateActions(self, shield, null);
                await self.Battle.GameLoop.OfferOptions(self, actions, true);
            };
        })
        .WithPrerequisite(values => values.Sheet.PreparedSpells.Any(pair => pair.Value is { SpellId: SpellId.Shield }) || values.SpellRepertoires.Any(pair => pair.Value.SpellsKnown.Any(spell => spell.SpellId == SpellId.Shield)) || values.InnateSpells.Any(pair => pair.Value.SpellsKnown.Any(spell => spell.SpellId == SpellId.Shield)), "You must be able to cast shield.");
    }
    private static void CreateCastMusicLogic(Feat musicCast)
    {
        musicCast.WithPermanentQEffect(null, effect =>
            {
                Creature self = effect.Owner;
                effect.StartOfCombat = async _ =>
                {
                    if (Possibilities.Create(self).Filter(ap =>
                        {
                            if (ap.CombatAction.SpellId != ExplorationSpells.MusicalAccompaniment)
                                return false;
                            ap.CombatAction.ActionCost = 0;
                            ap.RecalculateUsability();
                            return true;
                        }).CreateActions(true).FirstOrDefault(pw => pw.Action.SpellId == ExplorationSpells.MusicalAccompaniment) is CombatAction castMusic) await self.Battle.GameLoop.FullCast(castMusic);
                };
            })
            .WithPrerequisite(values => values.Sheet.PreparedSpells.Any(pair => pair.Value is { } spell && spell.SpellId == ExplorationSpells.MusicalAccompaniment) || values.SpellRepertoires.Any(pair => pair.Value.SpellsKnown.Any(spell => spell.SpellId == ExplorationSpells.MusicalAccompaniment)) || values.InnateSpells.Any(pair => pair.Value.SpellsKnown.Any(spell => spell.SpellId == ExplorationSpells.MusicalAccompaniment)), "You must be able to cast musical accompaniment.");
    }
    private static void CreateCastGlassLogic(Feat glassCast)
    {
        glassCast.WithPermanentQEffect(null, effect =>
            {
                Creature self = effect.Owner;
                effect.StartOfCombat = async _ =>
                {
                    if (Possibilities.Create(self).Filter(ap =>
                        {
                            if (ap.CombatAction.SpellId != ExplorationSpells.GlassShield)
                                return false;
                            ap.CombatAction.ActionCost = 0;
                            ap.RecalculateUsability();
                            return true;
                        }).CreateActions(true).FirstOrDefault(pw => pw.Action.SpellId == ExplorationSpells.GlassShield) is CombatAction castGlass) await self.Battle.GameLoop.FullCast(castGlass);
                };
            })
            .WithPrerequisite(values => values.Sheet.PreparedSpells.Any(pair => pair.Value is { } spell && spell.SpellId == ExplorationSpells.GlassShield) || values.SpellRepertoires.Any(pair => pair.Value.SpellsKnown.Any(spell => spell.SpellId == ExplorationSpells.GlassShield)) || values.InnateSpells.Any(pair => pair.Value.SpellsKnown.Any(spell => spell.SpellId == ExplorationSpells.GlassShield)), "You must be able to cast glass shield.");
    }

    private static void CreateInvestigateLogic(Feat investigate)
    {
        investigate.WithPermanentQEffect(null, effect =>
        {
            Feat glance = FeatRecallWeakness.SlightestGlanceWeakness;
            Creature self = effect.Owner;
            effect.StartOfCombat = async _ =>
            {
                if (Possibilities.Create(self).Filter(ap =>
                    {
                        if (!ap.CombatAction.Name.Contains("Recall Weakness"))
                            return false;
                        ap.CombatAction.ActionCost = 0;
                        ap.RecalculateUsability();
                        return true;
                    }).CreateActions(true).FirstOrDefault(pw => pw.Action.Name.Contains("Recall Weakness")) is CombatAction investigateAction)
                {
                    if (self.Battle.AllCreatures.Any(cr => cr.EnemyOf(self) && cr.DistanceTo(self) <= (self.HasFeat(glance.FeatName) && self.Proficiencies.Get(Trait.Perception) >= Proficiency.Master ? 24 : self.HasFeat(glance.FeatName) ? 12 : 6)))
                    {
                        if (self.PersistentCharacterSheet is { Class.ClassTrait: Trait.Investigator })
                        {
                            self.AddQEffect(new QEffect(ExpirationCondition.ExpiresAtStartOfYourTurn)
                            {
                                AfterYouTakeActionAgainstTarget = (_, action, _, _) =>
                                {
                                    if (action.Name.Contains("Recall Weakness"))
                                    {
                                        action.ChosenTargets.ChosenCreature?.AddQEffect(
                                            new QEffect("Person of Interest ",
                                                self.Name + " can declare a stratagem against this creature for free.",
                                                ExpirationCondition.Never, self, IllustrationName.HuntPrey)
                                            {
                                                Id = QEffectId.IsPersonOfInterest
                                            });
                                    }

                                    return Task.CompletedTask;
                                }
                            });
                        }
                        await self.Battle.GameLoop.FullCast(investigateAction);
                    }
                }
            };
        });
    }
    private static void CreateTrackLogic(Feat track)
    { 
        track.WithPrerequisite(sheet => sheet.GetProficiency(Trait.Survival) >= Proficiency.Trained, "You must be trained in Survival.")
            .WithPermanentQEffect(null, qf =>
            {
                Creature self = qf.Owner;
                qf.OfferAlternateSkillForInitiative = _ => Skill.Survival;
                var survivalMinusPerception = self.Skills.Get(Skill.Survival) - self.Perception + (ModManager.TryParse("Suli", out Trait suli) && self.HasTrait(suli) ? 1 : 0);
                if (survivalMinusPerception < 0)
                {
                    qf.BonusToInitiative = _ => new Bonus(survivalMinusPerception, BonusType.Untyped, "Track");
                }
                if (self.PersistentCharacterSheet?.Class is {ClassTrait: Trait.Ranger})
                    qf.StartOfCombat = async _ =>
                    {
                        List<Creature> possibleTargets = [];
                        foreach (Creature target in self.Battle.AllCreatures.Where(cr => self.Initiative >= cr.Initiative && cr.EnemyOf(self)))
                        {
                            possibleTargets.Add(target);
                        }
                        if (Possibilities.Create(self).Filter(ap =>
                            {
                                if (ap.CombatAction.ActionId != ActionId.HuntPrey)
                                    return false;
                                ap.CombatAction.ActionCost = 0;
                                ap.RecalculateUsability();
                                return true;
                            }).CreateActions(true).FirstOrDefault(pw => pw.Action.ActionId == ActionId.HuntPrey) is CombatAction hunt)
                        {
                            if (possibleTargets.Count > 0)
                            {
                                Creature? result = await self.Battle.AskToChooseACreature(self, possibleTargets,
                                    IllustrationName.HuntPrey, "Target a creature with hunt prey?",
                                    "Target this creature", "pass");
                                if (result != null)
                                {
                                    ChosenTargets target = ChosenTargets.CreateSingleTarget(result);
                                    await self.Battle.GameLoop.FullCast(hunt, target);
                                }
                            }
                        }
                    };
            });
    }
    private static void CreateImpersonateLogic(Feat impersonate)
    {
        impersonate.WithPrerequisite(ModData.FeatNames.DeceptiveApproach, "Deceptive Approach")
            .WithPermanentQEffect(null, effect =>
            {
                effect.OfferAlternateSkillForInitiative = _ => Skill.Deception;
            });
    }

    private static void CreateMakeAnImpressionLogic(Feat makeAnImpression)
    {
        makeAnImpression.WithPrerequisite(ModData.FeatNames.GladHand, "Glad-Hand")
            .WithPermanentQEffect( null, effect =>
                {
                    effect.OfferAlternateSkillForInitiative = _ => Skill.Diplomacy;
                }
                );
    }
    private static void CreateCoerceLogic(Feat coerce)
    {
        coerce.WithPrerequisite(ModData.FeatNames.ImposingPresence, "Imposing Presence")
            .WithPermanentQEffect( null, effect =>
                {
                    effect.OfferAlternateSkillForInitiative = _ => Skill.Intimidation;
                }
            );
    }

    private static void CreateAdditionalLoreLogic(Feat additionalLore)
    {
        additionalLore.WithOnSheet(values =>
        {
            values.GrantFeat(ModData.FeatNames.WarfareLore);
            values.AddAtLevel(3, v3 => v3.GrantFeat(ModData.FeatNames.WarfareLoreExpert));
            values.AddAtLevel(7, v7 => v7.GrantFeat(ModData.FeatNames.WarfareLoreMaster));
            values.AddAtLevel(15, v15 => v15.GrantFeat(ModData.FeatNames.WarfareLoreLegendary));
        });
    }

    private static void CreateBattlePlannerLogic(Feat battlePlanner)
    {
        battlePlanner.WithPrerequisite(ModData.FeatNames.WarfareLoreExpert, "Expert in Warfare Lore")
            .WithPermanentQEffect(null, effect =>
            {
                effect.StartOfCombat = _ =>
                {
                    Creature self = effect.Owner;
                    if (self.Battle.AllCreatures.Where(cr => cr.FriendOf(self))
                        .Any(creature => creature.HasFeat(ModData.FeatNames.ScoutActivity)))
                        self.AddQEffect(new QEffect()
                        {
                            OfferAlternateSkillForInitiative = _ => ModData.Skills.WarfareLore,
                        });
                    return Task.CompletedTask;
                };
            });
    }

    private static void CreatePickUpThePaceLogic(Feat pickUpThePace)
    {
        pickUpThePace.WithPrerequisite(values => values.FinalAbilityScores.TotalModifier(Ability.Constitution) >= 2,
                "You must have at least 14 Constitution.")
            .WithPermanentQEffect(null, effect =>
            {
                effect.StartOfCombat = _ =>
                {
                    Creature self = effect.Owner;
                    if (self.HasFeat(ModData.FeatNames.Hustle))
                    {
                        effect.AddGrantingOfTechnical(cr => cr.FriendOfAndNotSelf(self), qfTech =>
                        {
                            if (!qfTech.Owner.HasFeat(ModData.FeatNames.Hustle))
                                qfTech.Owner.AddQEffect(new QEffect(ExpirationCondition.ExpiresAtEndOfYourTurn)
                                {
                                    BonusToAllSpeeds = _ => new Bonus(self.Abilities.Constitution >= 4 ? 2 : 1, BonusType.Circumstance, "Hustle")
                                });
                        });
                    }
                    return Task.CompletedTask;
                };
            });
    }
}