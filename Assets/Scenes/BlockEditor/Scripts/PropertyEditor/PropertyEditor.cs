using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropertyEditor : MonoBehaviour {

    public Vector2 Origin;
    public float OffsetPerProperty;

    public GameObject StringPropertyPrefab;
    public GameObject NumberPropertyPrefab;
    public GameObject BooleanPropertyPrefab;

	// Use this for initialization
	void Start () {
        SelectionManager.OnSelectionChanged += OnSelectionChanged;		
	}

    // Update is called once per frame
    void Update () {
		
	}

    private void OnSelectionChanged(object sender, SelectionManager.SelectionChangedEvent e) {
        foreach (var child in gameObject.Children()) {
            Destroy(child);
        }

        if (e.Selection == null) {
            return;
        }

        var provider = e.Selection.GetComponent<PropertyProvider>();
        if (provider != null) {
            PropertyHolder holder = e.Selection.GetComponent<PropertyHolder>();
            if (holder == null) {
                holder = e.Selection.AddComponent<PropertyHolder>();
            }

            Vector3 position = Origin;
            position.z = 0;

            foreach (var property in provider.Properties) {
                GameObject field = null;

                switch (property.Type) {
                case PropertyType.String:
                    field = Instantiate(StringPropertyPrefab);
                    var textField = field.transform.GetChild(1).GetComponent<InputField>();
                    textField.text = holder[property.PropertyName] ?? property.DefaultValue ?? "";
                    textField.transform.GetChild(0).GetComponent<Text>().text = property.Tooltip ?? "";

                    textField.onValueChanged.AddListener(delegate {
                        PropertyChanged(property, holder, textField);
                    });

                    break;

                case PropertyType.Number:
                    field = Instantiate(NumberPropertyPrefab);
                    var numberField = field.transform.GetChild(1).GetComponent<InputField>();
                    numberField.text = holder[property.PropertyName] ?? property.DefaultValue ?? "";
                    numberField.transform.GetChild(0).GetComponent<Text>().text = property.Tooltip ?? "";

                    numberField.onValueChanged.AddListener(delegate {
                        PropertyChanged(property, holder, numberField);
                    });

                    break;

                case PropertyType.Boolean:
                    field = Instantiate(BooleanPropertyPrefab);
                    var toggle = field.transform.GetChild(1).GetComponent<Toggle>();
                    string propertyValue = holder[property.PropertyName] ?? property.DefaultValue ?? "";
                    toggle.isOn = propertyValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);

                    toggle.onValueChanged.AddListener(delegate {
                        PropertyChanged(property, holder, toggle);
                    });

                    break;
                }

                var label = field.transform.GetChild(0).GetComponent<Text>();
                label.text = property.PropertyName;

                field.transform.SetParent(gameObject.transform, false);
                var transform = field.GetComponent<RectTransform>();
                transform.anchoredPosition = position;

                position.y -= OffsetPerProperty;
            }
        }
    }

    private void PropertyChanged(PropertyDefinition property, PropertyHolder holder, ICanvasElement field) {
        string value = null;
        switch (property.Type) {
        case PropertyType.String:
        case PropertyType.Number:
            value = ((InputField)field).text;
            break;

        case PropertyType.Boolean:
            value = ((Toggle)field).isOn ? "true" : "false";
            break;
        }

        holder[property.PropertyName] = value;
    }
}
