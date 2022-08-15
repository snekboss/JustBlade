using UnityEngine;

public class StaticVariables : MonoBehaviour
{
    static StaticVariables instance;
    public static StaticVariables Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new StaticVariables();
            }

            return instance;
        }
    }

    public const string DefaultLayerName = "Default";
    public const string AgentLayerName = "Agent";
    public const string WeaponLayerName = "Weapon";
    public const string LimbLayerName = "Limb";

    public LayerMask DefaultLayer { get; private set; } = LayerMask.NameToLayer(DefaultLayerName);
    public LayerMask AgentLayer { get; private set; } = LayerMask.NameToLayer(AgentLayerName);
    public LayerMask WeaponLayer { get; private set; } = LayerMask.NameToLayer(WeaponLayerName);
    public LayerMask LimbLayer { get; private set; } = LayerMask.NameToLayer(LimbLayerName);
}

