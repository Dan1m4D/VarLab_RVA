using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImageOption
{
    public Sprite image;
    public string description;
}

public class ImageDatabase : MonoBehaviour
{
    public List<Sprite> images = new List<Sprite>();
    public List<string> descriptions = new List<string>();
    public List<ImageOption> imageOptions = new List<ImageOption>();

    private void OnValidate()
    {
        UpdateImageOptions();
    }

    private void Awake()
    {
        UpdateImageOptions(); // Ensure it's updated during runtime builds
    }

    private void UpdateImageOptions()
    {
        imageOptions.Clear();

        for (int i = 0; i < images.Count; i++)
        {
            ImageOption option = new ImageOption
            {
                image = images[i],
                description = i < descriptions.Count ? descriptions[i] : string.Empty
            };
            imageOptions.Add(option);
        }
    }
}
