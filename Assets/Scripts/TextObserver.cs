using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using TrendMe;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextObserver : MonoBehaviour
{
    [SerializeField] 
    private RemoteObservableField Observable;
    public string Format;

    private Text _text;
    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<Text>();
        if (Observable.Value == null)
        {
            throw new ArgumentNullException($"Observable not set at :{gameObject.GetGameObjectPath()}");
        }
        Observable.Value.Subscribe(ChangeText);
    }

    protected virtual void ChangeText(object obj)
    {
        _text.text = string.IsNullOrWhiteSpace(Format) ? obj.ToString() : string.Format(Format, obj);
    }
}
