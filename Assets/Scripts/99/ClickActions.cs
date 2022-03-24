using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NinetyNine
{
    [Serializable]
    public class UICardSelectedEvent : UnityEvent<UICard>
    {
    }

    public class ClickActions : MonoBehaviour
    {
        public UICardSelectedEvent OnCardSelected = new UICardSelectedEvent();

        void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                //Debug.Log("Mouse clicked..");
                UICard uicard = MouseOverUICard();

                if (uicard != null)
                {
                    //Debug.Log($"click card: {uicard.OwnerId}");
                    OnCardSelected.Invoke(uicard);
                }
            }
        }

        UICard MouseOverUICard()
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit)
            {
                UICard uicard = hit.transform.gameObject.GetComponent<UICard>();
                if (uicard != null)
                {
                    return uicard;
                }
            }

            return null;
        }
    }
}
