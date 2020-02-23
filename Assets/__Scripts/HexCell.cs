using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour {

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

            // update river if new elevation causes river to not be able to flow anymore
            if (_hasOutgoingRiver &&
            _elevation < GetNeighbor(_outgoingRiver)._elevation) {
                RemoveOutgoingRiver();
            }

            if (_hasIncomingRiver &&
                _elevation > GetNeighbor(_incomingRiver)._elevation) {
                RemoveIncomingRiver();
            }

            Refresh();
        }
    }

    public Vector3 Position {
        get { return transform.position; }
    }

    [Header("Rivers")]
    [SerializeField]
    private bool _hasIncomingRiver, _hasOutgoingRiver;
    public bool HasIncomingRiver {
        get { return _hasIncomingRiver; }
    }

    public bool HasOutgoingRiver {
        get { return _hasOutgoingRiver; }
    }

    [SerializeField]
    HexDirection _incomingRiver, _outgoingRiver;
    public HexDirection IncomingRiver {
        get { return _incomingRiver; }
    }
    public HexDirection OutGoingRiver {
        get { return _outgoingRiver; }
    }

    public bool HasRiver {
        get {
            return _hasIncomingRiver || _hasOutgoingRiver;
        }
    }

    public bool HasRiverBeginOrEnd {
        get {
            return _hasIncomingRiver != _hasOutgoingRiver;
        }
    }

    public float StreamBedY {
        get {
            return (_elevation + HexMetrics.streamBedElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    public float RiverSurfaceY {
        get {
            return (_elevation + HexMetrics.riverSurfaceElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    [SerializeField]
    private HexCell[] neighbors;


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

    public void Refresh(bool onlySelf=false) {
        if (!chunk) return;
    
        chunk.Refresh();

        if (onlySelf) return;

        foreach (var neighbor in neighbors) {
            if (!neighbor) continue;
            if (neighbor.chunk != chunk) {
                neighbor.chunk.Refresh();
            }
        }
    }

    public bool HasRiverThroughEdge(HexDirection direction) {
        return
            _hasIncomingRiver && _incomingRiver == direction ||
            _hasOutgoingRiver && _outgoingRiver == direction;
    }

    public void SetOutgoingRiver(HexDirection direction) {
        if (_hasOutgoingRiver && _outgoingRiver == direction) {
            return;
        }

        HexCell neighbor = GetNeighbor(direction);
        if (!neighbor || (_elevation < neighbor._elevation)) {
            return;
        }

        RemoveOutgoingRiver();
        if (_hasIncomingRiver && _incomingRiver == direction) {
            RemoveIncomingRiver();
        }

        // set the new outgoing river
        _hasOutgoingRiver = true;
        _outgoingRiver = direction;
        Refresh(true);

        // set the new incoming river for the neighboring cell
        neighbor.RemoveIncomingRiver();
        neighbor._hasIncomingRiver = true;
        neighbor._incomingRiver = direction.Opposite();
        neighbor.Refresh(true);
    }

    public void RemoveRiver() {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    public void RemoveOutgoingRiver() {
        if (!_hasOutgoingRiver) return;

        _hasOutgoingRiver = false;
        Refresh(true);

        HexCell neighbor = GetNeighbor(_outgoingRiver);
        neighbor._hasIncomingRiver = false;
        neighbor.Refresh(true);
    }

    public void RemoveIncomingRiver() {
        if (!_hasIncomingRiver) {
            return;
        }
        _hasIncomingRiver = false;
        Refresh(true);

        HexCell neighbor = GetNeighbor(_incomingRiver);
        neighbor._hasOutgoingRiver = false;
        neighbor.Refresh(true);
    }

    public override string ToString() {
        return "(" + Coordinates.X + ", " + Coordinates.Z + ")";
    }
}
