using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRemote : MonoBehaviour
{
    [SerializeField] string TagToCheck = "Player"; 
    [SerializeField] DoorController LinkedDoor;
    [SerializeField] float CloseDelay = 5f;

    bool IsBeingActivated = false;
    float TimeUntilClose = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsBeingActivated)
            TimeUntilClose = CloseDelay;
        else if (TimeUntilClose > 0f)
            TimeUntilClose -= Time.deltaTime;

        if (TimeUntilClose <= 0f)
            LinkedDoor.RequestClose(gameObject);
        else
            LinkedDoor.RequestOpen(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // Do nothing if the tag doesn't match
        if (!other.CompareTag(TagToCheck))
            return;

        IsBeingActivated = true;
    }

    void OnTriggerExit(Collider other)
    {
        // Do nothing if the tag doesn't match
        if (!other.CompareTag(TagToCheck))
            return;

        IsBeingActivated = false;
    }
}
