

public static class PlayerStatisticsTracker
{
    public static int PlayerTotalKillCount;
    public static int PlayerTotalDamageDealt;
    public static int PlayerTotalDamageTaken;
    public static int PlayerTotalSuccessfulBlocks;

    public static int PlayerTotalGoldEarned;
    public static int PlayerTotalGoldSpent;

    public static int NumTotalMercenariesHired;
    public static int NumTotalMercenaryUpgrades; // tracks the number of upgrades, not how many mercs were upgraded.
    public static int MercenariesTotalDeathCount;
    public static int MercenariesTotalKillCount;

    public static void Initialize()
    {
        PlayerTotalKillCount = 0;
        PlayerTotalDamageDealt = 0;
        PlayerTotalDamageTaken = 0;
        PlayerTotalSuccessfulBlocks = 0;

        PlayerTotalGoldEarned = 0;
        PlayerTotalGoldSpent = 0;

        NumTotalMercenariesHired = 0;
        NumTotalMercenaryUpgrades = 0;
        MercenariesTotalDeathCount = 0;
        MercenariesTotalKillCount = 0;
    }
}
