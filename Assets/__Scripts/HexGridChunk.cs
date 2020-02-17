using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridChunk : MonoBehaviour {
    HexCell[] cells;

    HexMesh hexMesh;
    Canvas gridCanvas;

    private int xCoord, zCoord;

    void Awake() {
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        ShowUI(false);
    }
    
    private void LateUpdate() {
        hexMesh.Triangulate(cells);
        enabled = false;
    }

    public void Initialize(int xCoord, int zCoord) {
        this.xCoord = xCoord;
        this.zCoord = zCoord;
        gameObject.name = GetType().Name + " " + this;
    }

    public void ShowUI(bool visible) {
        gridCanvas.gameObject.SetActive(visible);
    }

    public void AddCell(int index, HexCell cell) {
        cells[index] = cell;
        cell.chunk = this;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(gridCanvas.transform, false);
    }

    /// <summary>
    /// Refreshing enables the component and later triggers a redraw the the hexMesh
    ///   in LateUpdate
    /// </summary>
    public void Refresh() {
        enabled = true;
    }

    public override string ToString() {
        return "(" + xCoord + ", " + zCoord + ")";
    }
}
