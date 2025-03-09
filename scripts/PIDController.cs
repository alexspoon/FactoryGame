using Godot;
using System;

public partial class PIDController : Node2D
{
    //PID variables
    [Export] public float proportionalGain = 1;
    [Export] public float integralGain = 0;
    [Export] public float derivativeGain = 0;
    public float outputMin = -1000;
    public float outputMax = 1000;
    public float integralSaturation;
    public float errorLast;
    public float valueLast;
    public float integrationStored;
    public bool derivativeIntialized;

    public float UpdatePID(float currentValue, float targetValue, float deltaTime)
    {
        //Calculate error value
        float error = targetValue - currentValue;

        //Calculate proportional term
        float P = proportionalGain * error;

        //Calculate integral term
        integrationStored = Mathf.Clamp(integrationStored + (error * deltaTime), -integralSaturation, integralSaturation);
        float I = integralGain * integrationStored;

        //Calculate the change rate of error
        float errorRateOfChange = (error - errorLast) / deltaTime;
        errorLast = error;

        //Calculate the change rate of value
        float valueRateOfChange = (currentValue - valueLast) / deltaTime;
        valueLast = currentValue;

        //Calculate derivative term
        float D = derivativeGain * valueRateOfChange;

        //Calculate result
        float result = P + I + D;

        //Return result within minimum and maximum output range
        return Mathf.Clamp(result, outputMin, outputMax);
        //return result;
    }
}
