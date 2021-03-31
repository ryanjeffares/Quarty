using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlossaryDefinitionPageController : MonoBehaviour
{
    [SerializeField] private Text title, definitions;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        backButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }

    public string Title
    {
        set
        {
            title.text = value;
        }
    }

    public string Definition
    {
        set
        {
            definitions.text = value;
        }
    }    
}
