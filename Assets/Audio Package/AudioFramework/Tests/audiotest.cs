using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestTools;

public class audiotest
{
    // A Test behaves as an ordinary method
    [Test]
    public void audiotestSimplePasses()
    {
        // Use the Assert class to test conditions
        Debug.Assert(true);
    }


    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    // [UnityTest]
    // public IEnumerator audiotestWithEnumeratorPasses()
    // {
    //     // Use the Assert class to test conditions.
    //     // Use yield to skip a frame.
    //     yield return null;
    // }

    // [Test]
    // public void SamplePickModeLogicPasses()
    // {
    //     var testValue = 0.45f;
    //     var result = 0;
    //     var count = 9;
    //     var share = 1f / count;
    //     var shares = new float[count];
    //     for (var i = 0; i < count; i++)
    //     {
    //         shares[i] = share * i;
    //         if (testValue < shares[i])
    //             result = i - 1;
    //             break;
    //     }
    //
    //     Assert.That(result == 4);
    //
    //     // for (var i = 0; i < count; i++)
    //     // {
    //     //     Debug.Log($">>{i}:{shares[i]})");
    //     // }
    //     //
    //     // Debug.Assert(shares.Length == count);
    //
    //
    //
    // }
}
