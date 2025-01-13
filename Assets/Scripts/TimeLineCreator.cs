using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using TMPro;
using UnityEngine;

[System.Serializable]
public class TimelineData
{
    public Vector3 position;
    public Quaternion rotation;
    public List<TimeLineEvent> events;
}

[System.Serializable]
public class TimeLineEvent
{
    public string date;
    public string title;
    public string description;
    public Sprite image;
}

public class TimeLineCreator : MonoBehaviour
{
    public float timelineThickness = 0.02f;
    public float markerHeight = 0.03f;
    public List<TimeLineEvent> events;
    public GameObject cardPrefab;
    public GameObject timelinePrefab;
    public GameObject previewPrefab;
    public OVRInput.Button triggerButton;
    public Transform controllerTransform;

    private float spacingBetweenMarker = 0.5f;
    private GameObject previewObject;
    private bool isPreviewVisible = false;
    private GameObject activeTimeline;

    // Start is called before the first frame update
    void Start()
    { 
        if (LoadTimelineData(out TimelineData loadedData))
        {
            activeTimeline = Instantiate(timelinePrefab, loadedData.position, loadedData.rotation);
            events = loadedData.events;
            CreateTimeLine(activeTimeline, loadedData.rotation);
        }

        if (previewPrefab != null)
        {
            previewObject = Instantiate(previewPrefab);
            previewObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleWallPlacement();
    }

    private void HandleWallPlacement()
    {
        if (controllerTransform == null) return;

        // Raycast from the controller to detect valid wall surfaces
        Vector3 rayOrigin = controllerTransform.position;
        Vector3 rayDirection = controllerTransform.forward;
        Ray ray = new Ray(rayOrigin, rayDirection);

        if (MRUK.Instance?.GetCurrentRoom()?.Raycast(ray, Mathf.Infinity, out RaycastHit hit, out MRUKAnchor anchorHit) == true)
        {
            if (OVRInput.GetDown(triggerButton, OVRInput.Controller.RTouch))
            {
                previewObject.transform.position = hit.point;
                previewObject.transform.rotation = Quaternion.LookRotation(hit.normal);

                if (!isPreviewVisible)
                {
                    previewObject.SetActive(true);
                    isPreviewVisible = true;
                }

                // Place the timeline prefab when the trigger button is pressed
                if (OVRInput.GetDown(triggerButton, OVRInput.Controller.RTouch))
                {
                    PlaceTimeline(hit.point, hit.normal);
                }
            }
        }
        else
        {
            if (isPreviewVisible)
            {
                previewObject.SetActive(false);
                isPreviewVisible = false;
            }
        }
    }

    private void SaveTimelineData(Vector3 position, Quaternion rotation)
    {
        TimelineData data = new TimelineData
        {
            position = position,
            rotation = rotation,
            events = events
        };

        string jsonData = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SavedTimeline", jsonData);
        PlayerPrefs.Save();
    }

    private bool LoadTimelineData(out TimelineData data)
    {
        data = null;
        if (PlayerPrefs.HasKey("SavedTimeline"))
        {
            string jsonData = PlayerPrefs.GetString("SavedTimeline");
            data = JsonUtility.FromJson<TimelineData>(jsonData);
            return true;
        }
        return false;
    }

    private void PlaceTimeline(Vector3 position, Vector3 normal)
    {
        if (timelinePrefab == null) return;

        if (activeTimeline != null)
        {
            Destroy(activeTimeline);
        }

        // Instantiate the timeline at the position with the correct orientation
        Quaternion rotation = Quaternion.LookRotation(-normal);
        GameObject newTimeline = Instantiate(timelinePrefab, position, rotation);

        SaveTimelineData(
            position: position, 
            rotation: rotation
        );

        CreateTimeLine(timeline: newTimeline, rotation: rotation);

        // Hide the preview after placement
        previewObject.SetActive(false);
        isPreviewVisible = false;
    }


    void CreateTimeLine(GameObject timeline, Quaternion rotation)
    {
        if (events.Count == 0)
        {
            Debug.LogError("No events to create timeline");
            return;
        }

        Color timelineColor = timeline.GetComponent<Renderer>().material.color;

        float initialSpacing = spacingBetweenMarker / 2;
        float totalLength = initialSpacing + spacingBetweenMarker * events.Count;

        // Calculate the starting position of the timeline based on its length
        Vector3 timelineStartPosition = timeline.transform.position - timeline.transform.right * (totalLength / 2f);

        // Create the initial sphere
        GameObject startSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        startSphere.transform.localScale = new Vector3(timelineThickness, timelineThickness, timelineThickness);

        Vector3 spherePosition = timelineStartPosition;
        spherePosition += transform.right * (timelineThickness / 2f);
        startSphere.transform.position = spherePosition;
        startSphere.transform.SetParent(transform);

        // Change the sphere's color to match the timeline
        Renderer startSphereRenderer = startSphere.GetComponent<Renderer>();
        if (startSphereRenderer != null)
        {
            startSphereRenderer.material.color = timelineColor;
        }

        // Scale the timeline to match its calculated length
        timeline.transform.localScale = new Vector3(totalLength, timelineThickness, timelineThickness);

        bool isUpwards = true;

        for (int i = 0; i < events.Count; i++)
        {
            TimeLineEvent currentEvent = events[i];

            // Calculate marker's position
            float offset = i == 0 ?
                initialSpacing :
                initialSpacing + spacingBetweenMarker * i;

            Vector3 markerPosition = spherePosition + timeline.transform.right * offset;

            // Offset maker's position above or below the timeline
            Vector3 direction = isUpwards ? timeline.transform.up : -timeline.transform.up;
            markerPosition += direction * (timeline.transform.localScale.y / 2f + markerHeight / 2f);

            CreateMarker(
                position: markerPosition,
                direction: direction,
                date: currentEvent.date,
                timelineColor: timelineColor,
                rotation: rotation
            );

            Vector3 cardPosition = markerPosition + direction * 0.25f;
            CreateCard(
                timeLineEvent: currentEvent,
                cardPosition: cardPosition,
                rotation: rotation
            );

            isUpwards = !isUpwards;
        }
    }

    private void CreateMarker(Vector3 position, Vector3 direction, string date, Color timelineColor, Quaternion rotation)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.transform.position = position;
        marker.transform.rotation = rotation;
        marker.transform.localScale = new Vector3(timelineThickness, markerHeight, timelineThickness);
        marker.transform.SetParent(transform);

        CreateDateText(
            date: date,
            marker: marker,
            direction: direction,
            rotation: rotation
        );
        
        Renderer markRenderer = marker.GetComponent<Renderer>();
        if (markRenderer != null)
        {
            markRenderer.material.color = timelineColor;
        }

        marker.name = "Marker " + date;
    }

    void CreateDateText(string date, GameObject marker, Vector3 direction, Quaternion rotation)
    {
        GameObject dateObject = new GameObject("Date");
        dateObject.transform.SetParent(marker.transform);

        // Calculate the position for the date text
        Vector3 datePosition = marker.transform.position - direction * markerHeight * 2;
        dateObject.transform.position = datePosition;
        dateObject.transform.rotation = rotation;

        // Add the date text
        TextMeshPro text = dateObject.AddComponent<TextMeshPro>();
        text.text = date;
        text.fontSize = 0.5f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;

    }

    void CreateCard(TimeLineEvent timeLineEvent, Vector3 cardPosition, Quaternion rotation)
    {
        GameObject card = Instantiate(cardPrefab, cardPosition, Quaternion.identity);

        RectTransform canvasRect = card.GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = new Vector2(120, 120);
        }   

        card.transform.position = cardPosition;
        card.transform.rotation = rotation;

        Transform panel = card.transform.Find("CardCanvas/CardPanel");
        if (panel != null)
        {
            UnityEngine.UI.Image cardImage = panel.Find("Image").GetComponent<UnityEngine.UI.Image>();
            if (cardImage != null)
            {
                cardImage.sprite = timeLineEvent.image;
            }

            TextMeshPro cardTitle = panel.Find("Title").GetComponent<TextMeshPro>();
            if (cardTitle != null)
            {
                cardTitle.text = timeLineEvent.title;
            }

            TextMeshPro cardDescription = panel.Find("Description").GetComponent<TextMeshPro>();
            if (cardDescription != null)
            {
                cardDescription.text = timeLineEvent.description;
            }
        }
    }
}
