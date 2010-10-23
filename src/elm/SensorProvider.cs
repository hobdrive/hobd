namespace hobd{

using System;

/**
 * Abstract sensor provider
 * Provides a group of sensors
 */
public interface SensorProvider
{

	string GetName();
	
	string GetDescription();
	
	void Activate(SensorRegistry registry);

}

}