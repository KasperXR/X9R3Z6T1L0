using UnityEngine;
using UnityEngine.UI;

public class ListButtonController : MonoBehaviour
{
    public Button CurvedListButton;
    public Button StraightListButton;
    public Button OtherListButton;

    private GameObject _curvedListObjects;
    private GameObject _straightListObjects;
    private GameObject _otherListObjects;

    void Start()
    {
        _curvedListObjects = CurvedListButton.transform.Find("Objects").gameObject;
        _straightListObjects = StraightListButton.transform.Find("Objects").gameObject;
        _otherListObjects = OtherListButton.transform.Find("Objects").gameObject;

        CurvedListButton.onClick.AddListener(() => SwitchList(_curvedListObjects));
        StraightListButton.onClick.AddListener(() => SwitchList(_straightListObjects));
        OtherListButton.onClick.AddListener(() => SwitchList(_otherListObjects));

        // Set StraightList as the default active list
        SwitchList(_straightListObjects);
    }

    private void SwitchList(GameObject activeList)
    {
        _curvedListObjects.SetActive(false);
        _straightListObjects.SetActive(false);
        _otherListObjects.SetActive(false);

        activeList.SetActive(true);
    }
}