using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

    enum OptionalToggle {
        IGNORE, YES, NO
    }

    public Color[] colors;

    public HexGrid hexGrid;

    private Color activeColor;

    private int activeElevation;
    private int activeWaterLevel;

    private int brushSize;

    private bool applyColor = false;
    private bool applyElevation = true;
    private bool applyWaterLevel = true;

    private OptionalToggle riverMode, roadMode;

    private bool isDrag;
    private HexDirection dragDirection;
    private HexCell previousCell;

    private void Awake() {
        SelectColor(0);
        SetRiverMode(0);
        SetRoadMode(0);
    }

    private void Update() {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
            HandleInput();
        } else {
            previousCell = null;
        }
    }

    private void HandleInput() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            HexCell currentCell = hexGrid.GetCell(hit.point);
            if (previousCell && previousCell != currentCell) {
                ValidateDrag(currentCell);
            } else {
                isDrag = false;
            }

            EditCells(currentCell);
            previousCell = currentCell;
        } else {
            previousCell = null;
        }
    }

    private void ValidateDrag(HexCell currentCell) {
        for (dragDirection = HexDirection.NE;
            dragDirection <= HexDirection.NW;
            dragDirection++) {

            if (previousCell.GetNeighbor(dragDirection) == currentCell) {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
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

    public void SetRiverMode(int mode) {
        riverMode = (OptionalToggle)mode;
    }

    public void SetRoadMode(int mode) {
        roadMode = (OptionalToggle)mode;
    }

    public void SetApplyWaterLevel(bool toggle) {
        applyWaterLevel = toggle;
    }

    public void SetWaterLevel(float level) {
        activeWaterLevel = (int)level;
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
        if (applyWaterLevel) {
            cell.WaterLevel = activeWaterLevel;
        }

        if (roadMode == OptionalToggle.NO) {
            cell.RemoveRoads();
        }


        if (isDrag) {
            HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
            if (otherCell) {
                if (riverMode == OptionalToggle.YES) {
                    otherCell.SetOutgoingRiver(dragDirection);
                }
                if (roadMode == OptionalToggle.YES) {
                    otherCell.AddRoad(dragDirection);
                }
            }
        }
    }

    public void ShowUI(bool visible) {
        hexGrid.ShowUI(visible);
    }
}
