using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class AnchorLoader : MonoBehaviour
{
    private OVRSpatialAnchor anchorPrefab;
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
        if (!PlayerPrefs.HasKey(SpatialAnchorManager.NumUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(SpatialAnchorManager.NumUuidsPlayerPref, 0);
        }

        var playerUuidCount = PlayerPrefs.GetInt(SpatialAnchorManager.NumUuidsPlayerPref);
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

        Load(new OVRSpatialAnchor.LoadOptions{
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
            var uuid = spatialAnchor.GetComponentInChildren<Canvas>().transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var savedStatusText = spatialAnchor.GetComponentInChildren<Canvas>().transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            uuid.text = "Uuid: " + spatialAnchor.Uuid.ToString();
            savedStatusText.text = "Loaded from Device";
        }
    }
}
