using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    public float timelineThickness = 0.02f; // Thickness of the timeline
    public float markerHeight = 0.03f; // Height of each marker
    public List<TimeLineEvent> events; // List of events for the timeline
    public GameObject cardPrefab; // Prefab for the event cards

    private float spacingBetweenMarker = 0.5f; // Spacing between each marker

    // Lists to track dynamically created markers and cards
    private List<GameObject> markers = new List<GameObject>();
    private List<GameObject> cards = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        CreateTimeLine();
    }

    // Method to create the timeline
    void CreateTimeLine()
    {
        if (events.Count == 0)
        {
            Debug.LogError("No events to create timeline");
            return;
        }

        Color timelineColor = GetComponent<Renderer>().material.color; // Get the timeline's color
        float initialSpacing = spacingBetweenMarker / 2; // Initial spacing offset
        float totalLength = initialSpacing + spacingBetweenMarker * events.Count; // Total length of the timeline

        // Calculate the starting position of the timeline
        Vector3 timelineStartPosition = transform.position - transform.right * (totalLength / 2f);

        // Create the initial sphere
        GameObject startSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        startSphere.transform.localScale = new Vector3(timelineThickness, timelineThickness, timelineThickness);

        Vector3 spherePosition = timelineStartPosition;
        spherePosition += transform.right * (timelineThickness / 2f);
        startSphere.transform.position = spherePosition;
        startSphere.transform.SetParent(transform);

        Renderer startSphereRenderer = startSphere.GetComponent<Renderer>();
        if (startSphereRenderer != null)
        {
            startSphereRenderer.material.color = timelineColor; // Set the sphere's color
        }

        transform.localScale = new Vector3(totalLength, timelineThickness, timelineThickness);

        bool isUpwards = true; // Alternate marker placement above and below the timeline

        for (int i = 0; i < events.Count; i++)
        {
            TimeLineEvent currentEvent = events[i];

            // Calculate marker's position
            float offset = i == 0 ? initialSpacing : initialSpacing + spacingBetweenMarker * i;
            Vector3 markerPosition = spherePosition + transform.right * offset;
            Vector3 direction = isUpwards ? transform.up : -transform.up;
            markerPosition += direction * (transform.localScale.y / 2f + markerHeight / 2f);

            // Create marker and add it to the markers list
            GameObject marker = CreateMarker(markerPosition, direction, currentEvent.date, timelineColor);
            markers.Add(marker);

            // Create card and add it to the cards list
            Vector3 cardPosition = markerPosition + direction * 0.25f;
            GameObject card = CreateCard(currentEvent, cardPosition);
            cards.Add(card);

            isUpwards = !isUpwards; // Alternate the direction for the next marker
        }
    }

    // Method to create a marker
    GameObject CreateMarker(Vector3 position, Vector3 direction, string date, Color timelineColor)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.transform.position = position;
        marker.transform.localScale = new Vector3(timelineThickness, markerHeight, timelineThickness);
        marker.transform.SetParent(transform);

        CreateDateText(date, marker, direction);

        Renderer markRenderer = marker.GetComponent<Renderer>();
        if (markRenderer != null)
        {
            markRenderer.material.color = timelineColor; // Set marker color
        }

        marker.name = "Marker " + date;
        return marker;
    }

    // Method to create the date text for a marker
    void CreateDateText(string date, GameObject marker, Vector3 direction)
    {
        GameObject dateObject = new GameObject("Date");
        dateObject.transform.SetParent(marker.transform);

        Vector3 datePosition = marker.transform.position - direction * markerHeight * 2;
        dateObject.transform.position = datePosition;

        TextMeshPro text = dateObject.AddComponent<TextMeshPro>();
        text.text = date;
        text.fontSize = 0.5f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
    }

    // Method to create an event card
    GameObject CreateCard(TimeLineEvent timeLineEvent, Vector3 cardPosition)
    {
        GameObject card = Instantiate(cardPrefab, cardPosition, Quaternion.identity);

        RectTransform canvasRect = card.GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = new Vector2(120, 120); // Set the canvas size
        }

        card.transform.position = cardPosition;

        Transform panel = card.transform.Find("CardCanvas/CardPanel");
        if (panel != null)
        {
            UnityEngine.UI.Image cardImage = panel.Find("Image").GetComponent<UnityEngine.UI.Image>();
            if (cardImage != null)
            {
                cardImage.sprite = timeLineEvent.image; // Set card image
            }

            TextMeshPro cardTitle = panel.Find("Title").GetComponent<TextMeshPro>();
            if (cardTitle != null)
            {
                cardTitle.text = timeLineEvent.title; // Set card title
            }

            TextMeshPro cardDescription = panel.Find("Description").GetComponent<TextMeshPro>();
            if (cardDescription != null)
            {
                cardDescription.text = timeLineEvent.description; // Set card description
            }
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
}
