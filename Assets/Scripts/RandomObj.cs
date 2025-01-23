using UnityEngine;

public class RandomObj : MonoBehaviour
{
    private bool isSelected = false; 
    private GameObject[] selectChildren; 

    void Start()
    {
        selectChildren = FindSelectTaggedChildren();
        SetSelectChildrenActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    ToggleSelection();
                }
                else
                {
                    DeselectObject();
                }
            }
        }

        // R 키로 삭제 처리
        if (isSelected && Input.GetKeyDown(KeyCode.R))
        {
            DeleteObject();
        }
    }

    private void ToggleSelection()
    {
        isSelected = !isSelected;

        if (isSelected)
        {
            SetSelectChildrenActive(true);
        }
        else
        {
            DeselectObject();
        }
    }

    private void DeselectObject()
    {
        isSelected = false;
        SetSelectChildrenActive(false);
    }

    private void DeleteObject()
    {
        Destroy(gameObject);
    }

    // 'select' 태그를 가진 모든 차일드 찾기
    private GameObject[] FindSelectTaggedChildren()
    {
        var taggedChildren = new System.Collections.Generic.List<GameObject>();

        foreach (Transform child in transform)
        {
            if (child.CompareTag("select"))
            {
                taggedChildren.Add(child.gameObject);
            }
        }

        return taggedChildren.ToArray();
    }

    private void SetSelectChildrenActive(bool active)
    {
        foreach (GameObject child in selectChildren)
        {
            child.SetActive(active);
        }
    }
}
