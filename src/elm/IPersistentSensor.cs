using System;
using System.Collections.Generic;

namespace hobd
{

/**
 * This marker interface is used to denote sensors who want to make their value
 * persistent even between program runs. System will store/restore its value into persistent storage.
 */
public interface IPersistentSensor
{
    string StoreState();

    void RestoreState(string raw);    
}

}
