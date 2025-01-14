using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WallPrefabPlacer : MonoBehaviour
{

    public OVRSpatialAnchor anchorPrefab;
    public GameObject previewPrefab;
    private GameObject currentPreview;
    public const string NumUuidsPlayerPref = "numUuids";

    public List<Sprite> images;
    public List<string> descriptions;
    
    public OVRInput.Button triggerButton;
    public Transform controllerTransform;
    public OVRInput.Button nextButton;
    public OVRInput.Button previousButton;

    private Canvas canvas;
    private TextMeshProUGUI savedStatusText;
    private TextMeshProUGUI descriptionText;
    private Image wallImage;
    
    public List<OVRSpatialAnchor> anchors = new List<OVRSpatialAnchor>();
    private OVRSpatialAnchor lastCreatedAnchor;
    private AnchorLoader anchorLoader;
    private bool isInitialized;

    private int currentIndex = 0;

    public void Initialize() => isInitialized = true;

    private void Awake()
    {
        anchorLoader = GetComponent<AnchorLoader>();
        currentPreview = Instantiate(previewPrefab);

        Initialize();
    }

    void Update()
    {
        if (!isInitialized) return;

        Vector3 rayOrigin = controllerTransform.position;
        Vector3 rayDirection = controllerTransform.forward;

        // Raycast from the controller to the MRUK room
        Ray ray = new Ray(rayOrigin, rayDirection);

        if (MRUK.Instance?.GetCurrentRoom()?.Raycast(new Ray(rayOrigin, rayDirection), Mathf.Infinity, out RaycastHit hit, out MRUKAnchor anchorHit) == true)
        {

            if (anchorHit != null)
            {

                // load the preview prefab
                currentPreview.transform.position = hit.point;
                currentPreview.transform.rotation = Quaternion.LookRotation(hit.normal);


                if (OVRInput.GetDown(triggerButton, OVRInput.Controller.RTouch))
                {
                    Quaternion rotation = Quaternion.LookRotation(hit.normal);
                    CreateSpatialAnchor(hit.point, rotation);
                }
            }
        }

        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            SaveLastCreatedAnchor();
        }
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            UnsaveLastCreatedAnchor();
        }
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
        {
            UnsaveAllAnchors();
        }
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch))
        {
            LoadSavedAnchors();
        }

        if (OVRInput.GetDown(nextButton, OVRInput.Controller.RTouch))
        {
            Debug.Log("Next button pressed");
            NextImageAndDescription();
        }
        if (OVRInput.GetDown(previousButton, OVRInput.Controller.RTouch))
        {
            Debug.Log("Previous button pressed");
            PreviousImageAndDescription();
        }
    }

    public void CreateSpatialAnchor(Vector3 position, Quaternion rotation)
    {
        OVRSpatialAnchor workingAnchor = Instantiate(anchorPrefab, position, rotation);

        canvas = workingAnchor.gameObject.GetComponentInChildren<Canvas>();
        savedStatusText = canvas.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        descriptionText = canvas.gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        wallImage = canvas.gameObject.transform.GetChild(2).GetComponent<Image>();

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

        descriptionText.text = "Description";
        savedStatusText.text = "Not saved";
    }

    public List<OVRSpatialAnchor> GetAnchors()
    {
        return anchors;
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
                    textComponents[0].text = "Not saved";
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
        Debug.Log("Next image index: " + currentIndex);
        UpdateImageAndDescription();
    }

    private void PreviousImageAndDescription()
    {
        currentIndex = (currentIndex - 1 + images.Count) % images.Count;
        Debug.Log("Next image index: " + currentIndex);
        UpdateImageAndDescription();
    }

    private void UpdateImageAndDescription()
    {
        Debug.Log("Updating image and description");
        Debug.Log("Current index: " + currentIndex);
        Debug.Log("Images count: " + images.Count);
        Debug.Log("Descriptions count: " + descriptions.Count);
        if (images.Count == 0 || descriptions.Count == 0)
        {
            Debug.LogWarning("Images or descriptions are empty. No images to display.");
        }
        if (wallImage.sprite == null)
        {
            Debug.Log("Wall image sprite is null");
        }
        if (descriptionText == null)
        {
            Debug.Log("Description text is null");
        }
        if (wallImage != null && descriptionText != null)
        {
            Debug.Log("Wall image and description text are not null");
            wallImage.sprite = images[currentIndex];
            descriptionText.text = descriptions[currentIndex];
        }
        else
        {
            Debug.LogError("wallImage or descriptionText is null");
        }
    }
}
