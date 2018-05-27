using UnityEngine;

namespace UI
{

    [ExecuteInEditMode]
    public class HexGridLayoutGroup : MonoBehaviour
    {

        ///TODO: Update <seealso cref="HexGridLayoutGroup"></seealso>
        //[1]
        //Change the search for RectTransform(s) to Transform(s) and use "GetComponent<RectTransform>()" instead.
        //However, not needed right now. If you want to change that, then go to the foreach statement
        //and change "RectTransform" to "Transform" and use "UnityEngine.GetComponent<RectTransform>()" instead. Intellisense should guide you through it.

        //[2]
        //Upgrade the align features to work with dynamical row- and columnCounts.

        //[3]
        //Optimize.

        #region Fields

        [Tooltip("How big are the hex cells? For best looking result, hand edit this value.")] public Vector2 hexSize = new Vector2(100, 100);

        [Space]
        [Tooltip("How are the hexagon cells oriented?")] public HexShape shape = HexShape.PointyTop;

        [Space]
        [Tooltip("How should the grid constraint the cells?")] public HexGridConstraint constraint = HexGridConstraint.FixedRowCount;
        public int rowCount = 5;
        public int columnCount = 5;

        [Space]
        [Tooltip("Which direction should the grid build in? (Positive = Up/Right, Negative = Down/Left). X coordinate.")] public HexGridDirection rowDirection = HexGridDirection.Positive;
        [Tooltip("Which direction should the grid build in? (Positive = Up/Right, Negative = Down/Left). Y coordinate.")] public HexGridDirection columnDirection = HexGridDirection.Positive;

        [Space]
        [Tooltip("Which anchor should the cells use (RectTransform(s))?")] public HexAnchor anchor = HexAnchor.MiddleCenter;

        [Space]
        [Tooltip("Should it align with the middle cell? X coordinate. NOTE: This will ONLY work properly if the rowCount is set to the acutal amount of rows.")] public HexGridAlign alignX = HexGridAlign.None;
        [Tooltip("Should it align with the middle cell? Y coordinate. NOTE: This will ONLY work properly if the columnCount is set to the acutal amount of columns.")] public HexGridAlign alignY = HexGridAlign.None;

        [Space]
        [Tooltip("Which row will have the most amount of cells? NOTE: Will only work with constraint set to \"FixedRowCount\".")] public HexGridProminentRow prominentRow = HexGridProminentRow.Even;

        [Space]
        [Tooltip("When should the grid be built?")] public BuildSetupHexGrid buildSetup = BuildSetupHexGrid.Both;

        #endregion

        #region Private methods

        private void OnTransformChildrenChanged()
        {
            if (buildSetup != BuildSetupHexGrid.OnValidate)
            {
                Setup();
            }
        }

        private void OnValidate()
        {
            if (buildSetup != BuildSetupHexGrid.OnTransformChildrenChanged)
            {
                Setup();
            }
        }

        private void Setup()
        {
            rowCount = Mathf.Clamp(rowCount, 1, int.MaxValue);
            columnCount = Mathf.Clamp(columnCount, 1, int.MaxValue);

            Vector3 position = Vector3.zero;
            int x = 0, y = 0;

            float innerRadius = hexSize.y * 0.866025404f;
            float innerRadiusMul2 = innerRadius * 2;

            float hexSizeXDiv2 = hexSize.x / 2;
            float hexSizeYDiv2 = hexSize.y / 2;

            //Get middle of hex grid:
            float middleX = (rowCount * hexSize.x - ((1 - (int)prominentRow) * (1 - (int)constraint) * hexSize.x)) / 2;
            float middleY = (columnCount * innerRadius - innerRadius) / 2;

            middleX = ((int)alignX * middleX * ((int)rowDirection * -1));
            middleY = ((int)alignY * middleY * ((int)columnDirection * -1));

            foreach (RectTransform rt in transform)
            {
                SetAnchor(rt, anchor);

                if (shape == HexShape.PointyTop)
                {
                    position.x = (x * hexSize.x + ((y % 2) * hexSizeXDiv2)) * (int)rowDirection + middleX;
                    position.y = y * innerRadius * (int)columnDirection + middleY;
                }
                else
                {
                    position.x = (x * innerRadiusMul2 + ((y % 2) * innerRadius)) * (int)rowDirection + middleX;
                    position.y = y * hexSizeYDiv2 * (int)columnDirection + middleY;
                }

                rt.anchoredPosition = position;

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

        #endregion

        #region Static methods

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

        #endregion

        #region Enums

        /// <summary>
        /// How are the hexagon cells oriented?
        /// </summary>
        public enum HexShape
        {
            PointyTop,
            FlatTop
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
            Both
        }

        #endregion

    }

}
