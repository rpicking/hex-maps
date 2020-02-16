using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

    [SerializeField]
    private int width = 6;
    [SerializeField]
    private int height = 6;

    public HexCell cellPrefab;
    public Text cellLabelPrefab;
    public Texture2D noiseSource;

    [SerializeField]
    private Canvas gridCanvas;
    [SerializeField]
    private Color defaultColor = Color.white;


    private HexCell[] cells;
    private HexMesh hexMesh;

    private void OnEnable() {
        HexMetrics.noiseSource = noiseSource;
    }

    private void Awake() {
        HexMetrics.noiseSource = noiseSource;
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[height * width];

        for (int i = 0, z = 0; z < height; ++z) {
            for (int x = 0; x < width; ++x, ++i) {
                CreateCell(x, z, i);
            }
        }
    }

    private void Start() {
        hexMesh.Triangulate(cells);
    }

    private void CreateCell(int x, int z, int index) {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[index] = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.color = defaultColor;

        // set cell neighbors
        if (x > 0) {
            cell.SetNeighbor(HexDirection.W, cells[index - 1]);
        }
        if (z > 0) {
            if((z & 1) == 0) {
                cell.SetNeighbor(HexDirection.SE, cells[index - width]);
                if (x > 0) {
                    cell.SetNeighbor(HexDirection.SW, cells[index - width - 1]);
                }
            } else {
                cell.SetNeighbor(HexDirection.SW, cells[index - width]);
                if (x < (width - 1))
                    cell.SetNeighbor(HexDirection.SE, cells[index - width + 1]);
            }
        }

        // Create coordinate label
        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToSTringOnSeparateLine();

        cell.uiRect = label.rectTransform;

        cell.Elevation = 0;
    }


    public HexCell GetCell(Vector3 position) {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        return cells[index];
    }

    public void Refresh() {
        hexMesh.Triangulate(cells);
    }
}
