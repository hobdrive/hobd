namespace hobd{

using System;

/**
 * Abstract sensor provider
 */
public interface SensorProvider
{

	string GetName();
	
	string GetDescription();
	
	void Activate(SensorRegistry registry);

}

}