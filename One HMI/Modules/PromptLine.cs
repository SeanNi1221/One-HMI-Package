using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sean21.OneHMI
{
    using static Generics;
    public class PromptLine : MonoBehaviour
    {
        static PromptLine instance;
        [SerializeField]private Animator anim = null;
        // [SerializeField]private TMP_Text ui = null;
        [SerializeField]private GeneralText ui = null;
        [SerializeField][TextArea]private string previous = null;
        void Awake()
        {
            instance = this;
        }
        public static void Print(string content)
        {
            if (!instance) return;
            if ( !string.IsNullOrEmpty(instance.ui.text) ){
                instance.previous = instance.ui.text;
            }
            instance.ui.text = content;
            instance.anim.Play("textExpandIn",-1,0f);
        }
        public static void Change(string content)
        {
            if (!instance) return;
            if ( !string.IsNullOrEmpty(instance.ui.text) ){
                instance.previous = instance.ui.text;
            }
            instance.ui.text = content;        
        }
        public static void Restore()
        {
            if (!instance) return;
            if ( !string.IsNullOrEmpty(instance.previous) ){
                instance.ui.text = instance.previous;
            }
        }
        public static void Clear()
        {
            if (!instance) return;
            instance.anim.Play("textExpandStart");
        }
    }
}
