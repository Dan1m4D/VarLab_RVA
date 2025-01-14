using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentAnchorLoader : MonoBehaviour
{
    public OVRSpatialAnchor anchorPrefab;
    public const string NumUuidsPlayerPref = "equipmentNumUuids";
    private SpatialAnchorManager spatialAnchorManager;

    Action<OVRSpatialAnchor.UnboundAnchor, bool> _onLoadAnchor;


    private void Awake()
    {
        spatialAnchorManager = GetComponent<SpatialAnchorManager>();
        anchorPrefab = spatialAnchorManager.anchorPrefab;
        _onLoadAnchor = OnLocalized;
    }

    [Obsolete]
    public void LoadAnchorsByUuid()
    {
        if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
        }

        var playerUuidCount = PlayerPrefs.GetInt(NumUuidsPlayerPref);
        if (playerUuidCount == 0)
        {
            Debug.Log("No UUIDs found in PlayerPrefs");
            return;
        }

        var uuids = new Guid[playerUuidCount];
        for (int i = 0; i < playerUuidCount; i++)
        {
            var uuidKey = "uuid" + i;
            var currentUuid = PlayerPrefs.GetString(uuidKey);
            uuids[i] = new Guid(currentUuid);
        }

        Load(new OVRSpatialAnchor.LoadOptions
        {
            Timeout = 0,
            StorageLocation = OVRSpace.StorageLocation.Local,
            Uuids = uuids
        });
    }

    private void Load(OVRSpatialAnchor.LoadOptions loadOptions)
    {
        OVRSpatialAnchor.LoadUnboundAnchors(loadOptions, anchors =>
        {
            if (anchors == null || anchors.Length == 0)
            {
                Debug.Log("No anchors found");
                return;
            }

            foreach (var anchor in anchors)
            {
                if (anchor.Localized)
                {
                    Debug.Log($"Anchor localized: {anchor.Uuid}");
                    _onLoadAnchor(anchor, true);
                }
                else if (!anchor.Localizing)
                {
                    Debug.Log($"Localizing anchor: {anchor.Uuid}");
                    anchor.Localize(_onLoadAnchor);
                }
            }
        });

    }

    private void OnLocalized(OVRSpatialAnchor.UnboundAnchor unboundAnchor, bool success)
    {
        if (!success)
        {
            Debug.Log("Failed to localize anchor");
            return;
        }

        var pose = unboundAnchor.Pose;
        var spatialAnchor = Instantiate(anchorPrefab, pose.position, pose.rotation);
        unboundAnchor.BindTo(spatialAnchor);

        if (spatialAnchor.TryGetComponent<OVRSpatialAnchor>(out var anchor))
        {
            var canvas = spatialAnchor.GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found in anchor prefab");
                return;
            }

            

            var descriptionText = canvas.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            var savedStatusText = canvas.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
            var wallImage = canvas.transform.GetChild(0).GetChild(2).GetComponent<Image>();

            if (descriptionText == null || savedStatusText == null || wallImage == null)
            {
                Debug.LogError("One or more UI components not found in anchor prefab");
                return;
            }

            descriptionText.text = "Uuid: " + spatialAnchor.Uuid.ToString();
            savedStatusText.text = "";

            // Load the saved index for the image and description
            int playerNumUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            for (int i = 0; i < playerNumUuids; i++)
            {
                string uuidString = PlayerPrefs.GetString("uuid" + i);
                if (uuidString == spatialAnchor.Uuid.ToString())
                {
                    int index = PlayerPrefs.GetInt("index" + i);
                    // Assuming you have access to the lists of images and descriptions
                    var spatialAnchorManager = FindObjectOfType<SpatialAnchorManager>();
                    if (spatialAnchorManager != null && index < spatialAnchorManager.images.Count && index < spatialAnchorManager.descriptions.Count)
                    {
                        wallImage.sprite = spatialAnchorManager.images[index];
                        descriptionText.text = spatialAnchorManager.descriptions[index];
                    }
                    else
                    {
                        Debug.LogError("Invalid index or SpatialAnchorManager not found");
                    }
                    break;
                }
            }
        }
    }
}
