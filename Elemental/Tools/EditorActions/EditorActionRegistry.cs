using Elemental.Tools.EditorActions;
using Elemental.Tools.EditorServices;
using System;
using System.Collections.Generic;

namespace Elemental.Tools.EditorActions
{
    public sealed class EditorActionRegistry : IEditorService
    {
        private readonly Dictionary<EditorActionId, EditorAction> _actions = [];

        public void Register(EditorAction action)
        {
            if (!_actions.TryAdd(action.Id, action))
            {
                throw new InvalidOperationException($"An action with ID '{action.Id}' is already registered.");
            }
        }

        public void Unregister(EditorActionId id)
        {
            _actions.Remove(id);
        }

        public bool Contains(EditorActionId id)
        {
            return _actions.ContainsKey(id);
        }

        public bool TryGet(EditorActionId id, out EditorAction? action)
        {
            return _actions.TryGetValue(id, out action);
        }

        public EditorAction Get(EditorActionId id)
        {
            if (!_actions.TryGetValue(id, out var action))
            {
                throw new KeyNotFoundException($"Action '{id}' is not registered.");
            }

            return action;
        }

        public IEnumerable<EditorAction> All()
        {
            return _actions.Values;
        }

        public void Shutdown()
        {
            _actions.Clear();
        }
    }
}