using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour {

    [SerializeField]
    private HexCell[] neighbors;

    private HexCoordinates _coordinates;
    public HexCoordinates Coordinates {
        get { return _coordinates; }
        set {
            _coordinates = value;
            gameObject.name = GetType().Name + " " + this;
        }
    }
    public HexGridChunk chunk;
    public RectTransform uiRect;

    private Color _color;
    public Color Color {
        get {
            return _color;
        }
        set {
            if (_color == value) return;

            _color = value;
            Refresh();
        }
    }

    private int _elevation = int.MinValue;
    public int Elevation {
        get {
            return _elevation;
        }
        set {
            if (_elevation == value) return;

            _elevation = value;
            Vector3 position = transform.position;
            position.y = value * HexMetrics.elevationStep;
            position.y += 
                (HexMetrics.SampleNoise(position).y * 2f - 1f) *
                HexMetrics.elevationPerturbStrength;

            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            Refresh();
        }
    }

    public Vector3 Position {
        get { return transform.position; }
    }

    public HexCell GetNeighbor(HexDirection direction) {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell) {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection direction) {
        return HexMetrics.GetEdgeType(_elevation, GetNeighbor(direction)._elevation);
    }

    public HexEdgeType GetEdgeType(HexCell otherCell) {
        return HexMetrics.GetEdgeType(_elevation, otherCell._elevation);
    }

    public void Refresh() {
        if (!chunk) return;
    
        chunk.Refresh();

        foreach (var neighbor in neighbors) {
            if (!neighbor) continue;
            if (neighbor.chunk != chunk) {
                neighbor.chunk.Refresh();
            }
        }
    }

    public override string ToString() {
        return "(" + Coordinates.X + ", " + Coordinates.Z + ")";
    }
}
