using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class InspectorButtonAttribute : Attribute
{
    public string Description { get; }

    public InspectorButtonAttribute(string description)
    {
        Description = description;
    }
}

