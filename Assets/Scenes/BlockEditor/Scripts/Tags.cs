using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tags {
    // Tag that game objects should have if they are (real) blocks
    BLOCK,

    // Tag that blocks should have if they are used in the editor
    EDITOR_BLOCK,

    // Tag that camera's should have if they want to use 'flight' controls, rather than 'normal' controls
    FLIGHT_CAMERA,

    // Tag that blocks which are in the palette should have
    PALETTE_BLOCK,

    // Tag that pips should have
    PIP,

    // Tag that blocks which are templates should have
    TEMPLATE_BLOCK
}

static class TagMethods {
    public static string Name(this Tags tag) {
        switch (tag) {
        case Tags.BLOCK: return "Block";
        case Tags.EDITOR_BLOCK: return "EditorBlock";
        case Tags.FLIGHT_CAMERA: return "FlightCamera";
        case Tags.PALETTE_BLOCK: return "PaletteBlock";
        case Tags.PIP: return "Pip";
        case Tags.TEMPLATE_BLOCK: return "TemplateBlock";

        default:
            return null;
        }
    }

    public static bool HasTag(this Tags tag, Component comp) {
        return comp.CompareTag(tag.Name());
    }
    public static bool HasTag(this Tags tag, GameObject go) {
        return go.CompareTag(tag.Name());
    }
}