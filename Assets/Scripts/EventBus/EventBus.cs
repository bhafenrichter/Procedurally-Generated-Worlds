using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour
{

  public static EventBus Manager { get; private set; }

  private Dictionary<string, List<Action>> Events;

// potential event actions
  public enum Actions
  {
    GENERATE_WORLD,
    GENERATE_CHUNK
  };

  void Awake()
  {
    // events are tied to a hashset
    Events = new Dictionary<string, List<Action>>();

    if (Manager == null && Manager != this)
    {
      Destroy(gameObject);
    }

    Manager = this;

    DontDestroyOnLoad(gameObject);
  }

  public void Subscribe(Actions subscribeAction, Action Subscriber)
  {    
    string eventName = subscribeAction.ToString();
    List<Action> currentActions = new List<Action>();

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

  public void Broadcast(Actions broadcastEvent)
  {
    string eventName = broadcastEvent.ToString();
    var subscribers = Events[eventName];

    foreach (var sub in subscribers)
    {
      sub();
    }
  }
}
