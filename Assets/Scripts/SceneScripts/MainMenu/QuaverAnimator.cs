using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class QuaverAnimator : MonoBehaviour
{
    private Vector3 _position;
    private float _y;
    private float _timer;
    private bool _closed;
    private readonly Random _random = new Random();
    [SerializeField] private Sprite eyesClosed, eyesOpen;
    [SerializeField] private Image face;
    [SerializeField] private bool shouldBlink, shouldMove;

    private void Awake()
    {
        _position = transform.localPosition;
        _y = _position.y;
        if(shouldBlink || shouldMove)
            StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        while (enabled)
        {
            for (int i = 0; i < 360; i+=2)
            {
                if(shouldMove)
                {
                    _position.y = _y + (float) Persistent.sineWaveValues[i] * 15;
                    transform.localPosition = new Vector3(_position.x, _position.y);
                }

                if(shouldBlink)
                {
                    if (!_closed)
                    {
                        int num = _random.Next(1000);
                        if (num < 3)
                        {
                            face.sprite = eyesClosed;
                            _closed = true;
                        }
                    }
                    else
                    {
                        _timer += Time.deltaTime;
                        if (_timer >= 0.2f)
                        {
                            face.sprite = eyesOpen;
                            _timer = 0;
                            _closed = false;
                        }
                    }
                }
                
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
    }
}
