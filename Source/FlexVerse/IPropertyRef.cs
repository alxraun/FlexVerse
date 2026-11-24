// parents: @.frostline/graph/m-property-ref.xml

using System;

namespace FlexVerse
{
    /// <summary>References a reactive behavior property slot with change notifications. (M-PROPERTY-REF)</summary>
    public interface IPropertyRef
    {
        /// <summary>Gets the property name of the slot.</summary>
        string Name { get; }

        /// <summary>Gets the owner object holding the slot.</summary>
        object Owner { get; }

        /// <summary>Raises before a real occupant replacement.</summary>
        event Action Changing;

        /// <summary>Raises after a real occupant replacement.</summary>
        event Action Changed;

        /// <summary>Returns the current occupant of the slot. (REQ-003)</summary>
        Behavior Get();

        /// <summary>Assigns a new occupant to the slot. (REQ-003)</summary>
        void Set(Behavior value);
    }

    /// <summary>References a typed reactive behavior property slot.</summary>
    public interface IPropertyRef<T> : IPropertyRef
    {
        /// <summary>Returns the current typed occupant of the slot.</summary>
        new Behavior<T> Get();

        /// <summary>Assigns a new typed occupant to the slot.</summary>
        void Set(Behavior<T> value);
    }
}
