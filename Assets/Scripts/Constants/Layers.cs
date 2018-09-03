using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Layers {
    PIPS
}

static class LayerMethods {
    public static string LayerName(this Layers layer) {
        switch (layer) {
        case Layers.PIPS: return "Pips";

        default:
            return null;
        }
    }

    public static int Mask(this Layers layer) {
        return LayerMask.GetMask(layer.LayerName());
    }

    public static int ExclusionMask(this Layers layer) {
        return ~layer.Mask();
    }
}
