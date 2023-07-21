using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_DoorPrompt : MonoBehaviour
{
    [SerializeField] CanvasGroup PromptCanvasGroup;
    [SerializeField] TextMeshProUGUI PromptText;
    [SerializeField] string OpenPrompt = "Press [E] to Open";
    [SerializeField] string ClosePrompt = "Press [E] to Close";

    DoorController CurrentDoor = null;

    // Start is called before the first frame update
    void Start()
    {
        PromptCanvasGroup.alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnInteractRequested()
    {
        if (CurrentDoor == null)
            return;

        if (CurrentDoor.IsOpenOrOpening)
            CurrentDoor.RequestClose(gameObject);
        else
            CurrentDoor.RequestOpen(gameObject);
    }

    public void DoorStateChanged(bool isOpen, DoorController currentDoor)
    {
        if (currentDoor == CurrentDoor && CurrentDoor != null)
        {
            PromptText.text = isOpen ? ClosePrompt : OpenPrompt;
        }
    }

    public void ShowPrompt(bool isOpen, DoorController currentDoor)
    {
        CurrentDoor = currentDoor;

        PromptText.text = isOpen ? ClosePrompt : OpenPrompt;
        PromptCanvasGroup.alpha = 1;
    }

    public void HidePrompt()
    {
        CurrentDoor = null;

        PromptCanvasGroup.alpha = 0;
    }
}
