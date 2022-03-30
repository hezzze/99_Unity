using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NinetyNine
{
    public class TargetSelector : MonoBehaviour
    {
        [SerializeField]
        private Button ButtonPrefab;
        private List<Button> buttons = new List<Button>();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void RenderButtons(List<UIPlayer> uiplayers, Action<string, string> Act)
        {
            // destroy previous buttons
            foreach(var b in buttons)
            {
                Destroy(b.gameObject);
            }
            buttons.Clear();
            

            foreach (var p in uiplayers)
            {
                var button = Instantiate(ButtonPrefab);
                button.GetComponentInChildren<Text>().text = p.PlayerName;
                button.transform.SetParent(gameObject.transform, false);
                button.onClick.AddListener(() => Act(p.PlayerId, p.PlayerName));
                buttons.Add(button);
            }
        }
    }
}

