using UnityEngine;
using GameSavvy.Byterizer;

public class ByterizerLiteDemo : MonoBehaviour
{

    [ContextMenu("Append Demo")]
    private void AppendDemo()
    {
        //initialize object as empty
        var byteStream = new ByteStream();

        //Append parameters in order
        byteStream.Append(true);
        byteStream.Append(1);
        byteStream.Append(2f);
        byteStream.Append("Thsi is a string");

        print($"Byte Length -> {byteStream.Length}");

        //Pop and print from Byte stream in order
        print(byteStream.PopBool());
        print(byteStream.PopInt32());
        print(byteStream.PopFloat());
        print(byteStream.PopString());
    }

    [ContextMenu("Prepend Demo")]
    private void PrependDemo()
    {
        //initialize object as empty
        var byteStream = new ByteStream();

        //Append parameters in order
        byteStream.Append(true);
        byteStream.Append(2f);
        //Prepend a parameter
        byteStream.Prepend("Thsi is a Prepended String");
        byteStream.Prepend(1);

        print($"Byte Length -> {byteStream.Length}, Final order will be: [int, string, bool, float]");

        //Pop and print from Byte stream in order
        print(byteStream.PopInt32());
        print(byteStream.PopString());
        print(byteStream.PopBool());
        print(byteStream.PopFloat());
    }

    [ContextMenu("Encode Demo")]
    private void EcnodeDemo()
    {
        //initialize object as empty
        var byteStream = new ByteStream();

        //Encode parameters in order
        byteStream.Encode(true, 1, 2f, "String added with Encode");

        print($"Byte Length -> {byteStream.Length}");

        //Pop and print from Byte stream in order
        print(byteStream.PopBool());
        print(byteStream.PopInt32());
        print(byteStream.PopFloat());
        print(byteStream.PopString());
    }

    [ContextMenu("Load - Ctor Demo")]
    private void ByterizerLoadDemo()
    {
        //initialize object as empty
        var byteStream1 = new ByteStream();

        //encode parameters in order
        byteStream1.Encode(true, 1, 2f, "String added with Encode");

        //
        var byteStream2 = new ByteStream(byteStream1);
        byteStream2.Prepend(Vector3.one);

        var byteStream3 = new ByteStream();
        byteStream3.Load(byteStream2);
        byteStream3.Append(Quaternion.identity);

        var byteStream4 = new ByteStream();
        byteStream4.Load(byteStream3.ToArray());
        byteStream4.Append('Z');

        print(byteStream4.PopVector3());
        print(byteStream4.PopBool());
        print(byteStream4.PopInt32());
        print(byteStream4.PopFloat());
        print(byteStream4.PopString());
        print(byteStream4.PopQuaternion());
        print(byteStream4.PopChar());

        print($"BS1 Byte Length -> {byteStream1.Length}");
        print($"BS2 Byte Length -> {byteStream2.Length}");
        print($"BS3 Byte Length -> {byteStream3.Length}");
        print($"BS4 Byte Length -> {byteStream4.Length}");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Append Demo")) AppendDemo();
        if (GUILayout.Button("Prepend Demo")) PrependDemo();
        if (GUILayout.Button("Ecnode Demo")) EcnodeDemo();
        if (GUILayout.Button("Load - Ctor Demo")) ByterizerLoadDemo();
    }

}
