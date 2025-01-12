using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpatialAnchorManager : MonoBehaviour
{

    public OVRSpatialAnchor anchorPrefab;
    public const string NumUuidsPlayerPref = "numUuids";
    public OVRInput.Button triggerButton;
    public Transform controllerTransform;

    public List<Sprite> images;
    public List<string> descriptions;

    private Canvas canvas;
    private TextMeshProUGUI uuidText;
    private TextMeshProUGUI savedStatusText;
    private Image anchorImage;
    private TextMeshProUGUI descriptionText;
    private List<OVRSpatialAnchor> anchors = new List<OVRSpatialAnchor>();
    private OVRSpatialAnchor lastCreatedAnchor;
    private AnchorLoader anchorLoader;
    private int currentIndex = 0;

    private void Awake() {
        anchorLoader = GetComponent<AnchorLoader>();
    }

    void Update()
    {
        if(OVRInput.GetDown(triggerButton, OVRInput.Controller.RTouch)) {
            CreateSpatialAnchor();
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
        if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x > 0.5f) {
            NextImageAndDescription();
        }
        if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x < -0.5f) {
            PreviousImageAndDescription();
        }
    }

    public void CreateSpatialAnchor() {
        OVRSpatialAnchor workingAnchor = Instantiate(anchorPrefab, controllerTransform.position , controllerTransform.rotation);

        canvas = workingAnchor.gameObject.GetComponentInChildren<Canvas>();
        uuidText = canvas.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        savedStatusText = canvas.gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        anchorImage = canvas.gameObject.transform.GetChild(2).GetComponent<Image>();
        descriptionText = canvas.gameObject.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

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
        UpdateImageAndDescription();
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

    private void NextImageAndDescription() {
        currentIndex = (currentIndex + 1) % images.Count;
        UpdateImageAndDescription();
    }

    private void PreviousImageAndDescription() {
        currentIndex = (currentIndex - 1 + images.Count) % images.Count;
        UpdateImageAndDescription();
    }

    private void UpdateImageAndDescription() {
        if (anchorImage != null && descriptionText != null) {
            anchorImage.sprite = images[currentIndex];
            descriptionText.text = descriptions[currentIndex];
        }
    }
}
