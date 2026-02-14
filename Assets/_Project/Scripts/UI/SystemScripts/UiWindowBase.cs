using UnityEngine;

namespace Game.UI
{
    public abstract class UiWindowBase : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
           
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            
        }

    }
}