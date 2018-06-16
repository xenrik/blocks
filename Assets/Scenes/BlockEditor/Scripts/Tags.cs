﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tags {
    // Tag that game objects should have if they are (real) blocks
    BLOCK,

    // Tag that camera's should have if they want to use 'flight' controls, rather than 'normal' controls
    FLIGHT_CAMERA,

    // Tag that blocks which are in the palette should have
    PALETTE_BLOCK,

    // Tag that blocks which are templates should have
    TEMPLATE_BLOCK
}

static class TagMethods {
    public static string Name(this Tags tag) {
        switch (tag) {
        case Tags.BLOCK: return "Block";
        case Tags.FLIGHT_CAMERA: return "FlightCamera";
        case Tags.PALETTE_BLOCK: return "PaletteBlock";
        case Tags.TEMPLATE_BLOCK: return "TemplateBlock";

        default:
            return null;
        }
    }

    public static bool HasTag(this Tags tag, Component comp) {
        return comp.CompareTag(tag.Name());
    }
}