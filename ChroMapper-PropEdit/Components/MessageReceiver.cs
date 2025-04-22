using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChroMapper_PropEdit.Components {

// Holy fucking shit I hate unity
public class MessageReceiver : MonoBehaviour {
	public Dictionary<string, UnityAction<object>> events = new Dictionary<string, UnityAction<object>>();
	
	public MessageReceiver AddEvent(string name, UnityAction<object> action) {
		events[name] = action;
		return this;
	}
};

public static class MessageSenderExtentions {
	// Send message to the first Receiver up the hierarchy
	public static bool NotifyUpOnce(this GameObject go, string name, object arg) {
		var it = go.transform;
		// Start with the parent
		while (it = it.parent) {
			if (it.GetComponent<MessageReceiver>() is MessageReceiver receiver) {
				if (receiver.events.ContainsKey(name)) {
					receiver.events[name](arg);
					return true;
				}
			}
		}
		
		return false;
	}
};

}
