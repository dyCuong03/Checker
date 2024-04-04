using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonMenu : MonoBehaviour
{
	public GameObject Panel;
	public void ChoiMoi()
	{
		SceneManager.LoadScene(2);
	}

	public void Thoat()
	{
		Application.Quit();
	}

	public void LuatChoi()
	{
		SceneManager.LoadScene(1);
	}

	public void VeMenu()
	{
		SceneManager.LoadScene(0);
	}
	public void OpenPanel()
	{
		if (Panel != null)
		{
			Panel.SetActive(true);
		}
	}
}
