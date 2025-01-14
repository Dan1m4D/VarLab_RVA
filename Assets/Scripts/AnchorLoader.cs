using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnchorLoader : MonoBehaviour
{
    public OVRSpatialAnchor anchorPrefab;
    public const string NumUuidsPlayerPref = "numUuids";
    private WallPrefabPlacer spatialAnchorManager;

    Action<OVRSpatialAnchor.UnboundAnchor, bool> _onLoadAnchor;

    private void Awake()
    {
        spatialAnchorManager = GetComponent<WallPrefabPlacer>();
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
            if (anchors == null)
            {
                Debug.Log("No anchors found");
                return;
            }
            foreach (var anchor in anchors)
            {
                if (anchor.Localized)
                {
                    _onLoadAnchor(anchor, true);
                }
                else if (!anchor.Localizing)
                {
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

            if (canvas.transform.childCount < 3)
            {
                Debug.LogError($"Expected 3 children in canvas, found {canvas.transform.childCount}");
                return;
            }

            var savedStatusText = canvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var descriptionText = canvas.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var wallImage = canvas.transform.GetChild(2).GetComponent<Image>();

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
                    var wallPrefabPlacer = FindObjectOfType<WallPrefabPlacer>();
                    if (wallPrefabPlacer != null && index < wallPrefabPlacer.images.Count && index < wallPrefabPlacer.descriptions.Count)
                    {
                        wallImage.sprite = wallPrefabPlacer.images[index];
                        descriptionText.text = wallPrefabPlacer.descriptions[index];
                    }
                    else
                    {
                        Debug.LogError("Invalid index or WallPrefabPlacer not found");
                    }
                    break;
                }
            }
        }
    }
}
