/* ========= Soju06 Unity Utility =========
 * NAMESPACE: Soju06.Unity.Utility
 * LICENSE: MIT
 * Copyright by Soju06
 * ========= Soju06 Unity Utility ========= */
using System;
using System.Collections.Generic;

namespace Soju06.Net.Unity.Utility {
    public class CrossThreading {
        private readonly List<Action> Actions = new List<Action>();
        private readonly List<BeginAction> BeginActions = new List<BeginAction>();

        public CrossThreading() => Actions = new List<Action>();

        public void Invoke(Action action) => Actions.Add(action);
        public void BeginInvoke(Action action, AsyncCallback callback = null, 
            object @object = null) => BeginActions.Add(new BeginAction() 
            { Action = action, Callback = callback, Object = @object });

        public void QueueActionInvoke() {
            lock (Actions) { 
                if(Actions.Count > 0)
                    while (Actions.Count > 0) {
                        var action = Actions[0];
                        try {
                            action?.Invoke();
                        } catch (Exception ex) {
                            UnityEngine.Debug.LogError(ex);
                        } Actions.Remove(action);
                    }
            }
            lock (BeginActions) {
                if (BeginActions.Count > 0)
                    while (BeginActions.Count > 0) {
                        var beginAction = BeginActions[0];
                        try {
                            beginAction?.Action?.BeginInvoke(beginAction.Callback,
                                beginAction.Object);
                        } catch (Exception ex) {
                            UnityEngine.Debug.LogError(ex);
                        } BeginActions.Remove(beginAction);
                    }
            }
        }

        private class BeginAction {
            public Action Action { get; set; }
            public AsyncCallback Callback { get; set; }
            public object Object { get; set; }
        }
    }
}
