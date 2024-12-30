using UnityEngine;
using UnityEngine.EventSystems;

public class FCP_ExampleScript : MonoBehaviour
{
    public Material material; // 원래의 Material

    private Material instanceMaterial; // 개별 오브젝트에 사용할 Material 인스턴스
    private Color defaultColor; // 초기 색상 저장
    private Color highlightColor = new Color(0, 0, 0, 0.5f); // 반투명 검정색 (50% 투명도)
    private static FCP_ExampleScript currentlySelected; // 현재 선택된 오브젝트
    private FlexibleColorPicker fcp; // FlexibleColorPicker 참조
    private float fluidAlpha = 0.72f; // Fluid 태그의 투명도

    private void Start()
    {
        // FlexibleColorPicker 찾기
        fcp = GameObject.FindWithTag("FCP").GetComponent<FlexibleColorPicker>();

        // Material의 인스턴스 생성
        instanceMaterial = new Material(material);
        GetComponent<Renderer>().material = instanceMaterial;

        // 초기 색상 설정
        if (gameObject.CompareTag("Fluid"))
        {
            // Fluid 태그는 항상 투명도 72% 유지
            defaultColor = instanceMaterial.GetColor("_Color");
            SetFluidColor(defaultColor);
        }
        else
        {
            // 일반 오브젝트는 불투명 흰색으로 초기화
            defaultColor = Color.white;
            SetOpaqueMaterial();
            instanceMaterial.color = defaultColor;
        }

        // FlexibleColorPicker 초기화
        if (currentlySelected == this)
        {
            fcp.color = defaultColor;
        }

        // FlexibleColorPicker의 색상 변경 이벤트 등록
        fcp.onColorChange.AddListener(OnChangeColor);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; // UI 위에서 클릭한 경우 뒤쪽 3D 오브젝트로 이벤트 전달 차단
            }

            // UI 위가 아닌 경우 3D 오브젝트 클릭 처리
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide))
            {
                // 클릭한 오브젝트가 현재 오브젝트인지 확인
                if (hit.collider.gameObject == gameObject)
                {
                    SelectObject();
                }
            }
        }
    }

    private void SelectObject()
    {
        // 현재 선택된 오브젝트 상태 복구
        if (currentlySelected != null && currentlySelected != this)
        {
            currentlySelected.Deselect();
        }

        // 현재 오브젝트를 선택
        currentlySelected = this;

        // FlexibleColorPicker 리스너 초기화
        fcp.onColorChange.RemoveAllListeners();
        fcp.onColorChange.AddListener(OnChangeColor);

        // FlexibleColorPicker 색상 동기화
        fcp.color = defaultColor;

        // 하이라이트 색상 적용
        if (gameObject.CompareTag("Fluid"))
        {
            SetFluidColor(highlightColor);
        }
        else
        {
            SetTransparentMaterial();
            instanceMaterial.color = highlightColor;
        }
    }

    private void Deselect()
    {
        // 선택 해제 시 기본 상태 복구
        if (gameObject.CompareTag("Fluid"))
        {
            SetFluidColor(defaultColor);
        }
        else
        {
            SetOpaqueMaterial();
            instanceMaterial.color = defaultColor;
        }
    }

    private void OnChangeColor(Color co)
    {
        // 현재 선택된 오브젝트의 색상 변경
        if (currentlySelected == this)
        {
            if (gameObject.CompareTag("Fluid"))
            {
                SetFluidColor(co);
                defaultColor = co;
            }
            else
            {
                SetOpaqueMaterial();
                instanceMaterial.color = co;
                defaultColor = co;
            }
        }
    }

    private void SetFluidColor(Color baseColor)
    {
        Color fluidColor = baseColor;
        fluidColor.a = fluidAlpha; // 투명도 72% 유지
        instanceMaterial.SetColor("_Color", fluidColor);
    }

    private void SetTransparentMaterial()
    {
        instanceMaterial.SetOverrideTag("RenderType", "Transparent");
        instanceMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        instanceMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        instanceMaterial.SetInt("_ZWrite", 0);
        instanceMaterial.DisableKeyword("_ALPHATEST_ON");
        instanceMaterial.EnableKeyword("_ALPHABLEND_ON");
        instanceMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        instanceMaterial.renderQueue = 3000;
    }

    private void SetOpaqueMaterial()
    {
        instanceMaterial.SetOverrideTag("RenderType", "Opaque");
        instanceMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        instanceMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        instanceMaterial.SetInt("_ZWrite", 1);
        instanceMaterial.DisableKeyword("_ALPHATEST_ON");
        instanceMaterial.DisableKeyword("_ALPHABLEND_ON");
        instanceMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        instanceMaterial.renderQueue = 2000;
    }
}
