using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

    public Color[] colors;
    public HexGrid hexGrid;

    private Color activeColor;
    private bool applyColor;

    private bool applyElevation = true;
    private int activeElevation;

    private bool applyWaterLevel = true;
    private int activeWaterLevel;

    private int brushSize;

    private enum OptionalToggle
    {
        Ignore, Add, Remove
    }
    private OptionalToggle riverMode, roadMode;

    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;


    void Awake()
    {
        SelectColor(0);
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && 
            !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        else
        {
            previousCell = null;
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell currentCell = hexGrid.GetCell(hit.point);
            if(previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            EditCells(currentCell);
            previousCell = currentCell;
            isDrag = true;
        }
        else
        {
            previousCell = null;
        }
    }

    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    void EditCell(HexCell cell)
    {
        if (cell)
        {
            if (applyColor)
            {
                cell.Color = activeColor;
            }
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
            if (riverMode == OptionalToggle.Remove)
            {
                cell.RemoveRiver();
            }
            if (roadMode == OptionalToggle.Remove)
            {
                cell.RemoveRoads();
            }
            if (applyWaterLevel)
            {
                cell.WaterLevel = activeWaterLevel;
            }

            else if (isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell)
                {
                    if (riverMode == OptionalToggle.Add)
                    {
                        otherCell.SetOutgoingRiver(dragDirection);
                    }
                    if (roadMode == OptionalToggle.Add)
                    {
                        otherCell.AddRoad(dragDirection);
                    }
                }                
            }
        }
    }

    void ValidateDrag(HexCell currentCell)
    {
        for (dragDirection = HexDirection.NE; 
            dragDirection <= HexDirection.NW;
            dragDirection++)
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    #region Editor Methods
    public void SelectColor(int index)
    {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }        
    }

    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    public void ShowUI(bool visible)
    {
        hexGrid.ShowUI(visible);
    }

    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    public void SetWaterLevel(float level)
    {
        activeWaterLevel = (int)level;
    }

    public void SetApplyWaterLevel(bool toggle)
    {
        applyWaterLevel = toggle;
    }

    #endregion
}
