using Elemental.Tools.EditorActions;
using Elemental.Tools.EditorServices;
using System;
using System.Collections.Generic;

namespace Elemental.Tools.EditorActions
{
    public sealed class EditorActionRegistry : IEditorService
    {
        private readonly Dictionary<EditorActionId, EditorAction> actions = [];

        public void Register(EditorAction action)
        {
            if (!actions.TryAdd(action.Id, action))
            {
                throw new InvalidOperationException($"An action with ID '{action.Id}' is already registered.");
            }
        }

        public void Unregister(EditorActionId id)
        {
            actions.Remove(id);
        }

        public bool Contains(EditorActionId id)
        {
            return actions.ContainsKey(id);
        }

        public bool TryGet(EditorActionId id, out EditorAction? action)
        {
            return actions.TryGetValue(id, out action);
        }

        public EditorAction Get(EditorActionId id)
        {
            if (!actions.TryGetValue(id, out var action))
            {
                throw new KeyNotFoundException($"Action '{id}' is not registered.");
            }

            return action;
        }

        public IEnumerable<EditorAction> All()
        {
            return actions.Values;
        }

        public void Shutdown()
        {
            actions.Clear();
        }
    }
}