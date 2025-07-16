using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace ExplorationActivities;

public abstract class ModData
{
    public static class FeatNames
    {
        public static readonly FeatName ScoutActivity = ModManager.RegisterFeatName("ScoutActivity", "Scout");
        public static readonly FeatName AvoidNotice = ModManager.RegisterFeatName("AvoidNotice", "Avoid Notice");
        public static readonly FeatName GladHand = ModManager.RegisterFeatName("GladHand", "Glad-Hand");
        public static readonly FeatName DeceptiveApproach = ModManager.RegisterFeatName("DeceptiveApproach", "Deceptive Approach");
        public static readonly FeatName ImposingPresence = ModManager.RegisterFeatName("ImposingPresence", "Imposing Presence");
        public static readonly FeatName WarfareLore = ModManager.RegisterFeatName("WarfareLore", "Warfare Lore");
        public static readonly FeatName WarfareLoreExpert = ModManager.RegisterFeatName("WarfareLoreExpert", "Expert in Warfare Lore");
        public static readonly FeatName WarfareLoreMaster = ModManager.RegisterFeatName("WarfareLoreMaster", "Master in Warfare Lore");
        public static readonly FeatName WarfareLoreLegendary = ModManager.RegisterFeatName("WarfareLoreLegendary", "Legendary in Warfare Lore");
        public static readonly FeatName BattlePlanner =  ModManager.RegisterFeatName("BattlePlanner", "Battle Planner");
        public static readonly FeatName Hustle = ModManager.RegisterFeatName("Hustle");
    }

    public static class Traits
    {
        //This trait is used to assign feats as exploration activities so they may be selected. Add this trait to any feat you want to be an exploration activity.
        public static readonly Trait ExplorationActivity = ModManager.RegisterTrait("ExplorationActivity", new TraitProperties("Exploration", true));
        //other traits
        public static readonly Trait WarfareLore = ModManager.RegisterTrait("Warfare Lore", new TraitProperties("Warfare Lore", true));
    }

    public static class QEffectIds
    {
        internal static QEffectId GlassShieldEffect { get; } = ModManager.RegisterEnumMember<QEffectId>("GlassShieldEffect");
        internal static QEffectId MusicalAccompanimentQf { get; } = ModManager.RegisterEnumMember<QEffectId>("MusicalAccompanimentQf");
        public static QEffectId GreaterScoutActivity { get; } = ModManager.TryParse("GreaterScoutActivity", out QEffectId greaterScout) ? greaterScout : ModManager.RegisterEnumMember<QEffectId>("GreaterScoutActivity");
    }

    public static class Skills
    {
        public static readonly Skill WarfareLore = ModManager.RegisterEnumMember<Skill>("WarfareLore");
    }
}