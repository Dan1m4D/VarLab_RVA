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
    public const string NumUuidsPlayerPref = "equipmentNumUuids";

    public List<Sprite> images;
    public List<string> descriptions;

    public OVRInput.Button triggerButton;
    public Transform controllerTransform;
    public OVRInput.Controller controller;
    public OVRInput.Button nextButton;
    public OVRInput.Button previousButton;

    private Canvas canvas;
    private TextMeshProUGUI savedStatusText;
    private TextMeshProUGUI nameText;
    private Image anchorImage;

    private List<OVRSpatialAnchor> anchors = new List<OVRSpatialAnchor>();
    private OVRSpatialAnchor lastCreatedAnchor;
    private EquipmentAnchorLoader anchorLoader;
    private bool isInitialized;

    private int currentIndex = 0;

    private void Awake()
    {
        anchorLoader = GetComponent<EquipmentAnchorLoader>();
        Debug.Log("SpatialAnchorManager Awake");

        // Ensure controllerTransform is assigned
        if (controllerTransform == null)
        {
            controllerTransform = GameObject.Find("YourControllerName").transform; // Replace "YourControllerName" with the actual name of your controller GameObject
        }
    }

    void Update()
    {
        if (OVRInput.GetDown(triggerButton, controller))
        {
            Debug.Log("Trigger button pressed");
            CreateSpatialAnchor();
        }
        if (OVRInput.GetDown(OVRInput.Button.One, controller))
        {
            Debug.Log("Button One pressed");
            SaveLastCreatedAnchor();
        }
        if (OVRInput.GetDown(OVRInput.Button.Two, controller))
        {
            Debug.Log("Button Two pressed");
            UnsaveLastCreatedAnchor();
        }
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, controller))
        {
            Debug.Log("Primary Hand Trigger pressed");
            UnsaveAllAnchors();
        }
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, controller))
        {
            Debug.Log("Primary Thumbstick pressed");
            LoadSavedAnchors();
        }

        if (OVRInput.GetDown(nextButton, controller))
        {
            Debug.Log("Next button pressed");
            NextImageAndDescription();
        }
        if (OVRInput.GetDown(previousButton, controller))
        {
            Debug.Log("Previous button pressed");
            PreviousImageAndDescription();
        }
    }

    public void CreateSpatialAnchor()
    {
        Debug.Log("Creating Spatial Anchor");

        // Log the controller's position and rotation
        Debug.Log($"Controller Position: {controllerTransform.position}");
        Debug.Log($"Controller Rotation: {controllerTransform.rotation}");

        // Instantiate the anchor prefab at the controller's position and rotation
        OVRSpatialAnchor workingAnchor = Instantiate(anchorPrefab, controllerTransform.position, controllerTransform.rotation);

        // Get the canvas and UI elements from the instantiated prefab
        canvas = workingAnchor.gameObject.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            nameText = canvas.gameObject.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            savedStatusText = canvas.gameObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
            anchorImage = canvas.gameObject.transform.GetChild(0).GetChild(2).GetComponent<Image>();

            Debug.Log("Canvas and UI elements assigned");
        }
        else
        {
            Debug.LogError("Canvas not found in the anchor prefab");
        }

        StartCoroutine(AnchorCreated(workingAnchor));
    }

    private IEnumerator AnchorCreated(OVRSpatialAnchor workingAnchor)
    {
        while (!workingAnchor.Created && !workingAnchor.Localized)
        {
            yield return new WaitForEndOfFrame();
        }

        Guid anchorUuid = workingAnchor.Uuid;
        anchors.Add(workingAnchor);
        lastCreatedAnchor = workingAnchor;

        nameText.text = "Name";
        savedStatusText.text = "Not saved"; 
        UpdateImageAndDescription();
    }

    private void SaveLastCreatedAnchor()
    {
        lastCreatedAnchor.Save((lastCreatedAnchor, success) =>
        {
            if (success)
            {
                savedStatusText.text = "Saved";
            }
        });

        SaveUuidToPlayerPrefs(lastCreatedAnchor.Uuid, currentIndex);
    }

    void SaveUuidToPlayerPrefs(Guid uuid, int index)
    {
        if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
        }

        int playerNumUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
        PlayerPrefs.SetString("uuid" + playerNumUuids, uuid.ToString());
        PlayerPrefs.SetInt("index" + playerNumUuids, index);
        PlayerPrefs.SetInt(NumUuidsPlayerPref, ++playerNumUuids);

    }

    private void UnsaveLastCreatedAnchor()
    {
        lastCreatedAnchor.Erase((lastCreatedAnchor, success) =>
        {
            if (success)
            {
                savedStatusText.text = "Not saved";
            }
        });

        // Destroy(lastCreatedAnchor.gameObject);
    }

    private void UnsaveAllAnchors()
    {
        foreach (var anchor in anchors)
        {
            UnsaveAnchor(anchor);
        }
        anchors.Clear();
        ClearAllUuidsFromPlayerPrefs();
    }

    private void UnsaveAnchor(OVRSpatialAnchor anchor)
    {
        anchor.Erase((erasedAnchor, success) =>
        {
            if (success)
            {
                var textComponents = anchor.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
                if (textComponents.Length > 1)
                {
                    textComponents[1].text = "Not saved";
                }
            }
        });

        // Destroy(anchor.gameObject);
    }

    void ClearAllUuidsFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(NumUuidsPlayerPref))
        {
            int playerNumUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            for (int i = 0; i < playerNumUuids; i++)
            {
                PlayerPrefs.DeleteKey("uuid" + i);
                PlayerPrefs.DeleteKey("index" + i);
            }
            PlayerPrefs.DeleteKey(NumUuidsPlayerPref);
            PlayerPrefs.Save();
        }
    }

    public void LoadSavedAnchors()
    {
        anchorLoader.LoadAnchorsByUuid();
    }

    private void NextImageAndDescription()
    {
        currentIndex = (currentIndex + 1) % images.Count;
        UpdateImageAndDescription();
    }

    private void PreviousImageAndDescription()
    {
        currentIndex = (currentIndex - 1 + images.Count) % images.Count;
        UpdateImageAndDescription();
    }

    private void UpdateImageAndDescription()
    {
        if (anchorImage != null)
        {
            anchorImage.sprite = images[currentIndex];
            nameText.text = descriptions[currentIndex];
        }
        else
        {
            Debug.LogError("Anchor image or description text is null");
        }
    }

    public List<OVRSpatialAnchor> GetAnchors()
    {
        return anchors;
    }
}
