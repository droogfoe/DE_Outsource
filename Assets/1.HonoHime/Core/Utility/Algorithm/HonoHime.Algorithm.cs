using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HonoHime.Algorithm
{
    public static class Algorithm
    {
        public static T[] RandomSort<T>(T[] _ar)
        {
            System.Random random = new System.Random();
            var ar = _ar.OrderBy(x => random.Next()).ToArray();
            return ar;
        }
    }
}
