using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AppController : MonoBehaviour
{
    [SerializeField] private int simulationTextureSize = 48;
    [SerializeField] private Material targetMaterial;
    [SerializeField] private ReactionSimulation reactionSimulation;
    [SerializeField] private TMP_Dropdown textureFilteringDropdown;
    [SerializeField] private FilterMode startTextureFilterMode = FilterMode.Bilinear;
    private Texture2D _texture;

    private void Start()
    {
        _texture = new Texture2D(simulationTextureSize, simulationTextureSize, TextureFormat.RGB24, false)
        {
            filterMode = startTextureFilterMode,
            wrapMode = TextureWrapMode.Clamp,
        };
        _texture.Apply();
        targetMaterial.mainTexture = _texture;

        textureFilteringDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>
        {
            new($"Point"),
            new($"Bilinear"),
            new($"Trilinear")
        };
        textureFilteringDropdown.AddOptions(options);
        textureFilteringDropdown.SetValueWithoutNotify((int)startTextureFilterMode);
        textureFilteringDropdown.onValueChanged.AddListener(value => _texture.filterMode = (FilterMode)value);
            
        reactionSimulation.Initialize(_texture.width, _texture.height);
    }

    private void Update()
    {
        reactionSimulation.UpdateReaction();
        UpdateTexture();
    }

    private void UpdateTexture()
    {
        for (var i = 0; i < _texture.height; i++)
        {
            for (var j = 0; j < _texture.width; j++)
            {
                _texture.SetPixel(i, j, reactionSimulation.GetCellColor(i, j));
            }
        }

        _texture.Apply();
    }
}