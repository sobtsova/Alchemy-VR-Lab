using UnityEngine;

public class BookPagesManager : MonoBehaviour
{
    [Header("Book Pages")]
    public GameObject[] pages;
    private int currentPageIndex = 0;

    void Start()
    {
        ShowPage(currentPageIndex);
    }

    public void NextPage()
    {
        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            ShowPage(currentPageIndex);
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            ShowPage(currentPageIndex);
        }
    }

    void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == index);
            }
        }
    }
}