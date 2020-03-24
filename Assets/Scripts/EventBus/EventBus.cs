using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour
{

  public static EventBus Manager { get; private set; }

  private Dictionary<string, List<Action<dynamic>>> Events;

// potential event actions
  public enum Actions
  {
    GENERATE_WORLD,
    GENERATE_WORLD_COMPLETE,
    GENERATE_CHUNK
  };

  void Awake()
  {
    // events are tied to a hashset
    Events = new Dictionary<string, List<Action<dynamic>>>();

    if (Manager == null && Manager != this)
    {
      Destroy(gameObject);
    }

    Manager = this;

    // DontDestroyOnLoad(gameObject);
  }

  public void Subscribe(Actions subscribeAction, Action<dynamic> Subscriber)
  {    
    string eventName = subscribeAction.ToString();
    List<Action<dynamic>> currentActions = new List<Action<dynamic>>();

    if (Events.ContainsKey(eventName)) {
      // get the events and add it to the end
      currentActions = Events[eventName];
    }
    
    currentActions.Add(Subscriber);

    // update the events
    Events.Add(eventName, currentActions);
  }

  public void Unsubscribe(Action Subscriber)
  {
    Debug.Log("Unsubscribing: " + Subscriber.Method);
  }

  public void ClearSubscribers()
  {
    Events.Clear();
  }

  public void Broadcast(Actions broadcastEvent, dynamic parameter)
  {
    string eventName = broadcastEvent.ToString();
    var subscribers = Events[eventName];
 
    foreach (var sub in subscribers)
    {
      sub(parameter);
    }
  }
}
