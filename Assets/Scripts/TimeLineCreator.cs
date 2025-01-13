using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using TMPro;
using UnityEngine;

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

    private float spacingBetweenMarker = 0.5f;

    

    // Start is called before the first frame update
    void Start()
    {
        CreateTimeLine();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateTimeLine()
    {
        if (events.Count == 0)
        {
            Debug.LogError("No events to create timeline");
            return;
        }

        Color timelineColor = GetComponent<Renderer>().material.color;

        float initialSpacing = spacingBetweenMarker / 2;
        float totalLength = initialSpacing + spacingBetweenMarker * events.Count;

        // Calculate the starting position of the timeline based on its length
        Vector3 timelineStartPosition = transform.position - transform.right * (totalLength / 2f);

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
        transform.localScale = new Vector3(totalLength, timelineThickness, timelineThickness);

        bool isUpwards = true;

        for (int i = 0; i < events.Count; i++)
        {
            TimeLineEvent currentEvent = events[i];

            // Calculate marker's position
            float offset = i == 0 ?
                initialSpacing :
                initialSpacing + spacingBetweenMarker * i;

            Vector3 markerPosition = spherePosition + transform.right * offset;

            // Offset maker's position above or below the timeline
            Vector3 direction = isUpwards ? transform.up : -transform.up;
            markerPosition += direction * (transform.localScale.y / 2f + markerHeight / 2f);

            CreateMarker(
                position: markerPosition,
                direction: direction,
                date: currentEvent.date,
                timelineColor: timelineColor
            );

            Vector3 cardPosition = markerPosition + direction * 0.25f;
            CreateCard(
                timeLineEvent: currentEvent,
                cardPosition: cardPosition
            );

            isUpwards = !isUpwards;
        }
    }

    void CreateMarker(Vector3 position, Vector3 direction, string date, Color timelineColor)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.transform.position = position;
        marker.transform.localScale = new Vector3(timelineThickness, markerHeight, timelineThickness);
        marker.transform.SetParent(transform);

        CreateDateText(
            date: date,
            marker: marker,
            direction: direction
        );
        
        Renderer markRenderer = marker.GetComponent<Renderer>();
        if (markRenderer != null)
        {
            markRenderer.material.color = timelineColor;
        }

        marker.name = "Marker " + date;
    }

    void CreateDateText(string date, GameObject marker, Vector3 direction)
    {
        GameObject dateObject = new GameObject("Date");
        dateObject.transform.SetParent(marker.transform);

        // Calculate the position for the date text
        Vector3 datePosition = marker.transform.position - direction * markerHeight * 2;
        dateObject.transform.position = datePosition;

        // Add the date text
        TextMeshPro text = dateObject.AddComponent<TextMeshPro>();
        text.text = date;
        text.fontSize = 0.5f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;

    }

    void CreateCard(TimeLineEvent timeLineEvent, Vector3 cardPosition)
    {
        GameObject card = Instantiate(cardPrefab, cardPosition, Quaternion.identity);

        RectTransform canvasRect = card.GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = new Vector2(120, 120);
        }   

        card.transform.position = cardPosition;

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
