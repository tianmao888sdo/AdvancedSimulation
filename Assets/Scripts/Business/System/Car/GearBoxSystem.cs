using UnityEngine;
using System.Collections;

public class GearBoxSystem : MonoBehaviour
{
    public int gear = 0;

    public float GetNewRPM(float rpm)
    {
        if(gear==0)
        {

        }
        else if(gear==1)
        {
            return gear * rpm;
        }
        else if (gear == 2)
        {

        }
        else if (gear == 3)
        {


        }
        else if (gear == 4)
        {


        }
        else if (gear == 5)
        {


        }
        else if (gear == -1)
        {

        }

        return 0;
    }

}
