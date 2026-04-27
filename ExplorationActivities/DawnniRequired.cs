using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Possibilities;

namespace ExplorationActivities;

public class DawnniRequired
{
    public static void CreateInvestigateLogic(Feat investigate)
    {
        investigate.WithPermanentQEffect(null, effect =>
        {
            Feat glance = AllFeats.All.FirstOrDefault(f => f.Name.Contains("Slightest Glance Weakness"))!;
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
}