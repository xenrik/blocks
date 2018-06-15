using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tags {
    // Tag that camera's should have if they want to use 'flight' controls, rather than 'normal' controls
    FLIGHT_CAMERA,

    // Tag that game objects should have if they are blocks
    BLOCK
}

static class TagMethods {
    public static string Name(this Tags tag) {
        switch (tag) {
        case Tags.FLIGHT_CAMERA: return "FlightCamera";
        case Tags.BLOCK: return "Block";

        default:
            return null;
        }
    }

    public static bool HasTag(this Tags tag, Component comp) {
        return comp.CompareTag(tag.Name());
    }
}