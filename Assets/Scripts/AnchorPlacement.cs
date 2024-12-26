using System.Collections.Generic;
using Oculus.Interaction.Body.Input;
using UnityEngine;

public class AnchorPlacement : MonoBehaviour
{

    public GameObject anchorPrefab;
    public OVRInput.Button triggerButton;
    public Transform controllerTransform;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.GetDown(triggerButton)) {
            CreateSpatialAnchor();
        }
    }

    public void CreateSpatialAnchor() {
        GameObject anchor = Instantiate(anchorPrefab, controllerTransform.position, controllerTransform.rotation);
        anchor.AddComponent<OVRSpatialAnchor>();
    }
}
