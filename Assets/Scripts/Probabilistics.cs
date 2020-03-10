using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Probabilistics
{
    public struct Try
    {
        public Try(int successNumber, float exactly, float atLeast)
        {
            SuccessNumber = successNumber;
            Exactly = exactly;
            AtLeast = atLeast;
        }
        public int SuccessNumber { get; }
        public float Exactly { get; }
        public float AtLeast { get; }
    }

    //This returns the probability of hitting AT LEAST targetTimes when you try totalTimes
    //Useful when shooting multiple bullets
    public static IReadOnlyCollection<Try> HittingAtLeastTimes(float probability, int totalTimes)
    {
        Try[] tries = new Try[totalTimes+1];

        tries[0] = new Try(0, ProbabilityOfHittingAtLeastTimes(probability, totalTimes, 0), 1);
        float tot = 0;

        for (int i = 1; i <= totalTimes; i++)
        {
            float res = ProbabilityOfHittingAtLeastTimes(probability, totalTimes, i);
            tries[i] = new Try(i, res, 1 - tot);
            tot += res;
        }
        return tries;
    }

    private static float ProbabilityOfHittingAtLeastTimes(float probability, int totalTimes, int targetTimes)
    {
        return NCR(totalTimes, targetTimes) * Mathf.Pow(probability, targetTimes) *
                     Mathf.Pow(1 - probability, totalTimes - targetTimes);
    }

    private static long Factorial(int number)
    {
        if (number < 0)
            return -1; //Error

        long result = 1;

        for (int i = 1; i <= number; ++i)
            result *= i;

        return result;
    }

    public static long NCR(int n, int r)
    {
        return Factorial(n) / (Factorial(r) * Factorial(n - r));
    }
}
