using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tags {
    // Tag that game objects should have if they are (real) blocks
    BLOCK,

    // Tag that cameras that can be dragged should have
    DRAG_CAMERA,

    // Tag that blocks should have if they are used in the editor
    EDITOR_BLOCK,

    // Tag that pips on blocks that are in the editor should have
    EDITOR_PIP,

    // Tag that camera's should have if they want to use 'flight' controls, rather than 'normal' controls
    FLIGHT_CAMERA,

    // Tag that blocks which are in the palette should have
    PALETTE_BLOCK,

    // Tag that the camera for the palette has
    PALETTE_CAMERA,
    
    // Tag that blocks which are templates should have
    TEMPLATE_BLOCK,

    // Tag that pips which are templates should have
    TEMPLATE_PIP
}

static class TagMethods {
    public static string TagName(this Tags tag) {
        switch (tag) {
        case Tags.BLOCK: return "Block";
        case Tags.DRAG_CAMERA: return "DragCamera";
        case Tags.EDITOR_BLOCK: return "EditorBlock";
        case Tags.EDITOR_PIP: return "EditorPip";
        case Tags.FLIGHT_CAMERA: return "FlightCamera";
        case Tags.PALETTE_BLOCK: return "PaletteBlock";
        case Tags.PALETTE_CAMERA: return "PaletteCamera";
        case Tags.TEMPLATE_BLOCK: return "TemplateBlock";
        case Tags.TEMPLATE_PIP: return "TemplatePip";

        default:
            return null;
        }
    }

    public static bool HasTag(this Tags tag, Component comp) {
        return comp.CompareTag(tag.TagName());
    }
    public static bool HasTag(this Tags tag, GameObject go) {
        return go.CompareTag(tag.TagName());
    }
}