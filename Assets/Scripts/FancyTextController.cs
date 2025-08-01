using UnityEngine;
using TMPro;

/***
 * 
 * FancyText Controller, Responsible for updating the white, red and blue text components
 * 
 * Written by aleu0007
 * Last Modified: 1/08/2025
 * 
 */
public class FancyTextController : MonoBehaviour
{   
    [SerializeField] private TextMeshProUGUI[] textFields;
    [TextArea]
    [SerializeField] private string editorText = "Default Text";

    [ContextMenu("Update Texts")]
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
         if (textFields == null || textFields.Length == 0)
        {
            textFields = GetComponentsInChildren<TextMeshProUGUI>();
        }
    }

    // Update is called once per frame
    public void SetEditorText()
    {
        foreach (var tmp in textFields)
        {
            if (tmp != null)
                tmp.text = editorText;
        }
    }
}
