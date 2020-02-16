using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

    public Color[] colors;

    public HexGrid hexGrid;

    private Color activeColor;

    private int activeElevation;

    private void Awake() {
        SelectColor(0);
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            HandleInput();
        }
    }

    private void HandleInput() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            EditCell(hexGrid.GetCell(hit.point));
        }
    }

    public void SelectColor(int index) {
        activeColor = colors[index];
    }

    public void SetElevation(float elevation) {
        activeElevation = (int)elevation;
    }

    void EditCell(HexCell cell) {
        Debug.Log(cell.coordinates);
        cell.color = activeColor;
        cell.Elevation = activeElevation;
        hexGrid.Refresh();
    }
}
