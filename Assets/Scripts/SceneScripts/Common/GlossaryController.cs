using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlossaryController : BaseManager
{
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject tabPrefab;
    [SerializeField] private GameObject definitionPagePrefab;
    [SerializeField] private Text text;

    private List<GameObject> _tabs = new List<GameObject>();

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>        
        {
            {backButton, (g) => Destroy(gameObject) }
        };       
        if(Persistent.glossaryWords.Count == 0)
        {
            text.text = "Nothing here yet! Complete some of the lessons to add terms to your glossary.";
        }
        else
        {
            foreach (var (term, index) in Persistent.glossaryWords.WithIndex())
            {
                var tab = Instantiate(tabPrefab, content.transform);
                tab.GetComponentInChildren<Text>().text = term;
                var col = Persistent.rainbowColours[index % Persistent.rainbowColours.Count];
                col.a = 0.3f;
                tab.GetComponent<GlossaryTabController>().Show(col, index * 0.1f);
                buttonCallbackLookup.Add(tab, TabCallback);
                _tabs.Add(tab);
            }
            var size = content.GetComponent<RectTransform>().sizeDelta;
            content.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, _tabs.Sum(t => t.GetComponent<RectTransform>().sizeDelta.y + 30));
        }        
    }

    private void TabCallback(GameObject g)
    {
        var def = Instantiate(definitionPagePrefab, transform);
        def.GetComponent<GlossaryDefinitionPageController>().Title = Persistent.glossaryWords[_tabs.IndexOf(g)];
        def.GetComponent<GlossaryDefinitionPageController>().Definition = Persistent.glossaryDescriptions[Persistent.glossaryWords[_tabs.IndexOf(g)]];
    }
}
