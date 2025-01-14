using System.Collections.Generic;
using UnityEngine;

public class ConditionalRenderer : MonoBehaviour
{
    public List<GameObject> objectsToRender;

    public void SetObject(int i)
    {
        for (int index = 0; index < objectsToRender.Count; index++)
        {
            objectsToRender[index].SetActive(false);

            // Hide associated elements (anchors, markers, cards)
            HideAssociatedElements(objectsToRender[index]);
        }

        objectsToRender[i].SetActive(true);

        // Show associated elements (anchors, markers, cards)
        ShowAssociatedElements(objectsToRender[i]);
    }

    private void HideAssociatedElements(GameObject obj)
    {
        WallPrefabPlacer wallManager = obj.GetComponent<WallPrefabPlacer>();
        if (wallManager != null)
        {
            foreach (var anchor in wallManager.GetAnchors()) anchor.gameObject.SetActive(false);
        }

        TimeLineCreator timelineCreator = obj.GetComponent<TimeLineCreator>();
        if (timelineCreator != null)
        {
            foreach (var marker in timelineCreator.GetMarkers()) marker.SetActive(false);
            foreach (var card in timelineCreator.GetCards()) card.SetActive(false);
            timelineCreator.GetStartingSphere().SetActive(false);
            timelineCreator.GetActiveTimeline().SetActive(false);
        }
    }

    private void ShowAssociatedElements(GameObject obj)
    {
        WallPrefabPlacer wallManager = obj.GetComponent<WallPrefabPlacer>();
        if (wallManager != null)
        {
            foreach (var anchor in wallManager.GetAnchors()) anchor.gameObject.SetActive(true);
        }

        TimeLineCreator timelineCreator = obj.GetComponent<TimeLineCreator>();
        if (timelineCreator != null)
        {
            foreach (var marker in timelineCreator.GetMarkers()) marker.SetActive(true);
            foreach (var card in timelineCreator.GetCards()) card.SetActive(true);
            timelineCreator.GetStartingSphere().SetActive(true);
            timelineCreator.GetActiveTimeline().SetActive(true);
        }
    }
}
