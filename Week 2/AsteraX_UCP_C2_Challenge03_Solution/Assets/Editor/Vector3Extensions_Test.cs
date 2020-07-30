using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class Vector3Extensions_Test {

    [Test]
    public void ComponentDivide__8_3_3__By__2_3_0__ShouldBe__4_1_3()
    {
        Vector3 numerator = new Vector3(8,3,3);
        Vector3 denominator = new Vector3(2,3,0);

        Vector3 result = numerator.ComponentDivide(denominator);

        Assert.AreEqual(new Vector3(4,1,3), result);
    }

}
