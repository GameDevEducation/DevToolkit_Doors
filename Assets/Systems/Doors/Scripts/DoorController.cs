using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class DoorController : MonoBehaviour
{
    public enum EDoorMode
    {
        FullyAutomatic,
        FullyManual,
        ManualWithAutomaticClose
    }

    public enum EDoorState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    [SerializeField] EDoorMode Mode = EDoorMode.FullyAutomatic;
    [SerializeField] List<string> TagsToCheckFor;
    [SerializeField] bool FacingMatters = false;

    [SerializeField] float AutoCloseTime = 3.0f;

    [Header("Manual Door Prompt (Optional)")]
    [SerializeField] UI_DoorPrompt DoorPromptUI = null;
    [SerializeField] UnityEvent<bool, DoorController> OnShowDoorInteractionPrompt = new();
    [SerializeField] UnityEvent OnHideDoorInteractionPrompt = new();
    [SerializeField] UnityEvent<bool, DoorController> OnDoorStateChanged = new();

    public bool IsOpenOrOpening => CurrentState == EDoorState.Open || CurrentState == EDoorState.Opening;
    public bool IsClosedOrClosing => CurrentState == EDoorState.Closed || CurrentState == EDoorState.Closing;
    public bool IsManual => Mode == EDoorMode.FullyManual || Mode == EDoorMode.ManualWithAutomaticClose;

    bool InForwardsMode = true;

    float AutoCloseTimeRemaining = -1;

    List<GameObject> HeldOpenBy = new List<GameObject>();

    EDoorState CurrentState = EDoorState.Closed;
    Animator DoorAnimController;

    // Start is called before the first frame update
    void Start()
    {
        if (IsManual && DoorPromptUI == null)
        {
            DoorPromptUI = FindObjectOfType<UI_DoorPrompt>();

            if (DoorPromptUI != null) 
            {
                OnShowDoorInteractionPrompt.AddListener(DoorPromptUI.ShowPrompt);
                OnHideDoorInteractionPrompt.AddListener(DoorPromptUI.HidePrompt);
                OnDoorStateChanged.AddListener(DoorPromptUI.DoorStateChanged);
            }
        }

        DoorAnimController = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((HeldOpenBy.Count == 1) &&
            (DoorPromptUI != null) &&
            (HeldOpenBy[0] == DoorPromptUI.gameObject) &&
            (AutoCloseTimeRemaining > 0))
        {
            AutoCloseTimeRemaining -= Time.deltaTime;

            if (AutoCloseTimeRemaining <= 0)
            {
                RequestClose(HeldOpenBy[0]);
            }
        }

        // determine target state
        EDoorState targetState = HeldOpenBy.Count > 0 ? EDoorState.Open : EDoorState.Closed;

        // do we have nothing we need to do?
        if (targetState == CurrentState)
            return;
        if (targetState == EDoorState.Open && CurrentState == EDoorState.Opening)
            return;
        if (targetState == EDoorState.Closed && CurrentState == EDoorState.Closing)
            return;

        // are we trying to open?
        if (targetState == EDoorState.Open)
        {
            CurrentState = EDoorState.Opening;
            OnDoorStateChanged.Invoke(IsOpenOrOpening, this);

            if (FacingMatters)
            {
                Vector3 openerPosition = HeldOpenBy[0].transform.position;

                if ((DoorPromptUI != null) && (HeldOpenBy[0] == DoorPromptUI.gameObject))
                {
                    openerPosition = Camera.main.transform.position;
                }

                // get the vector to the opener
                var vecToOpener = openerPosition - transform.position;
                vecToOpener.y = 0f;

                // use dot product to check which side the opener is on
                InForwardsMode = Vector3.Dot(vecToOpener, transform.forward) < 0;

                DoorAnimController.SetTrigger(InForwardsMode ? "RequestOpen_Forward" : "RequestOpen_Backward");
            }
            else
                DoorAnimController.SetTrigger("RequestOpen");            
        }
        else if (targetState == EDoorState.Closed)
        {
            CurrentState = EDoorState.Closing;
            OnDoorStateChanged.Invoke(IsOpenOrOpening, this);

            if (FacingMatters)
                DoorAnimController.SetTrigger(InForwardsMode ? "RequestClose_Forward" : "RequestClose_Backward");
            else
                DoorAnimController.SetTrigger("RequestClose");
        }
    }

    public void OnOpeningCompleted()
    {
        CurrentState = EDoorState.Open;

        OnDoorStateChanged.Invoke(IsOpenOrOpening, this);

        Debug.Log("Open!");
    }

    public void OnClosingCompleted()
    {
        CurrentState = EDoorState.Closed;
        OnDoorStateChanged.Invoke(IsOpenOrOpening, this);

        Debug.Log("Close");
    }

    public void RequestOpen(GameObject requestor)
    {
        HeldOpenBy.Add(requestor);
    }
    
    public void RequestClose(GameObject requestor)
    {
        AutoCloseTimeRemaining = -1.0f;

        HeldOpenBy.Remove(requestor);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!TagsToCheckFor.Contains(other.tag))
            return;

        AutoCloseTimeRemaining = -1.0f;

        if (IsManual)
        {
            OnShowDoorInteractionPrompt.Invoke(IsOpenOrOpening, this);
            return;
        }

        RequestOpen(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (!TagsToCheckFor.Contains(other.tag))
            return;

        if (IsManual)
        {
            OnHideDoorInteractionPrompt.Invoke();

            if (Mode == EDoorMode.ManualWithAutomaticClose)
            {
                AutoCloseTimeRemaining = AutoCloseTime;
            }

            return;
        }

        RequestClose(other.gameObject);
    }
}
