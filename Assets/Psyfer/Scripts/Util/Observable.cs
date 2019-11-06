using System.Collections.Generic;
using UnityEngine;

public abstract class Observable<T> : MonoBehaviour
{
    private List<Observer<T>> observers = new List<Observer<T>>();

    public void AddObserver(Observer<T> observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(Observer<T> observer)
    {
        observers.Remove(observer);
    }

    protected void NotifyObservers(T data)
    {
        foreach(Observer<T> observer in observers)
        {
            observer.Notified(data);
        }
    }
}
