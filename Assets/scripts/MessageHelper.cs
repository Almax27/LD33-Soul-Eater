using UnityEngine;
using System.Collections;

public class MessageHelper : MonoBehaviour {

    [System.Serializable]
    public enum TriggerType
    {
        None,
        OnTriggerEnter
    }

    [System.Serializable]
    public class Message 
    {
        public TriggerType triggerType = TriggerType.None;
        public GameObject target = null;
        public string function = "";
        public Object value = null;
    }
    public Message[] messages = new Message[0];

    void ProcessMessages(TriggerType triggerType)
    {
        foreach (Message m in messages)
        {
            if(m.target && m.triggerType == triggerType)
            {
                m.target.SendMessage(m.function, m.value);
            }
        }
    }

    void OnTriggerEnter()
    {
        ProcessMessages(TriggerType.OnTriggerEnter);
    }
}
