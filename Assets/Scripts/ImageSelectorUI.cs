using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ImageSelectorUI : MonoBehaviour
{
    public GameObject imageButtonPrefab;
    public Transform contentPanel;
    public TextMeshProUGUI descriptionField;
    public Button applyButton;
    private ImageOption selectedOption;

    public ImageDatabase imageDatabase;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            PopulateUI();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error populating UI: {ex.Message}");
        }
        applyButton.onClick.AddListener(ApplySelection);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PopulateUI()
    {
        Debug.Log("Populating UI on device...");


        if (imageDatabase == null)
        {
            Debug.LogError("ImageDatabase is null. Please assign it in the Inspector.");
            return;
        }

        if (imageDatabase.imageOptions == null)
        {
            Debug.LogError("ImageDatabase.imageOptions is null. Ensure it's initialized and populated.");
            return;
        }

        if (imageDatabase.imageOptions.Count == 0)
        {
            Debug.LogWarning("ImageDatabase.imageOptions is empty. No images to display.");
            return;
        }
        // Get all the images in the Resources folder
        try
        {
            Debug.Log("Starting PopulateUI...");
            foreach (var imageOption in imageDatabase.imageOptions)
            {
                var buttonObject = Instantiate(imageButtonPrefab, contentPanel);
                var oldIMage = buttonObject.GetComponentInChildren<Image>();
                if (oldIMage == null)
                {
                    Debug.Log("Image component not found in the button prefab");
                }


                var image = buttonObject.GetComponentInChildren<Button>().GetComponentInChildren<Image>();
                if (image == null)
                {
                    Debug.Log("Image component not found in the button prefab");
                }
                var button = buttonObject.GetComponentInChildren<Button>();

                image.sprite = imageOption.image;
                button.onClick.AddListener(() => OnButtonClicked(imageOption));
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in PopulateUI: {ex.Message}\n{ex.StackTrace}");
        }
        Debug.Log("...UI Populated!!!");

        Debug.Log($"Content Panel Child Count: {contentPanel.childCount}");

        // Force layout rebuild
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());


    }

    void OnButtonClicked(ImageOption imageOption)
    {
        selectedOption = imageOption;
        descriptionField.text = imageOption.description;
        applyButton.interactable = true;
    }

    public void ApplySelection()
    {
        Debug.Log($"Selected Image: {selectedOption.image.name}, Description: {selectedOption.description}");
    }
}