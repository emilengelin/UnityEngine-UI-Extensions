using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{

    [ExecuteInEditMode]
    public class HexGridLayoutGroup : MonoBehaviour
    {

        ///TODO: Update <seealso cref="HexGridLayoutGroup"></seealso>

        //[1]
        //Upgrade the align features to work with dynamical row- and columnCounts.

        //[2]
        //Optimize.

        #region Fields

        /// <summary>
        /// How big are the hexagonal cells (not actual size, but calculation size)? For best looking result, hand edit this value. This value changes each items (width, height, NAN) (RectTransform) OR scale (x, y, z) (Transform)!
        /// </summary>
        [Header("General")]
        [Tooltip("How big are the hexagonal cells (not actual size, but calculation size)? For best looking result, hand edit this value. This value changes each items (width, height, NAN) (RectTransform) OR scale (x, y, z) (Transform)!")]
            public Vector2 cellSize = new Vector2(100, 100);
        /// <summary>
        /// Scale of the cells.
        /// </summary>
        [Tooltip("Scale of the cells.")]
            public float cellScale = 1;
        /// <summary>
        /// Offset for the entire grid
        /// </summary>
        [Tooltip("Offset for the entire grid.")]
            public Vector3 offset = new Vector3(0, 0, 0);

        /// <summary>
        /// 2D: (x, y); 3D: (x, z);
        /// </summary>
        [Space]
        [Tooltip("2D: (x, y); 3D: (x, z);")]
            public HexGridDimension dimension = HexGridDimension._2D;
        /// <summary>
        /// Square and/or Rectangle OR Spiral?
        /// </summary>
        [Tooltip("Square and/or Rectangle OR Spiral?")]
            public HexGridShape gridShape = HexGridShape.Square;
        /// <summary>
        /// The component to search for...
        /// </summary>
        [Tooltip("The component to search for...")]
            public HexGridComponent component = HexGridComponent.RectTransform;

        /// <summary>
        /// How are the hexagon cells oriented?
        /// </summary>
        [Space]
        [Tooltip("How are the hexagon cells oriented?")]
            public HexShape shape = HexShape.PointyTop;

        /// <summary>
        /// WARNING: May take extra processing power. NOTE: A bit buggy aswell.
        /// </summary>
        [Space]
        [Tooltip("WARNING: May take extra processing power. NOTE: A bit buggy aswell.")]
            public bool resetPositionsOnBuild = false;

        /// <summary>
        /// When should the grid be built?
        /// </summary>
        [Space]
        [Tooltip("When should the grid be built?")]
            public BuildSetupHexGrid buildSetup = BuildSetupHexGrid.Both;

        /// <summary>
        /// Which anchor should the cells use (RectTransform(s))?
        /// </summary>
        [Space]
        [Tooltip("Which anchor should the cells use (RectTransform(s))?")]
            public HexAnchor anchor = HexAnchor.MiddleCenter;

        /// <summary>
        /// How should the grid constraint the cells?
        /// </summary>
        [Header("Square and/or Rectangle")]
        [Tooltip("How should the grid constraint the cells?")]
            public HexGridConstraint constraint = HexGridConstraint.FixedRowCount;
        public int rowCount = 5;
        public int columnCount = 5;

        /// <summary>
        /// Which direction should the grid build in? (Positive = Up/Right, Negative = Down/Left). X coordinate.
        /// </summary>
        [Space]
        [Tooltip("Which direction should the grid build in? (Positive = Up/Right, Negative = Down/Left). X coordinate.")]
            public HexGridDirection rowDirection = HexGridDirection.Positive;
        /// <summary>
        /// Which direction should the grid build in? (Positive = Up/Right, Negative = Down/Left). Y coordinate.
        /// </summary>
        [Tooltip("Which direction should the grid build in? (Positive = Up/Right, Negative = Down/Left). Y coordinate.")]
            public HexGridDirection columnDirection = HexGridDirection.Positive;

        /// <summary>
        /// Should it align with the middle cell? X coordinate. NOTE: This will ONLY work properly if the rowCount is set to the acutal amount of rows.
        /// </summary>
        [Space]
        [Tooltip("Should it align with the middle cell? X coordinate. NOTE: This will ONLY work properly if the rowCount is set to the acutal amount of rows.")]
            public HexGridAlign alignX = HexGridAlign.None;
        /// <summary>
        /// Should it align with the middle cell? Y coordinate. NOTE: This will ONLY work properly if the columnCount is set to the acutal amount of columns.
        /// </summary>
        [Tooltip("Should it align with the middle cell? Y coordinate. NOTE: This will ONLY work properly if the columnCount is set to the acutal amount of columns.")]
            public HexGridAlign alignY = HexGridAlign.None;

        /// <summary>
        /// Which row will have the most amount of cells? NOTE: Will only work with constraint set to "FixedRowCount".
        /// </summary>
        [Space]
        [Tooltip("Which row will have the most amount of cells? NOTE: Will only work with constraint set to \"FixedRowCount\".")]
            public HexGridProminentRow prominentRow = HexGridProminentRow.Even;

        /// <summary>
        /// The radius of the spiral, needs to be hand edited.
        /// </summary>
        [Header("Spiral")]
        [Tooltip("The radius of the spiral, needs to be hand edited.")]
            public int spiralRadius = 10;
        /// <summary>
        /// Should the spiral radius be set automatically?
        /// </summary>
        [Tooltip("Should the spiral radius be set automatically?")]
            public bool spiralRadiusDynamic = true;
        /// <summary>
        /// Clockwise or counterclockwise?
        /// </summary>
        [Tooltip("Clockwise or counterclockwise?")]
            public bool spiralClockwise = true;
        /// <summary>
        /// Should the first cell start in the middle
        /// </summary>
        [Tooltip("Should the first cell start in the middle?")]
            public bool fromMiddle = true;

        #endregion

        #region Constants

        private const float Sqrt3Div2 = 0.86602540378f;

        #endregion

        #region Public methods

        /// <summary>
        /// Use this if you're building from your own code.
        /// </summary>
        public void BuildAutomatic()
        {
            ClampValues();

            if (gridShape == HexGridShape.Square)
            {
                BuildSquare();
            }
            else if (gridShape == HexGridShape.Spiral)
            {
                BuildSpiral();
            }
        }

        public void BuildSquare()
        {
            if (resetPositionsOnBuild)
            {
                ResetAllPositions();
            }

            float xQ = 0, yQ = 0;
            int x = 0, y = 0;

            float innerRadius = cellSize.y * Sqrt3Div2;
            float innerRadiusMul2 = innerRadius * 2;

            float hexSizeXDiv2 = cellSize.x / 2;
            float hexSizeYDiv2 = cellSize.y / 2;

            //Get middle of hex grid:
            float middleX = (rowCount * cellSize.x - ((1 - (int)prominentRow) * (1 - (int)constraint) * cellSize.x)) / 2;
            float middleY = (columnCount * innerRadius - innerRadius) / 2;

            middleX = ((int)alignX * middleX * ((int)rowDirection * -1));
            middleY = ((int)alignY * middleY * ((int)columnDirection * -1));

            if (component == HexGridComponent.RectTransform)
            {
                foreach (RectTransform rt in transform)
                {
                    rt.localScale = Vector3.one * cellScale;

                    SetAnchor(rt, anchor);

                    if (shape == HexShape.PointyTop)
                    {
                        xQ = (x * cellSize.x + ((y % 2) * hexSizeXDiv2)) * (int)rowDirection + middleX;
                        yQ = y * innerRadius * (int)columnDirection + middleY;
                    }
                    else
                    {
                        xQ = (x * innerRadiusMul2 + ((y % 2) * innerRadius)) * (int)rowDirection + middleX;
                        yQ = y * hexSizeYDiv2 * (int)columnDirection + middleY;
                    }

                    rt.anchoredPosition = GetVectorBasedDimension(xQ, yQ) + offset;

                    if (constraint == HexGridConstraint.FixedRowCount)
                    {
                        if (prominentRow == HexGridProminentRow.Even && y % 2 != 0)
                        {
                            if (x >= rowCount - 2)
                            {
                                y++;
                                x = 0;
                            }
                            else
                            {
                                x = Mathf.RoundToInt(Mathf.Repeat(x + 1, rowCount));
                            }
                        }
                        else
                        {
                            if (x == rowCount - 1)
                            {
                                y++;
                            }

                            x = Mathf.RoundToInt(Mathf.Repeat(x + 1, rowCount));
                        }
                    }
                    else
                    {
                        if (y == columnCount - 1)
                        {
                            x++;
                        }
                        
                        y = Mathf.RoundToInt(Mathf.Repeat(y + 1, columnCount));
                    }
                }
            }
            else
            {
                foreach (Transform t in transform)
                {
                    t.localScale = Vector3.one * cellScale;

                    if (shape == HexShape.PointyTop)
                    {
                        xQ = (x * cellSize.x + ((y % 2) * hexSizeXDiv2)) * (int)rowDirection + middleX;
                        yQ = y * innerRadius * (int)columnDirection + middleY;
                    }
                    else
                    {
                        xQ = (x * innerRadiusMul2 + ((y % 2) * innerRadius)) * (int)rowDirection + middleX;
                        yQ = y * hexSizeYDiv2 * (int)columnDirection + middleY;
                    }

                    t.localPosition = GetVectorBasedDimension(xQ, yQ) + offset;

                    if (constraint == HexGridConstraint.FixedRowCount)
                    {
                        if (prominentRow == HexGridProminentRow.Even && y % 2 != 0)
                        {
                            if (x >= rowCount - 2)
                            {
                                y++;
                                x = 0;
                            }
                            else
                            {
                                x = Mathf.RoundToInt(Mathf.Repeat(x + 1, rowCount));
                            }
                        }
                        else
                        {
                            if (x == rowCount - 1)
                            {
                                y++;
                            }

                            x = Mathf.RoundToInt(Mathf.Repeat(x + 1, rowCount));
                        }
                    }
                    else
                    {
                        if (y == columnCount - 1)
                        {
                            x++;
                        }
                        
                        y = Mathf.RoundToInt(Mathf.Repeat(y + 1, columnCount));
                    }
                }
            }
        }

        public void BuildSpiral()
        {
            if (resetPositionsOnBuild)
            {
                ResetAllPositions();
            }

            int radius = spiralRadius;
            if (spiralRadiusDynamic)
            {
                //Kind of works...
                radius = Mathf.Clamp(PotRegRadius(transform.childCount) + 2, 2, int.MaxValue);
            }

            List<Vector3> positions = new List<Vector3>();
            int x = 0, y = 0;

            positions.Add(new Vector3(x, y));
            int n = 1;
            for (n = 1; n < radius; ++n)
            {
                for (int i = 0; i < n; ++i) positions.Add(new Vector3(++x, y));    //Move right.
                for (int i = 0; i < n - 1; ++i) positions.Add(new Vector3(x, ++y));  //Move down right. Note N-1.
                for (int i = 0; i < n; ++i) positions.Add(new Vector3(--x, ++y));  //Move down left.
                for (int i = 0; i < n; ++i) positions.Add(new Vector3(--x, y));    //Move left.
                for (int i = 0; i < n; ++i) positions.Add(new Vector3(x, --y));  //Move up left.
                for (int i = 0; i < n; ++i) positions.Add(new Vector3(++x, --y));  //Move up right.
            }

            if (!fromMiddle)
            {
                positions.Reverse();
            }

            float xMul = 0, yMul = 0, xQ = 0, yQ = 0;
            n = 0;

            if (component == HexGridComponent.RectTransform)
            {
                foreach (RectTransform rt in transform)
                {
                    if (n == positions.Count)
                    {
                        Debug.LogWarning("Radius to small!");
                        break;
                    }

                    rt.localScale = Vector3.one * cellScale;

                    SetAnchor(rt, anchor);

                    xMul = positions[n].x * cellSize.x;
                    yMul = positions[n++].y * cellSize.y;

                    xQ = xMul + yMul / 2;
                    yQ = Sqrt3Div2 * yMul;

                    //Switch the coordinates
                    if (shape == HexShape.FlatTop)
                    {
                        float _yQ = yQ;
                        yQ = xQ;
                        xQ = _yQ;
                    }

                    if (spiralClockwise)
                    {
                        xQ *= -1 * (int)shape;
                    }
                    else
                    {
                        xQ *= (int)shape;
                    }

                    rt.anchoredPosition = GetVectorBasedDimension(xQ, yQ) + offset;
                }
            }
            else
            {
                foreach (Transform t in transform)
                {
                    if (n == positions.Count)
                    {
                        Debug.LogWarning("Radius to small!");
                        break;
                    }

                    t.localScale = Vector3.one * cellScale;

                    xMul = positions[n].x * cellSize.x;
                    yMul = positions[n++].y * cellSize.y;

                    xQ = xMul + yMul / 2;
                    yQ = Sqrt3Div2 * yMul;

                    //Switch the coordinates
                    if (shape == HexShape.FlatTop)
                    {
                        float _yQ = yQ;
                        yQ = xQ;
                        xQ = _yQ;
                    }

                    if (spiralClockwise)
                    {
                        xQ *= -1 * (int)shape;
                    }
                    else
                    {
                        xQ *= (int)shape;
                    }

                    t.localPosition = GetVectorBasedDimension(xQ, yQ) + offset;
                }
            }
        }

        #endregion

        #region Private methods

        private void OnTransformChildrenChanged()
        {
            if (buildSetup == BuildSetupHexGrid.OnTransformChildrenChanged || buildSetup == BuildSetupHexGrid.Both)
            {
                BuildAutomatic();
            }
        }

        private void OnValidate()
        {
            if (buildSetup == BuildSetupHexGrid.OnValidate || buildSetup == BuildSetupHexGrid.Both)
            {
                BuildAutomatic();
            }
        }

        private void ClampValues()
        {
            rowCount = Mathf.Clamp(rowCount, 1, int.MaxValue);
            columnCount = Mathf.Clamp(columnCount, 1, int.MaxValue);
            spiralRadius = Mathf.Clamp(spiralRadius, 2, int.MaxValue);
        }

        private void ResetAllPositions()
        {
            foreach (Transform t in transform)
            {
                t.localPosition = Vector3.zero;
            }
        }

        private Vector3 GetVectorBasedDimension(float x, float y)
        {
            if (dimension == HexGridDimension._2D)
            {
                return new Vector3(x, y, 0);
            }
            else
            {
                return new Vector3(x, 0, y);
            }
        }

        #endregion

        #region Private static methods

        private static void SetAnchor(RectTransform rectTransform, HexAnchor hexAnchor)
        {
            if (hexAnchor == HexAnchor.TopLeft)
            {
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
            }
            else if (hexAnchor == HexAnchor.TopCenter)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 1);
                rectTransform.anchorMax = new Vector2(0.5f, 1);
            }
            else if (hexAnchor == HexAnchor.TopRight)
            {
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
            }
            else if (hexAnchor == HexAnchor.MiddleLeft)
            {
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(0, 0.5f);
            }
            else if (hexAnchor == HexAnchor.MiddleCenter)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            }
            else if (hexAnchor == HexAnchor.MiddleRight)
            {
                rectTransform.anchorMin = new Vector2(1, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);
            }
            else if (hexAnchor == HexAnchor.BottomLeft)
            {
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
            }
            else if (hexAnchor == HexAnchor.BottomCenter)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0);
                rectTransform.anchorMax = new Vector2(0.5f, 0);
            }
            else
            {
                rectTransform.anchorMin = new Vector2(1, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
            }
        }

        [Obsolete("Not in use. Provides bad results.", true)]
        private static int ExpRegRadius(int childCount)
        {
            return Mathf.RoundToInt(1.232196873f * Mathf.Pow(1.017564087f, childCount));
        }

        private static int PotRegRadius(int childCount)
        {
            return Mathf.RoundToInt(0.3045320175768f * Mathf.Pow(childCount, 0.62678191505273f));
        }

        #endregion

        #region Enums

        /// <summary>
        /// How are the hexagon cells oriented?
        /// </summary>
        public enum HexShape
        {
            PointyTop   = 1,
            FlatTop     = -1
        }

        /// <summary>
        /// How should the grid constraint the cells?
        /// </summary>
        public enum HexGridConstraint
        {
            FixedRowCount,
            FixedColumnCount
        }

        /// <summary>
        /// Which direction should the grid build in? (Positive = Up/Right, Negative = Down/Left).
        /// </summary>
        public enum HexGridDirection
        {
            Positive = 1,
            Negative = -1
        }

        /// <summary>
        /// Which anchor should the cells use (RectTransform(s))?
        /// </summary>
        public enum HexAnchor
        {
            TopLeft,
            TopCenter,
            TopRight,

            MiddleLeft,
            MiddleCenter,
            MiddleRight,

            BottomLeft,
            BottomCenter,
            BottomRight
        }

        /// <summary>
        /// Should it align with the middle cell?
        /// </summary>
        public enum HexGridAlign
        {
            None,
            Center
        }

        /// <summary>
        /// Which row will have the most amount of cells?
        /// If Even Then (even = rowCount, uneven = rowCount - 1) Else If Uneven Then (even = rowCount - 1, uneven = rowCount).
        /// </summary>
        public enum HexGridProminentRow
        {
            Even,
            Uneven
        }

        /// <summary>
        /// When should the grid be built?
        /// </summary>
        public enum BuildSetupHexGrid
        {
            OnTransformChildrenChanged,
            OnValidate,
            Both,
            Never
        }

        /// <summary>
        /// 2D: (x, y); 3D: (x, z);
        /// </summary>
        public enum HexGridDimension
        {
            _2D,
            _3D
        }

        /// <summary>
        /// The component to search for...
        /// </summary>
        public enum HexGridComponent
        {
            Transform,
            RectTransform
        }

        /// <summary>
        /// Square and/or Rectangle OR Spiral?
        /// </summary>
        public enum HexGridShape
        {
            Square,
            Spiral
        }

        #endregion

    }

}
