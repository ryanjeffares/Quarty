using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GlossaryDefinitionPageController : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private GameObject bg;
    [SerializeField] private Text title, definitions;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        backButton.onClick.AddListener(() =>
        {
            StartCoroutine(FadeInScale(false));
        });
        transform.GetChild(0).localScale = new Vector3(1, 1);
        bg.transform.localScale = new Vector3(0, 0);
        StartCoroutine(FadeInScale(true));
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
    
    private IEnumerator FadeInScale(bool enlarge)
    {
        float timeCounter = 0f;
        while(timeCounter <= 0.5f)
        {
            if (enlarge)
            {
                bg.transform.localScale = new Vector3(curve.Evaluate(timeCounter / 0.5f), curve.Evaluate(timeCounter / 0.5f));
            }
            else
            {
                bg.transform.localScale = new Vector3(curve.Evaluate((0.5f - timeCounter) / 0.5f), curve.Evaluate((0.5f - timeCounter) / 0.5f));
            }
            timeCounter += Time.deltaTime;
            yield return null;
        }
        if (enlarge)
        {
            bg.transform.localScale = new Vector3(1, 1);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
