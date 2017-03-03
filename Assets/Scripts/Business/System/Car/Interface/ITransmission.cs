using UnityEngine;
using System.Collections;

public interface ITransmission
{
    void Move(CarAttributes.MotorMode motorMode, float motorTorque);
}
