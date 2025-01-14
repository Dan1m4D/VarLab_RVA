using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public OVRInput.Button triggerButton;
    public Transform controllerTransform;

    private float spacingBetweenMarker = 0.5f;
    private GameObject activeTimeline;
    private GameObject startingSphere;

    private List<GameObject> markers = new List<GameObject>();
    private List<GameObject> cards = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        if (LoadTimelineData(out TimelineData loadedData))
        {
            activeTimeline = Instantiate(timelinePrefab, loadedData.position, loadedData.rotation);
            events = loadedData.events;
            CreateTimeLine(activeTimeline, loadedData.rotation);
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
            if (OVRInput.GetDown(triggerButton, OVRInput.Controller.RTouch)) PlaceTimeline(hit.point, hit.normal);
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

            foreach (GameObject marker in markers) Destroy(marker);
            foreach (GameObject card in cards) Destroy(card);
            if (startingSphere != null) Destroy(startingSphere);

            markers.Clear();
            cards.Clear();

            // Delete the saved timeline data
            PlayerPrefs.DeleteKey("SavedTimeline");
        }

        // Instantiate the timeline at the position with the correct orientation
        Quaternion rotation = Quaternion.LookRotation(-normal);
        activeTimeline = Instantiate(timelinePrefab, position, rotation);

        SaveTimelineData(
            position: position,
            rotation: rotation
        );

        CreateTimeLine(timeline: activeTimeline, rotation: rotation);
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
        startingSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        startingSphere.transform.localScale = new Vector3(timelineThickness * 3, timelineThickness * 3, timelineThickness * 3);

        Vector3 spherePosition = timelineStartPosition;
        spherePosition += transform.right * (timelineThickness / 2f);
        startingSphere.transform.position = spherePosition;
        startingSphere.transform.SetParent(transform);

        Renderer startSphereRenderer = startingSphere.GetComponent<Renderer>();
        if (startSphereRenderer != null)
        {
            startSphereRenderer.material.color = timelineColor; // Set the sphere's color
        }

        // Scale the timeline to match its calculated length
        timeline.transform.localScale = new Vector3(totalLength, timelineThickness, timelineThickness);

        bool isUpwards = true; // Alternate marker placement above and below the timeline

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

            GameObject marker = CreateMarker(
                position: markerPosition,
                direction: direction,
                date: currentEvent.date,
                timelineColor: timelineColor,
                rotation: rotation
            );
            markers.Add(marker);

            // Create card and add it to the cards list
            Vector3 cardPosition = markerPosition + direction * 0.25f;
            GameObject card = CreateCard(
                timeLineEvent: currentEvent,
                cardPosition: cardPosition,
                rotation: rotation
            );
            cards.Add(card);

            isUpwards = !isUpwards; // Alternate the direction for the next marker
        }
    }

    private GameObject CreateMarker(Vector3 position, Vector3 direction, string date, Color timelineColor, Quaternion rotation)
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
            markRenderer.material.color = timelineColor; // Set marker color
        }

        marker.name = "Marker " + date;
        return marker;
    }

    void CreateDateText(string date, GameObject marker, Vector3 direction, Quaternion rotation)
    {
        GameObject dateObject = new GameObject("Date");
        dateObject.transform.SetParent(marker.transform);

        Vector3 datePosition = marker.transform.position - direction * markerHeight * 2;
        dateObject.transform.position = datePosition;
        dateObject.transform.rotation = rotation;

        TextMeshPro text = dateObject.AddComponent<TextMeshPro>();
        text.text = date;
        text.fontSize = 0.5f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
    }

    GameObject CreateCard(TimeLineEvent timeLineEvent, Vector3 cardPosition, Quaternion rotation)
    {
        GameObject card = Instantiate(cardPrefab, cardPosition, Quaternion.identity);

        RectTransform canvasRect = card.GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = new Vector2(160, 160); // Set the canvas size
        }

        card.transform.position = cardPosition;
        card.transform.rotation = rotation;

        Canvas canvas = card.GetComponentInChildren<Canvas>();
        
        Image cardImage = canvas.gameObject.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = timeLineEvent.image; // Set the card image
        }

        TextMeshProUGUI titleText = canvas.gameObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        if (titleText != null)
        {
            titleText.text = timeLineEvent.title; // Set the card title
        }

        TextMeshProUGUI descriptionText = canvas.gameObject.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>();
        if (descriptionText != null)
        {
            descriptionText.text = timeLineEvent.description; // Set the card description
        }

        return card;
    }

    // Public method to get the list of markers
    public List<GameObject> GetMarkers()
    {
        return markers;
    }

    // Public method to get the list of cards
    public List<GameObject> GetCards()
    {
        return cards;
    }

    public GameObject GetStartingSphere()
    {
        return startingSphere;
    }

    public GameObject GetActiveTimeline()
    {
        return activeTimeline;
    }
}
