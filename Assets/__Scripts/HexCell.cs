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

            ValidateRivers();

            for (int i = 0; i < roads.Length; ++i) {
                if (roads[i] && GetElevationDifference((HexDirection)i) > 1) {
                    SetRoad(i, false);
                }
            }

            Refresh();
        }
    }

    public Vector3 Position {
        get { return transform.position; }
    }

    [Header("Rivers")]
    [SerializeField] private bool _hasIncomingRiver, _hasOutgoingRiver;
    [SerializeField] HexDirection _incomingRiver, _outgoingRiver;

    [Space]
    [SerializeField] private bool[] roads;
    [SerializeField] private HexCell[] neighbors;

    public bool HasIncomingRiver {
        get { return _hasIncomingRiver; }
    }

    public bool HasOutgoingRiver {
        get { return _hasOutgoingRiver; }
    }

    public HexDirection IncomingRiver {
        get { return _incomingRiver; }
    }

    public HexDirection OutgoingRiver {
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

    public HexDirection RiverBeginOrEndDirection {
        get {
            return _hasIncomingRiver ? _incomingRiver : _outgoingRiver;
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
            return (_elevation + HexMetrics.waterElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    public bool HasRoads {
        get {
            for (int i = 0; i < roads.Length; ++i) {
                if (roads[i]) {
                    return true;
                }
            }
            return false;
        }
    }

    private int waterLevel;
    public int WaterLevel {
        get { return waterLevel; }
        set {
            if (waterLevel == value) {
                return;
            }

            waterLevel = value;
            ValidateRivers();
            Refresh();
        }
    }

    public float WaterSurfaceY {
        get {
            return (waterLevel + HexMetrics.waterElevationOffset) *
              HexMetrics.elevationStep; 
        }
    }

    public bool IsUnderwater {
        get { return waterLevel > _elevation; }
    }
    
    public int UrbanLevel {
        get {
            return urbanLevel;
        }
        set {
            if (urbanLevel != value) {
                urbanLevel = value;
                Refresh(true);
            }
        }
    }
    public int FarmLevel {
        get {
            return farmLevel;
        }
        set {
            if (farmLevel != value) {
                farmLevel = value;
                Refresh(true);
            }
        }
    }

    public int PlantLevel {
        get {
            return plantLevel;
        }
        set {
            if (plantLevel != value) {
                plantLevel = value;
                Refresh(true);
            }
        }
    }

    private int urbanLevel, farmLevel, plantLevel;



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
        if (!IsValidRiverDestination(neighbor)) {
            return;
        }

        RemoveOutgoingRiver();
        if (_hasIncomingRiver && _incomingRiver == direction) {
            RemoveIncomingRiver();
        }

        // set the new outgoing river
        _hasOutgoingRiver = true;
        _outgoingRiver = direction;

        // set the new incoming river for the neighboring cell
        neighbor.RemoveIncomingRiver();
        neighbor._hasIncomingRiver = true;
        neighbor._incomingRiver = direction.Opposite();

        // cannot have roads going in the same direction as the river
        SetRoad(direction, false);
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

    public bool HasRoadThroughEdge(HexDirection direction) {
        return roads[(int)direction];
    }

    public void AddRoad(HexDirection direction) {
        if (!roads[(int)direction] && !HasRiverThroughEdge(direction) &&
            GetElevationDifference(direction) <= 1) {
            SetRoad((int)direction, true);
        }
    }

    public void RemoveRoads() {
        for (int i = 0; i < neighbors.Length; ++i) {
            if (!roads[i]) continue;

            SetRoad(i, false);
        }
    }

    private void SetRoad(HexDirection direction, bool state) {
        SetRoad((int)direction, state);
    }

    private void SetRoad(int index, bool state) {
        roads[index] = state;
        neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
        neighbors[index].Refresh(true);
        Refresh(true);
    }

    public int GetElevationDifference(HexDirection direction) {
        int difference = _elevation - GetNeighbor(direction)._elevation;
        return difference >= 0 ? difference : -difference;
    }

    private bool IsValidRiverDestination(HexCell neighbor) {
        return neighbor && (
            _elevation >= neighbor._elevation || waterLevel == neighbor._elevation);
    }

    private void ValidateRivers() {
        if (_hasOutgoingRiver &&
            !IsValidRiverDestination(GetNeighbor(_outgoingRiver))) {
            RemoveOutgoingRiver();
        }
        if (_hasIncomingRiver &&
            !GetNeighbor(_incomingRiver).IsValidRiverDestination(this)) {
            RemoveIncomingRiver();
        }
    }

    public override string ToString() {
        return "(" + Coordinates.X + ", " + Coordinates.Z + ")";
    }
}
