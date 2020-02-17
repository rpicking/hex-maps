using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

    public Color[] colors;

    public HexGrid hexGrid;

    private Color activeColor;

    private int activeElevation;
    private int brushSize;

    private bool applyColor = false;
    private bool applyElevation = true;

    private void Awake() {
        SelectColor(0);
    }

    private void Update() {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
            HandleInput();
        }
    }

    private void HandleInput() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            EditCells(hexGrid.GetCell(hit.point));
        }
    }

    public void SelectColor(int index) {
        applyColor = index >= 0;

        if (applyColor) {
            activeColor = colors[index];
        }
    }

    public void SetApplyElevation(bool toggle) {
        applyElevation = toggle;
    }

    public void SetElevation(float elevation) {
        activeElevation = (int)elevation;
    }

    public void SetBrushSize(float size) {
        brushSize = (int)size;
    }

    private void EditCells(HexCell center) {
        int centerX = center.Coordinates.X;
        int centerZ = center.Coordinates.Z;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
            for (int x = centerX - r; x <= centerX + brushSize; x++) {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }

        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
            for (int x = centerX - brushSize; x <= centerX + r; x++) {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    private void EditCell(HexCell cell) {
        if (!cell) return;

        if (applyColor) {
            cell.Color = activeColor;
        }

        if (applyElevation) {
            cell.Elevation = activeElevation;
        }
    }

    public void ShowUI(bool visible) {
        hexGrid.ShowUI(visible);
    }
}
