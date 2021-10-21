using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public CombatGameManager combatManager;
    public GameObject CharacterSelectPanel;
    public CombatInputController inputController;
    public GameObject CameraL;
    public GameObject CameraR;
    public Button EscapeButton;
    public Button ReturnButton;
    public Button BasicAbility;
    public Button BasicDuoAbility;
    public Button Slap;
    public Button HunkerDown;
    public Button Heal;
    // Start is called before the first frame update
    void Start()
    {
        for (int i=0; i<4; i++)
        {
            int x = i;
             Button framei = CharacterSelectPanel.transform.GetChild(i).GetComponent<Button>();
            framei.onClick.AddListener(delegate { combatManager.SelectControllableUnit(x); });
        }
        CameraL.transform.GetComponent<Button>().onClick.AddListener(delegate { CombatGameManager.Instance.Camera.RotateCamera(-1); });
        CameraR.transform.GetComponent<Button>().onClick.AddListener(delegate { CombatGameManager.Instance.Camera.RotateCamera(1); });

        BasicAbility.onClick.AddListener(delegate { CombatGameManager.Instance.CurrentUnit.UseAbility(new BasicShot()); });
        BasicDuoAbility.onClick.AddListener(delegate { CombatGameManager.Instance.CurrentUnit.UseAbility(new BasicDuoShot()); });
        Slap.onClick.AddListener(delegate { CombatGameManager.Instance.CurrentUnit.UseAbility(new Slap()); });
        HunkerDown.onClick.AddListener(delegate { CombatGameManager.Instance.CurrentUnit.UseAbility(new HunkerDown()); });
        Heal.onClick.AddListener(delegate { CombatGameManager.Instance.CurrentUnit.UseAbility(new FirstAid()); });

        

    }

    // Update is called once per frame
    void Update()
    {
        if (CombatGameManager.Instance.CurrentAbility != null)
        {
            EscapeButton.onClick.AddListener(delegate { CombatGameManager.Instance.CurrentAbility.SetUICancelled(); });
            ReturnButton.onClick.AddListener(delegate { CombatGameManager.Instance.CurrentAbility.SetUIConfirmed(); });
        }
        
    }

    
}
