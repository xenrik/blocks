using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/**
 * Used to track the currently selected game object
 */
public static class SelectionManager {

    public static GameObject Selection {
        get {
            return _selection;
        }
        set {
            _selection = value;
            NotifySelectionChanged();
        }
    }

    public static event EventHandler<SelectionChangedEvent> OnSelectionChanged;

    private static GameObject _selection;

    private static void NotifySelectionChanged() {
        var eventHandler = OnSelectionChanged;
        if (eventHandler == null) {
            return;
        }

        eventHandler(null, new SelectionChangedEvent(_selection));
    }

    public class SelectionChangedEvent : EventArgs {
        public GameObject Selection { get; private set; }

        public SelectionChangedEvent(GameObject selection) {
            this.Selection = Selection;
        }
    }
}
