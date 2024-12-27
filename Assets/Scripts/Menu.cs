using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject popup; 
    private bool isPopupActive = false; 
    public GameObject title, control;

    private void Start()
    {
        popup.SetActive(false); // 처음에는 팝업창 비활성화
        title.SetActive(true);
        control.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            TogglePopup(); 
        }
    }

    public void Title()
    {
        title.SetActive(false);
    }

    // Info 버튼에 연결
    public void ShowPopup()
    {
        popup.SetActive(true);
        isPopupActive = true;
    }

    // Close 버튼에 연결
    public void ClosePopup()
    {
        if (popup != null)
        {
            popup.SetActive(false);
            isPopupActive = false;
        }
    }

    // 팝업창 상태를 토글
    private void TogglePopup()
    {
        isPopupActive = !isPopupActive;
        popup.SetActive(isPopupActive);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MenuControl()
    {
        control.SetActive(true);
    }

    public void CloseControl()
    {
        control.SetActive(false);
    }
}
