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

        

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
