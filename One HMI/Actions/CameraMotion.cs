using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sean21.OneHMI
{
    public static class CameraMotion
    {
        public static IEnumerator Morph(this Camera cam, Matrix4x4 source, Matrix4x4 dest, float duration) {
            float start = Time.time;
            Matrix4x4 current = source;
            while(true) {
                float t = (Time.time - start)/duration;
                float sinT = Mathf.Sin((Mathf.PI/2) * t);
                for (int i=0; i<16; i++) current[i] = Mathf.Lerp(source[i], dest[i], sinT);
                cam.projectionMatrix = current;
                if (t >= 1f) break;
                yield return null;
            }
            cam.projectionMatrix = dest;
        }
        // public static IEnumerator Frame(this Camera cam)
    }
}
