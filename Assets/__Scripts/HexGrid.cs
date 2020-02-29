using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

    public int chunkCountX = 4, chunkCountZ = 3;

    [SerializeField]
    private int cellCountX;
    [SerializeField]
    private int cellCountZ;

    public HexCell cellPrefab;
    public Text cellLabelPrefab;
    public HexGridChunk chunkPrefab;
    public Texture2D noiseSource;
    public int seed;

    [SerializeField]
    private Color defaultColor = Color.white;
    
    private HexCell[] cells;

    private HexGridChunk[] chunks;

    private void OnEnable() {
        if (!HexMetrics.noiseSource) {
            HexMetrics.noiseSource = noiseSource;
            HexMetrics.InitializeHashGrid(seed);
        }
    }

    private void Awake() {
        HexMetrics.noiseSource = noiseSource;
        HexMetrics.InitializeHashGrid(seed);

        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

        CreateChunks();
        CreateCells();
    }

    private void CreateChunks() {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++) {
            for (int x = 0; x < chunkCountX; x++) {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.Initialize(x, z);
                chunk.transform.SetParent(transform);
            }
        }
    }

    private void CreateCells() {
        cells = new HexCell[cellCountZ * cellCountX];

        for (int i = 0, z = 0; z < cellCountZ; ++z) {
            for (int x = 0; x < cellCountX; ++x, ++i) {
                CreateCell(x, z, i);
            }
        }
    }

    private void CreateCell(int x, int z, int index) {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[index] = Instantiate<HexCell>(cellPrefab);
        //cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.Color = defaultColor;

        // set cell neighbors
        if (x > 0) {
            cell.SetNeighbor(HexDirection.W, cells[index - 1]);
        }
        if (z > 0) {
            if((z & 1) == 0) {
                cell.SetNeighbor(HexDirection.SE, cells[index - cellCountX]);
                if (x > 0) {
                    cell.SetNeighbor(HexDirection.SW, cells[index - cellCountX - 1]);
                }
            } else {
                cell.SetNeighbor(HexDirection.SW, cells[index - cellCountX]);
                if (x < (cellCountX - 1))
                    cell.SetNeighbor(HexDirection.SE, cells[index - cellCountX + 1]);
            }
        }

        // Create coordinate label
        Text label = Instantiate<Text>(cellLabelPrefab);
        //label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = cell.Coordinates.ToStringOnSeparateLine();

        cell.uiRect = label.rectTransform;

        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);
    }

    private void AddCellToChunk(int x, int z, HexCell cell) {
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;

        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    public HexCell GetCell(Vector3 position) {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        return cells[index];
    }

    public HexCell GetCell(HexCoordinates coordinates) {
        int z = coordinates.Z;
        if (z < 0 || z >= cellCountZ) {
            return null;
        }

        int x = coordinates.X + z / 2;
        if (x < 0 || x >= cellCountX) {
            return null;
        }

        return cells[x + z * cellCountX];
    }

    public void ShowUI(bool visible) {
        foreach (var chunk in chunks) {
            chunk.ShowUI(visible);
        }
    }
}
