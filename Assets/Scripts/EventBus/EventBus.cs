using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour
{

  public static EventBus Manager { get; private set; }

  private Dictionary<string, List<Action<dynamic,dynamic>>> Events;

// potential event actions
  public enum Actions
  {
    GENERATE_WORLD,
    GENERATE_WORLD_COMPLETE,
    GENERATE_CHUNK
  };

  void Awake()
  {
    if (Manager == null && Manager != this)
    {
      Destroy(gameObject);
    }

    Manager = this;

    // events are tied to a hashset
    Events = new Dictionary<string, List<Action<dynamic,dynamic>>>();

    Events.Add(Actions.GENERATE_WORLD.ToString(), new List<Action<dynamic,dynamic>>());
    Events.Add(Actions.GENERATE_WORLD_COMPLETE.ToString(), new List<Action<dynamic,dynamic>>());
    Events.Add(Actions.GENERATE_CHUNK.ToString(), new List<Action<dynamic,dynamic>>());



    DontDestroyOnLoad(gameObject);
  }

  public void Subscribe(Actions subscribeAction, Action<dynamic,dynamic> Subscriber)
  {    
    string eventName = subscribeAction.ToString();

    var currentActions = Events[eventName];
    currentActions.Add(Subscriber);

    // update the events
    // Events.ContainsKey("rte");
  }

  public void Unsubscribe(Action Subscriber)
  {
    Debug.Log("Unsubscribing: " + Subscriber.Method);
  }

  public void ClearSubscribers()
  {
    Events.Clear();
  }

  public void Broadcast(Actions broadcastEvent, dynamic parameter, dynamic parameter2)
  {
    string eventName = broadcastEvent.ToString();
    var subscribers = Events[eventName];
 
    foreach (var sub in subscribers)
    {
      sub(parameter, parameter2);
    }
  }
}
