using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspensionDebug : MonoBehaviour {

    private bool _init;
    private TextMesh _text;

	// Use this for initialization
	void Start () {
        _text = this.GetComponent<TextMesh>();	
	}

    public void Init(){
        _init = true;
    }
	
	// Update is called once per frame
	public void UpdateData(float ratio) {
        if(_init){
            _text.text = string.Format("{0:.00}", ratio);
        }
	}
}
