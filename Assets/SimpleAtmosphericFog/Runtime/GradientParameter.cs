using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

namespace OG {
    [System.Serializable]
    public sealed class GradientParameter : VolumeParameter<Gradient>{
        public GradientParameter(bool overrideState) {
            this.overrideState = overrideState;

            var g = new Gradient();
            GradientColorKey[] colorKey;
            GradientAlphaKey[] alphaKey;

            colorKey = new GradientColorKey[2];
            colorKey[0].color = Color.white;
            colorKey[0].time = 0.0f;
            colorKey[1].color = new Color(0.7490196f, 0.8901961f, 1, 1);
            colorKey[1].time = 1.0f;

            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 1.0f;
            alphaKey[0].time = 0;
            alphaKey[1].alpha = 1f;
            alphaKey[1].time = 1.0f;

            g.SetKeys(colorKey, alphaKey);

            this.value = g;
        }
        public GradientParameter(Color color1, Color color2, bool overrideState){
            this.overrideState = overrideState;

            var g = new Gradient();

            GradientColorKey[] colorKey;
            GradientAlphaKey[] alphaKey;

            colorKey = new GradientColorKey[2];
            colorKey[0].color = color1;
            colorKey[0].time = 0.0f;
            colorKey[1].color = color2;
            colorKey[1].time = 1.0f;

            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 1.0f;
            alphaKey[0].time = 0;
            alphaKey[1].alpha = 1f;
            alphaKey[1].time = 1.0f;

            g.SetKeys(colorKey, alphaKey);

            this.value = g;
        }

        protected override void OnEnable(){
            //if (value == null) {}
        }
    }
}
