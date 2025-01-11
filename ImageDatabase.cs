using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImageOption : ScriptableObject
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
        imageOptions.Clear();
        for (int i = 0; i < images.Count; i++)
        {
            ImageOption option = ScriptableObject.CreateInstance<ImageOption>();
            option.image = images[i];
            option.description = i < descriptions.Count ? descriptions[i] : string.Empty;
            imageOptions.Add(option);
        }
    }
}