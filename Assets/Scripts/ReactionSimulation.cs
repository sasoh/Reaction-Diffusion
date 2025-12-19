using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ReactionSimulation : MonoBehaviour
{
    [SerializeField] private float diffusionA = 1.0f;
    [SerializeField] private float diffusionB = 0.5f;
    [SerializeField] private float feedRate = 0.055f;
    [SerializeField] private float killRate = 0.062f;
    [SerializeField] private float simulationSpeed = 1.0f;
    [SerializeField] private float startingCircleRadius = 3.0f;
    [SerializeField] private Vector2 startingCircleCenter = new(12.0f, 20.0f);
    private int _width;
    private int _height;

    private readonly List<float> _convolutionKernel = new()
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

    public void Initialize(int textureWidth, int textureHeight)
    {
        _width = textureWidth;
        _height = textureHeight;
        
        for (var i = 0; i < _height; i++)
        {
            var row = new List<Cell>();
            for (var j = 0; j < _width; j++)
            {
                var cell = new Cell(1.0f, 0.0f);
                cell = AddStartingCircle(i, j, cell);
                row.Add(cell);
            }

            _reaction.Add(row);
            _reactionLast.Add(row);
        }
    }

    private Cell AddStartingCircle(int i, int j, Cell cell)
    {
        if (!IsPointWithinCircle(i, j, startingCircleCenter, startingCircleRadius)) return cell;
        
        cell.A = 0.0f;
        cell.B = 1.0f;

        return cell;
    }

    private static bool IsPointWithinCircle(float x, float y, Vector2 center, float radius)
    {
        return (center.x - x) * (center.x - x) + (center.y - y) * (center.y - y) < radius * radius;
    }

    public void UpdateReaction()
    {
        Parallel.For(
            1,
            _height - 1,
            i =>
            {
                Parallel.For(
                    1,
                    _width - 1,
                    j =>
                    {
                        var cell = _reactionLast[i][j];
                        var chanceAtoB = cell.A * cell.B * cell.B;
                        cell.A += DiffA(i, j, chanceAtoB, cell) * simulationSpeed;
                        cell.B += DiffB(i, j, chanceAtoB, cell) * simulationSpeed;
                        _reaction[i][j] = cell;
                    }
                );
            }
        );

        for (var i = 0; i < _height; i++)
        {
            for (var j = 0; j < _width; j++)
            {
                _reactionLast[i][j] = _reaction[i][j];
            }
        }
    }

    public Color GetCellColor(int i, int j)
    {
        var c = _reaction[i][j];
        return new Color(c.A, 0.0f, c.B, 1.0f);
    }

    private float DiffA(int i, int j, float chanceAtoB, Cell cell) 
        => (diffusionA * ConvoluteA(i, j, _reactionLast) - chanceAtoB + feedRate * (1.0f - cell.A));

    private float DiffB(int i, int j, float chanceAtoB, Cell cell) 
        => (diffusionB * ConvoluteB(i, j, _reactionLast) + chanceAtoB - (killRate + feedRate) * cell.B);

    private float ConvoluteA(int x, int y, List<List<Cell>> source)
    {
        var result = 0.0f;
        result += _convolutionKernel[0] * source[x - 1][y - 1].A;
        result += _convolutionKernel[1] * source[x][y - 1].A;
        result += _convolutionKernel[2] * source[x + 1][y - 1].A;
        result += _convolutionKernel[3] * source[x - 1][y].A;
        result += _convolutionKernel[4] * source[x][y].A;
        result += _convolutionKernel[5] * source[x + 1][y].A;
        result += _convolutionKernel[6] * source[x - 1][y + 1].A;
        result += _convolutionKernel[7] * source[x][y + 1].A;
        result += _convolutionKernel[8] * source[x + 1][y + 1].A;
        return result;
    }

    private float ConvoluteB(int x, int y, List<List<Cell>> source)
    {
        var result = 0.0f;
        result += _convolutionKernel[0] * source[x - 1][y - 1].B;
        result += _convolutionKernel[1] * source[x][y - 1].B;
        result += _convolutionKernel[2] * source[x + 1][y - 1].B;
        result += _convolutionKernel[3] * source[x - 1][y].B;
        result += _convolutionKernel[4] * source[x][y].B;
        result += _convolutionKernel[5] * source[x + 1][y].B;
        result += _convolutionKernel[6] * source[x - 1][y + 1].B;
        result += _convolutionKernel[7] * source[x][y + 1].B;
        result += _convolutionKernel[8] * source[x + 1][y + 1].B;
        return result;
    }
}
