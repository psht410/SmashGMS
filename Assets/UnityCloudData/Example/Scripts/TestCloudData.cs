using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityCloudData;

public class TestCloudData : CloudDataMonoBehaviour {
    [CloudDataField]
    public int intValue = 1;

    [CloudDataField]
    public float floatValue = 1F;

    [CloudDataField]
    public string stringValue = "string";

    [CloudDataField]
    public bool boolValue = false;

    [CloudDataField]
    public List<string> listValue;

    [CloudDataField]
    public Color colorValue;

    [CloudDataField]
    public Vector3 vector3Value;

    [CloudDataField(sheetPath="localization")]
    public string localizedString = "abc";

    void Start () {
    }
}
