using Godot;

public partial class PIDController : Node2D
{
    //PID variables
    [Export] public float proportionalGain = 1;
    [Export] public float integralGain;
    [Export] public float derivativeGain;
    public float outputMin = -1000;
    public float outputMax = 1000;
    public float integralSaturationX;
    public float errorLastX;
    public float valueLastX;
    public float integrationStoredX;
    public float integralSaturationY;
    public float errorLastY;
    public float valueLastY;
    public float integrationStoredY;

    public float UpdatePIDX(float currentValue, float targetValue, float deltaTime)
    {
        //Calculate error value
        float error = targetValue - currentValue;

        //Calculate proportional term
        float P = proportionalGain * error;

        //Calculate integral term
        integrationStoredX = Mathf.Clamp(integrationStoredX + (error * deltaTime), -integralSaturationX, integralSaturationX);
        float I = integralGain * integrationStoredX;

        //Calculate the change rate of error
        float errorRateOfChange = (error - errorLastX) / deltaTime;
        errorLastX = error;

        //Calculate the change rate of value
        float valueRateOfChange = (currentValue - valueLastX) / deltaTime;
        valueLastX = currentValue;

        //Calculate derivative term
        float D = derivativeGain * valueRateOfChange;

        //Calculate result
        float resultX = P + I + D;

        //Return result
        return resultX;
    }
    
    public float UpdatePIDY(float currentValue, float targetValue, float deltaTime)
    {
        //Calculate error value
        float error = targetValue - currentValue;

        //Calculate proportional term
        float P = proportionalGain * error;

        //Calculate integral term
        integrationStoredY = Mathf.Clamp(integrationStoredY + (error * deltaTime), -integralSaturationY, integralSaturationY);
        float I = integralGain * integrationStoredY;

        //Calculate the change rate of error
        float errorRateOfChange = (error - errorLastY) / deltaTime;
        errorLastY = error;

        //Calculate the change rate of value
        float valueRateOfChange = (currentValue - valueLastY) / deltaTime;
        valueLastY = currentValue;

        //Calculate derivative term
        float D = derivativeGain * valueRateOfChange;

        //Calculate result
        float resultY = P + I + D;

        //Return result
        return resultY;
    }
}
