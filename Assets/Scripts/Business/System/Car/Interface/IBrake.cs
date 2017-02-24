using UnityEngine;
using System.Collections;

public interface IBrake
{
    void Brake(CarAttributes.BrakeMode brakeMode, float brakeInput);
}
