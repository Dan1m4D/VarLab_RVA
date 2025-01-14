    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalRenderer : MonoBehaviour
{
    // Script to handle the rendering of objects based on the selected part
    public List<GameObject> objectsToRender;

    public void SetObject(int i)
    {
        for (int index = 0; index < objectsToRender.Count; index++)
        {
            // Deactivate the object
            objectsToRender[index].SetActive(false);

            // Hide anchors dynamically for the object
            HideAnchors(objectsToRender[index]);
        }

        // Activate the selected object
        objectsToRender[i].SetActive(true);

        // Show anchors dynamically for the selected object
        ShowAnchors(objectsToRender[i]);
    }

    private void HideAnchors(GameObject obj)
    {
        WallPrefabPlacer manager = obj.GetComponent<WallPrefabPlacer>();
        if (manager != null)
        {
            List<OVRSpatialAnchor> spatialAnchors = manager.GetAnchors(); // Get anchors from the manager
            List<GameObject> anchors = new List<GameObject>();
            foreach (OVRSpatialAnchor spatialAnchor in spatialAnchors)
            {
                anchors.Add(spatialAnchor.gameObject);
            }
            foreach (GameObject anchor in anchors)
            {
                anchor.SetActive(false); // Hide each anchor
            }
        }

        TimeLineCreator timelineCreator = obj.GetComponent<TimeLineCreator>();
        if (timelineCreator != null)
        {

            var markers = timelineCreator.GetMarkers();
            if (markers != null)
            {
                foreach (GameObject marker in markers)
                {
                    marker.SetActive(false); // Hide each marker
                }
            }

            var cards = timelineCreator.GetCards();
            if (cards != null)
            {
                foreach (GameObject card in cards)
                {
                    card.SetActive(false); // Hide each card
                }
            }

            timelineCreator.GetActiveTimeline().SetActive(false); // Hide the active timeline
        }

        SpatialAnchorManager spatialAnchorManager = obj.GetComponent<SpatialAnchorManager>();
        if (spatialAnchorManager != null)
        {
            var spatial_anchors = spatialAnchorManager.GetAnchors();
            var _anchors = new List<GameObject>();
            foreach (OVRSpatialAnchor spatial_anchor in spatial_anchors)
            {
                _anchors.Add(spatial_anchor.gameObject);
            }

            if (_anchors != null)
            {
                foreach (GameObject anchor in _anchors)
                {
                    anchor.SetActive(false); // Hide each anchor
                }
            }
        }


    }

    private void ShowAnchors(GameObject obj)
    {
        WallPrefabPlacer manager = obj.GetComponent<WallPrefabPlacer>();
        if (manager != null)
        {
            List<OVRSpatialAnchor> spatialAnchors = manager.GetAnchors(); // Get anchors from the manager
            List<GameObject> anchors = new List<GameObject>();
            foreach (OVRSpatialAnchor spatialAnchor in spatialAnchors)
            {
                anchors.Add(spatialAnchor.gameObject);
            }
            foreach (GameObject anchor in anchors)
            {
                anchor.SetActive(true); // Show each anchor
            }
        }

        TimeLineCreator timelineCreator = obj.GetComponent<TimeLineCreator>();
        if (timelineCreator != null)
        {

            var markers = timelineCreator.GetMarkers();
            if (markers != null)
            {
                foreach (GameObject marker in markers)
                {
                    marker.SetActive(true); // Show each marker
                }
            }

            var cards = timelineCreator.GetCards();
            if (cards != null)
            {
                foreach (GameObject card in cards)
                {
                    card.SetActive(true); // Show each card
                }
            }

            timelineCreator.GetActiveTimeline().SetActive(true); // Show the active timeline
        }

        SpatialAnchorManager spatialAnchorManager = obj.GetComponent<SpatialAnchorManager>();
        if (spatialAnchorManager != null)
        {
            var spatial_anchors = spatialAnchorManager.GetAnchors();
            var _anchors = new List<GameObject>();
            foreach (OVRSpatialAnchor spatial_anchor in spatial_anchors)
            {
                _anchors.Add(spatial_anchor.gameObject);
            }

            if (_anchors != null)
            {
                foreach (GameObject anchor in _anchors)
                {
                    anchor.SetActive(true); // Show each anchor
                }
            }
        }
    }
}