using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollviewArrowAnimator : MonoBehaviour
{
    private RectTransform _rt;        

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        while(enabled)
        {
            for(int i = 0; i < 360; i += 3)
            {
                float val = (float)Persistent.sineWaveValues[i] * 0.1f;
                _rt.localScale = new Vector3(0.7f + val, 0.7f + val);
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
    }
}
