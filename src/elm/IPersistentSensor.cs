using System;
using System.Collections.Generic;

namespace hobd
{

/// <summary>Interface is used to denote sensors who want to make their value persistent</summary>
/// <para>Persistence means sensor last value is stored even between system runs. System will store/restore its value into persistent storage.</para>
public interface IPersistentSensor
{
    /// <summary>
    /// Serialization of current sensor state
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> presentation, which records current sensor state/value
    /// </returns>
    string StoreState();

    /// <summary>
    /// Deserialization of sensor state from string presentation
    /// </summary>
    /// <param name="raw">
    /// A <see cref="System.String"/>, from which sensor value is restored
    /// </param>
    void RestoreState(string raw);    
}

}
