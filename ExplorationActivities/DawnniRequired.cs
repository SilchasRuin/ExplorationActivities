using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options.Reactive;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;

namespace ExplorationActivities;

public class DawnniRequired
{
    public static void CreateInvestigateLogic(Feat investigate)
    {
        investigate.WithPermanentQEffect(null, effect =>
        {
            Creature self = effect.Owner;
            effect.StartOfCombatReaction = _ =>
            {
                if (Possibilities.Create(self).Filter(ap =>
                        {
                            if (!ap.CombatAction.Name.Contains("Recall Weakness"))
                                return false;
                            ap.CombatAction.ActionCost = 0;
                            ap.RecalculateUsability();
                            return true;
                        }).CreateActions(true)
                        .FirstOrDefault(pw => pw.Action.Name.Contains("Recall Weakness")) is not CombatAction
                    investigateAction) return null;
                if (investigateAction.Target is not CreatureTarget investigateTarget ||
                    !self.Battle.AllCreatures.Any(cr => investigateTarget.IsLegalTarget(self, cr))) return null;
                bool isInvestigator = self.PersistentCharacterSheet is { Class.ClassTrait: Trait.Investigator };
                return ReactionOption.CreateFromCombatActionCustom(investigateAction, "Recall weakness on a creature within range." +
                    (isInvestigator ? " The creature you recall weakness on becomes a person of interest." : ""), async () =>
                    {
                        if (isInvestigator)
                        {
                            self.AddQEffect(new QEffect(ExpirationCondition.ExpiresAtStartOfYourTurn)
                            {
                                AfterYouTakeActionAgainstTarget = async (_, action, _, _) =>
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
                                }
                            });
                        }
                        await self.Battle.GameLoop.FullCast(investigateAction);
                    });
            };
        });
    }
}