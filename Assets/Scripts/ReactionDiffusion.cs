using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ReactionDiffusion : MonoBehaviour
{
    public float diffusionA = 1.0f;
    public float diffusionB = 0.5f;
    public float feedRate = 0.055f;
    public float killRate = 0.062f;
    public float simulationSpeed = 1.0f;
    
    [SerializeField] private Material targetMaterial;
    private Texture2D _texture;
    private readonly List<float> _convolution = new()
    {
        0.05f, 0.2f, 0.05f,
        0.2f, -1.0f, 0.2f,
        0.05f, 0.2f, 0.05f,
    };

    private struct Cell
    {
        public float A;
        public float B;

        public Cell(float a, float b)
        {
            A = a;
            B = b;
        }
    }

    private readonly List<List<Cell>> _reaction = new();
    private readonly List<List<Cell>> _reactionLast = new();

    private void Start()
    {
        SetupTexture();
        SetupCells();
    }

    private void SetupCells()
    {
        var c = new Vector2(_texture.width / 4.0f, _texture.height / 6.0f);
        const float r = 20.0f;
        for (var i = 0; i < _texture.height; i++)
        {
            var row = new List<Cell>();
            for (var j = 0; j < _texture.width; j++)
            {
                var cell = new Cell(1.0f, 0.0f);
                
                // A circle of B in the middle.
                if ((c.y - i) * (c.y - i) + (c.x - j) * (c.x - j) < r * r)
                {
                    cell.A = 0.0f;
                    cell.B = 1.0f;
                }
                row.Add(cell);
            }

            _reaction.Add(row);
            _reactionLast.Add(row);
        }
    }

    private void SetupTexture()
    {
        _texture = new Texture2D(256, 256, TextureFormat.RGB24, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        _texture.Apply();

        targetMaterial.mainTexture = _texture;
    }

    private void DisplayCells()
    {
        for (var i = 0; i < _texture.height; i++)
        {
            for (var j = 0; j < _texture.width; j++)
            {
                var cell = _reaction[i][j];
                _texture.SetPixel(
                    i,
                    j,
                    new Color(cell.A, 0.0f, cell.B, 1.0f)
                );
            }
        }

        _texture.Apply();
    }

    private void Update()
    {
        UpdateReaction();
        DisplayCells();
    }

    private void UpdateReaction()
    {
        // for (var i = 1; i < _texture.height - 1; i++)
        // {
            // for (var j = 1; j < _texture.width - 1; j++)
            // {
            //     var cell = _reactionLast[i][j];
            //     var chanceAtoB = cell.A * cell.B * cell.B;
            //     cell.A += (diffusionA * ConvoluteA(i, j, _reactionLast) - chanceAtoB + feedRate * (1.0f - cell.A)) * simulationSpeed;
            //     cell.B += (diffusionB * ConvoluteB(i, j, _reactionLast) + chanceAtoB - (killRate + feedRate) * cell.B) * simulationSpeed;
            //     _reaction[i][j] = cell;
            // }

            Parallel.For(
                1,
                _texture.height - 1,
                i =>
                {
                    Parallel.For(
                        1,
                        _texture.width - 1,
                        j =>
                        {
                            var cell = _reactionLast[i][j];
                            var chanceAtoB = cell.A * cell.B * cell.B;
                            cell.A += (diffusionA * ConvoluteA(i, j, _reactionLast) - chanceAtoB + feedRate * (1.0f - cell.A)) * simulationSpeed;
                            cell.B += (diffusionB * ConvoluteB(i, j, _reactionLast) + chanceAtoB - (killRate + feedRate) * cell.B) * simulationSpeed;
                            _reaction[i][j] = cell;
                        }
                    );
                }
            );

        for (var i = 0; i < _texture.height; i++)
        {
            for (var j = 0; j < _texture.width; j++)
            {
                _reactionLast[i][j] = _reaction[i][j];
            }
        }
    }

    private float ConvoluteA(int x, int y, List<List<Cell>> source)
    {
        var result = 0.0f;
        result += _convolution[0] * source[x - 1][y - 1].A;
        result += _convolution[1] * source[x][y - 1].A;
        result += _convolution[2] * source[x + 1][y - 1].A;
        result += _convolution[3] * source[x - 1][y].A;
        result += _convolution[4] * source[x][y].A;
        result += _convolution[5] * source[x + 1][y].A;
        result += _convolution[6] * source[x - 1][y + 1].A;
        result += _convolution[7] * source[x][y + 1].A;
        result += _convolution[8] * source[x + 1][y + 1].A;
        return result;
    }

    private float ConvoluteB(int x, int y, List<List<Cell>> source)
    {
        var result = 0.0f;
        result += _convolution[0] * source[x - 1][y - 1].B;
        result += _convolution[1] * source[x][y - 1].B;
        result += _convolution[2] * source[x + 1][y - 1].B;
        result += _convolution[3] * source[x - 1][y].B;
        result += _convolution[4] * source[x][y].B;
        result += _convolution[5] * source[x + 1][y].B;
        result += _convolution[6] * source[x - 1][y + 1].B;
        result += _convolution[7] * source[x][y + 1].B;
        result += _convolution[8] * source[x + 1][y + 1].B;
        return result;
    }
}