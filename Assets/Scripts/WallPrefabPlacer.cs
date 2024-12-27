using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using TMPro;
using UnityEngine;

public class WallPrefabPlacer : MonoBehaviour
{
    
    public OVRSpatialAnchor anchorPrefab;
    public GameObject previewPrefab;
    private GameObject currentPreview;
    public const string NumUuidsPlayerPref = "numUuids";
    public OVRInput.Button triggerButton;
    public Transform controllerTransform;

    private Canvas canvas;
    private TextMeshProUGUI uuidText;
    private TextMeshProUGUI savedStatusText;
    public List<OVRSpatialAnchor> anchors = new List<OVRSpatialAnchor>();
    private OVRSpatialAnchor lastCreatedAnchor;
    private AnchorLoader anchorLoader;
    private bool isInitialized;

    public void Initialize() => isInitialized = true;

    private void Awake() {
        anchorLoader = GetComponent<AnchorLoader>();
        currentPreview = Instantiate(previewPrefab);

    }

    void Update()
    {
        if(!isInitialized) return;

        Vector3 rayOrigin = controllerTransform.position;
        Vector3 rayDirection = controllerTransform.forward;

        // Raycast from the controller to the MRUK room
        Ray ray = new Ray(rayOrigin, rayDirection);

        if(MRUK.Instance?.GetCurrentRoom()?.Raycast(new Ray(rayOrigin, rayDirection), Mathf.Infinity, out RaycastHit hit, out MRUKAnchor anchorHit) == true) {
            
            if (anchorHit != null) {
               
                // load the preview prefab
                currentPreview.transform.position = hit.point;
                currentPreview.transform.rotation = Quaternion.LookRotation(hit.normal);
                 

                if (OVRInput.GetDown(triggerButton, OVRInput.Controller.RTouch)) {
                    Quaternion rotation = Quaternion.LookRotation(hit.normal);
                    CreateSpatialAnchor(hit.point, rotation);
                }
            }
        }

        if(OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) {
            SaveLastCreatedAnchor();
        }
        if(OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) {
            UnsaveLastCreatedAnchor();
        }
        if(OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)) {
            UnsaveAllAnchors();
        }
        if(OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch)) {
            LoadSavedAnchors();
        }
    }

    public void CreateSpatialAnchor(Vector3 position, Quaternion rotation) {
        OVRSpatialAnchor workingAnchor = Instantiate(anchorPrefab, position, rotation);

        canvas = workingAnchor.gameObject.GetComponentInChildren<Canvas>();
        uuidText = canvas.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        savedStatusText = canvas.gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        StartCoroutine(AnchorCreated(workingAnchor));
    }

    private IEnumerator AnchorCreated(OVRSpatialAnchor workingAnchor) {
        while (!workingAnchor.Created && !workingAnchor.Localized) {
            yield return new WaitForEndOfFrame();
        }

        Guid anchorUuid = workingAnchor.Uuid;
        anchors.Add(workingAnchor);
        lastCreatedAnchor = workingAnchor;

        uuidText.text = "Uuid: " + anchorUuid.ToString();
        savedStatusText.text = "Not saved";
    }

    private void SaveLastCreatedAnchor() {
        lastCreatedAnchor.Save((lastCreatedAnchor, success) => {
            if (success) {
                savedStatusText.text = "Saved";
            }
        });

        SaveUuidToPlayerPrefs(lastCreatedAnchor.Uuid);
    }

    void SaveUuidToPlayerPrefs(Guid uuid) {
        if(!PlayerPrefs.HasKey(NumUuidsPlayerPref)) {
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
        }

        int playerNumUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
        PlayerPrefs.SetString("uuid" + playerNumUuids, uuid.ToString());
        PlayerPrefs.SetInt(NumUuidsPlayerPref, ++playerNumUuids);

    }

    private void UnsaveLastCreatedAnchor() {
        lastCreatedAnchor.Erase((lastCreatedAnchor, success) => {
            if (success) {
                savedStatusText.text = "Not saved";
            }
        });

        // Destroy(lastCreatedAnchor.gameObject);
    }

    private void UnsaveAllAnchors() {
        foreach (var anchor in anchors) {
            UnsaveAnchor(anchor);
        }
        anchors.Clear();
        ClearAllUuidsFromPlayerPrefs();
    }

    private void UnsaveAnchor(OVRSpatialAnchor anchor) {
        anchor.Erase((erasedAnchor, success) => {
            if (success) {
                var textComponents = anchor.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
                if (textComponents.Length > 1) {
                    textComponents[1].text = "Not saved";
                }
            }
        });

        // Destroy(anchor.gameObject);
    }

    void ClearAllUuidsFromPlayerPrefs() {
        if(PlayerPrefs.HasKey(NumUuidsPlayerPref)) {
            int playerNumUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            for (int i = 0; i < playerNumUuids; i++) {
                PlayerPrefs.DeleteKey("uuid" + i);
            }
            PlayerPrefs.DeleteKey(NumUuidsPlayerPref);
            PlayerPrefs.Save();
        }
    }

    public void LoadSavedAnchors() {
        anchorLoader.LoadAnchorsByUuid();
    }


}
