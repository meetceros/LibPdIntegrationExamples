using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using static UnityEditor.Experimental.GraphView.GraphView;

namespace AudeLeLuel.RetroOSUIPack
{
    public class CursorManager : MonoBehaviour
    {
        [SerializeField] private LayerMask uiLayer;

        [SerializeField] private Texture2D arrowCursor;
        [SerializeField] private Texture2D clickCursor;
        [SerializeField] private Texture2D waitingCursor;

        private Vector2 hotpoint = new Vector2(2, 2);

        void Start()
        {
            Cursor.SetCursor(arrowCursor, hotpoint, CursorMode.ForceSoftware);
        }

        private void Update()
        {
            List<RaycastResult> results = GetEventSystemRaycastResults();

            if (IsPointerOverSelectable(results, out Selectable selectable))
            {
                Cursor.SetCursor(clickCursor, hotpoint, CursorMode.ForceSoftware);
            }
            else
            {
                Cursor.SetCursor(arrowCursor, hotpoint, CursorMode.ForceSoftware);
            }

        }

        private bool IsPointerOverSelectable(List<RaycastResult> eventSystemRaysastResults, out Selectable selectable)
        {
            for (int index = 0; index < eventSystemRaysastResults.Count; index++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[index];

                if (((1 << curRaysastResult.gameObject.layer) & uiLayer) != 0)
                {
                    if (curRaysastResult.gameObject.TryGetComponent(out selectable))
                    {
                        return true;
                    }
                }
            }
            selectable = null;
            return false;
        }


        private List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }

    }
}