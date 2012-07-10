namespace hobd{

using System;

/// <summary>
/// Abstract sensor provider
/// </summary>
/// Provides a group of sensors to the SensorRegistry
public interface SensorProvider
{

    /// <summary>
    /// Name of this provider
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/>
    /// </returns>
    string GetName();
	/// <summary>
    /// Description of this provider
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/>
    /// </returns>
	string GetDescription();

    /// <summary>
    /// Provider activator. Provider should create its sensor group and attach all of them to the registry
    /// </summary>
    /// <param name="registry">
    /// A <see cref="SensorRegistry"/> which should be filled with all available sensors
    /// </param>
	void Activate(SensorRegistry registry);

}

}