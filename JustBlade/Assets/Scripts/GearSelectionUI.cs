using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GearSelectionUI : MonoBehaviour
{
    public float mannequinAgentSize;

    public Transform mannequinContainer;
    public GameObject mannequinAgentPrefab;

    public TextMeshProUGUI txtSelectedWeapon;
    public TextMeshProUGUI txtSelectedHeadArmor;
    public TextMeshProUGUI txtSelectedTorsoArmor;
    public TextMeshProUGUI txtSelectedHandArmor;
    public TextMeshProUGUI txtSelectedLegArmor;

    public Button btnNextWeapon;
    public Button btnNextHeadArmor;
    public Button btnNextTorsoArmor;
    public Button btnNextHandArmor;
    public Button btnNextLegArmor;

    public Button btnPrevWeapon;
    public Button btnPrevHeadArmor;
    public Button btnPrevTorsoArmor;
    public Button btnPrevHandArmor;
    public Button btnPrevLegArmor;

    public Button btnFight;

    MannequinAgent mannequinAgent;

    public void OnClick_ButtonNextWeapon()
    {
        TournamentVariables.PlayerChosenWeaponIndex++;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonNextHeadArmor()
    {
        TournamentVariables.PlayerChosenHeadArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonNextTorsoArmor()
    {
        TournamentVariables.PlayerChosenTorsoArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonNextHandArmor()
    {
        TournamentVariables.PlayerChosenHandArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonNextLegArmor()
    {
        TournamentVariables.PlayerChosenLegArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonPrevWeapon()
    {
        TournamentVariables.PlayerChosenWeaponIndex--;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonPrevHeadArmor()
    {
        TournamentVariables.PlayerChosenHeadArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonPrevTorsoArmor()
    {
        TournamentVariables.PlayerChosenTorsoArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonPrevHandArmor()
    {
        TournamentVariables.PlayerChosenHandArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonPrevLegArmor()
    {
        TournamentVariables.PlayerChosenLegArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonFight()
    {
        SceneManager.LoadScene("ArenaScene");
    }

    void Start()
    {
        OnMannequinEquipmentChanged();
    }

    void UpdateTexts()
    {
        txtSelectedWeapon.text = PrefabManager.Weapons[TournamentVariables.PlayerChosenWeaponIndex].shownName;
        txtSelectedHeadArmor.text = PrefabManager.HeadArmors[TournamentVariables.PlayerChosenHeadArmorIndex].shownName;
        txtSelectedTorsoArmor.text = PrefabManager.TorsoArmors[TournamentVariables.PlayerChosenTorsoArmorIndex].shownName;
        txtSelectedHandArmor.text = PrefabManager.HandArmors[TournamentVariables.PlayerChosenHandArmorIndex].shownName;
        txtSelectedLegArmor.text = PrefabManager.LegArmors[TournamentVariables.PlayerChosenLegArmorIndex].shownName;
    }

    void OnMannequinEquipmentChanged()
    {
        UpdateTexts();

        RespawnMannequinAgent();
    }

    void RespawnMannequinAgent()
    {
        if (mannequinAgent != null)
        {
            Destroy(mannequinAgent.gameObject);
            mannequinAgent = null;
        }

        GameObject mannequinGO = Instantiate(mannequinAgentPrefab);
        mannequinAgent = mannequinGO.GetComponent<MannequinAgent>();
        mannequinAgent.transform.parent = mannequinContainer;
        mannequinAgent.transform.localPosition = Vector3.zero;
        mannequinAgent.transform.localScale = Vector3.one * mannequinAgentSize;
    }
}
