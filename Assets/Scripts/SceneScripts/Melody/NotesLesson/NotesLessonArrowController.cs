using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesLessonArrowController : MonoBehaviour
{
    private IEnumerator Move()
    {
        while(GetComponent<RectTransform>().localPosition.x <= 220)
        {
            var pos = GetComponent<RectTransform>().position;
            pos.x += 2;
            GetComponent<RectTransform>().position = pos;
            yield return new WaitForSeconds(0.005f);
        }
    }
}
