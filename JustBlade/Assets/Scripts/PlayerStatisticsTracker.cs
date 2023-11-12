
/// <summary>
/// A static class which tracks certain statistics about the player.
/// Some examples of statistics are kill count, total gold earned, number of mercenaries hired, etc.
/// These statistics are shown in the <see cref="InformationMenuUI"/>, when the game is over.
/// The class and its fields are static, because there is only one <see cref="PlayerAgent"/>, and
/// the instance based alternative would have us involve managing game objects from scene to scene, since
/// Unity destroys all contents of an open scene before transitioning to another one.
/// For a game of this size, I think the static class approach is sufficient.
/// </summary>
public static class PlayerStatisticsTracker
{
    /// <summary>
    /// Tracks how enemies the player has killed.
    /// </summary>
    public static int PlayerTotalKillCount;
    /// <summary>
    /// Tracks how much damage the player has dealt.
    /// </summary>
    public static int PlayerTotalDamageDealt;
    /// <summary>
    /// Tracks how much damage the player has taken.
    /// </summary>
    public static int PlayerTotalDamageTaken;
    /// <summary>
    /// Tracks how many times the player has successfully blocked an attack.
    /// </summary>
    public static int PlayerTotalSuccessfulBlocks;

    /// <summary>
    /// Tracks how much gold the player has earned throughout the entire horde game.
    /// </summary>
    public static int PlayerTotalGoldEarned;
    /// <summary>
    /// Tracks how much gold the player has spent throughout the entire horde game.
    /// </summary>
    public static int PlayerTotalGoldSpent;


    /// <summary>
    /// Tracks how many mercenaries the player has hired.
    /// </summary>
    public static int NumTotalMercenariesHired;
    /// <summary>
    /// Tracks how many mercenary upgrades were made.
    /// Note that this is the "number of upgrades" and not "how many mercenaries were upgraded".
    /// </summary>
    public static int NumTotalMercenaryUpgrades;
    /// <summary>
    /// Tracks how many of player's mercenaries have died.
    /// </summary>
    public static int MercenariesTotalDeathCount;
    /// <summary>
    /// Tracks how many invaders the player's mercenaries have killed.
    /// </summary>
    public static int MercenariesTotalKillCount;

    /// <summary>
    /// Initializes the player statistics tracker.
    /// It resets the statistics values of the player from any other playthrough, and starts anew.
    /// </summary>
    public static void Initialize()
    {
        PlayerTotalKillCount = 0;
        PlayerTotalDamageDealt = 0;
        PlayerTotalDamageTaken = 0;
        PlayerTotalSuccessfulBlocks = 0;

        // Total gold earned starts with this, so that it doesn't look like we
        // spent more than we earnt.
        PlayerTotalGoldEarned = PlayerInventoryManager.DefaultPlayerGold;
        PlayerTotalGoldSpent = 0;

        NumTotalMercenariesHired = 0;
        NumTotalMercenaryUpgrades = 0;
        MercenariesTotalDeathCount = 0;
        MercenariesTotalKillCount = 0;
    }
}
