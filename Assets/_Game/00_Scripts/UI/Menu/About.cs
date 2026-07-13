using UnityEngine;

public class About : MonoBehaviour
{
    [SerializeField] private GameObject aboutMenu;

    public void CloseAbout()
    {
        aboutMenu.SetActive(false);
    }
}