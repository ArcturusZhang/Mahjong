using System;

namespace Common.Interfaces
{
    public interface ISubject<out T>
    {
        void AddObserver(IObserver<T> observer);
        void RemoveObserver(IObserver<T> observer);
        void NotifyObservers();
    }
}